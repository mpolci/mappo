using System;
using System.Collections.Generic;
using System.Text;
using OpenNETCF.Media.WaveAudio;
using System.IO;
using System.Media;
using System.Threading;
using System.Runtime.InteropServices;



namespace MapperTool
{
    class AudioRecorder
    {
        [DllImport("coredll")]
        extern static void SystemIdleTimerReset();
        
        private DateTime dtime_StartRec;

        private bool _running = false;
        private object runninglock = new object();
        private OpenNETCF.Media.WaveAudio.Recorder recorder;
        private const int rectimeout = 3600;
        private int currentrecseconds;
        private string _recfilename;
        private FileStream recordingStream;
        private Timer timer1;

        private SoundPlayer endrec_sound;

        public enum NewRecordState {
            NewRecordStarted,
            RunningRercordProlungated,
            Error
        }

        public bool Running
        {
            get
            {
                return _running;
            }
        }
        public string FileName
        {
            get
            {
                return _recfilename;
            }
        }

        public AudioRecorder()
        {
            this.recorder = new OpenNETCF.Media.WaveAudio.Recorder();
            this.recorder.DoneRecording += new WaveFinishedHandler(rec_done);
            try
            {
                if (File.Exists("\\Windows\\RecEnd.wav")) {
                    endrec_sound = new SoundPlayer();
                    endrec_sound.SoundLocation = "\\Windows\\RecEnd.wav";
                }
            }
            catch (Exception)
            { }

        }

        public NewRecordState start(string filename, int seconds)
        {
            lock (runninglock)
            {
                if (_running)
                {
                    this.currentrecseconds += seconds;
                    return NewRecordState.RunningRercordProlungated;
                }
                else
                {
                    this.recordingStream = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite);

                    this._recfilename = filename;
                    this.currentrecseconds = seconds;

                    this.dtime_StartRec = DateTime.Now;
                    this._running = true;

                    //start a new recording
                    recorder.RecordFor(this.recordingStream, AudioRecorder.rectimeout, SoundFormats.Mono16bit22kHz);
                }
            }
            // Per sicurezza fa partire il timer fuori dal lock. Se lo mettevo dentro e per qualche strano motivo partiva il primo tick 
            // prima di essere usciti dal lock si creava una situazione di deadlock
            this.timer1 = new Timer(new TimerCallback(timerTick), null, 0, 1000);
            return NewRecordState.NewRecordStarted;
        }

        private void timerTick(Object state)
        {
            SystemIdleTimerReset();
            lock (runninglock)
            {
                if (!_running) throw new Exception("Internal error: timer tick without recording");

                TimeSpan elapsed = DateTime.Now - this.dtime_StartRec;
                if (elapsed.Seconds >= this.currentrecseconds)
                {
                    timer1.Dispose();
                    timer1 = null;
                    _running = false;
                    this.recorder.Stop();
                }
            }

        }

        private void rec_done()
        {
            if (_running) throw new Exception("Internal Error: probally record timeout");
            recordingStream.Close();
            if (endrec_sound != null) endrec_sound.Play();
        }


    }
}
