namespace MapperTools.Pollicino
{
    partial class FormGPXDetails
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.MainMenu mainMenu_upload;

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
            this.mainMenu_upload = new System.Windows.Forms.MainMenu();
            this.menuItem_OK = new System.Windows.Forms.MenuItem();
            this.menuItem_Cancel = new System.Windows.Forms.MenuItem();
            this.label1 = new System.Windows.Forms.Label();
            this.tb_Description = new System.Windows.Forms.TextBox();
            this.tb_Tags = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.panel_ext = new System.Windows.Forms.Panel();
            this.l_filesize = new System.Windows.Forms.Label();
            this.l_points = new System.Windows.Forms.Label();
            this.l_length = new System.Windows.Forms.Label();
            this.l_duration = new System.Windows.Forms.Label();
            this.l_end = new System.Windows.Forms.Label();
            this.l_start = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.cb_flagged = new System.Windows.Forms.CheckBox();
            this.cb_uploaded = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.cb_public = new System.Windows.Forms.CheckBox();
            this.panel_ext.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainMenu_upload
            // 
            this.mainMenu_upload.MenuItems.Add(this.menuItem_Cancel);
            this.mainMenu_upload.MenuItems.Add(this.menuItem_OK);
            // 
            // menuItem_OK
            // 
            this.menuItem_OK.Text = "Upload";
            this.menuItem_OK.Click += new System.EventHandler(this.menuItem_Ok_Click);
            // 
            // menuItem_Cancel
            // 
            this.menuItem_Cancel.Text = "Cancel";
            this.menuItem_Cancel.Click += new System.EventHandler(this.menuItem_Cancel_Click);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(3, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 20);
            this.label1.Text = "Description";
            // 
            // tb_Description
            // 
            this.tb_Description.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tb_Description.Location = new System.Drawing.Point(3, 30);
            this.tb_Description.Name = "tb_Description";
            this.tb_Description.Size = new System.Drawing.Size(234, 21);
            this.tb_Description.TabIndex = 1;
            // 
            // tb_Tags
            // 
            this.tb_Tags.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tb_Tags.Location = new System.Drawing.Point(3, 77);
            this.tb_Tags.Name = "tb_Tags";
            this.tb_Tags.Size = new System.Drawing.Size(234, 21);
            this.tb_Tags.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(3, 56);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 20);
            this.label2.Text = "Tags";
            // 
            // panel_ext
            // 
            this.panel_ext.Controls.Add(this.l_filesize);
            this.panel_ext.Controls.Add(this.l_points);
            this.panel_ext.Controls.Add(this.l_length);
            this.panel_ext.Controls.Add(this.l_duration);
            this.panel_ext.Controls.Add(this.l_end);
            this.panel_ext.Controls.Add(this.l_start);
            this.panel_ext.Controls.Add(this.label8);
            this.panel_ext.Controls.Add(this.label7);
            this.panel_ext.Controls.Add(this.cb_flagged);
            this.panel_ext.Controls.Add(this.cb_uploaded);
            this.panel_ext.Controls.Add(this.label6);
            this.panel_ext.Controls.Add(this.label5);
            this.panel_ext.Controls.Add(this.label4);
            this.panel_ext.Controls.Add(this.label3);
            this.panel_ext.Location = new System.Drawing.Point(0, 130);
            this.panel_ext.Name = "panel_ext";
            this.panel_ext.Size = new System.Drawing.Size(240, 138);
            // 
            // l_filesize
            // 
            this.l_filesize.ForeColor = System.Drawing.SystemColors.Highlight;
            this.l_filesize.Location = new System.Drawing.Point(148, 100);
            this.l_filesize.Name = "l_filesize";
            this.l_filesize.Size = new System.Drawing.Size(89, 20);
            this.l_filesize.Text = "1548 KB";
            this.l_filesize.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // l_points
            // 
            this.l_points.ForeColor = System.Drawing.SystemColors.Highlight;
            this.l_points.Location = new System.Drawing.Point(148, 80);
            this.l_points.Name = "l_points";
            this.l_points.Size = new System.Drawing.Size(89, 20);
            this.l_points.Text = "190243 / 123";
            this.l_points.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // l_length
            // 
            this.l_length.ForeColor = System.Drawing.SystemColors.Highlight;
            this.l_length.Location = new System.Drawing.Point(92, 60);
            this.l_length.Name = "l_length";
            this.l_length.Size = new System.Drawing.Size(145, 20);
            this.l_length.Text = "1540 m";
            this.l_length.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // l_duration
            // 
            this.l_duration.ForeColor = System.Drawing.SystemColors.Highlight;
            this.l_duration.Location = new System.Drawing.Point(75, 40);
            this.l_duration.Name = "l_duration";
            this.l_duration.Size = new System.Drawing.Size(162, 20);
            this.l_duration.Text = "60:23";
            this.l_duration.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // l_end
            // 
            this.l_end.ForeColor = System.Drawing.SystemColors.Highlight;
            this.l_end.Location = new System.Drawing.Point(75, 20);
            this.l_end.Name = "l_end";
            this.l_end.Size = new System.Drawing.Size(162, 20);
            this.l_end.Text = "10/12/2008 11:23:00";
            this.l_end.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // l_start
            // 
            this.l_start.ForeColor = System.Drawing.SystemColors.Highlight;
            this.l_start.Location = new System.Drawing.Point(75, 0);
            this.l_start.Name = "l_start";
            this.l_start.Size = new System.Drawing.Size(162, 20);
            this.l_start.Text = "10/12/2008 10:29:14";
            this.l_start.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(3, 100);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(125, 20);
            this.label8.Text = "File size:";
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(3, 80);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(154, 20);
            this.label7.Text = "Track points / Waypoints:";
            // 
            // cb_flagged
            // 
            this.cb_flagged.Location = new System.Drawing.Point(148, 118);
            this.cb_flagged.Name = "cb_flagged";
            this.cb_flagged.Size = new System.Drawing.Size(89, 20);
            this.cb_flagged.TabIndex = 25;
            this.cb_flagged.Text = "Flagged";
            // 
            // cb_uploaded
            // 
            this.cb_uploaded.Location = new System.Drawing.Point(3, 118);
            this.cb_uploaded.Name = "cb_uploaded";
            this.cb_uploaded.Size = new System.Drawing.Size(129, 20);
            this.cb_uploaded.TabIndex = 24;
            this.cb_uploaded.Text = "Uploaded to OSM";
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(3, 60);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(100, 20);
            this.label6.Text = "Track length:";
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(3, 40);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(100, 20);
            this.label5.Text = "Total time:";
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(3, 20);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(100, 20);
            this.label4.Text = "End time:";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(3, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(100, 20);
            this.label3.Text = "Start time:";
            // 
            // cb_public
            // 
            this.cb_public.Location = new System.Drawing.Point(3, 104);
            this.cb_public.Name = "cb_public";
            this.cb_public.Size = new System.Drawing.Size(237, 20);
            this.cb_public.TabIndex = 24;
            this.cb_public.Text = "Public (OpenStreetMap)";
            // 
            // FormGPXDetails
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.ControlBox = false;
            this.Controls.Add(this.cb_public);
            this.Controls.Add(this.panel_ext);
            this.Controls.Add(this.tb_Tags);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tb_Description);
            this.Controls.Add(this.label1);
            this.Menu = this.mainMenu_upload;
            this.Name = "FormGPXDetails";
            this.Text = "Upload GPX to OSM";
            this.panel_ext.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.MenuItem menuItem_OK;
        private System.Windows.Forms.MenuItem menuItem_Cancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tb_Description;
        private System.Windows.Forms.TextBox tb_Tags;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panel_ext;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.CheckBox cb_flagged;
        private System.Windows.Forms.CheckBox cb_uploaded;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label l_filesize;
        private System.Windows.Forms.Label l_points;
        private System.Windows.Forms.Label l_length;
        private System.Windows.Forms.Label l_duration;
        private System.Windows.Forms.Label l_end;
        private System.Windows.Forms.Label l_start;
        private System.Windows.Forms.CheckBox cb_public;
    }
}