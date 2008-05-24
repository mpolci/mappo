using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using MapperTool;

namespace NMEA2GPX
{
    public static class GPXGenerator
    {
        public static void NMEAToGPX(string nmea_input, string gpx_output)
        {
            gpx gpxdata = new gpx();
            // The using statement also closes the StreamReader.
            using (System.IO.StreamReader sr = new System.IO.StreamReader(nmea_input))
            {
                String line;
                int count = 0;
                // Read and display lines from the file until the end of 
                // the file is reached.
                //DateTime lastTime = DateTime.Now; 
                //double lastLat, lastLon;
                while ((line = sr.ReadLine()) != null)
                {
                    switch (line.Substring(0, 6))
                    {
                        case "$GPRMC":
                            SharpGis.SharpGps.NMEA.GPRMC gpmrc = new SharpGis.SharpGps.NMEA.GPRMC(line);
                            trkpt tp = new trkpt();
                            tp.lat = gpmrc.Position.Latitude;
                            tp.lon = gpmrc.Position.Longitude;
                            tp.time = DateTime.SpecifyKind(gpmrc.TimeOfFix, DateTimeKind.Utc);
                            count++;
                            gpxdata.trk.trkseg.Add(tp);
                            break;
                        case "$GPWPL":
                            GPWPL gpwpl = new GPWPL(line);
                            waypoint w = new waypoint();
                            w.lat = gpwpl.latitude;
                            w.lon = gpwpl.longitude;
                            w.name = gpwpl.name;
                            if (w.name.StartsWith("wpt-"))
                            {
                                try
                                {
                                    w.time = DateTime.Parse(w.name.Substring(4)).ToUniversalTime();
                                }
                                catch (Exception) { }
                            }
                            gpxdata.wpt.Add(w);
                            break;
                    }
                }
            }
            StreamWriter outstream = new StreamWriter(gpx_output);
            XmlSerializer xmls = new XmlSerializer(typeof(gpx));
            xmls.Serialize(outstream, gpxdata);
        }

        [XmlRoot(Namespace = "http://www.topografix.com/GPX/1/1")]
        public class gpx
        {
            [XmlAttributeAttribute("schemaLocation", Namespace = System.Xml.Schema.XmlSchema.InstanceNamespace)]
            public string xsiSchemaLocation = "http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd";

            [XmlElement]
            public List<waypoint> wpt = new List<waypoint>();
            public track trk;

            public gpx()
            {
                trk.trkseg = new List<trkpt>();
            }
        }

        public struct waypoint
        {
            [XmlAttribute]
            public double lat;
            [XmlAttribute]
            public double lon;
            public string name;
            public DateTime time;
        }

        public struct track
        {
            public string name;
            public List<trkpt> trkseg;
        }

        public struct trkpt
        {
            [XmlAttribute]
            public double lat;
            [XmlAttribute]
            public double lon;

            public DateTime time;
        }


    }
}
