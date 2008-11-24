namespace MapperTools.Pollicino
{
    partial class FormOSMUploadGPX
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
            this.mainMenu1 = new System.Windows.Forms.MainMenu();
            this.menuItem_Upload = new System.Windows.Forms.MenuItem();
            this.menuItem_Cancel = new System.Windows.Forms.MenuItem();
            this.label1 = new System.Windows.Forms.Label();
            this.tb_Description = new System.Windows.Forms.TextBox();
            this.tb_Tags = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cb_public = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.Add(this.menuItem_Upload);
            this.mainMenu1.MenuItems.Add(this.menuItem_Cancel);
            // 
            // menuItem_Upload
            // 
            this.menuItem_Upload.Text = "Upload";
            this.menuItem_Upload.Click += new System.EventHandler(this.menuItem_Upload_Click);
            // 
            // menuItem_Cancel
            // 
            this.menuItem_Cancel.Text = "Cancel";
            this.menuItem_Cancel.Click += new System.EventHandler(this.menuItem_Cancel_Click);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(3, 7);
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
            this.label2.Location = new System.Drawing.Point(3, 54);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 20);
            this.label2.Text = "Tags";
            // 
            // cb_public
            // 
            this.cb_public.Location = new System.Drawing.Point(3, 104);
            this.cb_public.Name = "cb_public";
            this.cb_public.Size = new System.Drawing.Size(100, 20);
            this.cb_public.TabIndex = 5;
            this.cb_public.Text = "Public";
            // 
            // FormOSMUploadGPX
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.Controls.Add(this.cb_public);
            this.Controls.Add(this.tb_Tags);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tb_Description);
            this.Controls.Add(this.label1);
            this.Menu = this.mainMenu1;
            this.Name = "FormOSMUploadGPX";
            this.Text = "Upload GPX to OSM";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.MenuItem menuItem_Upload;
        private System.Windows.Forms.MenuItem menuItem_Cancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tb_Description;
        private System.Windows.Forms.TextBox tb_Tags;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox cb_public;
    }
}