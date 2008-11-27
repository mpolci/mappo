using System;
using System.Text;
using System.Windows.Forms;

namespace MapperTools.Pollicino
{
    public partial class FormGPXDetails : Form
    {
        public FormGPXDetails()
        {
            InitializeComponent();
        }

        private bool mFullDetailMode;
        public bool FullDetailsMode
        {
            set
            {
                panel_ext.Visible = value;
                mFullDetailMode = value;
                if (value)
                {
                    this.Text = "Track details";
                    menuItem_OK.Text = "Close";
                    //menuItem_Cancel.Enabled = false;
                }
                else
                {
                    this.Text = "Upload GPX to OSM";
                }
                // reload info values
                if (gpxf != null)
                    gpxfile = gpxf;
            }
            get
            {
                return mFullDetailMode;
            }
        }

        private GPXFile gpxf;
        public GPXFile gpxfile
        {
            set
            {
                gpxf = value;
                tb_Description.Text = value.Description;
                tb_Tags.Text = value.TagsString;
                cb_public.Checked = value.getPublic();
                PropertiesModified = false;
                // questi sotto, anche se non sempre visibili vengono impostati per semplificare la procedura di controllo dei valori modificati
                cb_flagged.Checked = value.getFlag();
                cb_uploaded.Checked = value.getUploaded();
                // I dati non modificabili vengono impostati solo se visibili
                if (mFullDetailMode)
                {
                    this.Text = System.IO.Path.GetFileName(value.FileName);
                    l_start.Text = (value.StartTime != null) ? value.StartTime.ToString() : "";
                    l_end.Text = (value.EndTime != null) ? value.EndTime.ToString() : "";
                    l_duration.Text = value.Duration.ToString();
                    l_points.Text = value.TrackPoints + " / " + value.WayPoints;

                    System.IO.FileInfo fi = new System.IO.FileInfo(value.FileName);
                    long len = fi.Length;
                    if (len >= 50000)
                        l_filesize.Text = (len / 1024).ToString() + " KB";
                    else
                        l_filesize.Text = len + " B";

                    l_length.Text = value.Length > 0 ? value.Length.ToString() : "";
                }
            }
        }

        public bool PropertiesModified { get; private set; }

        private void menuItem_Ok_Click(object sender, EventArgs e)
        {
            if (tb_Description.Text != gpxf.Description ||
                tb_Tags.Text != gpxf.TagsString ||
                cb_public.Checked != gpxf.getPublic())
            {
                gpxf.Description = tb_Description.Text;
                gpxf.TagsString = tb_Tags.Text;
                gpxf.OSMPublic = cb_public.Checked;                
                PropertiesModified = true;
            }
            if (cb_flagged.Checked != gpxf.getFlag())
            {
                gpxf.Flag = cb_flagged.Checked;
                PropertiesModified = true;
            }
            if (cb_uploaded.Checked != gpxf.getUploaded())
            {
                if (gpxf.OSMId != null)
                    gpxf.OSMId = -(int)gpxf.OSMId;
                else
                    gpxf.OSMId = 1; // id sconosciuto
                PropertiesModified = true;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void menuItem_Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}