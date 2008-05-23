using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace MapperTool
{
    public partial class FormOptions : Form
    {
        public FormOptions()
        {
            InitializeComponent();
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
                opt.Maps.OSM.OSMTileServer = tb_TileServer.Text;
                opt.Maps.OSM.TileCachePath = tb_TileCacheDir.Text;
                opt.Maps.OSM.AutoDownload = cb_autodownload.Checked;
                opt.Maps.OSM.DownloadDepth = (int) num_DownloadDepth.Value;
                opt.Maps.GMaps.CachePath = tb_GMapsCacheDir.Text;
                opt.Application.WaypointSoundPlay = cb_waypointsound.Checked;
                opt.Application.WaypointSoundFile = tb_waypointsound.Text;
                return opt;
            }
            set
            {
                tb_GPSPort.Text = value.GPS.PortName;
                tb_GPSPortSpeed.Text = value.GPS.PortSpeed.ToString();
                cb_Simulation.Checked = value.GPS.Simulation;
                tb_SimulationFile.Text = value.GPS.SimulationFile;
                tb_GPSLogPath.Text = value.GPS.LogsDir;
                tb_TileServer.Text = value.Maps.OSM.OSMTileServer;
                tb_TileCacheDir.Text = value.Maps.OSM.TileCachePath;
                cb_autodownload.Checked = value.Maps.OSM.AutoDownload;
                num_DownloadDepth.Value = value.Maps.OSM.DownloadDepth;
                tb_GMapsCacheDir.Text = value.Maps.GMaps.CachePath;
                cb_waypointsound.Checked = value.Application.WaypointSoundPlay;
                tb_waypointsound.Text = value.Application.WaypointSoundFile;
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

    }
}