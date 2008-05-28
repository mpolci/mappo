﻿using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using OpenNETCF.Media.WaveAudio;

namespace MapperTool
{
    public partial class FormOptions : Form
    {
        SoundFormats[] formats;

        public FormOptions()
        {
            InitializeComponent();

            int recdevices = OpenNETCF.Media.WaveAudio.Recorder.NumDevices;
            num_RecDeviceId.Maximum = (decimal) recdevices - 1;

            formats = new SoundFormats[recdevices];
            OpenNETCF.Media.WaveAudio.Recorder rec = new OpenNETCF.Media.WaveAudio.Recorder();
            for (int i = 0; i < recdevices; i++)
            {
                formats[i] = rec.SupportedRecordingFormats(i);
            }


            num_RecDeviceId.Value = 0;
            fill_RecFormats();
        }

        public ApplicationOptions data
        {
            get
            {
                ApplicationOptions opt = new ApplicationOptions();
                opt.GPS.PortName = tb_GPSPort.Text;
                opt.GPS.PortSpeed = int.Parse(tb_GPSPortSpeed.Text);
                opt.GPS.Simulation = cb_Simulation.Checked;
                opt.GPS.SimulationFile = tb_SimulationFile.Text;
                opt.GPS.LogsDir = tb_GPSLogPath.Text;
                opt.Maps.OSM.OSMTileServer = combo_TileServer.Text;
                opt.Maps.OSM.TileCachePath = tb_TileCacheDir.Text;
                opt.Maps.OSM.AutoDownload = cb_autodownload.Checked;
                opt.Maps.OSM.DownloadDepth = (int) num_DownloadDepth.Value;
                opt.Maps.GMaps.CachePath = tb_GMapsCacheDir.Text;
                opt.Application.WaypointSoundPlay = cb_waypointsound.Checked;
                opt.Application.WaypointSoundFile = tb_waypointsound.Text;
                opt.Application.WaypointRecordAudio = cb_recordaudio.Checked;
                opt.Application.WaypointRecordAudioSeconds = (int) num_recordaudioseconds.Value;
                opt.Application.RecordAudioDevice = (int)num_RecDeviceId.Value;
                opt.Application.RecordAudioFormat = (OpenNETCF.Media.WaveAudio.SoundFormats)combo_RecFormat.SelectedItem;
                return opt;
            }
            set
            {
                tb_GPSPort.Text = value.GPS.PortName;
                tb_GPSPortSpeed.Text = value.GPS.PortSpeed.ToString();
                cb_Simulation.Checked = value.GPS.Simulation;
                tb_SimulationFile.Text = value.GPS.SimulationFile;
                tb_GPSLogPath.Text = value.GPS.LogsDir;
                combo_TileServer.Text = value.Maps.OSM.OSMTileServer;
                tb_TileCacheDir.Text = value.Maps.OSM.TileCachePath;
                cb_autodownload.Checked = value.Maps.OSM.AutoDownload;
                try {
                    num_DownloadDepth.Value = value.Maps.OSM.DownloadDepth;
                } catch (Exception) { }
                tb_GMapsCacheDir.Text = value.Maps.GMaps.CachePath;
                cb_waypointsound.Checked = value.Application.WaypointSoundPlay;
                tb_waypointsound.Text = value.Application.WaypointSoundFile;
                cb_recordaudio.Checked = value.Application.WaypointRecordAudio;
                try {
                    num_recordaudioseconds.Value = value.Application.WaypointRecordAudioSeconds;
                } catch (Exception) {}
                try {
                    num_RecDeviceId.Value = value.Application.RecordAudioDevice;
                } catch (Exception) {}
                try {
                    combo_RecFormat.SelectedItem = value.Application.RecordAudioFormat;
                } catch (Exception) {}
            }
        }

        private void button_SelectSimulationFile_Click(object sender, EventArgs e)
        {
            string opendir = Path.GetDirectoryName(tb_SimulationFile.Text);
            if (!Directory.Exists(opendir))
                opendir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            using (FormOpenFile openfiledlg = new FormOpenFile(opendir, false))
                if (openfiledlg.ShowDialog() == DialogResult.OK)
                    tb_SimulationFile.Text = openfiledlg.openfile;
        }

        private static void selectDir(TextBox tb)
        {
            string opendir = tb.Text;
            if (!Directory.Exists(opendir)) 
                opendir = Path.GetDirectoryName(opendir);
            if (!Directory.Exists(opendir))
                opendir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            using (FormOpenFile openfiledlg = new FormOpenFile(opendir, true))
                if (openfiledlg.ShowDialog() == DialogResult.OK)
                    tb.Text = openfiledlg.directoty;
        }

        private void button_gpslogpath_Click(object sender, EventArgs e)
        {
            selectDir(tb_GPSLogPath);
        }

        private void button_TileCacheDir_Click(object sender, EventArgs e)
        {
            selectDir(tb_TileCacheDir);
        }

        private void button_GMapsCacheDir_Click(object sender, EventArgs e)
        {
            selectDir(tb_GMapsCacheDir);
        }

        private void num_recordaudioseconds_ValueChanged(object sender, EventArgs e)
        {
            if (num_recordaudioseconds.Value < 15)
                num_recordaudioseconds.Increment = 1;
            else if (num_recordaudioseconds.Value < 60)
                num_recordaudioseconds.Increment = 5;
            else if (num_recordaudioseconds.Value < 240)
                num_recordaudioseconds.Increment = 30;
            else if (num_recordaudioseconds.Value < 600)
                num_recordaudioseconds.Increment = 60;
            else
                num_recordaudioseconds.Increment = 120;
        }

        private void num_RecDeviceId_ValueChanged(object sender, EventArgs e)
        {
            fill_RecFormats();
        }

        private void fill_RecFormats()
        {
            combo_RecFormat.Items.Clear();
            SoundFormats devformats = formats[(int) num_RecDeviceId.Value];

            for (int i = 0; i < sizeof(SoundFormats) * 8 - 1; i++)
            {
                SoundFormats curf = (SoundFormats) (1 << i);
                if ((devformats & curf) != 0)
                    combo_RecFormat.Items.Add(curf);
            }

        }

        private void button_emptytilescache_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("It will completely delete the cache directory and subdirs.", "", MessageBoxButtons.OKCancel, MessageBoxIcon.None, MessageBoxDefaultButton.Button2) != DialogResult.OK)
                return;
            System.Windows.Forms.Cursor.Current = Cursors.WaitCursor;
            try {
                Directory.Delete(tb_TileCacheDir.Text, true);
            } catch (Exception) {}
            System.Windows.Forms.Cursor.Current = Cursors.Default;
        }


    }
}