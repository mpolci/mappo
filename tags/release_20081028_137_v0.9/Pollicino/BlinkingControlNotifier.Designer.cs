namespace MapperTools.Pollicino
{
    partial class BlinkingControlNotifier
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
            this.timerBlinking = new System.Windows.Forms.Timer();
            // 
            // timerBlinking
            // 
            this.timerBlinking.Interval = 500;
            this.timerBlinking.Tick += new System.EventHandler(this.timerBlinking_Tick);

        }

        #endregion

        private System.Windows.Forms.Timer timerBlinking;
    }
}
