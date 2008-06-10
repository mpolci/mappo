namespace MapperTool
{
    partial class GPXControl
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
            this.label_gpx = new System.Windows.Forms.Label();
            this.timerBlinking = new System.Windows.Forms.Timer();
            this.SuspendLayout();
            // 
            // label_gpx
            // 
            this.label_gpx.Font = new System.Drawing.Font("Tahoma", 7F, System.Drawing.FontStyle.Regular);
            this.label_gpx.Location = new System.Drawing.Point(0, 0);
            this.label_gpx.Name = "label_gpx";
            this.label_gpx.Size = new System.Drawing.Size(32, 17);
            this.label_gpx.Text = "GPX";
            // 
            // timerBlinking
            // 
            this.timerBlinking.Interval = 400;
            this.timerBlinking.Tick += new System.EventHandler(this.timerBlinking_Tick);
            // 
            // GPXControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.label_gpx);
            this.ForeColor = System.Drawing.Color.Transparent;
            this.Name = "GPXControl";
            this.Size = new System.Drawing.Size(46, 27);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label_gpx;
        private System.Windows.Forms.Timer timerBlinking;
    }
}
