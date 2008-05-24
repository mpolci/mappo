using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.IO;
using SharpGis.SharpGps;
using SharpGis.SharpGps.NMEA;
using MapsLibrary;
using System.Runtime.InteropServices;


namespace MapperTool
{
    public partial class GPSControl : UserControl
    {
        public struct GPSPosition
        {
            public GeoPoint position;
            public DateTime fixtime;
        }

        public GPSControl()
        {
            InitializeComponent();

            //ProcessGPSEventAsync = new AsyncEventHandler(GPSEventAsync);
            //gpshandler = new GPSHandler(parent); //Initialize GPS handler
            gpshandler = new GPSHandler(this); //Initialize GPS handler
            gpshandler.TimeOut = 5; //Set timeout to 5 seconds
            gpshandler.NewGPSFix += new GPSHandler.NewGPSFixHandler(this.GPSEventHandler); 
            started = false;
        }
        private object gpsdatalock = new object();
        private GPSPosition _gpsdata;
        private GPSHandler gpshandler;
        private StreamWriter swGPSLog;
        private bool started;

        public delegate void PositionUpdateHandler(GPSControl sender, GPSPosition newpos);
        public event PositionUpdateHandler PositionUpdated;

        /*
        // utilizzate per effettuare in modo asincrono le elaborazioni dell'evento gps di minore urgenza
        private delegate void AsyncEventHandler(GPSPosition gpsdata, GPSHandler.GPSEventArgs e, bool sendevent);
        private AsyncEventHandler ProcessGPSEventAsync;
        private static AsyncCallback callback = new AsyncCallback(EndEvent);
        //private void GPSEventAsync(GPSPosition gpsdata, GPSHandler.GPSEventArgs e, bool sendevent);
        */

        #region Properties...
        /// <summary>
        /// True se il GPS è attivo
        /// </summary>
        public bool Started
        {
            get
            {
                return started;
            }
        }
        
        public string SimulationFile
        {
            set
            {
                if (string.IsNullOrEmpty(value))
                    gpshandler.Emulate = false;
                else
                    gpshandler.EnableEmulate(value);                    
            }
        }

        public GPSPosition PositionData
        {
            get
            {
                lock (gpsdatalock)
                    return _gpsdata;
            }
        }
        #endregion

        [DllImport("coredll")]
        extern static void SystemIdleTimerReset();

        /// <summary>
        /// Crea un nuovo waypoint nella posizione attuale e lo registra nel log
        /// </summary>
        public GPSPosition saveWaypoint()
        {
            GPSPosition gpsdata = this.PositionData;

            if (swGPSLog != null)
            {
                GPWPL nmeawpt = new GPWPL("WPT " + gpsdata.fixtime.ToString("yyyy-MM-dd_HHmmss"), gpsdata.position.dLat, gpsdata.position.dLon);
                lock (swGPSLog)
                {
                    swGPSLog.WriteLine(nmeawpt.NMEASentence);
                }
            }
            return gpsdata;
        }

        /// <summary>
        /// Attiva il GPS
        /// </summary>
        public void start(string port, int speed, string logfilename)
        {
            if (started)
                throw new Exception("GPS Already started");

            if (!string.IsNullOrEmpty(logfilename))
            {
                FileInfo logfile = new FileInfo(logfilename);
                if (!logfile.Directory.Exists)
                    logfile.Directory.Create();
                swGPSLog = new StreamWriter(logfile.FullName);
                swGPSLog.AutoFlush = true;
            }
            gpshandler.Start(port, speed);
            this.started = true;
        }

        /// <summary>
        /// Disattiva il GPS
        /// </summary>
        public void stop()
        {
            if (!started)
                throw new Exception("GPS not started");
            gpshandler.Stop();
            if (swGPSLog != null)
            {
                swGPSLog.Close();
                swGPSLog.Dispose();
                swGPSLog = null;
            }
            this.started = false;
        }


        protected void GPSEventHandler(object sender, GPSHandler.GPSEventArgs e)
        {
            bool sendevent = false;
            // per prima cosa aggiorna lo stato
            GPSPosition gpsdata = new GPSPosition();
            switch (e.TypeOfEvent)
            {
                case GPSEventType.GPRMC:  //Recommended minimum specific GPS/Transit data
                    if (gpshandler.HasGPSFix)
                    {
                        //gpsdata = new GPSPosition();
                        gpsdata.position = new GeoPoint(gpshandler.GPRMC.Position.Latitude, gpshandler.GPRMC.Position.Longitude);
                        gpsdata.fixtime = gpshandler.GPRMC.TimeOfFix;
                        lock (gpsdatalock)
                            _gpsdata = gpsdata;
                        sendevent = true;

                    }
                    else
                    {
                        // NOFIX
                    }
                    break;
            }

            SystemIdleTimerReset();

            //ProcessGPSEventAsync.BeginInvoke(gpsdata, e, sendevent, callback, this);
            if (swGPSLog != null)
                lock (swGPSLog) swGPSLog.WriteLine(e.Sentence);

            if (sendevent && PositionUpdated != null)
                PositionUpdated(this, gpsdata);
                //PositionUpdated.BeginInvoke(this, gpsdata, new AsyncCallback(EndEvent), this);
            
                
        }

        private void GPSEventAsync(GPSPosition gpsdata, GPSHandler.GPSEventArgs e, bool sendevent)
        {
            if (swGPSLog != null)
                lock (swGPSLog) swGPSLog.WriteLine(e.Sentence);

            if (sendevent && PositionUpdated != null) 
                PositionUpdated(this, gpsdata);
            
        }
        /*
        private static void EndEvent(IAsyncResult result)
        {
            //AsyncResult delegateResult = (AsyncResult) result;
            //AsyncEventHandler delegateInstance = (AsyncEventHandler) ((AsyncResult)result).AsyncDelegate;           
            AsyncEventHandler delegateInstance = ((GPSControl)result.AsyncState).ProcessGPSEventAsync;
            delegateInstance.EndInvoke(result);
        }
        */
        private static void EndEvent(IAsyncResult result)
        {
            ((GPSControl)result.AsyncState).PositionUpdated.EndInvoke(result);
        }
    }
}
