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
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace WaveIn4CF
{
    using HANDLE = IntPtr;
    using DWORD_PTR = Int32;
    using UINT_PTR = IntPtr;
    using UINTMSG = UInt32;
    using DWORD = UInt32;
    using WORD = UInt16;
    using LPSTR = IntPtr;
    using UINT = UInt32;

    using MMRESULT = Int32;

    [StructLayout(LayoutKind.Sequential)]
    public struct WAVEINCAPS
    {
        public WORD wMid;              /* manufacturer ID */
        public WORD wPid;              /* product ID */
        public WORD vDriverVersion;    /* version of the driver */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public char[] szPname;                /* product name (NULL terminated string) */
        //char [32] szPname;
        public DWORD dwFormats;               /* formats supported */
        public WORD wChannels;                /* number of channels supported */
        public WORD wReserved1;               /* structure packing */


        public const int WAVE_INVALIDFORMAT = 0x00000000;    // invalid format
        public const int WAVE_FORMAT_1M08 = 0x00000001;    // 11.025 kHz, Mono,   8-bit 
        public const int WAVE_FORMAT_1S08 = 0x00000002;    // 11.025 kHz, Stereo, 8-bit 
        public const int WAVE_FORMAT_1M16 = 0x00000004;    // 11.025 kHz, Mono,   16-bit
        public const int WAVE_FORMAT_1S16 = 0x00000008;    // 11.025 kHz, Stereo, 16-bit
        public const int WAVE_FORMAT_2M08 = 0x00000010;    // 22.05  kHz, Mono,   8-bit 
        public const int WAVE_FORMAT_2S08 = 0x00000020;    // 22.05  kHz, Stereo, 8-bit 
        public const int WAVE_FORMAT_2M16 = 0x00000040;    // 22.05  kHz, Mono,   16-bit
        public const int WAVE_FORMAT_2S16 = 0x00000080;    // 22.05  kHz, Stereo, 16-bit
        public const int WAVE_FORMAT_4M08 = 0x00000100;    // 44.1   kHz, Mono,   8-bit 
        public const int WAVE_FORMAT_4S08 = 0x00000200;    // 44.1   kHz, Stereo, 8-bit 
        public const int WAVE_FORMAT_4M16 = 0x00000400;    // 44.1   kHz, Mono,   16-bit
        public const int WAVE_FORMAT_4S16 = 0x00000800;    // 44.1   kHz, Stereo, 16-bit
    }

    [Flags]
    public enum WaveFormats
    {
        // Summary:
        //     Format is not valid
        InvalidFormat = 0,
        //
        // Summary:
        //     Mono, 8 bit, 11025 Hz
        Mono8bit11kHz = 1,
        //
        // Summary:
        //     Stereo, 8 bit, 11025 Hz
        Stereo8bit11kHz = 2,
        //
        // Summary:
        //     Mono, 16 bit, 11025 Hz
        Mono16bit11kHz = 4,
        //
        // Summary:
        //     Stereo, 16 bit, 11025 Hz
        Stereo16bit11kHz = 8,
        //
        // Summary:
        //     Mono, 8 bit, 22050 Hz
        Mono8bit22kHz = 16,
        //
        // Summary:
        //     Stereo, 8 bit, 22050 Hz
        Stereo8bit22kHz = 32,
        //
        // Summary:
        //     Mono, 16 bit, 22050 Hz
        Mono16bit22kHz = 64,
        //
        // Summary:
        //     Stereo, 16 bit, 22050 Hz
        Stereo16bit22kHz = 128,
        //
        // Summary:
        //     Mono, 8 bit, 44100 Hz
        Mono8bit44kHz = 256,
        //
        // Summary:
        //     Stereo, 8 bit, 44100 Hz
        Stereo8bit44kHz = 512,
        //
        // Summary:
        //     Mono, 16 bit, 44100 Hz
        Mono16bit44kHz = 1024,
        //
        // Summary:
        //     Stereo, 16 bit, 44100 Hz
        Stereo16bit44kHz = 2048,
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct WAVEFORMATEX
    {
        public WORD wFormatTag;
        public WORD nChannels;
        public DWORD nSamplesPerSec;
        public DWORD nAvgBytesPerSec;
        public WORD nBlockAlign;
        public WORD wBitsPerSample;
        public WORD cbSize;

        public enum FormatTags : ushort
        {
            Pcm = 1,
            Float = 3,
            Gsm610 = 31
        }

        public WAVEFORMATEX(WORD formatTag, WORD channels, DWORD samplesPerSec, DWORD avgBytesPerSec,
                                   WORD blockAlign, WORD bitsPerSample, WORD size)
        {
            wFormatTag = formatTag;
            nChannels = channels;
            nSamplesPerSec = samplesPerSec;
            nAvgBytesPerSec = avgBytesPerSec;
            nBlockAlign = blockAlign;
            wBitsPerSample = bitsPerSample;
            cbSize = size;
        }

        public static WAVEFORMATEX FMT_RIFF8k16b1c()  {
            const WORD blockAlign = 2;
            return new WAVEFORMATEX((WORD)FormatTags.Pcm, 1, 8000, 80000 * blockAlign, blockAlign, 16, 0);
        }

        public static WAVEFORMATEX FMT_RIFF11k16b1c()
        {
            const WORD blockAlign = 2;
            return new WAVEFORMATEX((WORD)FormatTags.Pcm, 1, 11025, 11025 * blockAlign, blockAlign, 16, 0);
        }

        public static WAVEFORMATEX FMT_RIFF44k16b2c()
        {
            const WORD blockAlign = 4;
            return new WAVEFORMATEX((WORD)FormatTags.Pcm, 2, 44100, 44100 * blockAlign, blockAlign, 16, 0);
        }

        public static WAVEFORMATEX FMT_RIFF(WaveFormats wfmt)
        {
            ushort res = 0,
                   ch = 0;
            uint   freq = 0,
                   fmt = (uint)wfmt;

            if ((fmt & 0x000F) != 0)
                freq = 11025;
            else if ((fmt & 0x00F0) != 0) {
                freq = 22050;
                fmt >>= 4;
            } else if ((fmt & 0x0F00) != 0) {
                freq = 44100;
                fmt >>= 8;
            } else 
                throw new Exception("Invalid Format");
            /*
            freq = ((int)fmt & 0x000F) != 0 ? 11025 :
                   ((int)fmt & 0x00F0) != 0 ? 22050 : 
                   ((int)fmt & 0x0F00) != 0 ? 44100 : 0;
             */
            if (fmt == 0)
                throw new Exception("Invalid Format");
            else if ((int)fmt < 3)
                res = 8;
            else {
                res = 16;
                fmt >>= 2;
            }
            ch = (ushort) fmt;
            if (ch != 1 && ch != 2)
                throw new Exception("Invalid Format");

            WORD blockAlign = (ushort) ((res / 8) * ch);
            return new WAVEFORMATEX((WORD)FormatTags.Pcm, ch, freq, freq * blockAlign, blockAlign, res, 0);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WAVEHDR
    {
        public LPSTR lpData;
        public DWORD dwBufferLength;
        public DWORD dwBytesRecorded;
        public DWORD_PTR dwUser;
        public DWORD dwFlags;
        public DWORD dwLoops;
        public DWORD_PTR lpNext;
        public DWORD_PTR reserved;

        public enum Flags: uint {
            /* flags for dwFlags field of WAVEHDR */
            WHDR_DONE      = 0x00000001,  /* done bit */
            WHDR_PREPARED  = 0x00000002,  /* set if this header has been prepared */
            WHDR_BEGINLOOP = 0x00000004,  /* loop start block */
            WHDR_ENDLOOP   = 0x00000008,  /* loop end block */
            WHDR_INQUEUE   = 0x00000010   /* reserved for driver */
        }
    }

    public class Native
    {
        public const UINT WAVE_MAPPER = 0xFFFFFFFF; 
        public const DWORD CALLBACK_FUNCTION = 0x00030000;
        public const MMRESULT MMSYSERR_NOERROR = 0;
        public const uint MM_WIM_OPEN = 0x3BE;
        public const uint MM_WIM_CLOSE = 0x3BF;
        public const uint MM_WIM_DATA = 0x3C0;

//        public delegate void waveInProc_delegate(HANDLE hwi, UINTMSG uMsg, DWORD_PTR dwInstance, DWORD_PTR dwParam1, DWORD_PTR dwParam2); // parametri ok
        public delegate void waveInProc_delegate(HANDLE hwi, UINTMSG uMsg, DWORD_PTR dwInstance, ref WAVEHDR dwParam1, DWORD_PTR dwParam2); // parametri ok

        private const string sourcedll = "coredll.dll";    // WinCE
        //private const string sourcedll = "winmm.dll";      // Win32

        [DllImport(sourcedll)]
        public static extern UINT waveInGetNumDevs();
        [DllImport(sourcedll)]
        public static extern MMRESULT waveInGetDevCaps(UINT uDeviceID, out WAVEINCAPS pwic, UINT cbwic); 
        [DllImport(sourcedll)]
        public static extern MMRESULT waveInAddBuffer(HANDLE hwi, ref WAVEHDR pwh, UINT cbwh);
        [DllImport(sourcedll)]
        public static extern MMRESULT waveInClose(HANDLE hwi);
        [DllImport(sourcedll)]
        public static extern MMRESULT waveInOpen(out HANDLE phwi, UINT uDeviceID, ref WAVEFORMATEX pwfx, DWORD_PTR dwCallback, DWORD_PTR dwInstance, DWORD fdwOpen);  //parametri ok
        [DllImport(sourcedll)]
        public static extern MMRESULT waveInPrepareHeader(HANDLE hwi, ref WAVEHDR pwh, UINT cbwh);
        [DllImport(sourcedll)]
        public static extern MMRESULT waveInUnprepareHeader(HANDLE hWaveIn, ref WAVEHDR pwh, UINT cbwh);
        [DllImport(sourcedll)]
        public static extern MMRESULT waveInReset(HANDLE hwi);
        [DllImport(sourcedll)]
        public static extern MMRESULT waveInStart(HANDLE hwi);
        [DllImport(sourcedll)]
        public static extern MMRESULT waveInStop(HANDLE hwi);

    }

    internal class WaveInBuffer: IDisposable
    {
        bool disposed = false;
        public byte[] mData;
        public WAVEHDR mWHdr;
        GCHandle mH_Data;
        GCHandle mH_WHdr;
        HANDLE mhwi;

        public WaveInBuffer(HANDLE hwi, uint bufsize, DWORD_PTR dwUser)
        {
            mhwi = hwi;
            mH_WHdr = GCHandle.Alloc(mWHdr, GCHandleType.Pinned);
            mData = new byte[bufsize];
            mH_Data = GCHandle.Alloc(mData, GCHandleType.Pinned);
            mWHdr.lpData = mH_Data.AddrOfPinnedObject();
            mWHdr.dwBufferLength = bufsize;
            mWHdr.dwUser = dwUser;
            MMRESULT res = Native.waveInPrepareHeader(hwi, ref mWHdr, (UINT)Marshal.SizeOf(mWHdr));
            if (res != Native.MMSYSERR_NOERROR) throw new Exception("Impossibile preparare il buffer");
            RecycleBuffer();
        }

        public void RecycleBuffer()
        {
            MMRESULT res = Native.waveInAddBuffer(mhwi, ref mWHdr, (UINT)Marshal.SizeOf(mWHdr));
            if (res != Native.MMSYSERR_NOERROR) throw new Exception("Immpossibile associare il buffer");
        }
        #region IDisposable Members

        public void Dispose()
        {
            if (disposed) 
                throw new Exception("already disposed exception");
            Native.waveInUnprepareHeader(mhwi, ref mWHdr, (UINT) Marshal.SizeOf(mWHdr));
            mH_WHdr.Free();
            mH_Data.Free();
            disposed = true;
        }

        #endregion
    }


    public class WaveInRecorder: IDisposable
    {
        public delegate void RecordFinishedDelegate(WaveInRecorder recorder);
        public event RecordFinishedDelegate RecFinishedEvent;
        const int BUFFERSCOUNT = 5;
        const uint BUFFERSIZE = 16384;

        WaveInBuffer[] buffers;
        protected HANDLE hwi;
        WAVEFORMATEX mWFmt;

        private Native.waveInProc_delegate mWaveInProc = new Native.waveInProc_delegate(waveInProc);
        protected BinaryWriter mOutWriter;
        private uint mDataCounter;

        protected GCHandle mH_this;

        protected bool mRecFinished;
        protected int mCurrentBuffer;
        private System.Threading.ManualResetEvent mAudioEvent;

        public WaveInRecorder(uint devid, WaveFormats wfmt)
        {
            mH_this = GCHandle.Alloc(this);

            mWFmt = WAVEFORMATEX.FMT_RIFF(wfmt);
            
            MMRESULT mmresult = Native.waveInOpen(out hwi, devid, ref mWFmt, Marshal.GetFunctionPointerForDelegate(mWaveInProc).ToInt32(),
                                                  ((IntPtr)mH_this).ToInt32(), Native.CALLBACK_FUNCTION);
            if (mmresult != Native.MMSYSERR_NOERROR)
            {
                throw new Exception("Errore nell'apertura del dispositivo: " + mmresult.ToString());
            }
            // buffer per la registrazione
            buffers = new WaveInBuffer[BUFFERSCOUNT];
            for (int i = 0; i < BUFFERSCOUNT; i++) 
                buffers[i] = new WaveInBuffer(hwi, BUFFERSIZE, i);

            mAudioEvent = new System.Threading.ManualResetEvent(false);
            mRecFinished = true;

        }

        public void StartRec(Stream output)
        {
            if (!mRecFinished)
                StopRec();
            System.Diagnostics.Trace.Assert(mDataCounter == 0, "mDataCounter non zero");
            // file di output
            if (output.Length != 0 || !output.CanWrite)
                throw new Exception("Empty writeable stream required");
            mOutWriter = new BinaryWriter(output);
            writeFileHeader();
            // Thread per il salvataggio su file
            mRecFinished = false;
            mCurrentBuffer = 0;
            mAudioEvent.Reset();
            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(this.recThreadProc));
            //avvia la registrazione
            MMRESULT res = Native.waveInStart(hwi);
            if (res != Native.MMSYSERR_NOERROR)
                throw new Exception("impossibile aprire il dispositivo audio");
        }


        public void StopRec()
        {
            MMRESULT res = Native.waveInStop(hwi);
            if (res != Native.MMSYSERR_NOERROR)
                throw new Exception("errore nell'arresto della registrazione audio");
            mRecFinished = true;
            mAudioEvent.Set();
            lock (this) { }; // aspetta il termine del thread di salvataggio
        }

        public static WaveFormats GetDevCaps(UINT uDeviceID)
        {
            WAVEINCAPS caps = new WAVEINCAPS();
            //GCHandle hcaps = GCHandle.Alloc(caps, GCHandleType.Pinned);
            Native.waveInGetDevCaps(uDeviceID, out caps, (UINT)Marshal.SizeOf(caps));
            //hcaps.Free();
            return (WaveFormats) caps.dwFormats;
        }

//        private static void waveInProc(HANDLE hwi, UINTMSG uMsg, DWORD_PTR dwInstance, DWORD_PTR dwParam1, DWORD_PTR dwParam2)
        private static void waveInProc(HANDLE hwi, UINTMSG uMsg, DWORD_PTR dwInstance, ref WAVEHDR dwParam1, DWORD_PTR dwParam2)
        {
            WaveInRecorder recorder = (WaveInRecorder) ((GCHandle)(IntPtr)dwInstance).Target;
            switch (uMsg) {
                //case Native.MM_WIM_OPEN:
                //    break;
                case Native.MM_WIM_DATA:
                    System.Diagnostics.Debug.Assert((dwParam1.dwFlags & (uint)WAVEHDR.Flags.WHDR_DONE) == (uint)WAVEHDR.Flags.WHDR_DONE);
                    recorder.mAudioEvent.Set();
                    break;
                //case Native.MM_WIM_CLOSE:
                //    System.Diagnostics.Trace.Assert(!recorder.mRecFinished, "la registrazione è già marcata come terminata");
                //    recorder.mRecFinished = true;
                //    break;
             }
        }

        private void recThreadProc(Object state)
        {
            lock (this)
            {
                do
                {
                    mAudioEvent.WaitOne();
                    while ((buffers[mCurrentBuffer].mWHdr.dwFlags & (uint)WAVEHDR.Flags.WHDR_DONE) == (uint)WAVEHDR.Flags.WHDR_DONE)
                    {
                        //TODO: salva i dati presenti nel buffer
                        writeData(mCurrentBuffer);
                        // Registra il buffer per il riutilizzo
                        buffers[mCurrentBuffer].RecycleBuffer();
                        // avanza al buffer successivo
                        mCurrentBuffer++;
                        if (mCurrentBuffer >= BUFFERSCOUNT) mCurrentBuffer = 0;
                    }
                    mAudioEvent.Reset();
                    if (mRecFinished)
                        WriteFileProlog();
                } while (!mRecFinished);
            }
            if (RecFinishedEvent != null)
                RecFinishedEvent(this);
        }

        private void writeFileHeader()
        {
            mOutWriter.Write("RIFF".ToCharArray());
            mOutWriter.Write((Int32)0); // size, written later
            mOutWriter.Write("WAVEfmt ".ToCharArray());
            mOutWriter.Write((Int32)16);
            mOutWriter.Write(mWFmt.wFormatTag);
            mOutWriter.Write(mWFmt.nChannels);
            mOutWriter.Write(mWFmt.nSamplesPerSec);
            mOutWriter.Write(mWFmt.nAvgBytesPerSec);
            mOutWriter.Write(mWFmt.nBlockAlign);
            mOutWriter.Write(mWFmt.wBitsPerSample);
            mOutWriter.Write("data".ToCharArray());
            mOutWriter.Write((Int32)0); // size, written later
        }

        private void WriteFileProlog()
        {
            mOutWriter.Seek(4, System.IO.SeekOrigin.Begin);
            System.Diagnostics.Debug.Assert(mDataCounter + 36 == mOutWriter.BaseStream.Length - 8);
            mOutWriter.Write((Int32)mOutWriter.BaseStream.Length - 8);  
            mOutWriter.Seek(40, SeekOrigin.Begin);
            mOutWriter.Write((Int32) mDataCounter);  
            // Chiusura file
            mOutWriter.Close();
            mDataCounter = 0;
        }

        private void writeData(int mCurrentBuffer)
        {
            WaveInBuffer buf = buffers[mCurrentBuffer];
            byte[] bufdata = buf.mData;
            int len = (int)buf.mWHdr.dwBytesRecorded;
            System.Diagnostics.Debug.Assert((len <= buf.mWHdr.dwBufferLength), "Inconsistenza fra byte registrati e dimensione buffer");
            System.Diagnostics.Debug.Assert((buf.mWHdr.dwBytesRecorded <= buf.mWHdr.dwBufferLength), "Inconsistenza fra byte registrati e dimensione buffer");
            mOutWriter.Write(bufdata, 0, len);
            mDataCounter += buf.mWHdr.dwBytesRecorded;
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (!mRecFinished)
                StopRec();
            lock (this)
            {
                for (uint i = 0; i < BUFFERSCOUNT; i++)
                    buffers[i].Dispose();
                Native.waveInClose(hwi);
                mH_this.Free();
            }

        }

        #endregion
    }
    


}
