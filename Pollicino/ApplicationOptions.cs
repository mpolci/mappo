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
//using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace MapperTools.Pollicino
{
    public class ApplicationOptions : ICloneable
    {
        public struct GPSOptions
        {
            public string PortName;
            public int PortSpeed;
            public bool Autostart;
            public bool Simulation;
            public string SimulationFile;
            public string LogsDir;
        }
        public struct MapsOptions
        {
            public struct OSMOptions {
                public string OSMTileServer;
                public string TileCachePath;
                public int DownloadDepth;
            }
            public struct GMAPSOptions {
                public string CachePath;
            }

            public OSMOptions OSM;
            public GMAPSOptions GMaps;
            public bool AutoDownload;
        }
        public struct InterfaceOptions
        {
            public bool DelayGPXTrackStart;
            public bool WaypointSoundPlay;
            public string WaypointSoundFile;
            public bool WaypointRecordAudio;
            public int WaypointRecordAudioSeconds;
            public uint RecordAudioDevice;
            public WaveIn4CF.WaveFormats RecordAudioFormat;
            public bool AutoCentreMap;
            public bool ShowPosition;
            public bool ShowScale;
            public MapsLibrary.GeoPoint InitialMapPosition;
            public bool FullScreen;
            public Microsoft.WindowsCE.Forms.HardwareKeys CameraButton;
            public string OSMUsername;
            public string OSMPassword;
        }

        /// <summary>
        /// Versione attuale delle opzioni dell'applicazione
        /// </summary>
        /// <remarks>
        /// Quando viene modificata questa classe è probabile che questo valore debba essere incrementato di uno.
        /// Attenzione al tipo di modifiche sulla classe. Eliminare o rinominare dei campi potrebbe creare dei problemi quando viene caricato un file di configurazione di una vecchia versione. Sarebbe meglio aggiungere nuovi campi e considerati deprecati i nomi vecchi e non più utilizzati.
        /// </remarks>
        public const uint CurrentVersion = 2;
        /// <summary>
        /// Versione dellle opzioni.
        /// </summary>
        /// <remarks>Permette di salvare nel file di configurazione il numero di versione delle opzioni. Grazie a questo è possibile effettuare eventuali aggiornamenti ai dati salvati quando si fa un aggiornamento del programma.</remarks>
        public uint version;
        public GPSOptions GPS;
        public MapsOptions Maps;
        public InterfaceOptions Application;

        public void SaveToFile(string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ApplicationOptions));
#if DEBUG
            using (TextWriter wdebug = new StreamWriter(filename + ".txt"))
            {
                serializer.Serialize(wdebug, this);
                wdebug.Close();
            }
#endif
            try
            {
                if (File.Exists(filename))
                {
                    string bckname = filename + "_backup";
                    if (File.Exists(bckname))
                        File.Delete(bckname);
                    File.Move(filename, bckname);
                }
                using (TextWriter writer = new StreamWriter(filename))
                {
                    serializer.Serialize(writer, this);
                    writer.Close();
                }
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("Error saving configuration. Skipping.");
                System.Diagnostics.Trace.WriteLine("--- Errore nel salvataggio della configurazione: " + e.ToString());
            }
        }

        public static ApplicationOptions FromFile(string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ApplicationOptions));
            ApplicationOptions opts;
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                opts = (ApplicationOptions)serializer.Deserialize(fs);
                fs.Close();
            }
            return opts;
        }

        public ApplicationOptions()
        {
            version = 0;
            GPS.PortName = "";
            GPS.SimulationFile = "";
            GPS.LogsDir = "";
            Maps.OSM.OSMTileServer = "";
            Maps.OSM.TileCachePath = "";
            Maps.GMaps.CachePath = "";
            Application.WaypointSoundFile = "";
        }

        #region ICloneable Members

        public object Clone()
        {
            ApplicationOptions cloned = new ApplicationOptions();
            System.Diagnostics.Debug.Assert(GPS.GetType().IsValueType && Maps.GetType().IsValueType && Application.GetType().IsValueType);
            cloned.version = version;
            cloned.GPS = this.GPS;
            cloned.Maps = this.Maps;
            cloned.Application = this.Application;
            return cloned;
        }

        #endregion
    }
}
