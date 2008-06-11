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

namespace MapperTool
{
    partial class Form_MapperToolMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.MainMenu mainMenu;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_MapperToolMain));
            this.mainMenu = new System.Windows.Forms.MainMenu();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.menuItem_zoomin = new System.Windows.Forms.MenuItem();
            this.menuItem_zoomout = new System.Windows.Forms.MenuItem();
            this.menuItem5 = new System.Windows.Forms.MenuItem();
            this.menuItem_waypoint = new System.Windows.Forms.MenuItem();
            this.menuItem_gpsactivity = new System.Windows.Forms.MenuItem();
            this.menuItem4 = new System.Windows.Forms.MenuItem();
            this.menuItem_downloadmaps = new System.Windows.Forms.MenuItem();
            this.menuItem_refreshTileCache = new System.Windows.Forms.MenuItem();
            this.menuItem_loadtrack = new System.Windows.Forms.MenuItem();
            this.menuItem_exit = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.menuItem_map_osm = new System.Windows.Forms.MenuItem();
            this.menuItem_map_gmaps = new System.Windows.Forms.MenuItem();
            this.menuItem_showpos = new System.Windows.Forms.MenuItem();
            this.menuItem_followGPS = new System.Windows.Forms.MenuItem();
            this.menuItem_autodownload = new System.Windows.Forms.MenuItem();
            this.menuItem_config = new System.Windows.Forms.MenuItem();
            this.label1 = new System.Windows.Forms.Label();
            this.label_zoom = new System.Windows.Forms.Label();
            this.mapcontrol = new MapsLibrary.MapControl();
            this.hardwareButton_app3 = new Microsoft.WindowsCE.Forms.HardwareButton();
            this.gpsControl = new MapperTool.GPSControl();
            this.gpxControl1 = new MapperTool.GPXControl();
            this.blinkimg_download = new MapperTool.BlinkingImageControl();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainMenu
            // 
            this.mainMenu.MenuItems.Add(this.menuItem1);
            this.mainMenu.MenuItems.Add(this.menuItem2);
            // 
            // menuItem1
            // 
            this.menuItem1.MenuItems.Add(this.menuItem_zoomin);
            this.menuItem1.MenuItems.Add(this.menuItem_zoomout);
            this.menuItem1.MenuItems.Add(this.menuItem5);
            this.menuItem1.MenuItems.Add(this.menuItem_waypoint);
            this.menuItem1.MenuItems.Add(this.menuItem_gpsactivity);
            this.menuItem1.MenuItems.Add(this.menuItem4);
            this.menuItem1.MenuItems.Add(this.menuItem_exit);
            this.menuItem1.Text = "Commands";
            // 
            // menuItem_zoomin
            // 
            this.menuItem_zoomin.Text = "Zoom in";
            this.menuItem_zoomin.Click += new System.EventHandler(this.menuItem_zoomin_Click);
            // 
            // menuItem_zoomout
            // 
            this.menuItem_zoomout.Text = "Zoom out";
            this.menuItem_zoomout.Click += new System.EventHandler(this.menuItem_zoomout_Click);
            // 
            // menuItem5
            // 
            this.menuItem5.Text = "-";
            // 
            // menuItem_waypoint
            // 
            this.menuItem_waypoint.Text = "Mark waypoint";
            this.menuItem_waypoint.Click += new System.EventHandler(this.menuItem_waypoint_Click);
            // 
            // menuItem_gpsactivity
            // 
            this.menuItem_gpsactivity.Text = "start GPS";
            this.menuItem_gpsactivity.Click += new System.EventHandler(this.menuItem_gpsactivity_Click);
            // 
            // menuItem4
            // 
            this.menuItem4.MenuItems.Add(this.menuItem_downloadmaps);
            this.menuItem4.MenuItems.Add(this.menuItem_refreshTileCache);
            this.menuItem4.MenuItems.Add(this.menuItem_loadtrack);
            this.menuItem4.Text = "Other";
            // 
            // menuItem_downloadmaps
            // 
            this.menuItem_downloadmaps.Text = "Download maps";
            this.menuItem_downloadmaps.Click += new System.EventHandler(this.menuItem_downloadmaps_Click);
            // 
            // menuItem_refreshTileCache
            // 
            this.menuItem_refreshTileCache.Text = "Refresh tiles cache";
            this.menuItem_refreshTileCache.Click += new System.EventHandler(this.menuItem_refreshTileCache_Click);
            // 
            // menuItem_loadtrack
            // 
            this.menuItem_loadtrack.Text = "Load track...";
            this.menuItem_loadtrack.Click += new System.EventHandler(this.menuItem_loadtrack_Click);
            // 
            // menuItem_exit
            // 
            this.menuItem_exit.Text = "Exit";
            this.menuItem_exit.Click += new System.EventHandler(this.menuItem_exit_Click);
            // 
            // menuItem2
            // 
            this.menuItem2.MenuItems.Add(this.menuItem3);
            this.menuItem2.MenuItems.Add(this.menuItem_showpos);
            this.menuItem2.MenuItems.Add(this.menuItem_followGPS);
            this.menuItem2.MenuItems.Add(this.menuItem_autodownload);
            this.menuItem2.MenuItems.Add(this.menuItem_config);
            this.menuItem2.Text = "Options";
            // 
            // menuItem3
            // 
            this.menuItem3.MenuItems.Add(this.menuItem_map_osm);
            this.menuItem3.MenuItems.Add(this.menuItem_map_gmaps);
            this.menuItem3.Text = "View Map";
            // 
            // menuItem_map_osm
            // 
            this.menuItem_map_osm.Enabled = false;
            this.menuItem_map_osm.Text = "OSM";
            this.menuItem_map_osm.Click += new System.EventHandler(this.menuItem_map_osm_Click);
            // 
            // menuItem_map_gmaps
            // 
            this.menuItem_map_gmaps.Text = "GMaps";
            this.menuItem_map_gmaps.Click += new System.EventHandler(this.menuItem_map_gmaps_Click);
            // 
            // menuItem_showpos
            // 
            this.menuItem_showpos.Text = "Show position";
            this.menuItem_showpos.Click += new System.EventHandler(this.menuItem_showpos_Click);
            // 
            // menuItem_followGPS
            // 
            this.menuItem_followGPS.Text = "Follow GPS";
            this.menuItem_followGPS.Click += new System.EventHandler(this.menuItem_followGPS_Click);
            // 
            // menuItem_autodownload
            // 
            this.menuItem_autodownload.Text = "Autodownload maps";
            this.menuItem_autodownload.Click += new System.EventHandler(this.menuItem_autodownload_Click);
            // 
            // menuItem_config
            // 
            this.menuItem_config.Text = "Configuration...";
            this.menuItem_config.Click += new System.EventHandler(this.menuItem_config_Click);
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.label1.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Regular);
            this.label1.Location = new System.Drawing.Point(187, 252);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(36, 16);
            this.label1.Text = "Zoom:";
            // 
            // label_zoom
            // 
            this.label_zoom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.label_zoom.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Regular);
            this.label_zoom.Location = new System.Drawing.Point(223, 252);
            this.label_zoom.Name = "label_zoom";
            this.label_zoom.Size = new System.Drawing.Size(17, 16);
            this.label_zoom.Text = "14";
            // 
            // mapcontrol
            // 
            this.mapcontrol.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
            this.mapcontrol.Location = new System.Drawing.Point(0, 0);
            this.mapcontrol.Name = "mapcontrol";
            this.mapcontrol.ShowPosition = false;
            this.mapcontrol.Size = new System.Drawing.Size(240, 268);
            this.mapcontrol.TabIndex = 0;
            this.mapcontrol.Zoom = ((uint)(0u));
            this.mapcontrol.ZoomChanged += new MapsLibrary.MapControl.MapControlEventHandler(this.mapcontrol_ZoomChanged);
            // 
            // hardwareButton_app3
            // 
            this.hardwareButton_app3.AssociatedControl = this;
            this.hardwareButton_app3.HardwareKey = Microsoft.WindowsCE.Forms.HardwareKeys.ApplicationKey3;
            // 
            // gpsControl
            // 
            this.gpsControl.BackColor = System.Drawing.SystemColors.Window;
            this.gpsControl.Location = new System.Drawing.Point(22, 0);
            this.gpsControl.Name = "gpsControl";
            this.gpsControl.Size = new System.Drawing.Size(12, 12);
            this.gpsControl.TabIndex = 7;
            // 
            // gpxControl1
            // 
            this.gpxControl1.BackColor = System.Drawing.Color.White;
            this.gpxControl1.Font = new System.Drawing.Font("Tahoma", 7F, System.Drawing.FontStyle.Regular);
            this.gpxControl1.Location = new System.Drawing.Point(40, 0);
            this.gpxControl1.Name = "gpxControl1";
            this.gpxControl1.Size = new System.Drawing.Size(30, 12);
            this.gpxControl1.TabIndex = 14;
            this.gpxControl1.Visible = false;
            // 
            // blinkimg_download
            // 
            this.blinkimg_download.BackColor = System.Drawing.Color.White;
            this.blinkimg_download.Blink = false;
            this.blinkimg_download.BlinkingInterval = 400;
            this.blinkimg_download.Image = ((System.Drawing.Image)(resources.GetObject("blinkimg_download.Image")));
            this.blinkimg_download.Location = new System.Drawing.Point(0, 0);
            this.blinkimg_download.Name = "blinkimg_download";
            this.blinkimg_download.Size = new System.Drawing.Size(16, 16);
            this.blinkimg_download.TabIndex = 17;
            this.blinkimg_download.VisibleOnStop = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.blinkimg_download);
            this.panel1.Controls.Add(this.gpxControl1);
            this.panel1.Controls.Add(this.gpsControl);
            this.panel1.Location = new System.Drawing.Point(112, 252);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(75, 16);
            // 
            // Form_MapperToolMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label_zoom);
            this.Controls.Add(this.mapcontrol);
            this.KeyPreview = true;
            this.Menu = this.mainMenu;
            this.Name = "Form_MapperToolMain";
            this.Text = "Pollicino";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.Form_MapperToolMain_Closing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form_MapperToolMain_KeyDown);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private MapsLibrary.MapControl mapcontrol;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label_zoom;
        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.MenuItem menuItem_gpsactivity;
        private System.Windows.Forms.MenuItem menuItem2;
        private System.Windows.Forms.MenuItem menuItem_config;
        private System.Windows.Forms.MenuItem menuItem_waypoint;
        private System.Windows.Forms.MenuItem menuItem3;
        private System.Windows.Forms.MenuItem menuItem_map_osm;
        private System.Windows.Forms.MenuItem menuItem_map_gmaps;
        private System.Windows.Forms.MenuItem menuItem_zoomin;
        private System.Windows.Forms.MenuItem menuItem_zoomout;
        private System.Windows.Forms.MenuItem menuItem5;
        private System.Windows.Forms.MenuItem menuItem_downloadmaps;
        private System.Windows.Forms.MenuItem menuItem_loadtrack;
        private System.Windows.Forms.MenuItem menuItem_followGPS;
        private Microsoft.WindowsCE.Forms.HardwareButton hardwareButton_app3;
        private System.Windows.Forms.MenuItem menuItem_exit;
        private System.Windows.Forms.MenuItem menuItem_refreshTileCache;
        private System.Windows.Forms.MenuItem menuItem4;
        private GPSControl gpsControl;
        private System.Windows.Forms.MenuItem menuItem_autodownload;
        private GPXControl gpxControl1;
        private System.Windows.Forms.MenuItem menuItem_showpos;
        private BlinkingImageControl blinkimg_download;
        private System.Windows.Forms.Panel panel1;




    }
}

