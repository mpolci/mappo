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

namespace MapperTool
{
    partial class FormOptions
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage_GPS = new System.Windows.Forms.TabPage();
            this.button_gpslogpath = new System.Windows.Forms.Button();
            this.tb_GPSLogPath = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.button_SelectSimulationFile = new System.Windows.Forms.Button();
            this.tb_SimulationFile = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tb_GPSPortSpeed = new System.Windows.Forms.TextBox();
            this.cb_Simulation = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tb_GPSPort = new System.Windows.Forms.TextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.combo_TileServer = new System.Windows.Forms.ComboBox();
            this.button_emptytilescache = new System.Windows.Forms.Button();
            this.button_GMapsCacheDir = new System.Windows.Forms.Button();
            this.button_TileCacheDir = new System.Windows.Forms.Button();
            this.num_DownloadDepth = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.tb_GMapsCacheDir = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tb_TileCacheDir = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.combo_RecFormat = new System.Windows.Forms.ComboBox();
            this.label11 = new System.Windows.Forms.Label();
            this.num_RecDeviceId = new System.Windows.Forms.NumericUpDown();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.num_recordaudioseconds = new System.Windows.Forms.NumericUpDown();
            this.cb_recordaudio = new System.Windows.Forms.CheckBox();
            this.button_waypointsound = new System.Windows.Forms.Button();
            this.tb_waypointsound = new System.Windows.Forms.TextBox();
            this.cb_waypointsound = new System.Windows.Forms.CheckBox();
            this.mainMenu1 = new System.Windows.Forms.MainMenu();
            this.tabControl1.SuspendLayout();
            this.tabPage_GPS.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage_GPS);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(240, 268);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage_GPS
            // 
            this.tabPage_GPS.AutoScroll = true;
            this.tabPage_GPS.Controls.Add(this.button_gpslogpath);
            this.tabPage_GPS.Controls.Add(this.tb_GPSLogPath);
            this.tabPage_GPS.Controls.Add(this.label8);
            this.tabPage_GPS.Controls.Add(this.button_SelectSimulationFile);
            this.tabPage_GPS.Controls.Add(this.tb_SimulationFile);
            this.tabPage_GPS.Controls.Add(this.label3);
            this.tabPage_GPS.Controls.Add(this.tb_GPSPortSpeed);
            this.tabPage_GPS.Controls.Add(this.cb_Simulation);
            this.tabPage_GPS.Controls.Add(this.label2);
            this.tabPage_GPS.Controls.Add(this.label1);
            this.tabPage_GPS.Controls.Add(this.tb_GPSPort);
            this.tabPage_GPS.Location = new System.Drawing.Point(0, 0);
            this.tabPage_GPS.Name = "tabPage_GPS";
            this.tabPage_GPS.Size = new System.Drawing.Size(240, 245);
            this.tabPage_GPS.Text = "GPS";
            // 
            // button_gpslogpath
            // 
            this.button_gpslogpath.Location = new System.Drawing.Point(216, 77);
            this.button_gpslogpath.Name = "button_gpslogpath";
            this.button_gpslogpath.Size = new System.Drawing.Size(21, 21);
            this.button_gpslogpath.TabIndex = 16;
            this.button_gpslogpath.Text = "...";
            this.button_gpslogpath.Click += new System.EventHandler(this.button_gpslogpath_Click);
            // 
            // tb_GPSLogPath
            // 
            this.tb_GPSLogPath.Location = new System.Drawing.Point(7, 77);
            this.tb_GPSLogPath.Name = "tb_GPSLogPath";
            this.tb_GPSLogPath.Size = new System.Drawing.Size(206, 21);
            this.tb_GPSLogPath.TabIndex = 11;
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(7, 56);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(90, 20);
            this.label8.Text = "Log path";
            // 
            // button_SelectSimulationFile
            // 
            this.button_SelectSimulationFile.Location = new System.Drawing.Point(216, 167);
            this.button_SelectSimulationFile.Name = "button_SelectSimulationFile";
            this.button_SelectSimulationFile.Size = new System.Drawing.Size(21, 21);
            this.button_SelectSimulationFile.TabIndex = 9;
            this.button_SelectSimulationFile.Text = "...";
            this.button_SelectSimulationFile.Click += new System.EventHandler(this.button_SelectSimulationFile_Click);
            // 
            // tb_SimulationFile
            // 
            this.tb_SimulationFile.Location = new System.Drawing.Point(7, 167);
            this.tb_SimulationFile.Name = "tb_SimulationFile";
            this.tb_SimulationFile.Size = new System.Drawing.Size(206, 21);
            this.tb_SimulationFile.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(7, 146);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(90, 20);
            this.label3.Text = "Simulation file";
            // 
            // tb_GPSPortSpeed
            // 
            this.tb_GPSPortSpeed.Location = new System.Drawing.Point(101, 30);
            this.tb_GPSPortSpeed.Name = "tb_GPSPortSpeed";
            this.tb_GPSPortSpeed.Size = new System.Drawing.Size(112, 21);
            this.tb_GPSPortSpeed.TabIndex = 5;
            // 
            // cb_Simulation
            // 
            this.cb_Simulation.Location = new System.Drawing.Point(7, 121);
            this.cb_Simulation.Name = "cb_Simulation";
            this.cb_Simulation.Size = new System.Drawing.Size(148, 20);
            this.cb_Simulation.TabIndex = 4;
            this.cb_Simulation.Text = "GPS Simulation";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(7, 31);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 20);
            this.label2.Text = "Speed";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(7, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 20);
            this.label1.Text = "Port";
            // 
            // tb_GPSPort
            // 
            this.tb_GPSPort.Location = new System.Drawing.Point(101, 3);
            this.tb_GPSPort.Name = "tb_GPSPort";
            this.tb_GPSPort.Size = new System.Drawing.Size(112, 21);
            this.tb_GPSPort.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.AutoScroll = true;
            this.tabPage2.Controls.Add(this.combo_TileServer);
            this.tabPage2.Controls.Add(this.button_emptytilescache);
            this.tabPage2.Controls.Add(this.button_GMapsCacheDir);
            this.tabPage2.Controls.Add(this.button_TileCacheDir);
            this.tabPage2.Controls.Add(this.num_DownloadDepth);
            this.tabPage2.Controls.Add(this.label7);
            this.tabPage2.Controls.Add(this.label6);
            this.tabPage2.Controls.Add(this.tb_GMapsCacheDir);
            this.tabPage2.Controls.Add(this.label5);
            this.tabPage2.Controls.Add(this.tb_TileCacheDir);
            this.tabPage2.Controls.Add(this.label4);
            this.tabPage2.Location = new System.Drawing.Point(0, 0);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Size = new System.Drawing.Size(240, 245);
            this.tabPage2.Text = "Maps";
            // 
            // combo_TileServer
            // 
            this.combo_TileServer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.combo_TileServer.Items.Add("http://tile.openstreetmap.org/");
            this.combo_TileServer.Items.Add("http://tah.openstreetmap.org/Tiles/tile.php/");
            this.combo_TileServer.Location = new System.Drawing.Point(7, 26);
            this.combo_TileServer.Name = "combo_TileServer";
            this.combo_TileServer.Size = new System.Drawing.Size(226, 22);
            this.combo_TileServer.TabIndex = 28;
            // 
            // button_emptytilescache
            // 
            this.button_emptytilescache.Location = new System.Drawing.Point(46, 152);
            this.button_emptytilescache.Name = "button_emptytilescache";
            this.button_emptytilescache.Size = new System.Drawing.Size(149, 20);
            this.button_emptytilescache.TabIndex = 23;
            this.button_emptytilescache.Text = "Empty tiles cache";
            this.button_emptytilescache.Click += new System.EventHandler(this.button_emptytilescache_Click);
            // 
            // button_GMapsCacheDir
            // 
            this.button_GMapsCacheDir.Location = new System.Drawing.Point(212, 221);
            this.button_GMapsCacheDir.Name = "button_GMapsCacheDir";
            this.button_GMapsCacheDir.Size = new System.Drawing.Size(21, 21);
            this.button_GMapsCacheDir.TabIndex = 18;
            this.button_GMapsCacheDir.Text = "...";
            this.button_GMapsCacheDir.Click += new System.EventHandler(this.button_GMapsCacheDir_Click);
            // 
            // button_TileCacheDir
            // 
            this.button_TileCacheDir.Location = new System.Drawing.Point(212, 74);
            this.button_TileCacheDir.Name = "button_TileCacheDir";
            this.button_TileCacheDir.Size = new System.Drawing.Size(21, 21);
            this.button_TileCacheDir.TabIndex = 17;
            this.button_TileCacheDir.Text = "...";
            this.button_TileCacheDir.Click += new System.EventHandler(this.button_TileCacheDir_Click);
            // 
            // num_DownloadDepth
            // 
            this.num_DownloadDepth.Location = new System.Drawing.Point(184, 101);
            this.num_DownloadDepth.Maximum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.num_DownloadDepth.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.num_DownloadDepth.Name = "num_DownloadDepth";
            this.num_DownloadDepth.Size = new System.Drawing.Size(49, 22);
            this.num_DownloadDepth.TabIndex = 10;
            this.num_DownloadDepth.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(7, 103);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(162, 20);
            this.label7.Text = "Download command depth";
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(7, 200);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(149, 20);
            this.label6.Text = "GMaps cache path";
            // 
            // tb_GMapsCacheDir
            // 
            this.tb_GMapsCacheDir.Location = new System.Drawing.Point(7, 221);
            this.tb_GMapsCacheDir.Name = "tb_GMapsCacheDir";
            this.tb_GMapsCacheDir.Size = new System.Drawing.Size(199, 21);
            this.tb_GMapsCacheDir.TabIndex = 7;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(7, 53);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(149, 20);
            this.label5.Text = "Tiles cache path";
            // 
            // tb_TileCacheDir
            // 
            this.tb_TileCacheDir.Location = new System.Drawing.Point(7, 74);
            this.tb_TileCacheDir.Name = "tb_TileCacheDir";
            this.tb_TileCacheDir.Size = new System.Drawing.Size(199, 21);
            this.tb_TileCacheDir.TabIndex = 4;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(7, 6);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(149, 20);
            this.label4.Text = "OSM tiles server";
            // 
            // tabPage3
            // 
            this.tabPage3.AutoScroll = true;
            this.tabPage3.Controls.Add(this.combo_RecFormat);
            this.tabPage3.Controls.Add(this.label11);
            this.tabPage3.Controls.Add(this.num_RecDeviceId);
            this.tabPage3.Controls.Add(this.label10);
            this.tabPage3.Controls.Add(this.label9);
            this.tabPage3.Controls.Add(this.num_recordaudioseconds);
            this.tabPage3.Controls.Add(this.cb_recordaudio);
            this.tabPage3.Controls.Add(this.button_waypointsound);
            this.tabPage3.Controls.Add(this.tb_waypointsound);
            this.tabPage3.Controls.Add(this.cb_waypointsound);
            this.tabPage3.Location = new System.Drawing.Point(0, 0);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(240, 245);
            this.tabPage3.Text = "Interface";
            // 
            // combo_RecFormat
            // 
            this.combo_RecFormat.Location = new System.Drawing.Point(69, 138);
            this.combo_RecFormat.Name = "combo_RecFormat";
            this.combo_RecFormat.Size = new System.Drawing.Size(164, 22);
            this.combo_RecFormat.TabIndex = 26;
            // 
            // label11
            // 
            this.label11.Location = new System.Drawing.Point(7, 140);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(56, 20);
            this.label11.Text = "Format";
            // 
            // num_RecDeviceId
            // 
            this.num_RecDeviceId.Location = new System.Drawing.Point(165, 110);
            this.num_RecDeviceId.Name = "num_RecDeviceId";
            this.num_RecDeviceId.Size = new System.Drawing.Size(68, 22);
            this.num_RecDeviceId.TabIndex = 23;
            this.num_RecDeviceId.ValueChanged += new System.EventHandler(this.num_RecDeviceId_ValueChanged);
            // 
            // label10
            // 
            this.label10.Location = new System.Drawing.Point(7, 112);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(100, 20);
            this.label10.Text = "Device ID";
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(7, 84);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(141, 20);
            this.label9.Text = "Seconds to record:";
            // 
            // num_recordaudioseconds
            // 
            this.num_recordaudioseconds.Location = new System.Drawing.Point(165, 82);
            this.num_recordaudioseconds.Maximum = new decimal(new int[] {
            7200,
            0,
            0,
            0});
            this.num_recordaudioseconds.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.num_recordaudioseconds.Name = "num_recordaudioseconds";
            this.num_recordaudioseconds.Size = new System.Drawing.Size(68, 22);
            this.num_recordaudioseconds.TabIndex = 20;
            this.num_recordaudioseconds.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.num_recordaudioseconds.ValueChanged += new System.EventHandler(this.num_recordaudioseconds_ValueChanged);
            // 
            // cb_recordaudio
            // 
            this.cb_recordaudio.Location = new System.Drawing.Point(0, 56);
            this.cb_recordaudio.Name = "cb_recordaudio";
            this.cb_recordaudio.Size = new System.Drawing.Size(230, 20);
            this.cb_recordaudio.TabIndex = 19;
            this.cb_recordaudio.Text = "Record Audio on new Waypoint";
            // 
            // button_waypointsound
            // 
            this.button_waypointsound.Location = new System.Drawing.Point(212, 29);
            this.button_waypointsound.Name = "button_waypointsound";
            this.button_waypointsound.Size = new System.Drawing.Size(21, 21);
            this.button_waypointsound.TabIndex = 18;
            this.button_waypointsound.Text = "...";
            // 
            // tb_waypointsound
            // 
            this.tb_waypointsound.Location = new System.Drawing.Point(7, 29);
            this.tb_waypointsound.Name = "tb_waypointsound";
            this.tb_waypointsound.Size = new System.Drawing.Size(199, 21);
            this.tb_waypointsound.TabIndex = 1;
            this.tb_waypointsound.Text = "\\Windows\\Infbeg.wav";
            // 
            // cb_waypointsound
            // 
            this.cb_waypointsound.Checked = true;
            this.cb_waypointsound.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_waypointsound.Location = new System.Drawing.Point(3, 3);
            this.cb_waypointsound.Name = "cb_waypointsound";
            this.cb_waypointsound.Size = new System.Drawing.Size(230, 20);
            this.cb_waypointsound.TabIndex = 0;
            this.cb_waypointsound.Text = "Play sound on new waypoint";
            // 
            // FormOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.Controls.Add(this.tabControl1);
            this.Menu = this.mainMenu1;
            this.Name = "FormOptions";
            this.Text = "MapperTool Options";
            this.tabControl1.ResumeLayout(false);
            this.tabPage_GPS.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage_GPS;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TextBox tb_GPSPort;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tb_SimulationFile;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tb_GPSPortSpeed;
        private System.Windows.Forms.CheckBox cb_Simulation;
        private System.Windows.Forms.Button button_SelectSimulationFile;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tb_GMapsCacheDir;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tb_TileCacheDir;
        private System.Windows.Forms.NumericUpDown num_DownloadDepth;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox tb_GPSLogPath;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.MainMenu mainMenu1;
        private System.Windows.Forms.Button button_gpslogpath;
        private System.Windows.Forms.Button button_GMapsCacheDir;
        private System.Windows.Forms.Button button_TileCacheDir;
        private System.Windows.Forms.Button button_emptytilescache;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TextBox tb_waypointsound;
        private System.Windows.Forms.CheckBox cb_waypointsound;
        private System.Windows.Forms.Button button_waypointsound;
        private System.Windows.Forms.NumericUpDown num_recordaudioseconds;
        private System.Windows.Forms.CheckBox cb_recordaudio;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.NumericUpDown num_RecDeviceId;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ComboBox combo_RecFormat;
        private System.Windows.Forms.ComboBox combo_TileServer;
    }
}