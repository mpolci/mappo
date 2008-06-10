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
                System.Diagnostics.Trace.Assert(fi_in.Name.EndsWith(".txt"), "NMEAToGPX() - Invalid log file name: " + nmea_input);
                String basedir = fi_in.DirectoryName,
                       audiodir = fi_in.Name.Substring(0, fi_in.Name.Length - 4);
                // buffer
                String line;
                int count = 0;
                // Read and display lines from the file until the end of 
                // the file is reached.
                while ((line = sr.ReadLine()) != null)
                {
                    count++;
                    if (line.Length < 6)
                    {
                        System.Diagnostics.Debug.WriteLine("NMEAToGPX() - Invalid NMEA sentence (line " + count.ToString() + "): " + line);
                        continue;
                    }
                    switch (line.Substring(0, 6))
                    {
                        case "$GPRMC":
                            SharpGis.SharpGps.NMEA.GPRMC gpmrc = new SharpGis.SharpGps.NMEA.GPRMC(line);
                            waypoint tp = new waypoint();
                            tp.lat = gpmrc.Position.Latitude;
                            tp.lon = gpmrc.Position.Longitude;
                            tp.time = DateTime.SpecifyKind(gpmrc.TimeOfFix, DateTimeKind.Utc);
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

    }
}
