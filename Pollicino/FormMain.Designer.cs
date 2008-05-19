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
            this.mainMenu = new System.Windows.Forms.MainMenu();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.menuItem_zoomin = new System.Windows.Forms.MenuItem();
            this.menuItem_zoomout = new System.Windows.Forms.MenuItem();
            this.menuItem_downloadmaps = new System.Windows.Forms.MenuItem();
            this.menuItem5 = new System.Windows.Forms.MenuItem();
            this.menuItem_waypoint = new System.Windows.Forms.MenuItem();
            this.menuItem_gpsactivity = new System.Windows.Forms.MenuItem();
            this.menuItem_loadtrack = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.menuItem_config = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.menuItem_map_osm = new System.Windows.Forms.MenuItem();
            this.menuItem_map_gmaps = new System.Windows.Forms.MenuItem();
            this.menuItem_followGPS = new System.Windows.Forms.MenuItem();
            this.label1 = new System.Windows.Forms.Label();
            this.label_zoom = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label_lon = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label_lat = new System.Windows.Forms.Label();
            this.pb_GPSActvity = new System.Windows.Forms.PictureBox();
            this.mapcontrol = new MapsLibrary.MapControl();
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
            this.menuItem1.MenuItems.Add(this.menuItem_downloadmaps);
            this.menuItem1.MenuItems.Add(this.menuItem5);
            this.menuItem1.MenuItems.Add(this.menuItem_waypoint);
            this.menuItem1.MenuItems.Add(this.menuItem_gpsactivity);
            this.menuItem1.MenuItems.Add(this.menuItem_loadtrack);
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
            // menuItem_downloadmaps
            // 
            this.menuItem_downloadmaps.Text = "Download maps";
            this.menuItem_downloadmaps.Click += new System.EventHandler(this.menuItem_downloadmaps_Click);
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
            this.menuItem_gpsactivity.Text = "Start GPS";
            this.menuItem_gpsactivity.Click += new System.EventHandler(this.menuItem_gpsactivity_Click);
            // 
            // menuItem_loadtrack
            // 
            this.menuItem_loadtrack.Text = "Load track...";
            this.menuItem_loadtrack.Click += new System.EventHandler(this.menuItem_loadtrack_Click);
            // 
            // menuItem2
            // 
            this.menuItem2.MenuItems.Add(this.menuItem_config);
            this.menuItem2.MenuItems.Add(this.menuItem3);
            this.menuItem2.MenuItems.Add(this.menuItem_followGPS);
            this.menuItem2.Text = "Options";
            // 
            // menuItem_config
            // 
            this.menuItem_config.Text = "Configuration...";
            this.menuItem_config.Click += new System.EventHandler(this.menuItem_config_Click);
            // 
            // menuItem3
            // 
            this.menuItem3.MenuItems.Add(this.menuItem_map_osm);
            this.menuItem3.MenuItems.Add(this.menuItem_map_gmaps);
            this.menuItem3.Text = "View Map";
            // 
            // menuItem_map_osm
            // 
            this.menuItem_map_osm.Text = "OSM";
            this.menuItem_map_osm.Click += new System.EventHandler(this.menuItem_map_osm_Click);
            // 
            // menuItem_map_gmaps
            // 
            this.menuItem_map_gmaps.Text = "GMaps";
            this.menuItem_map_gmaps.Click += new System.EventHandler(this.menuItem_map_gmaps_Click);
            // 
            // menuItem_followGPS
            // 
            this.menuItem_followGPS.Text = "Follow GPS";
            this.menuItem_followGPS.Click += new System.EventHandler(this.menuItem_followGPS_Click);
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.label1.Font = new System.Drawing.Font("Tahoma", 7F, System.Drawing.FontStyle.Regular);
            this.label1.Location = new System.Drawing.Point(193, 256);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(31, 12);
            this.label1.Text = "Zoom:";
            // 
            // label_zoom
            // 
            this.label_zoom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.label_zoom.Font = new System.Drawing.Font("Tahoma", 7F, System.Drawing.FontStyle.Regular);
            this.label_zoom.Location = new System.Drawing.Point(224, 256);
            this.label_zoom.Name = "label_zoom";
            this.label_zoom.Size = new System.Drawing.Size(17, 12);
            this.label_zoom.Text = "14";
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Tahoma", 7F, System.Drawing.FontStyle.Regular);
            this.label3.Location = new System.Drawing.Point(79, 256);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(22, 12);
            this.label3.Text = "Lon:";
            // 
            // label_lon
            // 
            this.label_lon.Font = new System.Drawing.Font("Tahoma", 7F, System.Drawing.FontStyle.Regular);
            this.label_lon.Location = new System.Drawing.Point(101, 256);
            this.label_lon.Name = "label_lon";
            this.label_lon.Size = new System.Drawing.Size(57, 12);
            this.label_lon.Text = "12,3456789";
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Tahoma", 7F, System.Drawing.FontStyle.Regular);
            this.label2.Location = new System.Drawing.Point(0, 256);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(20, 12);
            this.label2.Text = "Lat:";
            // 
            // label_lat
            // 
            this.label_lat.Font = new System.Drawing.Font("Tahoma", 7F, System.Drawing.FontStyle.Regular);
            this.label_lat.Location = new System.Drawing.Point(22, 256);
            this.label_lat.Name = "label_lat";
            this.label_lat.Size = new System.Drawing.Size(60, 12);
            this.label_lat.Text = "42,1234567";
            // 
            // pb_GPSActvity
            // 
            this.pb_GPSActvity.Location = new System.Drawing.Point(161, 256);
            this.pb_GPSActvity.Name = "pb_GPSActvity";
            this.pb_GPSActvity.Size = new System.Drawing.Size(12, 12);
            // 
            // mapcontrol
            // 
            this.mapcontrol.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
            this.mapcontrol.Location = new System.Drawing.Point(0, 0);
            this.mapcontrol.Map = null;
            this.mapcontrol.Name = "mapcontrol";
            this.mapcontrol.Size = new System.Drawing.Size(240, 256);
            this.mapcontrol.TabIndex = 0;
            this.mapcontrol.Zoom = ((uint)(0u));
            this.mapcontrol.ZoomChanged += new MapsLibrary.MapControl.MapControlEventHandler(this.mapcontrol_ZoomChanged);
            // 
            // Form_MapperToolMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.Controls.Add(this.pb_GPSActvity);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label_zoom);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label_lon);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label_lat);
            this.Controls.Add(this.mapcontrol);
            this.KeyPreview = true;
            this.Menu = this.mainMenu;
            this.Name = "Form_MapperToolMain";
            this.Text = "Pollicino";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form_MapperToolMain_KeyDown);
            this.ResumeLayout(false);

        }

        #endregion

        private MapsLibrary.MapControl mapcontrol;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label_zoom;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label_lon;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label_lat;
        private System.Windows.Forms.PictureBox pb_GPSActvity;
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




    }
}

