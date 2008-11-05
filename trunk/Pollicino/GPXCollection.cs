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
            Dictionary<string, int> gpxidx = new Dictionary<string, int>(gpxfiles.Count);
            for (int i = 0; i < gpxfiles.Count; i++)
                gpxidx.Add(gpxfiles[i].FileName, i);

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
                        GPXFile gpxf = gpxfiles[gpxidx[fi.FullName]];
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
                        GPXFile gpxf = gpxfiles[gpxidx[fi.FullName]];
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

        private void SaveCollection()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<GPXFile>));
            //using (TextWriter writer = new StringWriter())
            using (FileStream sw = new FileStream(datafilename, FileMode.Create))
            {
                //serializer.Serialize(writer, item);
                //sw.WriteLine(writer.ToString());
                serializer.Serialize(sw, gpxfiles);
            }
        }
    }

    //class invece che struct per problemi con la serializzazione xml
    public class GPXFile
    {
        public string FileName;
        [XmlElement(typeof(DateTime))]
        public object StartTime;
        [XmlElement(typeof(DateTime))]
        public object EndTime;
        public int WayPoints, TrackPoints;
        public bool UploadedToOSM;

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
            UploadedToOSM = false;
        }

        public GPXFile()
        {
            FileName = null;
            StartTime = null;
            EndTime = null;
            WayPoints = 0;
            TrackPoints = 0;            
        }
    }
}
