using System;
//using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace MapperTool
{
    public class MapperToolOptions
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

        public GPSOptions GPS;
        public MapsOptions Maps;

        public void SaveToFile(string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(MapperToolOptions));
            using (TextWriter writer = new StreamWriter(filename)) {
                serializer.Serialize(writer, this);            
                writer.Close();
            }
        }

        public static MapperToolOptions FromFile(string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(MapperToolOptions));
            MapperToolOptions opts;
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                opts = (MapperToolOptions)serializer.Deserialize(fs);
                fs.Close();
            }
            return opts;
        }

        public MapperToolOptions()
        {
            GPS.PortName = "";
            GPS.SimulationFile = "";
            GPS.LogsDir = "";
            Maps.OSM.OSMTileServer = "";
            Maps.OSM.TileCachePath = "";
            Maps.GMaps.CachePath = "";
        }
    }
}
