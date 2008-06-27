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

        IWorkNotifier _notifier;
        public IWorkNotifier Notifier
        {
            get { return _notifier; }
            set { _notifier = value; }  // cambiare in esecuzione è un problema, CORREGGERE!
        }

        private class ConvertWork
        {
            //private delegate void InvokeDelegate();
            IWorkNotifier notifier;
            string logfilename;

            public ConvertWork(IWorkNotifier worknotifier, string log)
            {
                notifier = worknotifier;
                logfilename = log;
            }

            public void GPXJob()
            {
                string outfile, outdir;
                // se il file di log è vuoto viene cancellato
                FileInfo fiLog = new FileInfo(logfilename);
                if (fiLog.Length == 0)
                {
                    fiLog.Delete();
                    return;
                }

                //gpxcontrol_owner.BeginInvoke(new InvokeDelegate(gpxcontrol_owner.job_begin));
                if (notifier != null) notifier.WorkBegin();

                // la directory di output si chiama come file di log tolta l'estensione ".txt"
                outdir = (logfilename.EndsWith(".txt")) ? logfilename.Substring(0, logfilename.Length - 4) : logfilename;
                // il file di output ha lo stesso nome del log ma estensione GPX
                //outfile = outdir + '\\' + Path.GetFileName(outdir) + ".gpx";
                outfile = outdir + ".gpx";
                try
                {
                    MapperTools.NMEA2GPX.GPXGenerator.NMEAToGPX(logfilename, outfile);
                    // archivia il file di log
                    if (!Directory.Exists(outdir))
                        Directory.CreateDirectory(outdir);
                    File.Move(logfilename, outdir + '\\' + Path.GetFileName(logfilename));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine("\n---- GPXJob(" + logfilename + ")\n" + ex.ToString() + "\n----\n");
                    System.Windows.Forms.MessageBox.Show("Error converting log to GPX: " + Path.GetFileName(logfilename));
                }

                //gpxcontrol_owner.BeginInvoke(new InvokeDelegate(gpxcontrol_owner.job_end));
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
            //ThreadPool.QueueUserWorkItem(new WaitCallback(this.GPXJob), logfilename);
            ConvertWork work = new ConvertWork(_notifier, logfilename);
            Thread gpxthr = new Thread(new ThreadStart(work.GPXJob));
            gpxthr.Priority = ThreadPriority.Lowest;
            gpxthr.Name = "GPX Converter Thread";
            gpxthr.Start();
        }
    }
}
