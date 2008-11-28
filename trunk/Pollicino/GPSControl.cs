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
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.IO;
using SharpGis.SharpGps;
using SharpGis.SharpGps.NMEA;
using MapsLibrary;
using System.Runtime.InteropServices;


namespace MapperTools.Pollicino
{
    public partial class GPSControl : UserControl
    {
        public struct GPSPosition
        {
            /// <summary>
            /// Posizione rilevata
            /// </summary>
            public GeoPoint position;
            /// <summary>
            /// Ora del fix indicata dal gps
            /// </summary>
            public DateTime fixtime;
            /// <summary>
            /// Ora di ricezione del messaggio NMEA con il fix. L'orologio di riferimento è quello interno del dispositivo.
            /// </summary>
            public DateTime receivedtime;
        }

        public GPSControl()
        {
            InitializeComponent();

#if PocketPC
            this.pb_GPSActvity.Image = Properties.Resources.ImgGPS;
#else 
            //TODO: risorsa??
#endif

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
        private string lastlogname;
        private bool started;

        public delegate void PositionUpdateHandler(GPSControl sender, GPSPosition newpos);
        public event PositionUpdateHandler PositionUpdated;


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

        /// <summary>
        /// Crea un nuovo waypoint nella posizione attuale e lo registra nel log
        /// </summary>
        /// <param name="name">
        /// Il nome del waypoint viene generato con l'istruzione <code>string.Format(name, fixtime)</code>
        /// quindi per inserire la data e l'ora del fix nel nome utilizzare il riferimento al parametro zero, ad esempio {0:o}.
        /// </param>
        /// <param name="maxage">L'età massima del fix, in secondi, oltre la quale il waypoint non viene salvato. 0 indica che il waypoint deve essere salvato con i dati dell'ultimo fix, indipendentemente dalla sua età.</param>
        public GPSPosition? saveWaypoint(string name, int maxage)
        {
            GPSPosition gpsdata = this.PositionData;
            TimeSpan age = DateTime.Now - gpsdata.receivedtime;

            if (maxage == 0 || age.TotalSeconds <= maxage)
            {
                if (swGPSLog != null)
                {
                    string wptname = string.Format(name, gpsdata.fixtime);
                    GPWPL nmeawpt = new GPWPL(wptname, gpsdata.position.dLat, gpsdata.position.dLon);
                    lock (swGPSLog)
                    {
                        swGPSLog.WriteLine(nmeawpt.NMEASentence);
                    }
                }
                return gpsdata;
            }
            else
                return null;
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
                lastlogname = logfile.FullName;
                swGPSLog = new StreamWriter(lastlogname);
                swGPSLog.AutoFlush = true;
            }
            gpshandler.Start(port, speed);
            this.started = true;
        }

        /// <summary>
        /// Disattiva il GPS
        /// </summary>
        /// <returns>nome del file di log salvato</returns>
        public string stop()
        {
            if (!started)
                throw new Exception("GPS not started");
            gpshandler.Stop();
            string logname = null;
            if (swGPSLog != null)
            {
                logname = lastlogname;
                swGPSLog.Close();
                swGPSLog.Dispose();
                swGPSLog = null;
            }
            this.started = false;
            return logname;
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
                        gpsdata.position = new GeoPoint(gpshandler.GPRMC.Position.Latitude, gpshandler.GPRMC.Position.Longitude);
                        gpsdata.fixtime = DateTime.SpecifyKind(gpshandler.GPRMC.TimeOfFix, DateTimeKind.Utc);
                        gpsdata.receivedtime = DateTime.Now;
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

            PlatformSpecificCode.SystemIdleTimerReset();

            if (swGPSLog != null)
                lock (swGPSLog) swGPSLog.WriteLine(e.Sentence);

            if (sendevent && PositionUpdated != null)
                PositionUpdated(this, gpsdata);

            this.pb_GPSActvity.Visible = !this.pb_GPSActvity.Visible;            
                
        }

        private void GPSEventAsync(GPSPosition gpsdata, GPSHandler.GPSEventArgs e, bool sendevent)
        {
            if (swGPSLog != null)
                lock (swGPSLog) swGPSLog.WriteLine(e.Sentence);

            if (sendevent && PositionUpdated != null) 
                PositionUpdated(this, gpsdata);
            
        }
        private static void EndEvent(IAsyncResult result)
        {
            ((GPSControl)result.AsyncState).PositionUpdated.EndInvoke(result);
        }
    }
}
