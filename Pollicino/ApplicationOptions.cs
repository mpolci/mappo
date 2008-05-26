using System;
//using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace MapperTool
{
    public class ApplicationOptions
    {
        public struct GPSOptions
        {
            public string PortName;
            public int PortSpeed;
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
                public bool AutoDownload;
            }
            public struct GMAPSOptions {
                public string CachePath;
            }

            public OSMOptions OSM;
            public GMAPSOptions GMaps;
        }
        public struct InterfaceOptions
        {
            public bool WaypointSoundPlay;
            public string WaypointSoundFile;
            public bool WaypointRecordAudio;
            public int WaypointRecordAudioSeconds;
            public int RecordAudioDevice;
            public OpenNETCF.Media.WaveAudio.SoundFormats RecordAudioFormat;
        }

        public GPSOptions GPS;
        public MapsOptions Maps;
        public InterfaceOptions Application;

        public void SaveToFile(string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ApplicationOptions));
            using (TextWriter writer = new StreamWriter(filename)) {
                serializer.Serialize(writer, this);            
                writer.Close();
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
            GPS.PortName = "";
            GPS.SimulationFile = "";
            GPS.LogsDir = "";
            Maps.OSM.OSMTileServer = "";
            Maps.OSM.TileCachePath = "";
            Maps.GMaps.CachePath = "";
            Application.WaypointSoundFile = "";
        }
    }
}
