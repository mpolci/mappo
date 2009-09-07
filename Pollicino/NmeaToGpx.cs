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
using MapperTools.Pollicino;

namespace MapperTools.NMEA2GPX
{
    public static class GPXGenerator
    {
        /// <summary>
        /// Genera un file GPX partendo da un log NMEA
        /// </summary>
        /// <param name="nmea_input">file del log NMEA</param>
        /// <param name="gpx_output">file GPX da salvare</param>
        /// <param name="delaytrackstart">se true la traccia gpx inizia al primo waypoint</param>
        /// <returns>True se è stato generato il file gpx. Il file GPX non viene creato nel caso il log nmea non contenga trackpoint né waypoint.</returns>
        public static bool NMEAToGPX(string nmea_input, string gpx_output, bool delaytrackstart, out GPXInfo info)
                                     //out int tpts, out int wpts, out DateTime begintrk, out DateTime endtrk)
        {
            info = new GPXInfo();
            GPX11Type gpxdata = GPXGenerator.NMEAToGPX(nmea_input, delaytrackstart);
            if (gpxdata == null)
                return false;
            info.waypoints = gpxdata.wpt.Count;
            info.trackpoints = gpxdata.trk.GetTotalPoints();
            if (info.trackpoints > 0)
            {
                info.begin_track_time = (DateTime)gpxdata.trk.FirstPoint.time;
                info.end_track_time = (DateTime)gpxdata.trk.LastPoint.time;
            }
            //else
            //{
            //    begintrk = new DateTime();
            //    endtrk = new DateTime();
            //}
            //string audiodir = WaypointNames.DataDir(nmea_input);
            //if (wpts == 0 && tpts == 0 && !Directory.Exists(audiodir))
            //    return false;
            using (StreamWriter outstream = new StreamWriter(gpx_output))
            {
                // viene specificato il namespace di default a causa della diversa serializzazione su PocketPC
                XmlSerializer xmls = new XmlSerializer(typeof(GPX11Type), GPX11Type.GPX11Namespace);
                xmls.Serialize(outstream, gpxdata);
                outstream.Close();
            }
            return true;
        }

        public static GPX11Type NMEAToGPX(string nmea_input, bool delaytrackstart)
        {    
            GPX11Type gpxdata = new GPX11Type();
            gpxdata.init();
            int nNMEAMsgs = 0;
            // The using statement also closes the StreamReader.
            using (System.IO.StreamReader sr = new System.IO.StreamReader(nmea_input))
            {
                FileInfo fi_in = new FileInfo(nmea_input);
                System.Diagnostics.Trace.Assert(fi_in.Name.EndsWith(".txt"), "NMEAToGPX() - Invalid log file name: " + nmea_input);
                // buffer
                String line;
                bool elaboratetrack = !delaytrackstart;
                //List<WaypointType> tracksegpoints = gpxdata.trk.trkseg;
                System.Diagnostics.Debug.Assert(gpxdata.trk.trkseg.Length == 1, "Numero segmenti non valido nella traccia GPX");
                List<WaypointType> tracksegpoints = gpxdata.trk.trkseg[0].trkpt;
                // Read and display lines from the file until the end of 
                // the file is reached.
                while ((line = sr.ReadLine()) != null)
                {
                    nNMEAMsgs++;
                    if (line.Length < 6)
                    {
                        System.Diagnostics.Debug.WriteLine("NMEAToGPX() - Invalid NMEA sentence (line " + nNMEAMsgs.ToString() + "): " + line);
                        continue;
                    }
                    switch (line.Substring(0, 6))
                    {
                        case "$GPRMC":
                            if (elaboratetrack)
                            {
                                SharpGis.SharpGps.NMEA.GPRMC gpmrc = new SharpGis.SharpGps.NMEA.GPRMC(line);
                                if (gpmrc.Status == SharpGis.SharpGps.NMEA.GPRMC.StatusEnum.OK)
                                {
                                    WaypointType tp = new WaypointType();
                                    tp.lat = gpmrc.Position.Latitude;
                                    tp.lon = gpmrc.Position.Longitude;
                                    tp.time = DateTime.SpecifyKind(gpmrc.TimeOfFix, DateTimeKind.Utc);
                                    tp.timeSpecified = true;
                                    tracksegpoints.Add(tp);
                                }
                            }
                            break;
                        case "$GPWPL":
                            elaboratetrack = true;
                            GPWPL gpwpl = new GPWPL(line);
                            WaypointType w = new WaypointType();
                            w.lat = gpwpl.latitude;
                            w.lon = gpwpl.longitude;
                            w.name = gpwpl.name;
                            // time & audio
                            try
                            {
                                DateTime wptime = WaypointNames.DecodeWPName(w.name); // può lanciare un'eccezione
                                // check for audio record
                                string recname = WaypointNames.AudioRecFile(nmea_input, wptime);
                                if (File.Exists(recname))
                                    w.link = new LinkType(WaypointNames.AudioRecFileLink(nmea_input, wptime));
                                else
                                {
                                    // because of a josm bug, time is inserted only if an audio link doesn't exists
                                    w.time = wptime;
                                    w.timeSpecified = true;
                                    string imagename = WaypointNames.PictureFile(nmea_input, wptime);
                                    if (File.Exists(imagename))
                                        w.link = new LinkType(WaypointNames.PictureFileLink(nmea_input, wptime));
                                }
                            }
                            catch (Exception)
                            {
                                System.Diagnostics.Trace.WriteLine("Invalid waypoint name: " + w.name);
                            }

                            gpxdata.wpt.Add(w);
                            break;
                    }
                }
            }
            string audiodir = WaypointNames.DataDir(nmea_input);
            if (!Directory.Exists(audiodir) && gpxdata.wpt.Count == 0 && gpxdata.trk.GetTotalPoints() == 0)
                return null;
            else 
                return gpxdata;
        }

        public struct GPXInfo
        {
            public string filename;
            public int trackpoints;
            public int waypoints;
            public DateTime begin_track_time;
            public DateTime end_track_time;
            public long length;
        }


        //public static void GetGPXInfo(string gpxfile, out int tpts, out int wpts, out DateTime begintrk, out DateTime endtrk)
        public static GPXInfo GetGPXInfo(string gpxfile)
        {
            GPXInfo info = new GPXInfo();
            GPXBaseType gpxdata = GPXBaseType.Deserialize(gpxfile);

            info.filename = gpxfile;
            info.trackpoints = gpxdata.HasTrack ? gpxdata.trk.GetTotalPoints() : 0;
            info.waypoints = gpxdata.wpt != null ? gpxdata.wpt.Count : 0;
            if (info.trackpoints > 0)
            {
                info.begin_track_time = (DateTime)gpxdata.trk.FirstPoint.time;
                info.end_track_time = (DateTime)gpxdata.trk.LastPoint.time;
                info.length = CalcLength(gpxdata);
            }
            //else
            //{
            //    begintrk = new DateTime();
            //    endtrk = new DateTime();
            //}
            return info;
        }

        public static long CalcLength(GPXBaseType gpxdata)
        {
            // calcola un'approssimazione della lunghezza della traccia su massimo 200 punti
            double distance = 0;
            try
            {
                int len = gpxdata.trk.GetTotalPoints();
                int step = len / 200;
                if (step == 0) step = 1;
                WaypointType[] trackpoints = gpxdata.trk.GetPoints();
                MapsLibrary.GeoPoint last = new MapsLibrary.GeoPoint(trackpoints[0].lat, trackpoints[0].lon);
                MapsLibrary.GeoPoint current;
                for (int i = step; i < len; i += step)
                {
                    current = new MapsLibrary.GeoPoint(trackpoints[i].lat, trackpoints[i].lon);
                    distance += last.Distance(current);
                    last = current;
                }
                WaypointType lastpoint = trackpoints[len-1];
                distance += last.Distance(new MapsLibrary.GeoPoint(lastpoint.lat, lastpoint.lon));
                // approssima per cercare di compensare l'errore
                distance *= 1 + (double)(step - 1) / 5.1 / 100;  
            }
            catch (Exception) { }
            return (long) distance;
        }


    }

    [Serializable]
    [XmlRoot("gpx", Namespace = GPX10Namespace, IsNullable = false)]
    public class GPX10Type : GPXBaseType
    {
        [XmlIgnore]
        public const string GPX10Namespace = "http://www.topografix.com/GPX/1/0";

        [XmlAttribute("schemaLocation", Namespace = System.Xml.Schema.XmlSchema.InstanceNamespace)]
        public string xsiSchemaLocation = "http://www.topografix.com/GPX/1/0 http://www.topografix.com/GPX/1/0/gpx.xsd";
    }

    [Serializable]
    [XmlRoot("gpx", Namespace = GPX11Namespace, IsNullable = false)]
    public class GPX11Type: GPXBaseType
    {
        [XmlIgnore]
        public const string GPX11Namespace = "http://www.topografix.com/GPX/1/1";

        [XmlAttribute("schemaLocation", Namespace = System.Xml.Schema.XmlSchema.InstanceNamespace)]
        public string xsiSchemaLocation = "http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd";
    
    }

    [Serializable]
    public class GPXBaseType
    {
        [XmlElement]
        public List<WaypointType> wpt;
        public TrackType trk;

        public GPXBaseType()
        {
            wpt = null;
            trk = null;
        }

        public void init()
        {
            wpt = new List<WaypointType>();
            trk = new TrackType();
            //trk.trkseg = new List<WaypointType>();
            TrksegType seg = new TrksegType();
            seg.trkpt = new List<WaypointType>();
            trk.trkseg = new TrksegType[] { seg };
        }

        public bool HasTrack {
            get {
                return this.trk != null && this.trk.trkseg != null;
            }
        }

        public static GPXBaseType Deserialize(string gpxfile)
        {
            using (FileStream gpxstream = new FileStream(gpxfile, FileMode.Open))
            {
                GPXBaseType gpxdata = null;
                try
                {
                    // viene specificato il namespace di default a causa della diversa serializzazione su PocketPC
                    XmlSerializer xmls = new XmlSerializer(typeof(GPX11Type), GPX11Type.GPX11Namespace);
                    gpxdata = (GPXBaseType)xmls.Deserialize(gpxstream);
                }
                catch (InvalidOperationException)
                {
                    // In ambiente windows viene lanciata un'eccezione se il namespace non corrisponde
                    // mentre su PocketPC semplicemente i campi rimangono null;
                }
                if (gpxdata == null || gpxdata.trk == null) {
                    // Tenta di ricaricare come GPX 1.0
                    gpxstream.Position = 0;
                    // viene specificato il namespace di default a causa della diversa serializzazione su PocketPC
                    XmlSerializer xmls2 = new XmlSerializer(typeof(GPX10Type), GPX10Type.GPX10Namespace);
                    gpxdata = (GPXBaseType)xmls2.Deserialize(gpxstream);
                }
                if (gpxdata == null || gpxdata.trk == null)
                    throw new InvalidOperationException("File gpx non valido");
                return gpxdata;
            }
        }
    }

    [Serializable]
    public class WaypointType
    {
        [XmlAttribute]
        public double lat;
        [XmlAttribute]
        public double lon;
        public string name;
        public LinkType link;

        private System.DateTime timeField;
        private bool timeFieldSpecified;

        private decimal eleField;
        private bool eleFieldSpecified;

        public decimal ele
        {
            get
            {
                return this.eleField;
            }
            set
            {
                this.eleField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool eleSpecified
        {
            get
            {
                return this.eleFieldSpecified;
            }
            set
            {
                this.eleFieldSpecified = value;
            }
        }

        public DateTime time
        {
            get
            {
                return this.timeField;
            }
            set
            {
                this.timeField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool timeSpecified
        {
            get
            {
                return this.timeFieldSpecified;
            }
            set
            {
                this.timeFieldSpecified = value;
            }
        }
    }

    [Serializable]
    public class LinkType
    {
        [XmlAttribute]
        public string href;

        public LinkType(string uri)
        {
            href = uri;
        }
        public LinkType()
        {
            href = string.Empty;
        }
    }

    [Serializable]
    public class TrackType
    {
        public string name;
        //[XmlArrayItem(ElementName = "trkpt",Type=typeof(WaypointType))]
        //[XmlArray]
        //public List<WaypointType> trkseg;

        [XmlElement]
        public TrksegType[] trkseg;

        public int GetTotalPoints()
        {
            int total = 0;
            try
            {
                foreach (TrksegType seg in trkseg)
                    if (seg.trkpt != null) total += seg.trkpt.Count;
            }
            catch (NullReferenceException) { }
            return total;
        }

        public WaypointType[] GetPoints()
        {
            WaypointType[] aggregated = new WaypointType[GetTotalPoints()];
            int i = 0;
            foreach (TrksegType seg in trkseg)
            {
                seg.trkpt.CopyTo(aggregated, i);
                i += seg.trkpt.Count;
            }
            return aggregated;
        }

        [XmlIgnore]
        public WaypointType FirstPoint {
            get {
                return trkseg[0].trkpt[0]; // può lanciare un'eccezione
            }
        }

        [XmlIgnore]
        public WaypointType LastPoint {
            get 
            {
                // può lanciare un'eccezione
                TrksegType seg = trkseg[trkseg.Length - 1];
                return seg.trkpt[seg.trkpt.Count - 1];
            }
        }
    }

    [Serializable]
    public class TrksegType
    {
        [XmlElement]
        public List<WaypointType> trkpt;
    }

}
