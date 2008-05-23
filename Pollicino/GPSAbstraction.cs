using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SharpGis.SharpGps;
using SharpGis.SharpGps.NMEA;
using MapsLibrary;
using System.Runtime.InteropServices;

namespace MapperTool
{
    public class GPSAbstraction
    {
        public struct GPSPosition
        {
            public GeoPoint position;
            public DateTime fixtime;
        }

        private GPSPosition _gpsdata;
        private GPSHandler gpshandler;
        private StreamWriter swGPSLog;
        private bool started;

        public delegate void PositionUpdateHandler(GPSAbstraction sender, GPSPosition newpos);
        public event PositionUpdateHandler PositionUpdated;

        public GPSAbstraction(System.Windows.Forms.Control parent)
        {
            gpshandler = new GPSHandler(parent); //Initialize GPS handler
            gpshandler.TimeOut = 5; //Set timeout to 5 seconds
            gpshandler.NewGPSFix += new GPSHandler.NewGPSFixHandler(this.GPSEventHandler); //Hook up GPS data events to Y handler
            started = false;
        }

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
                lock (this)
                    return _gpsdata;
            }
        }

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
                        lock (this)
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

            if (swGPSLog != null)
                lock (swGPSLog) swGPSLog.WriteLine(e.Sentence);

            if (sendevent && PositionUpdated != null) {
                // bisognerebbe lanciare l'evento in modo asincrono
                PositionUpdated(this, gpsdata);
            }
                
        }

    }
}
