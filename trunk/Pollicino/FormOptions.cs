﻿/*******************************************************************************
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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Microsoft.WindowsCE.Forms;

namespace MapperTools.Pollicino
{
    public partial class FormOptions : Form
    {
        private const int MAXAUDIOREC = 18000;
        WaveIn4CF.WaveFormats[] formats;

        public FormOptions()
        {
            InitializeComponent();

            num_recordaudioseconds.Maximum = MAXAUDIOREC;

            uint recdevices = WaveIn4CF.Native.waveInGetNumDevs();
            num_RecDeviceId.Maximum = (decimal) recdevices - 1;

            formats = new WaveIn4CF.WaveFormats[recdevices];
            for (uint i = 0; i < recdevices; i++)
            {
                formats[i] = WaveIn4CF.WaveInRecorder.GetSupportedFormats(i);
            }
            num_RecDeviceId.Value = 0;
            fill_RecFormats();

        }

        private ApplicationOptions _data;
        public ApplicationOptions data
        {
            get
            {
                if (_data == null) 
                    _data = new ApplicationOptions();
                _data.GPS.PortName = tb_GPSPort.Text;
                _data.GPS.PortSpeed = int.Parse(tb_GPSPortSpeed.Text);
                _data.GPS.Simulation = cb_Simulation.Checked;
                _data.GPS.SimulationFile = tb_SimulationFile.Text;
                _data.GPS.LogsDir = tb_GPSLogPath.Text;
                _data.GPS.Autostart = cb_gps_autostart.Checked;
                _data.Maps.OSM.OSMTileServer = combo_TileServer.Text;
                _data.Maps.OSM.TileCachePath = tb_TileCacheDir.Text;
                _data.Maps.OSM.DownloadDepth = (int) num_DownloadDepth.Value;
                _data.Maps.GMaps.CachePath = tb_GMapsCacheDir.Text;
                _data.Application.DelayGPXTrackStart = cb_delayTrackStart.Checked;
                _data.Application.WaypointSoundPlay = cb_waypointsound.Checked;
                _data.Application.WaypointSoundFile = tb_waypointsound.Text;
                _data.Application.WaypointRecordAudio = !rb_audiorec_disabled.Checked;
                _data.Application.WaypointRecordAudioSeconds = (int) num_recordaudioseconds.Value;
                _data.Application.RecordAudioDevice = (uint)num_RecDeviceId.Value;
                _data.Application.RecordAudioFormat = (WaveIn4CF.WaveFormats)combo_RecFormat.SelectedItem;
                _data.Application.FullScreen = cb_fullscreen.Checked;
                _data.Application.CameraButton = (HardwareKeys) combo_CameraButton.SelectedItem;
                return _data;
            }
            set
            {
                _data = value;
                tb_GPSPort.Text = value.GPS.PortName;
                tb_GPSPortSpeed.Text = value.GPS.PortSpeed.ToString();
                cb_Simulation.Checked = value.GPS.Simulation;
                tb_SimulationFile.Text = value.GPS.SimulationFile;
                tb_GPSLogPath.Text = value.GPS.LogsDir;
                cb_gps_autostart.Checked = value.GPS.Autostart;
                combo_TileServer.Text = value.Maps.OSM.OSMTileServer;
                tb_TileCacheDir.Text = value.Maps.OSM.TileCachePath;
                try {
                    num_DownloadDepth.Value = value.Maps.OSM.DownloadDepth;
                } catch (Exception) { }
                tb_GMapsCacheDir.Text = value.Maps.GMaps.CachePath;
                cb_delayTrackStart.Checked = value.Application.DelayGPXTrackStart;
                cb_waypointsound.Checked = value.Application.WaypointSoundPlay;
                tb_waypointsound.Text = value.Application.WaypointSoundFile;
                if (!value.Application.WaypointRecordAudio)
                    rb_audiorec_disabled.Checked = true;
                else if (value.Application.DelayGPXTrackStart && 
                         value.Application.WaypointRecordAudioSeconds == MAXAUDIOREC)
                    rb_audiorec_continuous.Checked = true;
                else 
                    rb_audiorec_multiple.Checked = true;

                try {
                    num_recordaudioseconds.Value = value.Application.WaypointRecordAudioSeconds;
                } catch (Exception) {}
                try {
                    num_RecDeviceId.Value = value.Application.RecordAudioDevice;
                } catch (Exception) {}
                try {
                    combo_RecFormat.SelectedItem = value.Application.RecordAudioFormat;
                } catch (Exception) {}
                cb_fullscreen.Checked = value.Application.FullScreen;
                combo_CameraButton.SelectedItem = value.Application.CameraButton;
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
            else if (num_recordaudioseconds.Value < 1800)
                num_recordaudioseconds.Increment = 120;
            else
                num_recordaudioseconds.Increment = 900;
        }

        private void num_RecDeviceId_ValueChanged(object sender, EventArgs e)
        {
            fill_RecFormats();
        }

        private void fill_RecFormats()
        {
            combo_RecFormat.Items.Clear();
            WaveIn4CF.WaveFormats devformats = formats[(int)num_RecDeviceId.Value];

            for (int i = 0; i < sizeof(WaveIn4CF.WaveFormats) * 8 - 1; i++)
            {
                WaveIn4CF.WaveFormats curf = (WaveIn4CF.WaveFormats)(1 << i);
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

        private void button_waypointsound_Click(object sender, EventArgs e)
        {
            string opendir = Path.GetDirectoryName(tb_waypointsound.Text);
            if (!Directory.Exists(opendir))
                opendir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            using (FormOpenFile openfiledlg = new FormOpenFile(opendir, false))
                if (openfiledlg.ShowDialog() == DialogResult.OK)
                    tb_waypointsound.Text = openfiledlg.openfile;
        }

        private void rb_audiorec_disabled_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_audiorec_disabled.Checked)
            {
                num_RecDeviceId.Enabled = false;
                num_recordaudioseconds.Enabled = false;
                combo_RecFormat.Enabled = false;
            }
        }

        private void rb_audiorec_multiple_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_audiorec_multiple.Checked)
            {
                num_RecDeviceId.Enabled = true;
                num_recordaudioseconds.Enabled = true;
                combo_RecFormat.Enabled = true;
            }
        }

        private void rb_audiorec_continuous_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_audiorec_continuous.Checked)
            {
                num_RecDeviceId.Enabled = true;
                num_recordaudioseconds.Enabled = false;
                combo_RecFormat.Enabled = true;
                num_recordaudioseconds.Value = MAXAUDIOREC;
                cb_delayTrackStart.Checked = true;
                cb_delayTrackStart.Enabled = false;
            }
            else 
            {
                cb_delayTrackStart.Enabled = true;
                if (_data.Application.WaypointRecordAudioSeconds == MAXAUDIOREC && _data.Application.DelayGPXTrackStart)
                {
                    num_recordaudioseconds.Value = 10;
                    cb_delayTrackStart.Checked = false;
                }
                else
                {
                    num_recordaudioseconds.Value = _data.Application.WaypointRecordAudioSeconds;
                    cb_delayTrackStart.Checked = _data.Application.DelayGPXTrackStart;
                }

            }

        }

    }
}