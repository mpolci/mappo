using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Serialization;

namespace MapperTools.Pollicino
{
    public class GPXCollection
    {
        private string datafilename;
        [XmlElement]
        private List<GPXFile> gpxfiles;
        [XmlIgnore]
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
            Dictionary<string, GPXFile> gpxidx = new Dictionary<string, GPXFile>(gpxfiles.Count);
            // uso una copia di gpxfiles per scorre la lista perché potrebbe essere modificata in corso
            GPXFile[] lst = new GPXFile[gpxfiles.Count];
            gpxfiles.CopyTo(lst);
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
                if (gpxidx.ContainsKey(fi.FullName)) continue;
                int wpts, tpts;
                DateTime t_start, t_end;
                try
                {
                    NMEA2GPX.GPXGenerator.GetGPXInfo(fi.FullName, out tpts, out wpts, out t_start, out t_end);
                    if (!gpxidx.ContainsKey(fi.FullName))
                        gpxfiles.Add(new GPXFile(fi.FullName, t_start, t_end, tpts, wpts));
                    else
                    {
                        GPXFile gpxf = gpxidx[fi.FullName];
                        gpxf.StartTime = t_start;
                        gpxf.EndTime = t_end;
                        gpxf.TrackPoints = tpts;
                        gpxf.WayPoints = wpts;
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
                        gpxfiles.Add(new GPXFile(fi.FullName, nodate, nodate, -1, -1));
                    }
                }

            }
            SaveCollection();
        }

        public void AddGPX(GPXFile item)
        {
            gpxfiles.Add(item);
            SaveCollection();
        }

        public void Remove(GPXFile item)
        {
            gpxfiles.Remove(item);
            SaveCollection();
        }

        public void SaveCollection()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<GPXFile>));
            using (FileStream sw = new FileStream(datafilename, FileMode.Create))
            {
                serializer.Serialize(sw, gpxfiles);
            }
        }
    }

    // class invece che struct per problemi con la serializzazione xml e perché si gestisce meglio in un List<T>.
    public class GPXFile
    {
        public string FileName;
        [XmlElement(typeof(DateTime))]
        public object StartTime;
        [XmlElement(typeof(DateTime))]
        public object EndTime;
        public int WayPoints, TrackPoints;
        [XmlElement(typeof(bool))]
        public object Flag;
        public string Description;
        public string TagsString;
        [XmlElement(typeof(bool))]
        public object OSMPublic;
        [XmlElement(typeof(int))]
        public object OSMId;
        // Questo campo è ridondante. La presenza di OSMId sarebbe sufficiente ad indicare se 
        // la traccia è stata caricata su OSM. Meglio non utilizzare!
        //[XmlElement(typeof(bool))]
        //public object UploadedToOSM;  

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

        public bool getPublic()
        {
            return OSMPublic != null && (bool)OSMPublic;
        }

        public bool getUploaded()
        {
            return OSMId != null && (int)OSMId > 0;
        }

        public bool getFlag()
        {
            return Flag != null && (bool)Flag;
        }

        internal static bool NotExists(GPXFile gpxf)
        {
            return !File.Exists(gpxf.FileName);
        }

        public GPXFile(string fullpath_filename, DateTime start, DateTime end, int tpts, int wpts)
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
            OSMPublic = null;
            OSMId = null;
            //UploadedToOSM = null;
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
            OSMPublic = null;
            OSMId = null;
            //UploadedToOSM = null;  
        }
    }
}
