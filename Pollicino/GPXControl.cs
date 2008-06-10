using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using NMEA2GPX;

namespace MapperTool
{
    public partial class GPXControl : UserControl
    {
        private class ConvertWork
        {
            private delegate void InvokeDelegate();
            GPXControl gpxcontrol_owner;
            string logfilename;

            public ConvertWork(GPXControl owner, string log)
            {
                gpxcontrol_owner = owner;
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

                gpxcontrol_owner.BeginInvoke(new InvokeDelegate(gpxcontrol_owner.job_begin));

                // la directory di output si chiama come file di log tolta l'estensione ".txt"
                outdir = (logfilename.EndsWith(".txt")) ? logfilename.Substring(0, logfilename.Length - 4) : logfilename;
                // il file di output ha lo stesso nome del log ma estensione GPX
                //outfile = outdir + '\\' + Path.GetFileName(outdir) + ".gpx";
                outfile = outdir + ".gpx";
                try
                {
                    NMEA2GPX.GPXGenerator.NMEAToGPX(logfilename, outfile);
                    // archivia il file di log
                    if (!Directory.Exists(outdir))
                        Directory.CreateDirectory(outdir);
                    File.Move(logfilename, outdir + '\\' + Path.GetFileName(logfilename));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine("\n---- GPXJob(" + logfilename + ")\n" + ex.ToString() + "\n----\n");
                    MessageBox.Show("Error converting log to GPX: " + Path.GetFileName(logfilename));
                }

                gpxcontrol_owner.BeginInvoke(new InvokeDelegate(gpxcontrol_owner.job_end));
            }

        }
        
        private object lockBlink;
        private int blink;

        public GPXControl()
        {
            InitializeComponent();
            lockBlink = new object();
            blink = 0;
            label_gpx.Visible = false;
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
            ConvertWork work = new ConvertWork(this, logfilename);
            Thread gpxthr = new Thread(new ThreadStart(work.GPXJob));
            gpxthr.Priority = ThreadPriority.Lowest;
            gpxthr.Name = "GPX Converter Thread";
            gpxthr.Start();
        }


        #region Gestione dell'etichetta lampeggiante...

        public void job_begin()
        {
            lock (lockBlink)
            {
                if (blink == 0)
                    this.timerBlinking.Enabled = true;
                blink++;
            }
        }

        public void job_end()
        {
            lock (lockBlink)
            {
                System.Diagnostics.Trace.Assert(blink > 0, "job_end(): blink is not greather than 0");
                blink--;
                if (blink == 0)
                {
                    this.timerBlinking.Enabled = false;
                    this.label_gpx.Visible = false;
                }
            }
        }

        private void timerBlinking_Tick(object sender, EventArgs e)
        {
            this.label_gpx.Visible = !this.label_gpx.Visible;
        }

        #endregion
    }
}
