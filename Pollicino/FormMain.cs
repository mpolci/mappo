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
using GMapsDataAPI;
#if PocketPC || Smartphone || WindowsCE
using Microsoft.WindowsMobile.Forms;
using Microsoft.WindowsCE.Forms;
#endif 
using System.Threading;

namespace MapperTools.Pollicino
{
    public partial class Form_MapperToolMain : Form
    {
#if PocketPC || Smartphone || WindowsCE
        NotifyIcon mNotifyIcon;
        private Microsoft.WindowsCE.Forms.HardwareButton hardwareButton_app3;
        protected ExtendedInput ext_in;
#endif

        private DateTime activatedTime = DateTime.MinValue;

        /// <summary>
        /// Utilizzato per eliminare i rimbalzi del tasto enter (pressioni multiple molto ravvicinate nel tempo).
        /// </summary>
        private int lastEntertime = 0;

        SoundPlayer wpt_sound;
        AudioRecorder wpt_recorder;

        protected ApplicationOptions options;
                
        string configfile;

        protected string logname;
        protected Downloader downloader;
        protected OnlineTrackingHandler tracking;
        protected Odometer odo;

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

        public bool ShowPosition
        {
            get
            {
                System.Diagnostics.Debug.Assert(options.Application.ShowPosition == mapcontrol.ShowPosition, @"ShowPosition inconsintence");
                System.Diagnostics.Debug.Assert(options.Application.ShowPosition == menuItem_showpos.Checked, @"ShowPosition inconsintence");
                return options.Application.ShowPosition; ;
            }
            set
            {
                mapcontrol.ShowPosition = value;
                menuItem_showpos.Checked = value;
                options.Application.ShowPosition = value;
            }
        }

        public bool ShowScaleRef
        {
            get
            {
                System.Diagnostics.Debug.Assert(options.Application.ShowScale == mapcontrol.ShowScaleRef, @"ShowPosition inconsintence");
                System.Diagnostics.Debug.Assert(options.Application.ShowScale == menuItem_showscale.Checked, @"ShowPosition inconsintence");
                return options.Application.ShowScale; ;
            }
            set
            {
                mapcontrol.ShowScaleRef = value;
                menuItem_showscale.Checked = value;
                options.Application.ShowScale = value;
            }
        }

        public bool ShowOdometer
        {
            get
            {
                System.Diagnostics.Debug.Assert(options.Application.ShowOdometer == menuItem_Odometer.Checked, @"ShowOdometer inconsitence");
                System.Diagnostics.Debug.Assert(options.Application.ShowOdometer == label_odometer.Visible, @"ShowOdometer inconsitence");
                return options.Application.ShowOdometer;
            }
            set
            {
                options.Application.ShowOdometer = value;
                menuItem_Odometer.Checked = value;
                label_odometer.Visible = value;
            }
        }

        public Form_MapperToolMain()
        {
            InitializeComponent();
            // carica le opzioni dal file di configurazione
            carica_opzioni();

#if PocketPC || Smartphone || WindowsCE
            // 
            // hardwareButton_app3
            // 
            this.hardwareButton_app3 = new Microsoft.WindowsCE.Forms.HardwareButton();
            this.hardwareButton_app3.AssociatedControl = this;
            // pulsante per la fotocamera
            this.hardwareButton_app3.HardwareKey = options.Application.CameraButton;
            // mNotifyIcon
            mNotifyIcon = new NotifyIcon(Properties.Resources.Map);
            mNotifyIcon.Click += new EventHandler(this.notify_icon_click);
            // Extended Input (sensors)
            ext_in = new ExtendedInput(this);
            ext_in.GDisplayOff = true;
            ext_in.NavCW += new ExtendedInput.NavHandler(() => mapcontrol.Zoom++);
            ext_in.NavCCW += new ExtendedInput.NavHandler(() => mapcontrol.Zoom--);
#endif
            // disabilito l'autodownload
            options.Maps.AutoDownload = false;
            // FIXME: disabilito l'autostart
            options.GPS.Autostart = false;

            //modalità full screen
            this.WindowState = options.Application.FullScreen ? FormWindowState.Maximized : FormWindowState.Normal;

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

            // Sistema eventuali log rimasti in sospeso
            string append_to_log = recover_logs();
            // Aggiorna il DB dei GPX (imposta gpx_saver.GPXFilesDB) e converte eventuali log non ancora convertiti
            gpx_saver.Notifier = blinkcnGPX;
            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(this.init_gpx_db));

            // thread per il download automatico delle mappe
            downloader = new Downloader(this.blinkcnDownloader);
            downloader.startThread();
            // autodownload flag
            menuItem_autodownload.Checked = options.Maps.AutoDownload;
            // show position flag
            //menuItem_showpos.Checked = mapcontrol.ShowPosition;
            ShowPosition = options.Application.ShowPosition;
            ShowScaleRef = options.Application.ShowScale;
            //HiRes flags
            menuItem_HiRes.Checked = mapcontrol.HiResMode;
            menuItem_HiRes_customdraw.Checked = mapcontrol.HiResModeCustomDraw;
            
            // Gestore del tracking online
            tracking = new OnlineTrackingHandler();
            // Contachilometri
            odo = new Odometer();
            ShowOdometer = options.Application.ShowOdometer;

            // mappe
            InitMaps();

            mapcontrol.Map = lmap;
            this.mapcontrol.PrePaint += new MapControl.MapControlEventHandler(this.prepaint_mappe);

            mapcontrol.Zoom = 12;
            mapcontrol.Center = map.mapsystem.CalcProjection(options.Application.InitialMapPosition);

            autocenter = options.Application.AutoCentreMap;
            this.gpsControl.PositionUpdated += new GPSControl.PositionUpdateHandler(GPSEventHandler);

            PlatformSpecificCode.Hibernate += new EventHandler(this.HibernateHandler);

            if (append_to_log != null)
            {
                action_loadtrack(append_to_log);
                action_StartGPS(append_to_log);
            }
            else if (options.GPS.Autostart)
                action_StartGPS(null);

        }

        private void init_gpx_db(object p)
        {
            // TODO: quando cambio directory per i log dovrei aggiornare il db come qui sotto
            GPXCollection gpxc = new GPXCollection(options.GPS.LogsDir + Path.DirectorySeparatorChar + "gpxdb.xml");
            gpx_saver.GPXFilesDB = gpxc;
            gpxc.ScanDir(options.GPS.LogsDir);
            // processa eventuali file di log che non sono ancora stati convertiti in GPX
            if (Directory.Exists(options.GPS.LogsDir))
                gpx_saver.ParseLogsDir(options.GPS.LogsDir);

        }

        private void HibernateHandler(Object sender, EventArgs e)
        {
            map.Hibernate();
        }

        private uint required_buffers(bool minimized_memory)
        {
            Size cs = this.ClientSize;
            // FIXME: il codice qui sotto dipende dalla dimensione del tile
            uint x = (uint)Math.Ceiling((double)cs.Width / 256.0 + 1);
            uint y = (uint)Math.Ceiling((double)cs.Height / 256.0 + 1);
            if (minimized_memory)
                return x * y;
            else
                return x * y + x + y - 1;
        }

        private void carica_opzioni()
        {
            string path = Program.GetPath();
            configfile = path + '\\' + System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + ".cfg";
            try
            {
                if (System.IO.File.Exists(configfile))
                {
                    options = ApplicationOptions.FromFile(configfile);
                    // qui posso controllare il numero di versione ed effettua eventuali aggiornamenti
                    //if (options.version < ??) ...
                    // ATTENZIONE! il numero di versione verrà aggiornato in verifica_opzioni_finale()
                }
                else
                    options = DefaultOptions();
            }
            catch (Exception)
            {
                MessageBox.Show("Config file error! Resetting options to default.");
                options = DefaultOptions();
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

            tracking.HandleGPSEvent(gpsdata, options.OnlineTracking.UpdateInterval);
            odo.HandleGPSEvent(gpsdata);
            label_odometer.Text = odo.ToString();
        }

        /// <summary>
        /// Recupera e corregge eventuali log con estensione .log rimasti in sospeso
        /// </summary>
        /// <remarks>Se c'è un file di log da ripristinare restituisce il nome del file di log da recuperare, null altrimenti.</remarks>
        private string recover_logs()
        {
            string logsdir = options.GPS.LogsDir,
                   append_to = null;
            try
            {
                DirectoryInfo di = new DirectoryInfo(options.GPS.LogsDir);
                FileInfo[] files = di.GetFiles("*.nmea");
                if (files.Length > 0)
                {
                    Array.Sort<FileInfo>(files, (FileInfo f1, FileInfo f2) => (f1.LastWriteTime < f2.LastWriteTime) ? -1 : (f1.LastWriteTime == f2.LastWriteTime ? 0 : 1));
                    //TODO: chiedere conferma se continuare il log del file
                    append_to = files[files.Length - 1].FullName;
                    // Cambia l'estensione degli altri file di log rimasti in sospeso
                    int last = files.Length - 2;
                    for (int i = 0; i <= last; i++)
                        File.Move(files[i].FullName, Path.ChangeExtension(files[i].FullName, ".txt"));
                }
            }
            catch (DirectoryNotFoundException) { }
            return append_to;
        }

        /// <summary>
        /// Avvia il GPS e la registrazione dei dati sul log
        /// </summary>
        /// <remarks>
        /// Il file di log creato ha inizialmente estensione ".nmea", una volta finita la registrazione l'estensione verrà  cambiata in ".txt". 
        /// Questo serve ad evitare che la funzione di conversione in gpx dei log rimasti in sospeso possa tentare di convertire un log appena aperto e con la registrazione ancora in corso.
        /// </remarks>
        private void action_StartGPS(string appendlog)
        {
            logname = null;
            if (options.GPS.Simulation && File.Exists(options.GPS.SimulationFile))
            {
                gpsControl.SimulationFile = options.GPS.SimulationFile;
#if DEBUG
                logname = (appendlog != null) ? appendlog : options.GPS.LogsDir + "\\gpslog_" + DateTime.Now.ToString(DateOnFilenameFormat) + ".nmea";
#endif
            }
            else
            {
                gpsControl.SimulationFile = null;
                logname = (appendlog != null) ? appendlog : options.GPS.LogsDir + "\\gpslog_" + DateTime.Now.ToString(DateOnFilenameFormat) + ".nmea";
            }
            gpsControl.start(options.GPS.PortName, options.GPS.PortSpeed, logname);
            this.menuItem_gpsactivity.Text = "Stop GPS";
        }

        private void action_StopGPS()
        {
            if (wpt_recorder.Running)
                wpt_recorder.stop();
            string orglogfilename = gpsControl.stop();
            if (orglogfilename != null)
            {
                string logfile = Path.ChangeExtension(orglogfilename, ".txt");
                File.Move(orglogfilename, logfile);
                // sarebbe più sensato impostare DelayTrackStart quando cambio l'opzione relativa
                gpx_saver.DelayTrackStart = options.Application.DelayGPXTrackStart;
                gpx_saver.SaveGPX(logfile);
            }
            this.menuItem_gpsactivity.Text = "Start GPS";
            // stops odometer
            odo.stop();
        }

        private void action_CentreMap()
        {
            if (gpsControl.Started)
                mapcontrol.Center = map.mapsystem.CalcProjection(this.gpsControl.PositionData.position);
        }

        private void action_CiclesVisibleMap()
        {
            ShowNextMap();
        }

        private void action_loadtrack(string file)
        {
            if (file == null)
            {
                string opendir = options.GPS.LogsDir;
                if (!Directory.Exists(opendir))
                    opendir = Program.GetPath();
                using (FormOpenFile openfiledlg = new FormOpenFile(opendir, false))
                {
                    if (openfiledlg.ShowDialog() == DialogResult.OK)
                        file = openfiledlg.openfile;
                    else
                        return;
                }
            }

            // count: for debug
            int count = 0;
            System.Windows.Forms.Cursor.Current = Cursors.WaitCursor;
            SetTrackVisibility(false);
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
                        switch (line.Substring(0, 6))
                        {
                            case "$GPRMC":
                                SharpGis.SharpGps.NMEA.GPRMC gpmrc = new SharpGis.SharpGps.NMEA.GPRMC(line);
                                gp = new GeoPoint(gpmrc.Position.Latitude, gpmrc.Position.Longitude);
                                this.trackpoints.addPoint(map.mapsystem.CalcProjection(gp));
                                count++;
                                break;
                            case "$GPWPL":
                                GPWPL gpwpl = new GPWPL(line);
                                gp = new GeoPoint(gpwpl.latitude, gpwpl.longitude);
                                this.waypoints.addPoint(map.mapsystem.CalcProjection(gp));
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
            SetTrackVisibility(true);
            System.Windows.Forms.Cursor.Current = Cursors.Default;
        }

        private void action_TracksManager()
        {
            using (FormTracksManager form = new FormTracksManager())
            {
                form.AppOptions = this.options;
                form.TracksCollection = gpx_saver.GPXFilesDB;
                if (form.ShowDialog() == DialogResult.OK)
                {
                    SetTrackVisibility(false);
                    if (MessageBox.Show("Clear current track data from the map? If you press yes the current track and way points will be removed from the map before loading the selected track.", "Load track", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                    {
                        this.trackpoints.clear();
                        this.waypoints.clear();
                    }
                    System.Windows.Forms.Cursor.Current = Cursors.WaitCursor;

                    string gpxfile = form.SelectedTrackFileName;

                    try
                    {
                        NMEA2GPX.GPXBaseType gpxdata = NMEA2GPX.GPXBaseType.Deserialize(gpxfile);
                        ProjectedGeoPoint pgp = this.mapcontrol.Center;
                        if (gpxdata.HasTrack)
                        {
                            NMEA2GPX.WaypointType[] trackpoints = gpxdata.trk.GetPoints();
                            foreach (NMEA2GPX.WaypointType wp in trackpoints)
                            {
                                pgp = map.mapsystem.CalcProjection(new GeoPoint(wp.lat, wp.lon));
                                this.trackpoints.addPoint(pgp);
                            }
                        }
                        // centra la mappa alla fine della traccia caricata
                        this.mapcontrol.Center = pgp;

                        if (gpxdata.wpt != null)
                            foreach (NMEA2GPX.WaypointType wp in gpxdata.wpt)
                            {
                                pgp = map.mapsystem.CalcProjection(new GeoPoint(wp.lat, wp.lon));
                                this.waypoints.addPoint(pgp);
                            }
                        System.Windows.Forms.Cursor.Current = Cursors.Default;
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Trace.WriteLine(e);
                        System.Windows.Forms.Cursor.Current = Cursors.Default;
                        MessageBox.Show("Track load error");
                    }
                    finally
                    {
                        SetTrackVisibility(true);

                    }
                }
            }
        }

        private void action_ToggleOnlineTracking()
        {
            try
            {
                if (!tracking.initialized)
                    if (PlatformSpecificCode.IsNetworkAvailable)
                        tracking.InitSession(options.OnlineTracking.TrackDescription, options.OnlineTracking.GMapsEmail, options.OnlineTracking.GMapsPassword);
                    else
                    {
                        SystemSounds.Asterisk.Play();
                        throw new Exception();
                    }
                tracking.active = !tracking.active;
                if (tracking.active)
                    menuItem_onlinetracking.Text = "Stop online tracking";
                else
                    menuItem_onlinetracking.Text = "Start online tracking";
            }
            catch (System.Net.WebException e)
            {
                System.Diagnostics.Debug.WriteLine("Problema di connessione: " + e);
            }
            catch (Exception)
            { }
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
            action_loadtrack(null);
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
            ShowPosition = !ShowPosition;
        }

        private void menuItem_showscale_Click(object sender, EventArgs e)
        {
            ShowScaleRef = !ShowScaleRef;
        }

        private void menuItem_photo_Click(object sender, EventArgs e)
        {
            action_takephoto();
        }

        private void menuItem_gpsactivity_Click(object sender, EventArgs e)
        {
            if (!this.gpsControl.Started)
                action_StartGPS(null);
            else
                action_StopGPS();
        }

        private void menuItem_downloadmaps_Click(object sender, EventArgs e)
        {
            ProjectedGeoArea area = mapcontrol.VisibleArea;
            int end = (mapcontrol.Zoom > 1) ? -1 : 0;
            for (int i = options.Maps.DownloadDepth; i >= end; i--)
            {
                uint z = (uint)(mapcontrol.Zoom + i);
                downloader.addDownloadArea(map, area, z);
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
                opt.data = (ApplicationOptions)this.options.Clone();
                opt.ShowDialog();
                ApplicationOptions newopt = opt.data;
                // Controlla se la è cambiata la lista delle mappe attive
                bool changemap = options.Maps.ActiveTileMaps.Count != newopt.Maps.ActiveTileMaps.Count ||
                          !options.Maps.ActiveTileMaps.TrueForAll(new Predicate<string>(
                             (name) => newopt.Maps.ActiveTileMaps.Contains(name))
                          );
                if (options.Application.WaypointSoundFile != newopt.Application.WaypointSoundFile)
                    wpt_sound.SoundLocation = newopt.Application.WaypointSoundFile;
                wpt_recorder.DeviceID = newopt.Application.RecordAudioDevice;
                wpt_recorder.RecordingFormat = (WaveIn4CF.WaveFormats)newopt.Application.RecordAudioFormat;
                //modalità full screen
                this.WindowState = newopt.Application.FullScreen ? FormWindowState.Maximized : FormWindowState.Normal;
#if PocketPC || Smartphone || WindowsCE
                this.hardwareButton_app3.HardwareKey = newopt.Application.CameraButton;
#endif
                options = newopt;
                if (changemap)
                    RefreshActiveMapsList();
                options.SaveToFile(this.configfile);
            }
        }

        private void menuItem_waypoint_Click(object sender, EventArgs e)
        {
            action_CreateWaypoint();
        }

        private void menuItem_exit_Click(object sender, EventArgs e)
        {
            if (!gpsControl.Started || MessageBox.Show("Close application. Are you sure?", "", MessageBoxButtons.YesNo, MessageBoxIcon.None, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                this.Close();
        }

       private void prepaint_mappe(MapControl sender)
        {
            if (options.Maps.AutoDownload)
            {
                //IDownloadableMap vm = lmap.isVisible(idx_layer_osm) ? (IDownloadableMap)map : (IDownloadableMap)gmap;
                downloader.addDownloadArea(map, mapcontrol.VisibleArea, mapcontrol.Zoom);
            }
        }

        private static ApplicationOptions DefaultOptions()
        {
            string programpath = Program.GetPath();
            ApplicationOptions opt = new ApplicationOptions();
            opt.Init();
            opt.GPS.PortName = "COM1";
            opt.GPS.PortSpeed = 9600;
            opt.GPS.Simulation = false;
            opt.GPS.SimulationFile = "";
            opt.GPS.LogsDir = programpath + "\\gpslogs";
            opt.Maps.TileCachePath = programpath + "\\tiles";
            opt.Maps.DownloadDepth = 3;
            opt.Maps.AutoDownload = false;
            opt.Application.DelayGPXTrackStart = false;
            opt.Application.WaypointSoundPlay = true;
            opt.Application.WaypointSoundFile = "\\Windows\\Infbeg.wav";
            opt.Application.WaypointRecordAudioSeconds = 10;
            opt.Application.WaypointRecordAudio = true;
            opt.Application.RecordAudioDevice = 0;
            opt.Application.RecordAudioFormat = WaveIn4CF.WaveFormats.Mono16bit11kHz;
            opt.Application.AutoCentreMap = true;
            opt.Application.InitialMapPosition = new GeoPoint(44.1429, 12.2618);
            opt.Application.FullScreen = false;
            opt.Application.ShowPosition = false;
            opt.Application.ShowScale = false;
            opt.Application.ShowOdometer = false;
#if PocketPC || Smartphone || WindowsCE
            opt.Application.CameraButton = HardwareKeys.ApplicationKey3;
#endif
            opt.OnlineTracking.GMapsEmail = "";
            opt.OnlineTracking.GMapsPassword = "";
            opt.OnlineTracking.UpdateInterval = 120;
            opt.OnlineTracking.TrackDescription = "Track";
            opt.version = ApplicationOptions.CurrentVersion;
            return opt;
        }

        private void waypoint_created(GPSControl.GPSPosition? gpsdata)
        {
            if (gpsdata != null)
            {
                if (options.Application.WaypointSoundPlay && wpt_sound != null)
                    wpt_sound.Play();
                waypoints.addPoint(map.mapsystem.CalcProjection(gpsdata.Value.position));
            }
            else
                SystemSounds.Exclamation.Play();
        }

        private void action_CreateWaypoint()
        {
            if (gpsControl.Started)
            {
                GPSControl.GPSPosition? gpsdata = gpsControl.saveWaypoint(WaypointNames.WPNameFormatString, 5);
                waypoint_created(gpsdata);
                //record audio
                if (gpsdata != null && options.Application.WaypointRecordAudio)
                {
                    string recdir = WaypointNames.DataDir(logname),
                           recfilename = WaypointNames.AudioRecFile(logname, gpsdata.Value.fixtime);
                    if (!Directory.Exists(recdir))
                        Directory.CreateDirectory(recdir);
                    wpt_recorder.start(recfilename, options.Application.WaypointRecordAudioSeconds);
                }
            }
        }

        private void action_takephoto()
        {
            if (gpsControl.Started)
            {
#if PocketPC || Smartphone || WindowsCE
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
                            GPSControl.GPSPosition gpsdata = gpsControl.saveWaypoint(WaypointNames.WPNameFormatString, 0).Value;
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
#endif
            }
        }

#if !(PocketPC || Smartphone || WindowsCE)
        /// <summary>
        /// Permette al metodo Form_MapperToolMain_KeyDown di elaborare i tasti freccia.
        /// </summary>
        protected override bool ProcessDialogKey(Keys key)
        {
            switch (key)
            {
                case (Keys.Left):
                case (Keys.Right):
                case (Keys.Up):
                case (Keys.Down):
                    return false;
            }
            return base.ProcessDialogKey(key);

        }
#endif

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
                    if (gpsControl.Started)
                        action_CreateWaypoint();
                    else
                        action_StartGPS(null);
                #if DEBUG
                else 
                    System.Diagnostics.Debug.WriteLine("Ignoring key - time from last keypress: " + intervalFromLast);
                #endif
            }
#if PocketPC || Smartphone || WindowsCE
            //FIXME: il tasto associato alla macchina fotografica dipende dal dispositivo
            if ((HardwareKeys)e.KeyCode == Microsoft.WindowsCE.Forms.HardwareKeys.ApplicationKey3)
            {
                // se il tasto è stato premuto a meno di 1 secondo dall'attivazione probabilmente è stato
                // la causa stessa dell'attivazione quindi non scatto la foto
                TimeSpan timefromactivation = DateTime.Now - activatedTime;
                System.Diagnostics.Debug.WriteLine("--- HardwareKey pressed - activation time: " + timefromactivation);
                if (timefromactivation.TotalSeconds > 1)
                    action_takephoto();
            }
#endif
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
#if PocketPC || Smartphone || WindowsCE
            mNotifyIcon.Dispose();
            ext_in.Dispose();
#endif
            map.Dispose();
            gpx_saver.Dispose();

            options.Application.InitialMapPosition = map.mapsystem.CalcInverseProjection(mapcontrol.Center);
            options.SaveToFile(this.configfile);
        }

        private void Form_MapperToolMain_Activated(object sender, EventArgs e)
        {
            activatedTime = DateTime.Now;
        }

        private void menuItem_TracksManager_Click(object sender, EventArgs e)
        {
            action_TracksManager();
        }

        private void mapcontrol_Resize(object sender, EventArgs e)
        {
            this.map.CacheLen = required_buffers(true);
        }

        private void menuItem_onlinetracking_Click(object sender, EventArgs e)
        {
            action_ToggleOnlineTracking();
        }

        private void menuItem_HiRes_Click(object sender, EventArgs e)
        {
            menuItem_HiRes.Checked = !menuItem_HiRes.Checked;
            mapcontrol.HiResMode = menuItem_HiRes.Checked;
        }

        private void menuItem_HiRes_customdraw_Click(object sender, EventArgs e)
        {
            menuItem_HiRes_customdraw.Checked = !menuItem_HiRes_customdraw.Checked;
            mapcontrol.HiResModeCustomDraw = menuItem_HiRes_customdraw.Checked;
        }

        private void menuItem_Odometer_Click(object sender, EventArgs e)
        {
            ShowOdometer = !ShowOdometer;
        }
    }

    public class OnlineTrackingHandler
    {
        DateTime lastOnlineTrackTime = new DateTime(0);
        GMapsDataAPI.GMAPSMap tracking_map;
        public bool active { get; set; }
        public bool initialized { get { return tracking_map != null; } }

        public void InitSession(string name, string email, string pw)
        {
            if (string.IsNullOrEmpty(name)) name = "Tracking";
            name += " " + DateTime.Now.ToString();
            ThreadPool.QueueUserWorkItem(new WaitCallback(
                (object s) => this.tracking_map = new GMAPSMap(name, name, GMAPSService.Login(email, pw))
            ));
        }

        public void StopSession()
        {
            tracking_map = null;
        }

        public void HandleGPSEvent(GPSControl.GPSPosition gpsdata, int updateinterval)
        {
            if (active && tracking_map != null &&
                (gpsdata.receivedtime - lastOnlineTrackTime).TotalSeconds >= updateinterval &&
                PlatformSpecificCode.IsNetworkAvailable)
            {
                // la seguente istruzione garantisce che prima di un certo tempo non venga ritentato un upload senza che il corrente abbia terminato
                lastOnlineTrackTime.AddSeconds(120);
                ThreadPool.QueueUserWorkItem(new WaitCallback(
                    (object s) =>
                    {

                        try
                        {
                            tracking_map.AddPoint(gpsdata.position.dLat, gpsdata.position.dLon, 0, gpsdata.receivedtime.ToLongTimeString());
                            lastOnlineTrackTime = gpsdata.receivedtime;
                        }
                        catch (System.Net.WebException e)
                        {
                            System.Diagnostics.Debug.WriteLine("Problema di connessione: " + e);
                        }
                    }
                ));
            }
        }        
    }

}