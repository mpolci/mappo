using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.IO;

namespace MapperTools.Pollicino
{
    public partial class GPXSaver : Component
    {
        public GPXSaver()
        {
            InitializeComponent();
        }

        public GPXSaver(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        public IWorkNotifier Notifier { get; set; }

        public bool DelayTrackStart { get; set; }

        /// <summary>
        /// Mandatory to set before to create jobs.
        /// </summary>
        public GPXCollection GPXFilesDB { get; set; }  

        private class ConvertWork
        {
            //private delegate void InvokeDelegate();
            IWorkNotifier notifier;
            string logfilename;
            bool delaytrackstart;
            GPXCollection gpxcollection;

            public ConvertWork(IWorkNotifier worknotifier, string log, bool trackfromfirstwpt, GPXCollection gpxcoll)
            {
                notifier = worknotifier;
                logfilename = log;
                delaytrackstart = trackfromfirstwpt;
                gpxcollection = gpxcoll;
            }

            public void GPXJob()
            {
                // se il file di log è vuoto viene cancellato
                FileInfo fiLog = new FileInfo(logfilename);
                if (fiLog.Length == 0)
                {
                    fiLog.Delete();
                    return;
                }

                //gpxcontrol_owner.BeginInvoke(new InvokeDelegate(gpxcontrol_owner.job_begin));
                if (notifier != null) notifier.WorkBegin();

                string outfile, outdir;
                // la directory di output si chiama come file di log tolta l'estensione ".txt"
                outdir = (logfilename.EndsWith(".txt")) ? logfilename.Substring(0, logfilename.Length - 4) : logfilename;
                // il file di output ha lo stesso nome del log ma estensione GPX
                outfile = outdir + ".gpx";
                try
                {
                    int wpts, tpts;
                    DateTime t_start, t_end;
                    string logdestdir;
                    if (MapperTools.NMEA2GPX.GPXGenerator.NMEAToGPX(logfilename, outfile, delaytrackstart,
                                                                out tpts, out wpts, out t_start, out t_end))
                    {
                        System.Diagnostics.Debug.WriteLine("-- " + logfilename + " tp: " + tpts + " wp: " + wpts + " start: " + t_start + " end: " + t_end);
                        gpxcollection.AddGPX(new GPXFile(outfile, t_start, t_end, tpts, wpts));
                        // archivia il file di log
                        if (!Directory.Exists(outdir))
                            Directory.CreateDirectory(outdir);
                        logdestdir = outdir;
                    }
                    else
                    {   
                        // Il file di log non contiene dati validi, viene quindi spostato nel cestino. 
                        logdestdir = fiLog.DirectoryName + Path.DirectorySeparatorChar + "Trash";
                        if (!Directory.Exists(logdestdir))
                            Directory.CreateDirectory(logdestdir);
                    }
                    File.Move(logfilename, logdestdir + Path.DirectorySeparatorChar + Path.GetFileName(logfilename));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine("\n---- GPXJob(" + logfilename + ")\n" + ex.ToString() + "\n----\n");
                    System.Windows.Forms.MessageBox.Show("Error converting log to GPX: " + Path.GetFileName(logfilename));
                }
                if (notifier != null) notifier.WorkEnd();
            }
        }
        
        public void ParseLogsDir(string dir)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(this.parsedirJob), dir);
        }

        private void parsedirJob(object objDirPath)
        {
            // il ? serve per filtrare i file che hanno l'estensione .txt e non che inizia per .txt - vedere documentazione
            string[] logs = Directory.GetFiles((string)objDirPath, "*?.txt");
            foreach (string file in logs)
            {
                SaveGPX(file);
                do
                {
                    Thread.Sleep(4000);
                } while (File.Exists(file));
            }
        }


        public void SaveGPX(string logfilename)
        {
            // Non uso ThreadPool.QueueUserWorkItem perché voglio un thread a bassa priorità
            ConvertWork work = new ConvertWork(Notifier, logfilename, DelayTrackStart, GPXFilesDB);
            Thread gpxthr = new Thread(new ThreadStart(work.GPXJob));
            gpxthr.Priority = ThreadPriority.Lowest;
            gpxthr.Name = "GPX Converter Thread";
            gpxthr.Start();
        }
    }
}
