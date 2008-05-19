using System;
//using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MapperTool
{
    public partial class FormOptions : Form
    {
        public FormOptions()
        {
            InitializeComponent();
        }
        public MapperToolOptions data
        {
            get
            {
                MapperToolOptions opt = new MapperToolOptions();
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
            }
        }

        private void button_SelectSimulationFile_Click(object sender, EventArgs e)
        {
            try
            {
                //System.IO.FileInfo simfile = new System.IO.FileInfo(tb_SimulationFile.Text);
                //System.IO.File.
                //if (simfile.Directory.Exists)
                //{
                    openFileDialog1.FileName = tb_SimulationFile.Text;
                //    openFileDialog1.InitialDirectory = simfile.DirectoryName;
                //}
            }
            catch (ArgumentException) { }
            if (this.openFileDialog1.ShowDialog() == DialogResult.OK)
                tb_SimulationFile.Text = openFileDialog1.FileName;
        }

        private void button_gpslogpath_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.
        }
        
    }


}