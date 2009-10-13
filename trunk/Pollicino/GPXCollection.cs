/*******************************************************************************
 *  Mappo! - A tool for gps mapping.
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
using System.IO;
using System.Xml.Serialization;

namespace MapperTools.Pollicino
{
    //TODO: Implementa solo un livello minimo di sincronizzazione con i lock
    public class GPXCollection
    {
        private string datafilename;
        private List<GPXFile> gpxfiles;
        public List<GPXFile> Items { get { return gpxfiles; } }

        public GPXCollection(string filename)
        {
            datafilename = filename;
            string dir = Path.GetDirectoryName(filename);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<GPXFile>));
                using (FileStream fs = new FileStream(datafilename, FileMode.Open))
                {
                    gpxfiles = (List<GPXFile>)serializer.Deserialize(fs);
                    fs.Close();
                }
                // verify if files exists
                gpxfiles.RemoveAll(GPXFile.NotExists);
            }
            catch (Exception e)
            {
                //File.Create(datafilename).Close();
                gpxfiles = new List<GPXFile>();
            }
        }

        public void ScanDir(string path)
        {
            GPXFile[] lst;
            Dictionary<string, GPXFile> gpxidx;
            lock (gpxfiles)
            {
                gpxidx = new Dictionary<string, GPXFile>(gpxfiles.Count);
                // uso una copia di gpxfiles per scorre la lista perché potrebbe essere modificata in corso
                lst = new GPXFile[gpxfiles.Count];
                gpxfiles.CopyTo(lst);
            }
            foreach (GPXFile gpxf in lst) {
                try {
                    gpxidx.Add(gpxf.FileName, gpxf);
                } catch (ArgumentException) {
                    // la lista potrebbe essere corrotta ed avere dei duplicati
                    System.Diagnostics.Trace.WriteLine("GPX DB contiene un duplicato: " + gpxf.FileName);
                    // rimuove elemento precedente, considerato più vecchio
                    gpxfiles.Remove(gpxidx[gpxf.FileName]);
                    gpxidx[gpxf.FileName] = gpxf;
                }
            }

            DirectoryInfo di = new DirectoryInfo(path);
            FileInfo[] files = di.GetFiles("*.gpx");
            foreach (FileInfo fi in files)
            {
                // HACK: controllare la scansione dei gpx. Aggiornare tutti i gpx è troppo dispendioso 
                // quindi carica solo quelli non presenti nell'indice
                if (gpxidx.ContainsKey(fi.FullName)) continue;
                try
                {
                    NMEA2GPX.GPXGenerator.GPXInfo info = NMEA2GPX.GPXGenerator.GetGPXInfo(fi.FullName);
                    if (!gpxidx.ContainsKey(fi.FullName))
                        lock (gpxfiles)
                            gpxfiles.Add(new GPXFile(info));
                    else
                    {
                        // questo codice non sarà mai eseguito
                        GPXFile gpxf = gpxidx[fi.FullName];
                        gpxf.UpdateInfo(info);
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Trace.WriteLine("Invalid GPX: " + fi.FullName);
                    DateTime nodate = new DateTime();
                    if (gpxidx.ContainsKey(fi.FullName))
                    {
                        GPXFile gpxf = gpxidx[fi.FullName];
                        gpxf.StartTime = nodate;
                        gpxf.EndTime = nodate;
                        gpxf.TrackPoints = -1;
                        gpxf.WayPoints = -1;
                    }
                    else
                    {
                        lock (gpxfiles)
                            gpxfiles.Add(new GPXFile(fi.FullName, nodate, nodate, -1, -1));
                    }
                }
            }
            SaveCollection();
        }

        public void ImportGPX(string filename)
        {
            NMEA2GPX.GPXGenerator.GPXInfo info = NMEA2GPX.GPXGenerator.GetGPXInfo(filename);
            GPXFile gpxf = gpxfiles.Find((GPXFile g) => g.FileName == filename);
            if (gpxf == null) 
                lock (gpxfiles) 
                    gpxfiles.Add(new GPXFile(info));
            else
            {
                gpxf.StartTime = info.begin_track_time;
                gpxf.EndTime = info.end_track_time;
                gpxf.TrackPoints = info.trackpoints;
                gpxf.WayPoints = info.waypoints;
            }
        }


        public void AddGPX(GPXFile item)
        {
            lock (gpxfiles)
                gpxfiles.Add(item);
            SaveCollection();
        }

        public void Remove(GPXFile item)
        {
            lock (gpxfiles)
                gpxfiles.Remove(item);
            SaveCollection();
        }

        public void SaveCollection()
        {
            //TODO: gestione eccezioni, da fare probabilmente nel metodo chiamante
            XmlSerializer serializer = new XmlSerializer(typeof(List<GPXFile>));
            using (FileStream sw = new FileStream(datafilename, FileMode.Create))
            {
                lock(gpxfiles)
                    serializer.Serialize(sw, gpxfiles);
            }
        }
    }

    // class invece che struct per problemi con la serializzazione xml e perché si gestisce meglio in un List<T>.
    [Serializable]
    public class GPXFile
    {
        public string FileName;
        public DateTime? StartTime;
        public DateTime? EndTime;
        public int WayPoints, TrackPoints;
        public bool? Flag;
        public string Description;
        public string TagsString;
        public string OSMVisibility;  
        public int? OSMId;
        public long Length;

        public string Name { 
            get {
                return Path.GetFileNameWithoutExtension(FileName);
            }
        }

        public TimeSpan Duration {
            get {
                if (StartTime != null && EndTime != null)
                    return (DateTime)EndTime - (DateTime)StartTime;
                else
                    return TimeSpan.Zero;
            }
        }

        public bool getUploaded()
        {
            return OSMId != null && (int)OSMId > 0;
        }

        public bool getFlag()
        {
            return Flag != null && (bool)Flag;
        }
        /*
        public long getLength()
        {
            return Length != null ? (long)Length : 0;
        }
        */
        internal static bool NotExists(GPXFile gpxf)
        {
            return !File.Exists(gpxf.FileName);
        }

        public GPXFile(NMEA2GPX.GPXGenerator.GPXInfo info)
        {
            _init(info.filename, info.begin_track_time, info.end_track_time, info.trackpoints, info.waypoints);
            Length = info.length;
        }

        //TODO: eliminare questo costruttore
        public GPXFile(string fullpath_filename, DateTime start, DateTime end, int tpts, int wpts)
        {
            _init(fullpath_filename, start, end, tpts, wpts);
        }

        private void _init(string fullpath_filename, DateTime start, DateTime end, int tpts, int wpts)
        {
            FileName = fullpath_filename;
            if (tpts > 0)
            {
                StartTime = start;
                EndTime = end;
            }
            else
            {
                StartTime = null;
                EndTime = null;
            }
            WayPoints = wpts;
            TrackPoints = tpts;

            Flag = null;
            Description = null;
            TagsString = null;
            OSMVisibility = null;
            OSMId = null;
            //Length = 0;
        }


        internal void UpdateInfo(MapperTools.NMEA2GPX.GPXGenerator.GPXInfo info)
        {
            System.Diagnostics.Trace.Assert(info.filename == FileName, "Cannot update info from different file");
            if (info.trackpoints > 0)
            {
                StartTime = info.begin_track_time;
                EndTime = info.end_track_time;
            }
            else
            {
                StartTime = null;
                EndTime = null;
            }
            WayPoints = info.waypoints;
            TrackPoints = info.trackpoints;
            Length = info.length;
        }

        public GPXFile()
        {
            FileName = null;
            StartTime = null;
            EndTime = null;
            WayPoints = 0;
            TrackPoints = 0;            

            Flag = null;
            Description = null;
            TagsString = null;
            OSMVisibility = null;
            OSMId = null;
            //Length = 0;
        }
    }
}
