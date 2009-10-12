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
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Collections;
using MapsLibrary;

namespace MapperTools.Pollicino
{
    [Serializable]
    public class ApplicationOptions : ICloneable
    {
        [Serializable]
        public struct GPSOptions
        {
            public string PortName;
            public int PortSpeed;
            public bool Autostart;
            public bool Simulation;
            public string SimulationFile;
            public string LogsDir;
        }
        [Serializable]
        public class MapsOptions: ICloneable
        {
            public bool AutoDownload;
            public int DownloadDepth;
            public string TileCachePath;

            /// <summary>
            /// Elenco degli identificativi delle mappe da usare per la visualizzazione.
            /// </summary>
            [XmlArrayItem(ElementName="id")]
            public List<string> ActiveTileMaps;

            /// <summary>
            /// Tabella hash di ConfigurableMapSystems
            /// </summary>
            [XmlIgnore]
            public Hashtable TileMaps;

            #region ICloneable Members
            public object Clone()
            {
                MapsOptions o = new MapsOptions();
                o.AutoDownload = AutoDownload;
                o.DownloadDepth = DownloadDepth;
                o.TileCachePath = (string) TileCachePath.Clone();
                o.ActiveTileMaps = new List<string>(ActiveTileMaps);
                o.TileMaps = (Hashtable) TileMaps.Clone();
                return o;
            }
            #endregion
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
            public bool ShowOdometer;
            public MapsLibrary.GeoPoint InitialMapPosition;
            public bool FullScreen;
            public bool HiResDisplayMode;
            public bool CustomHiResDisplayMode;
#if PocketPC || Smartphone || WindowsCE
			public Microsoft.WindowsCE.Forms.HardwareKeys CameraButton;
#else
			public int CameraButton;
#endif
            // TODO: i parametri di OSM non è bene che stiano nella classe InterfaceOptions
			public string OSMUsername;
            public string OSMPassword;
        }

        public struct TrackingOptions
        {
            public string GMapsEmail;
            public string GMapsPassword;
            public int UpdateInterval;
            public string TrackDescription;
        }

        /// <summary>
        /// Versione attuale delle opzioni dell'applicazione
        /// </summary>
        /// <remarks>
        /// Quando viene modificata questa classe è probabile che questo valore debba essere incrementato di uno.
        /// Attenzione al tipo di modifiche sulla classe. Eliminare o rinominare dei campi potrebbe creare dei problemi quando viene caricato un file di configurazione di una vecchia versione. Sarebbe meglio aggiungere nuovi campi e considerati deprecati i nomi vecchi e non più utilizzati.
        /// </remarks>
        public const uint CurrentVersion = 3;
        /// <summary>
        /// Versione dellle opzioni.
        /// </summary>
        /// <remarks>Permette di salvare nel file di configurazione il numero di versione delle opzioni. Grazie a questo è possibile effettuare eventuali aggiornamenti ai dati salvati quando si fa un aggiornamento del programma.</remarks>
        public uint version;
        public GPSOptions GPS;
        public MapsOptions Maps = new MapsOptions();
        public InterfaceOptions Application;
        public TrackingOptions OnlineTracking;

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

            // carica le definizioni dei tileserver
            opts.Maps.TileMaps = new Hashtable();
            string path = Path.GetDirectoryName(filename);
            string tsconfdir = Path.Combine(path, "TileServersConf");
            XmlSerializer ser = new XmlSerializer(typeof(ConfigurableMapSystem));
            if (Directory.Exists(tsconfdir))
            {
                foreach (string cfn in Directory.GetFiles(tsconfdir, "*.tms"))
                    using (FileStream fs = new FileStream(cfn, FileMode.Open))
                    {
                        try
                        {
                            ConfigurableMapSystem tms = (ConfigurableMapSystem)ser.Deserialize(fs);
                            opts.Maps.TileMaps.Add(tms.identifier, tms);
                        }
                        catch (Exception) { }
                    }
            }

            return InitNullFields(opts);
        }

        protected static ApplicationOptions InitNullFields(ApplicationOptions opts)
        {
            Init(ref opts.GPS.PortName , "");
            Init(ref opts.GPS.SimulationFile, "");
            Init(ref opts.GPS.LogsDir, "");
            Init(ref opts.Maps.TileCachePath, "");
            if (opts.Maps.TileMaps == null) opts.Maps.TileMaps = new Hashtable();
            if (opts.Maps.ActiveTileMaps == null) opts.Maps.ActiveTileMaps = new List<string>(); 
            Init(ref opts.Application.WaypointSoundFile, "");
            Init(ref opts.OnlineTracking.GMapsEmail, "");
            Init(ref opts.OnlineTracking.GMapsPassword, "");
            Init(ref opts.OnlineTracking.TrackDescription, "");
            if (opts.OnlineTracking.UpdateInterval == 0) opts.OnlineTracking.UpdateInterval = 300;
            return opts;
        }

        private static void Init(ref string var, string val)
        {
            if (var == null)
                var = "";
        }

        #region ICloneable Members

        public object Clone()
        {
            ApplicationOptions cloned = new ApplicationOptions();
            System.Diagnostics.Debug.Assert(GPS.GetType().IsValueType && Maps.GetType().IsValueType && Application.GetType().IsValueType);
            cloned.version = version;
            cloned.GPS = this.GPS;
            cloned.Maps = (MapsOptions) this.Maps.Clone();
            cloned.Application = this.Application;
            cloned.OnlineTracking = this.OnlineTracking;
            return cloned;
        }

        #endregion
    }
}
