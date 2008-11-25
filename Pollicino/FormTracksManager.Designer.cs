namespace MapperTools.Pollicino
{
    partial class FormTracksManager
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.MainMenu mainMenu1;

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
            System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormTracksManager));
            this.mainMenu1 = new System.Windows.Forms.MainMenu();
            this.menuItem_Close = new System.Windows.Forms.MenuItem();
            this.menuItem_LoadTrack = new System.Windows.Forms.MenuItem();
            this.lw_Tracks = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
            this.imglist_gpxstatus = new System.Windows.Forms.ImageList();
            this.contextMenu_track = new System.Windows.Forms.ContextMenu();
            this.menuItem_Upload = new System.Windows.Forms.MenuItem();
            this.menuItem_delete = new System.Windows.Forms.MenuItem();
            this.menuItem_Info = new System.Windows.Forms.MenuItem();
            this.SuspendLayout();
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.Add(this.menuItem_Close);
            this.mainMenu1.MenuItems.Add(this.menuItem_LoadTrack);
            // 
            // menuItem_Close
            // 
            this.menuItem_Close.Text = "Close";
            this.menuItem_Close.Click += new System.EventHandler(this.menuItem_Close_Click);
            // 
            // menuItem_LoadTrack
            // 
            this.menuItem_LoadTrack.Text = "Load Track";
            this.menuItem_LoadTrack.Click += new System.EventHandler(this.menuItem_LoadTrack_Click);
            // 
            // lw_Tracks
            // 
            this.lw_Tracks.Columns.Add(this.columnHeader1);
            this.lw_Tracks.Columns.Add(this.columnHeader2);
            this.lw_Tracks.Columns.Add(this.columnHeader3);
            this.lw_Tracks.Columns.Add(this.columnHeader4);
            this.lw_Tracks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lw_Tracks.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Regular);
            this.lw_Tracks.FullRowSelect = true;
            this.lw_Tracks.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            listViewItem2.ImageIndex = 0;
            listViewItem2.Tag = "gpslog_20081105_1134.gpx";
            listViewItem2.Text = "2008-11-05_113400";
            listViewItem2.SubItems.Add("3:48");
            listViewItem2.SubItems.Add("12345");
            listViewItem2.SubItems.Add("23");
            this.lw_Tracks.Items.Add(listViewItem2);
            this.lw_Tracks.Location = new System.Drawing.Point(0, 0);
            this.lw_Tracks.Name = "lw_Tracks";
            this.lw_Tracks.Size = new System.Drawing.Size(240, 268);
            this.lw_Tracks.SmallImageList = this.imglist_gpxstatus;
            this.lw_Tracks.TabIndex = 0;
            this.lw_Tracks.View = System.Windows.Forms.View.Details;
            this.lw_Tracks.SelectedIndexChanged += new System.EventHandler(this.lw_Tracks_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Name";
            this.columnHeader1.Width = 115;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Len";
            this.columnHeader2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader2.Width = 37;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "TP";
            this.columnHeader3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader3.Width = 42;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "WP";
            this.columnHeader4.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader4.Width = 30;
            this.imglist_gpxstatus.Images.Clear();
            this.imglist_gpxstatus.Images.Add(((System.Drawing.Image)(resources.GetObject("resource"))));
            this.imglist_gpxstatus.Images.Add(((System.Drawing.Image)(resources.GetObject("resource1"))));
            // 
            // contextMenu_track
            // 
            this.contextMenu_track.MenuItems.Add(this.menuItem_Upload);
            this.contextMenu_track.MenuItems.Add(this.menuItem_delete);
            this.contextMenu_track.MenuItems.Add(this.menuItem_Info);
            // 
            // menuItem_Upload
            // 
            this.menuItem_Upload.Text = "Upload to OSM ...";
            this.menuItem_Upload.Click += new System.EventHandler(this.menuItem_Upload_Click);
            // 
            // menuItem_delete
            // 
            this.menuItem_delete.Text = "Delete track";
            this.menuItem_delete.Click += new System.EventHandler(this.menuItem_delete_Click);
            // 
            // menuItem_Info
            // 
            this.menuItem_Info.Text = "Track Info ...";
            this.menuItem_Info.Click += new System.EventHandler(this.menuItem_Info_Click);
            // 
            // FormTracksManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.Controls.Add(this.lw_Tracks);
            this.Menu = this.mainMenu1;
            this.Name = "FormTracksManager";
            this.Text = "Tracks Manager";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lw_Tracks;
        private System.Windows.Forms.ContextMenu contextMenu_track;
        private System.Windows.Forms.MenuItem menuItem_Upload;
        private System.Windows.Forms.MenuItem menuItem_delete;
        private System.Windows.Forms.MenuItem menuItem_Info;
        private System.Windows.Forms.MenuItem menuItem_Close;
        private System.Windows.Forms.MenuItem menuItem_LoadTrack;
        private System.Windows.Forms.ImageList imglist_gpxstatus;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
    }
}