using System;
using System.Text;
using System.Windows.Forms;

namespace MapperTools.Pollicino
{
    public partial class FormOSMUploadGPX : Form
    {
        public FormOSMUploadGPX()
        {
            InitializeComponent();
        }

        private GPXFile gpxf;
        public GPXFile gpxfile
        {
            set
            {
                gpxf = value;
                tb_Description.Text = value.Description;
                tb_Tags.Text = value.TagsString;
                cb_public.Checked = value.Public;
                PropertiesModified = false;
            }
        }

        public bool PropertiesModified { get; private set; }

        private void menuItem_Upload_Click(object sender, EventArgs e)
        {
            if (tb_Description.Text != gpxf.Description ||
                tb_Tags.Text != gpxf.TagsString ||
                cb_public.Checked != (bool)gpxf.Public)
            {
                gpxf.Description = tb_Description.Text;
                gpxf.TagsString = tb_Tags.Text;
                gpxf.OSMPublic = cb_public.Checked;
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