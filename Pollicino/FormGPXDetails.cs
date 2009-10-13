/*******************************************************************************
 *  Mappo! - A tool for gps mapping.
 *  Copyright (C) 2008  Marco Polci
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see http://www.gnu.org/licenses/gpl.html.
 * 
 ******************************************************************************/

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
                cb_visibility.Text = value.OSMVisibility;
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
                cb_visibility.Text  != gpxf.OSMVisibility)
            {
                gpxf.Description = tb_Description.Text;
                gpxf.TagsString = tb_Tags.Text;
                gpxf.OSMVisibility = cb_visibility.Text;
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