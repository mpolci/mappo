namespace MapperTool
{
    partial class FormOptions
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
            this.menuItem_ok = new System.Windows.Forms.MenuItem();
            this.menuItem_Cancel = new System.Windows.Forms.MenuItem();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage_GPS = new System.Windows.Forms.TabPage();
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
            this.cb_autodownload = new System.Windows.Forms.CheckBox();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.textBox6 = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox5 = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.tabControl1.SuspendLayout();
            this.tabPage_GPS.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.Add(this.menuItem_ok);
            this.mainMenu1.MenuItems.Add(this.menuItem_Cancel);
            // 
            // menuItem_ok
            // 
            this.menuItem_ok.Text = "Ok";
            // 
            // menuItem_Cancel
            // 
            this.menuItem_Cancel.Text = "Cancel";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage_GPS);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(240, 268);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage_GPS
            // 
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
            // tb_GPSLogPath
            // 
            this.tb_GPSLogPath.Location = new System.Drawing.Point(101, 120);
            this.tb_GPSLogPath.Name = "tb_GPSLogPath";
            this.tb_GPSLogPath.Size = new System.Drawing.Size(112, 21);
            this.tb_GPSLogPath.TabIndex = 11;
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(7, 121);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(90, 20);
            this.label8.Text = "Log path";
            // 
            // button_SelectSimulationFile
            // 
            this.button_SelectSimulationFile.Location = new System.Drawing.Point(216, 93);
            this.button_SelectSimulationFile.Name = "button_SelectSimulationFile";
            this.button_SelectSimulationFile.Size = new System.Drawing.Size(21, 21);
            this.button_SelectSimulationFile.TabIndex = 9;
            this.button_SelectSimulationFile.Text = "...";
            this.button_SelectSimulationFile.Click += new System.EventHandler(this.button_SelectSimulationFile_Click);
            // 
            // tb_SimulationFile
            // 
            this.tb_SimulationFile.Location = new System.Drawing.Point(101, 93);
            this.tb_SimulationFile.Name = "tb_SimulationFile";
            this.tb_SimulationFile.Size = new System.Drawing.Size(112, 21);
            this.tb_SimulationFile.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(7, 94);
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
            this.cb_Simulation.Location = new System.Drawing.Point(7, 67);
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
            this.tabPage2.Controls.Add(this.cb_autodownload);
            this.tabPage2.Controls.Add(this.numericUpDown1);
            this.tabPage2.Controls.Add(this.label7);
            this.tabPage2.Controls.Add(this.label6);
            this.tabPage2.Controls.Add(this.textBox6);
            this.tabPage2.Controls.Add(this.label5);
            this.tabPage2.Controls.Add(this.textBox5);
            this.tabPage2.Controls.Add(this.label4);
            this.tabPage2.Controls.Add(this.textBox4);
            this.tabPage2.Location = new System.Drawing.Point(0, 0);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Size = new System.Drawing.Size(232, 242);
            this.tabPage2.Text = "Maps";
            // 
            // cb_autodownload
            // 
            this.cb_autodownload.Location = new System.Drawing.Point(7, 126);
            this.cb_autodownload.Name = "cb_autodownload";
            this.cb_autodownload.Size = new System.Drawing.Size(226, 20);
            this.cb_autodownload.TabIndex = 11;
            this.cb_autodownload.Text = "Automatic tiles download";
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(184, 101);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.numericUpDown1.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(49, 22);
            this.numericUpDown1.TabIndex = 10;
            this.numericUpDown1.Value = new decimal(new int[] {
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
            this.label6.Location = new System.Drawing.Point(7, 168);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(149, 20);
            this.label6.Text = "GMaps cache path";
            // 
            // textBox6
            // 
            this.textBox6.Location = new System.Drawing.Point(7, 191);
            this.textBox6.Name = "textBox6";
            this.textBox6.Size = new System.Drawing.Size(226, 21);
            this.textBox6.TabIndex = 7;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(7, 51);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(149, 20);
            this.label5.Text = "Tiles cache path";
            // 
            // textBox5
            // 
            this.textBox5.Location = new System.Drawing.Point(7, 74);
            this.textBox5.Name = "textBox5";
            this.textBox5.Size = new System.Drawing.Size(226, 21);
            this.textBox5.TabIndex = 4;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(7, 4);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(149, 20);
            this.label4.Text = "OSM tiles server";
            // 
            // textBox4
            // 
            this.textBox4.Location = new System.Drawing.Point(7, 27);
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(226, 21);
            this.textBox4.TabIndex = 1;
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
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.MenuItem menuItem_ok;
        private System.Windows.Forms.MenuItem menuItem_Cancel;
        private System.Windows.Forms.Button button_SelectSimulationFile;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBox6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox5;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.CheckBox cb_autodownload;
        private System.Windows.Forms.TextBox tb_GPSLogPath;
        private System.Windows.Forms.Label label8;
    }
}