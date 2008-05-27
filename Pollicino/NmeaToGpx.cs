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
                // questa parte di generazione dei nomi sarebbe da inserire in una classe apposita
                FileInfo fi_in = new FileInfo(nmea_input);
                String basedir = fi_in.DirectoryName,
                       audiodir = fi_in.Name.Substring(0, fi_in.Name.Length - 4);
                // buffer
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
                            waypoint tp = new waypoint();
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
                            // time & audio
                            try
                            {
                                DateTime wptime = WaypointNames.DecodeWPName(w.name);
                                // check for audio record
                                string recname = WaypointNames.AudioRecFile(nmea_input, wptime);
                                if (File.Exists(recname))
                                    w.link = new Link(WaypointNames.AudioRecFileLink(nmea_input, wptime));
                                else // because of a josm bug, insert time only if an audio link doesn't exists
                                    w.time = wptime;

                            }
                            catch (Exception) { }

                            gpxdata.wpt.Add(w);
                            break;
                    }
                }
            }
            using (StreamWriter outstream = new StreamWriter(gpx_output))
            {
                XmlSerializer xmls = new XmlSerializer(typeof(gpx));
                xmls.Serialize(outstream, gpxdata);
                outstream.Close();
            }
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
                trk.trkseg = new List<waypoint>();
            }
        }

        public struct waypoint
        {
            [XmlAttribute]
            public double lat;
            [XmlAttribute]
            public double lon;
            public string name;
            // oggetto di tipo DateTime, definito come object per avere un tipo riferimento e quindi opzionale
            [XmlElement(typeof(DateTime))]
            public object time; 
            public Link link;

            [XmlElement(typeof(double))]
            public object ele;
            
        }

        public class Link
        {
            [XmlAttribute]
            public string href;

            public Link(string uri) {
                href = uri;
            }
        }


        public struct track
        {
            public string name;
            [XmlArrayItem(ElementName = "trkpt")]
            public List<waypoint> trkseg;
        }

        /*
        public struct trkpt
        {
            [XmlAttribute]
            public double lat;
            [XmlAttribute]
            public double lon;

            // oggetto di tipo DateTime, definito come object per avere un tipo riferimento
            [XmlElement(typeof(DateTime))]
            public object time;

            [XmlElement(typeof(double))]
            public object ele;
        }
        */

    }
}
