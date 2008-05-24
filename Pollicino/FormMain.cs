using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Media;
using SharpGis.SharpGps;
using MapsLibrary;
using System.Runtime.InteropServices;


namespace MapperTool
{
    public partial class Form_MapperToolMain : Form
    {
        [DllImport("coredll")]
        extern static void SystemIdleTimerReset();

        SoundPlayer sount_wpt;
        
        protected ApplicationOptions options;
        string configfile;

        protected MapTS map;
        protected SparseImagesMap gmap;
        protected LayeredMap lmap;
        protected LayerPoints trackpoints;
        protected LayerPoints waypoints;
        protected int idx_layer_gmaps, idx_layer_osm;
        protected bool autocenter;

        public Form_MapperToolMain()
        {
            InitializeComponent();

            // carica le opzioni dal file di configurazione
            string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            configfile = path + '\\' + System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + ".cfg";
            try
            {
                options = (System.IO.File.Exists(configfile)) ?
                           ApplicationOptions.FromFile(configfile) : DefaultOptions();
            }
            catch (Exception)
            {
                MessageBox.Show("Config file error! Resetting options to default.");
                options = DefaultOptions();
            }

            // sound per nuovo waypoint
            try
            {
                sount_wpt = new SoundPlayer();
                if (File.Exists(options.Application.WaypointSoundFile))
                    sount_wpt.SoundLocation = options.Application.WaypointSoundFile;
            }
            catch (Exception)
            { }

            this.lmap = new LayeredMap();
            // OSM
            this.map = new CachedMapTS(options.Maps.OSM.TileCachePath, new OSMTileMapSystem(options.Maps.OSM.OSMTileServer), 20);
            idx_layer_osm = lmap.addLayerOnTop(this.map);
            // Google MAPS
            gmap = new SparseImagesMap(new SparseImagesMapSystem(), options.Maps.GMaps.CachePath);
            idx_layer_gmaps = lmap.addLayerOnTop(gmap);
            lmap.setVisibility(idx_layer_gmaps, false);
            // Tracciato GPS
            trackpoints = new LayerBufferedPoints(map.mapsystem);
            lmap.addLayerOnTop(trackpoints);
            // Waypoints
            waypoints = new LayerPoints(map.mapsystem);
            waypoints.SetDrawPointFunction(LayerPoints.DrawEmptySquare, new Pen(Color.Red));
            lmap.addLayerOnTop(waypoints);
            // Croce centrale
            lmap.addLayerOnTop(new LayerCrossCenter(20));


            mapcontrol.Map = lmap;
            this.mapcontrol.PrePaint += new MapControl.MapControlEventHandler(this.scarica_tiles);

            GeoPoint gp = new GeoPoint(44.1429, 12.2618);
            mapcontrol.Zoom = 12;
            mapcontrol.Center = map.mapsystem.CalcProjection(gp);

            this.gpsControl.PositionUpdated += new GPSControl.PositionUpdateHandler(GPSEventHandler);
        
        }

        /// <summary>
        /// Responds to sentence events from GPS receiver
        /// </summary>
        //protected void GPSEventHandler(object sender, GPSHandler.GPSEventArgs e)
        protected void GPSEventHandler(GPSControl sender, GPSControl.GPSPosition gpsdata)
        {
            this.trackpoints.addPoint(map.mapsystem.CalcProjection(gpsdata.position));
            this.label_lat.Text = gpsdata.position.dLat.ToString("F7");
            this.label_lon.Text = gpsdata.position.dLon.ToString("F7");
            if (this.autocenter)
                mapcontrol.Center = map.mapsystem.CalcProjection(gpsdata.position);

        }

        private void mapcontrol_ZoomChanged(MapsLibrary.MapControl sender)
        {
            this.label_zoom.Text = sender.Zoom.ToString();
        }

        private void menuItem_zoomin_Click(object sender, EventArgs e)
        {
            this.mapcontrol.Zoom++;
        }

        private void menuItem_zoomout_Click(object sender, EventArgs e)
        {
            this.mapcontrol.Zoom--;
        }

        private void menuItem_map_osm_Click(object sender, EventArgs e)
        {
            if (!lmap.isVisible(idx_layer_osm))
            {
                lmap.setVisibility(idx_layer_osm, true);
                lmap.setVisibility(idx_layer_gmaps, false);
                mapcontrol.Invalidate();   
                this.menuItem_map_gmaps.Enabled = true;
                this.menuItem_map_osm.Enabled = false;
            }
        }

        private void menuItem_map_gmaps_Click(object sender, EventArgs e)
        {
            if (!lmap.isVisible(idx_layer_gmaps))
            {
                lmap.setVisibility(idx_layer_osm, false);
                lmap.setVisibility(idx_layer_gmaps, true);
                mapcontrol.Invalidate();
                this.menuItem_map_gmaps.Enabled = false;
                this.menuItem_map_osm.Enabled = true;
            }
        }

        private void menuItem_loadtrack_Click(object sender, EventArgs e)
        {
            string opendir = options.GPS.LogsDir,
                   file;
            if (!Directory.Exists(opendir))
                opendir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            using (FormOpenFile openfiledlg = new FormOpenFile(opendir, false))
            {
                if (openfiledlg.ShowDialog() == DialogResult.OK)
                    file = openfiledlg.openfile;
                else
                    return;
            }
            
            // count: for debug
            int count = 0;
            System.Windows.Forms.Cursor.Current = Cursors.WaitCursor;
            try
            {
                // Create an instance of StreamReader to read from a file.
                // The using statement also closes the StreamReader.
                using (System.IO.StreamReader sr = new System.IO.StreamReader(file))
                {
                    GeoPoint gp;
                    String line;
                    // Read and display lines from the file until the end of 
                    // the file is reached.
                    while ((line = sr.ReadLine()) != null)
                    {
                        switch (line.Substring(0,6)) {
                            case "$GPRMC":
                                SharpGis.SharpGps.NMEA.GPRMC gpmrc = new SharpGis.SharpGps.NMEA.GPRMC(line);
                                gp = new GeoPoint(gpmrc.Position.Latitude, gpmrc.Position.Longitude);
                                this.trackpoints.addPoint(this.map.mapsystem.CalcProjection(gp));
                                count++;
                                break;
                            case "$GPWPL":
                                GPWPL gpwpl = new GPWPL(line);
                                gp = new GeoPoint(gpwpl.latitude, gpwpl.longitude);
                                this.waypoints.addPoint(this.map.mapsystem.CalcProjection(gp));
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Let the user know what went wrong.
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(ex.Message);
            }
            System.Windows.Forms.Cursor.Current = Cursors.Default;
            mapcontrol.Invalidate();
        }

        private void menuItem_followGPS_Click(object sender, EventArgs e)
        {
            autocenter = !autocenter;
            this.menuItem_followGPS.Text = autocenter ? "Don't follow GPS" : "Follow GPS";
        }

        private void menuItem_gpsactivity_Click(object sender, EventArgs e)
        {
            if (!this.gpsControl.Started)
            {
                string logname = null;
                if (options.GPS.Simulation && File.Exists(options.GPS.SimulationFile))
                {
                    gpsControl.SimulationFile = options.GPS.SimulationFile;
                    //DEBUG
                    logname = options.GPS.LogsDir + "\\gpslog_" + DateTime.Now.ToString("yyyy-MM-dd_HHmmss") + ".txt";
                }
                else
                {
                    gpsControl.SimulationFile = null;
                    logname = options.GPS.LogsDir + "\\gpslog_" + DateTime.Now.ToString("yyyy-MM-dd_HHmmss") + ".txt";
                }
                gpsControl.start(options.GPS.PortName, options.GPS.PortSpeed, logname);
                this.menuItem_gpsactivity.Text = "stop GPS";
            }
            else
            {
                gpsControl.stop();
                this.menuItem_gpsactivity.Text = "start GPS";
            }
        }

        private void menuItem_downloadmaps_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Cursor.Current = Cursors.WaitCursor;

            PxCoordinates c1, c2;
            //c1 = map.mapsystem.TileCooToPx(this.pb_Centered.tc);
            c1 = map.mapsystem.PointToPx(mapcontrol.Center, mapcontrol.Zoom);
            c1.xpx -= this.mapcontrol.Size.Width / 2;
            c1.ypx -= this.mapcontrol.Size.Height / 2;
            c2 = c1;
            c2.xpx += this.mapcontrol.Size.Width;
            c2.ypx += this.mapcontrol.Size.Height;
            ProjectedGeoArea area = new ProjectedGeoArea(map.mapsystem.PxToPoint(c1, mapcontrol.Zoom), map.mapsystem.PxToPoint(c2, mapcontrol.Zoom));
            for (uint i = 1; i <= options.Maps.OSM.DownloadDepth; i++)
                map.downloadArea(area, mapcontrol.Zoom + i, false);

            // scarica le mappe di google maps
            gmap.downloadAt(mapcontrol.Center, mapcontrol.Zoom, false);
            gmap.downloadAt(mapcontrol.Center, mapcontrol.Zoom + 1, false);
            Int32 delta = gmap.mapsystem.PxToPoint(new PxCoordinates(200, 0), mapcontrol.Zoom + 2).nLon;  // dipende dalla dimensione massima di un'immagine di mappa
            ProjectedGeoPoint p = mapcontrol.Center;
            gmap.downloadAt(new ProjectedGeoPoint(p.nLat - delta, p.nLon - delta), mapcontrol.Zoom + 2, false);
            gmap.downloadAt(new ProjectedGeoPoint(p.nLat - delta, p.nLon + delta), mapcontrol.Zoom + 2, false);
            gmap.downloadAt(new ProjectedGeoPoint(p.nLat + delta, p.nLon - delta), mapcontrol.Zoom + 2, false);
            gmap.downloadAt(new ProjectedGeoPoint(p.nLat + delta, p.nLon + delta), mapcontrol.Zoom + 2, false);

            System.Windows.Forms.Cursor.Current = Cursors.Default;
        }

        private void scarica_tiles(MapControl sender)
        {
            if (options.Maps.OSM.AutoDownload && lmap.isVisible(idx_layer_osm)) {
                lock (this.map)
                {
                    try
                    {
                        this.map.downloadAt(sender.Center, sender.Zoom, false);
                    }
                    catch (System.Net.WebException)
                    {
                        if (MessageBox.Show("Disable autodownload?", "Download Error", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1)
                            == DialogResult.Yes)
                        {
                            options.Maps.OSM.AutoDownload = false;
                        }
                    }
                }
            }
        }

        private void menuItem_config_Click(object sender, EventArgs e)
        {
            using (FormOptions opt = new FormOptions())
            {
                opt.data = this.options;
                opt.ShowDialog();
                ApplicationOptions newopt = opt.data;
                options.SaveToFile(this.configfile);
                if (options.Maps.OSM.OSMTileServer != newopt.Maps.OSM.OSMTileServer) 
                    MessageBox.Show("The tile server is changed. You need to restart the application and you may need to refresh or delete the cache.", "Attention!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                if (options.Application.WaypointSoundFile != newopt.Application.WaypointSoundFile)
                    sount_wpt.SoundLocation = newopt.Application.WaypointSoundFile;
                options = newopt;
            }
        }

        private static ApplicationOptions DefaultOptions()
        {
            string programpath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            ApplicationOptions opt = new ApplicationOptions();
            opt.GPS.PortName = "COM1";
            opt.GPS.PortSpeed = 9600;
            opt.GPS.Simulation = false;
            opt.GPS.SimulationFile = "";
            opt.GPS.LogsDir = programpath + "\\gpslogs";
            opt.Maps.OSM.OSMTileServer = "http://tile.openstreetmap.org/";
            opt.Maps.OSM.TileCachePath = programpath + "\\tiles";
            opt.Maps.OSM.DownloadDepth = 3;
            opt.Maps.OSM.AutoDownload = true;
            opt.Maps.GMaps.CachePath = programpath + "\\gmaps";
            opt.Application.WaypointSoundPlay = true;
            opt.Application.WaypointSoundFile = "\\Windows\\Infbeg.wav";
            return opt;
        }

        private void menuItem_waypoint_Click(object sender, EventArgs e)
        {
            create_waypoint();
        }

        private void create_waypoint()
        {
            if (gpsControl.Started) {
                GeoPoint pos = gpsControl.saveWaypoint().position;

                waypoints.addPoint(map.mapsystem.CalcProjection(pos));
                if (options.Application.WaypointSoundPlay && sount_wpt != null)
                    sount_wpt.Play();
            }
        }

        // Calculates the checksum for a sentence
        private static string getNMEAChecksum(string sentence)
        {
            //start with first Item
            int checksum = Convert.ToByte(sentence[sentence.IndexOf('$') + 1]);
            // Loop through all chars to get a checksum
            for (int i = sentence.IndexOf('$') + 2; i < sentence.IndexOf('*'); i++)
            {
                // No. XOR the checksum with this character's value
                checksum ^= Convert.ToByte(sentence[i]);
            }
            // Return the checksum formatted as a two-character hexadecimal
            return checksum.ToString("X2");
        }

        private void Form_MapperToolMain_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == System.Windows.Forms.Keys.Up))
            {
                // Up
            }
            if ((e.KeyCode == System.Windows.Forms.Keys.Down))
            {
                // Down
            }
            if ((e.KeyCode == System.Windows.Forms.Keys.Left))
            {
                // Left
            }
            if ((e.KeyCode == System.Windows.Forms.Keys.Right))
            {
                // Right
            }
            if ((e.KeyCode == System.Windows.Forms.Keys.Enter))
            {
                // Enter
                create_waypoint();
            }

        }

        private void menuItem_exit_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Close application. Are you sure?", "", MessageBoxButtons.YesNo,MessageBoxIcon.None,MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                Application.Exit();

        }

        private void menuItem_savegpx_Click(object sender, EventArgs e)
        {
            string opendir = options.GPS.LogsDir,
                   file, outfile;
            if (!Directory.Exists(opendir))
                opendir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            using (FormOpenFile openfiledlg = new FormOpenFile(opendir, false))
            {
                if (openfiledlg.ShowDialog() == DialogResult.OK)
                    file = openfiledlg.openfile;
                else
                    return;
            }
            System.Windows.Forms.Cursor.Current = Cursors.WaitCursor;
            outfile = (file.EndsWith(".txt")) ? file.Substring(0, file.Length - 4) : file;
            outfile += ".gpx";
            try
            {
                NMEA2GPX.GPXGenerator.NMEAToGPX(file, outfile);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error!");
            }
            System.Windows.Forms.Cursor.Current = Cursors.Default;
        }



    }
}