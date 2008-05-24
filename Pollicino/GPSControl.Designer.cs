namespace MapperTool
{
    partial class GPSControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pb_GPSActvity = new System.Windows.Forms.PictureBox();
            this.SuspendLayout();
            // 
            // pb_GPSActvity
            // 
            this.pb_GPSActvity.Location = new System.Drawing.Point(0, 0);
            this.pb_GPSActvity.Name = "pb_GPSActvity";
            this.pb_GPSActvity.Size = new System.Drawing.Size(12, 12);
            // 
            // GPSControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.pb_GPSActvity);
            this.Name = "GPSControl";
            this.Size = new System.Drawing.Size(25, 24);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pb_GPSActvity;
    }
}
