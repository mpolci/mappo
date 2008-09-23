/*******************************************************************************
 *  Pollicino - A tool for gps mapping.
 *  Copyright (C) 2008  Marco Polci
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see http://www.gnu.org/licenses/gpl.html.
 * 
 ******************************************************************************/

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
using OpenNETCF.Windows.Forms;
using Microsoft.WindowsMobile.Forms;
using Microsoft.WindowsCE.Forms;
//using System.Runtime.InteropServices;



namespace MapperTools.Pollicino
{
    public partial class Form_MapperToolMain : Form
    {
        OpenNETCF.Windows.Forms.NotifyIcon notify_icon;

//        [DllImport("coredll")]
//        extern static void SystemIdleTimerReset();

        private DateTime activatedTime = DateTime.MinValue;

        /// <summary>
        /// Utilizzato per eliminare i rimbalzi del tasto enter (pressioni multiple molto ravvicinate nel tempo).
        /// </summary>
        private int lastEntertime = 0;

        SoundPlayer wpt_sound;
        AudioRecorder wpt_recorder;
        
        protected ApplicationOptions options;
        string configfile;

        protected CachedTilesMap map;
        protected SparseImagesMap gmap;
        protected LayeredMap lmap;
        protected LayerPoints trackpoints;
        protected LayerPoints waypoints;
        protected int idx_layer_gmaps, idx_layer_osm, idx_layer_trkpnt, idx_layer_waypnt;
        protected string logname;
        protected Downloader downloader;

        protected const string DateOnFilenameFormat = "yyyy-MM-dd_HHmmss";

        private bool autocenter
        {
            get { return options.Application.AutoCentreMap; }
            set
            {
                options.Application.AutoCentreMap = value;
                menuItem_followGPS.Checked = value;
            }
        }

        public Form_MapperToolMain()
        {
            InitializeComponent();

            notify_icon = new OpenNETCF.Windows.Forms.NotifyIcon();
            notify_icon.Icon = Properties.Resources.Map;
            notify_icon.Visible = true;
            notify_icon.Click += new EventHandler(this.notify_icon_click);

            gpx_saver.Notifier = blinkcnGPX;
            
            // carica le opzioni dal file di configurazione
            carica_opzioni();
            // disabilito l'autodownload
            options.Maps.AutoDownload = false;

            //modalità full screen
            this.WindowState = options.Application.FullScreen ? FormWindowState.Maximized : FormWindowState.Normal;
            // pulsante per la fotocamera
            this.hardwareButton_app3.HardwareKey = options.Application.CameraButton;

            // sound per nuovo waypoint
            try
            {
                if (File.Exists(options.Application.WaypointSoundFile))
                {
                    wpt_sound = new SoundPlayer();
                    wpt_sound.SoundLocation = options.Application.WaypointSoundFile;
                }
            }
            catch (Exception)
            { }

            wpt_recorder = new AudioRecorder(options.Application.RecordAudioDevice);

            // thread per il download automatico delle mappe
            downloader = new Downloader(this.blinkcnDownloader);
            downloader.startThread();
            // autodownload flag
            menuItem_autodownload.Checked = options.Maps.AutoDownload;
            // show position flag
            menuItem_showpos.Checked = mapcontrol.ShowPosition;

            // mappe
            this.lmap = new LayeredMap();
            // OSM
            this.map = new CachedTilesMap(options.Maps.OSM.TileCachePath, OSMTileMapSystem.CreateOSMMapSystem(options.Maps.OSM.OSMTileServer), required_buffers());

            //this.map = new ReadAheadCachedTilesMap(options.Maps.OSM.TileCachePath, new OSMTileMapSystem(options.Maps.OSM.OSMTileServer), 20, new Size(320, 240));
            idx_layer_osm = lmap.addLayerOnTop(this.map);
            // Google MAPS
            gmap = new SparseImagesMap(new GoogleMapsSystem(Properties.Resources.GoogleMapsKey), options.Maps.GMaps.CachePath, 150);
            idx_layer_gmaps = lmap.addLayerOnTop(gmap);
            lmap.setVisibility(idx_layer_gmaps, false);
            // Tracciato GPS
            trackpoints = new LayerPoints(map.mapsystem);
            idx_layer_trkpnt = lmap.addLayerOnTop(trackpoints);
            // Waypoints
            waypoints = new LayerPoints(map.mapsystem);
            waypoints.SetDrawPointFunction(LayerPoints.DrawEmptySquare, new Pen(Color.Red));
            idx_layer_waypnt = lmap.addLayerOnTop(waypoints);

            verifica_opzioni_intermedio();

            mapcontrol.Map = lmap;
            this.mapcontrol.PrePaint += new MapControl.MapControlEventHandler(this.prepara_mappe);

            mapcontrol.Zoom = 12;
            mapcontrol.Center = map.mapsystem.CalcProjection(options.Application.InitialMapPosition);

            autocenter = options.Application.AutoCentreMap;
            this.gpsControl.PositionUpdated += new GPSControl.PositionUpdateHandler(GPSEventHandler);

            verifica_opzioni_finale();

            // processa eventuali file di log che non sono ancora stati convertiti in GPX
            if (Directory.Exists(options.GPS.LogsDir)) 
                gpx_saver.ParseLogsDir(options.GPS.LogsDir);
        
        }

        private uint required_buffers()
        {
            Size cs = this.ClientSize;
            // FIXME: il codice qui sotto dipende dalla dimensione del tile
            uint x = (uint)Math.Ceiling((double)cs.Width / 256.0 + 1);
            uint y = (uint)Math.Ceiling((double)cs.Height / 256.0 + 1);
            return x * y + x + y - 1;
        }

        private void carica_opzioni()
        {
            string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            configfile = path + '\\' + System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + ".cfg";
            try
            {
                if (System.IO.File.Exists(configfile)) {
                    options = ApplicationOptions.FromFile(configfile);
                    // qui posso controllare il numero di versione ed effettua eventuali aggiornamenti
                    //if (options.version < ??) ...
                    // ATTENZIONE! il numero di versione verrà aggiornato in verifica_opzioni_finale()
                } else 
                    options = DefaultOptions();
            }
            catch (Exception)
            {
                MessageBox.Show("Config file error! Resetting options to default.");
                options = DefaultOptions();
            }
        }

        private void verifica_opzioni_intermedio()
        {
            if (options.version < 1)
            {
                try
                {
                    // è necessario spostare gli eventuali file dei tile
                    DirectoryInfo newdir = new DirectoryInfo(map.TileCachePath);
                    if (!newdir.Exists) newdir.Create();
                    DirectoryInfo source = new DirectoryInfo(options.Maps.OSM.TileCachePath);
                    foreach (DirectoryInfo zoomdir in source.GetDirectories())
                    {
                        try
                        {
                            zoomdir.MoveTo(newdir.FullName + zoomdir.Name);
                        }
                        catch (IOException) { }  // l'eccezione verrà sicuramente generata perché newdir è sottodirectory di source
                        catch (Exception ex)
                        {
                            System.Diagnostics.Trace.WriteLine("---- " + DateTime.Now + " ---- Eccezione inaspettata");
                            System.Diagnostics.Trace.WriteLine("Stato: " + source.FullName + " - " + newdir.FullName + " - " + zoomdir.Name);
                            System.Diagnostics.Trace.WriteLine(ex.ToString());
                            throw;
                        }
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Errore inaspettato! Si consiglia di inviare il file di log \"pollicino_log.txt\" all'autore.");
                }
            }
        }

        private void verifica_opzioni_finale()
        {
            if (options.version < ApplicationOptions.CurrentVersion)
            {
                options.version = ApplicationOptions.CurrentVersion;
                options.SaveToFile(this.configfile);
            }
        }

        /// <summary>
        /// Responds to sentence events from GPS receiver
        /// </summary>
        protected void GPSEventHandler(GPSControl sender, GPSControl.GPSPosition gpsdata)
        {
            this.trackpoints.addPoint(map.mapsystem.CalcProjection(gpsdata.position));
            if (autocenter)
                mapcontrol.Center = map.mapsystem.CalcProjection(gpsdata.position);

        }

        private void action_CentreMap()
        {
            if (gpsControl.Started) 
                mapcontrol.Center = map.mapsystem.CalcProjection(this.gpsControl.PositionData.position);
        }

        private void action_CiclesVisibleMap()
        {
            if (lmap.isVisible(idx_layer_gmaps))
            {
                lmap.setVisibility(idx_layer_osm, true);
                lmap.setVisibility(idx_layer_gmaps, false);
                mapcontrol.Invalidate();
                this.menuItem_map_gmaps.Enabled = true;
                this.menuItem_map_osm.Enabled = false;
            }
            else
            {
                lmap.setVisibility(idx_layer_osm, false);
                lmap.setVisibility(idx_layer_gmaps, true);
                mapcontrol.Invalidate();
                this.menuItem_map_gmaps.Enabled = false;
                this.menuItem_map_osm.Enabled = true;
            }
        }

        private void action_loadtrack()
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
            lmap.setVisibility(idx_layer_trkpnt, false);
            lmap.setVisibility(idx_layer_waypnt, false);
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
            lmap.setVisibility(idx_layer_trkpnt, true);
            lmap.setVisibility(idx_layer_waypnt, true);
            System.Windows.Forms.Cursor.Current = Cursors.Default;
            
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
            action_CiclesVisibleMap();
        }

        private void menuItem_map_gmaps_Click(object sender, EventArgs e)
        {
            action_CiclesVisibleMap();
        }

        private void menuItem_loadtrack_Click(object sender, EventArgs e)
        {
            action_loadtrack();
        }

        private void menuItem_followGPS_Click(object sender, EventArgs e)
        {
            autocenter = !autocenter;
        }

        private void menuItem_autodownload_Click(object sender, EventArgs e)
        {
            options.Maps.AutoDownload = !options.Maps.AutoDownload;
            menuItem_autodownload.Checked = options.Maps.AutoDownload;
            if (!options.Maps.AutoDownload)
                downloader.clearQueue();
        }

        private void menuItem_showpos_Click(object sender, EventArgs e)
        {
            mapcontrol.ShowPosition = !mapcontrol.ShowPosition;
            menuItem_showpos.Checked = mapcontrol.ShowPosition;
        }

        private void menuItem_showscale_Click(object sender, EventArgs e)
        {
            mapcontrol.ShowScaleRef = !mapcontrol.ShowScaleRef;
            menuItem_showscale.Checked = mapcontrol.ShowScaleRef;
        }

        private void menuItem_photo_Click(object sender, EventArgs e)
        {
            action_takephoto();
        }


        private void menuItem_gpsactivity_Click(object sender, EventArgs e)
        {
            if (!this.gpsControl.Started)
            {
                logname = null;
                if (options.GPS.Simulation && File.Exists(options.GPS.SimulationFile))
                {
                    gpsControl.SimulationFile = options.GPS.SimulationFile;
                    //DEBUG
                    logname = options.GPS.LogsDir + "\\gpslog_" + DateTime.Now.ToString(DateOnFilenameFormat) + ".txt";
                }
                else
                {
                    gpsControl.SimulationFile = null;
                    logname = options.GPS.LogsDir + "\\gpslog_" + DateTime.Now.ToString(DateOnFilenameFormat) + ".txt";
                }
                gpsControl.start(options.GPS.PortName, options.GPS.PortSpeed, logname);
                this.menuItem_gpsactivity.Text = "stop GPS";
            }
            else
            {
                if (wpt_recorder.Running)
                    wpt_recorder.stop();
                string logfile = gpsControl.stop();
                // sarebbe più sensato impostare DelayTrackStart quando cambio l'opzione relativa
                gpx_saver.DelayTrackStart = options.Application.DelayGPXTrackStart;
                gpx_saver.SaveGPX(logfile);
                this.menuItem_gpsactivity.Text = "start GPS";
            }
        }

        private void menuItem_downloadmaps_Click(object sender, EventArgs e)
        {
            ProjectedGeoArea area = mapcontrol.VisibleArea;
            int end = (mapcontrol.Zoom > 1) ? -1 : 0;
            for (int i = options.Maps.OSM.DownloadDepth; i >=  end ; i--)
            {
                uint z = (uint) (mapcontrol.Zoom + i);
                downloader.addDownloadArea(map, area, z);
                if (i <= 2)
                    downloader.addDownloadArea(gmap, area, z);
            }

        }
/*
        private void menuItem_refreshTileCache_Click(object sender, EventArgs e)
        {

            if (MessageBox.Show("Maybe it will takes long time. Do you want to continue?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                return;

            System.Windows.Forms.Cursor.Current = Cursors.WaitCursor;

            ProjectedGeoArea area = mapcontrol.VisibleArea;
            uint maxzoom = Math.Min(mapcontrol.Zoom + 6, map.mapsystem.MaxZoom);
            int errors = 0;
            for (uint i = mapcontrol.Zoom; i <= maxzoom ; i++)
            {
                try {
                    map.updateTilesInArea(area, i); 
                } catch (Exception ex) {
                    System.Diagnostics.Trace.WriteLine("----\n" + ex.ToString() + "\n----");
                    if (++errors >= 10) break;
                }
            }
            if (errors > 0) {
                MessageBox.Show("There was problems to get some tiles.");
            }

            System.Windows.Forms.Cursor.Current = Cursors.Default;
        }
*/
        private void menuItem_config_Click(object sender, EventArgs e)
        {
            using (FormOptions opt = new FormOptions())
            {
                opt.data = (ApplicationOptions) this.options.Clone();
                opt.ShowDialog();
                ApplicationOptions newopt = opt.data;
                if (options.Maps.OSM.OSMTileServer != newopt.Maps.OSM.OSMTileServer)
                    this.map.mapsystem = OSMTileMapSystem.CreateOSMMapSystem(newopt.Maps.OSM.OSMTileServer);
                if (options.Application.WaypointSoundFile != newopt.Application.WaypointSoundFile)
                    wpt_sound.SoundLocation = newopt.Application.WaypointSoundFile;
                wpt_recorder.DeviceID = newopt.Application.RecordAudioDevice;
                wpt_recorder.RecordingFormat = newopt.Application.RecordAudioFormat;
                //modalità full screen
                this.WindowState = newopt.Application.FullScreen ? FormWindowState.Maximized : FormWindowState.Normal;
                this.hardwareButton_app3.HardwareKey = newopt.Application.CameraButton;
                options = newopt;
                options.SaveToFile(this.configfile);
            }
        }

        private void menuItem_waypoint_Click(object sender, EventArgs e)
        {
            action_CreateWaypoint();
        }

        private void menuItem_exit_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Close application. Are you sure?", "", MessageBoxButtons.YesNo, MessageBoxIcon.None, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                this.Close();
        }

        private void prepara_mappe(MapControl sender)
        {
            if (options.Maps.AutoDownload)
            {
                IDownloadableMap vm = lmap.isVisible(idx_layer_osm) ? (IDownloadableMap)map : (IDownloadableMap)gmap;
                downloader.addDownloadArea(vm, mapcontrol.VisibleArea, mapcontrol.Zoom);
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
            opt.Maps.AutoDownload = false;
            opt.Maps.GMaps.CachePath = programpath + "\\gmaps";
            opt.Application.DelayGPXTrackStart = false;
            opt.Application.WaypointSoundPlay = true;
            opt.Application.WaypointSoundFile = "\\Windows\\Infbeg.wav";
            opt.Application.WaypointRecordAudioSeconds = 10;
            opt.Application.WaypointRecordAudio = true;
            opt.Application.RecordAudioDevice = 0;
            opt.Application.RecordAudioFormat = OpenNETCF.Media.WaveAudio.SoundFormats.Mono16bit11kHz;
            opt.Application.AutoCentreMap = true;
            opt.Application.InitialMapPosition = new GeoPoint(44.1429, 12.2618);
            opt.Application.FullScreen = false;
            opt.Application.CameraButton = HardwareKeys.ApplicationKey3;
            opt.version = ApplicationOptions.CurrentVersion;
            return opt;
        }

        private void waypoint_created(GPSControl.GPSPosition gpsdata)
        {
            if (options.Application.WaypointSoundPlay && wpt_sound != null)
                wpt_sound.Play();
            waypoints.addPoint(map.mapsystem.CalcProjection(gpsdata.position));
        }

        private void action_CreateWaypoint()
        {
            if (gpsControl.Started) {
                GPSControl.GPSPosition gpsdata = gpsControl.saveWaypoint(WaypointNames.WPNameFormatString);
                waypoint_created(gpsdata);
                //record audio
                if (options.Application.WaypointRecordAudio)
                {
                    string recdir = WaypointNames.DataDir(logname),
                           recfilename = WaypointNames.AudioRecFile(logname, gpsdata.fixtime);
                    if (!Directory.Exists(recdir))
                        Directory.CreateDirectory(recdir);
                    wpt_recorder.start(recfilename, options.Application.WaypointRecordAudioSeconds);
                }
            }
#if DEBUG
            else
                System.Diagnostics.Debug.WriteLine(DateTime.Now.ToString() + " --- No waypoint created because GPS is not started" );
#endif
        }

        private void action_takephoto()
        {
            if (gpsControl.Started)
            {
                System.Diagnostics.Debug.Assert(logname != null, "action_takephoto() - No log file");
                string outdir = WaypointNames.DataDir(this.logname);
                if (!Directory.Exists(outdir))
                    Directory.CreateDirectory(outdir);

                try
                {
                    using (CameraCaptureDialog cameraCapture = new CameraCaptureDialog())
                    {
                        cameraCapture.Owner = this;
                        cameraCapture.InitialDirectory = outdir;
                        cameraCapture.DefaultFileName = "temp.jpg";
                        cameraCapture.Title = "Waypoint photo";
                        //cameraCapture.Resolution = new Size(176, 144);
                        cameraCapture.Mode = CameraCaptureMode.Still;
                        if (cameraCapture.ShowDialog() == DialogResult.OK)
                        {
                            GPSControl.GPSPosition gpsdata = gpsControl.saveWaypoint(WaypointNames.WPNameFormatString);
                            File.Move(cameraCapture.FileName, WaypointNames.PictureFile(this.logname, gpsdata.fixtime));
                            waypoint_created(gpsdata);
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("Funzionalità non supportata in questo dispositivo.");
                    System.Diagnostics.Trace.WriteLine("--- Errore nell'utilizzo della fotocamera: " + e.ToString());
                }
            }
        }

        private void Form_MapperToolMain_KeyDown(object sender, KeyEventArgs e)
        { 
            if ((e.KeyCode == System.Windows.Forms.Keys.Up))
            {
                // Up
                mapcontrol.Zoom++;
            }
            if ((e.KeyCode == System.Windows.Forms.Keys.Down))
            {
                // Down
                mapcontrol.Zoom--;
            }
            if ((e.KeyCode == System.Windows.Forms.Keys.Left))
            {
                // Left
                action_CentreMap();
            }
            if ((e.KeyCode == System.Windows.Forms.Keys.Right))
            {
                // Right
                action_CiclesVisibleMap();
            }
            if ((e.KeyCode == System.Windows.Forms.Keys.Enter))
            {
                // Enter
                // deve passare un tempo minimo fra un tasto e l'altro, altrimenti ignora il tasto
                int now = Environment.TickCount,
                    intervalFromLast = now - lastEntertime;
                lastEntertime = now;
                //System.Diagnostics.Debug.WriteLine("Key " + e.KeyCode + " tick: " + now + "last key tick: " + lastEntertime + " difference: " + intervalFromLast);
                if (intervalFromLast > 500) 
                    action_CreateWaypoint();
                #if DEBUG
                else 
                    System.Diagnostics.Debug.WriteLine("Ignoring key - time from last keypress: " + intervalFromLast);
                #endif
            }
            if ((HardwareKeys)e.KeyCode == Microsoft.WindowsCE.Forms.HardwareKeys.ApplicationKey3)
            {
                // se il tasto è stato premuto a meno di 1 secondo dall'attivazione probabilmente è stato
                // la causa stessa dell'attivazione quindi non scatto la foto
                TimeSpan timefromactivation = DateTime.Now - activatedTime;
                System.Diagnostics.Debug.WriteLine("--- HardwareKey pressed - activation time: " + timefromactivation);
                if (timefromactivation.TotalSeconds > 1) 
                    action_takephoto();
            }
        }

        private void notify_icon_click(object obj, EventArgs args)
        {
            this.Activate();
        }

        private void Form_MapperToolMain_Closing(object sender, CancelEventArgs e)
        {
            if (wpt_recorder.Running)
                wpt_recorder.stop();
            if (gpsControl.Started)
                gpsControl.stop();
            downloader.stopThread();
            notify_icon.Dispose();
            map.Dispose();
            gpx_saver.Dispose();

            options.Application.InitialMapPosition = map.mapsystem.CalcInverseProjection(mapcontrol.Center);
            options.SaveToFile(this.configfile);
        }

        private void Form_MapperToolMain_Activated(object sender, EventArgs e)
        {
            activatedTime = DateTime.Now;
        }
    }
}