/*******************************************************************************
 *  MyTracks - A tool for gps mapping.
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

        public bool DelayTrackStart { get; set; }

        private class ConvertWork
        {
            //private delegate void InvokeDelegate();
            IWorkNotifier notifier;
            string logfilename;
            bool delaytrackstart;

            public ConvertWork(IWorkNotifier worknotifier, string log, bool trackfromfirstwpt)
            {
                notifier = worknotifier;
                logfilename = log;
                delaytrackstart = trackfromfirstwpt;
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
                    MapperTools.NMEA2GPX.GPXGenerator.NMEAToGPX(logfilename, outfile, delaytrackstart);
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
            // Non uso ThreadPool.QueueUserWorkItem perché voglio un thread a bassa priorità
            ConvertWork work = new ConvertWork(_notifier, logfilename, DelayTrackStart);
            Thread gpxthr = new Thread(new ThreadStart(work.GPXJob));
            gpxthr.Priority = ThreadPriority.Lowest;
            gpxthr.Name = "GPX Converter Thread";
            gpxthr.Start();
        }
    }
}
