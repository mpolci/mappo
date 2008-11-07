using System;
using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
//using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace MapperTools.Pollicino
{
    public partial class FormTracksManager : Form
    {
        public FormTracksManager()
        {
            InitializeComponent();
        }

        private GPXCollection mTracks;
        public GPXCollection TracksCollection
        {
            get { return mTracks; }
            set
            {
                mTracks = value;
                UpdateList();
            }
        }

        public string SelectedTrackFileName
        {
            get
            {
                System.Diagnostics.Debug.Assert(lw_Tracks.SelectedIndices.Count == 1, "Numero elementi selezionati non valido: " + lw_Tracks.SelectedIndices.Count);
                return ((GPXFile)lw_Tracks.FocusedItem.Tag).FileName;
            }
        }
                

        private void UpdateList()
        {
            lw_Tracks.Items.Clear();
            if (mTracks == null) return;
            int idx = 0;
            foreach (GPXFile gpxf in mTracks.Items)
            {
                string name = gpxf.Name;
                if (name.StartsWith("gpslog_")) 
                    name = name.Substring(7);  // remove "gpslog_"
                ListViewItem lwi = new ListViewItem(name);
                //lwi.Tag = gpxf.FileName;
                lwi.Tag = gpxf;
                lwi.ImageIndex = (gpxf.UploadedToOSM) ? 1 : 0;
                // Durata della traccia
                TimeSpan d = gpxf.Duration;
                string duration;
                if (d.TotalSeconds > 120)
                    duration = string.Format("{0:D}:{1:D2}", (int)d.TotalHours, d.Minutes);
                else
                    duration = string.Format("{0:D}s", (int)d.TotalSeconds);
                //string duration = gpxf.Duration.ToString();
                //if (duration[0] == '0') duration = duration.Substring(1);  // trasform 01:23:45 to 1:23:45
                lwi.SubItems.Add(duration);
                lwi.SubItems.Add(gpxf.TrackPoints.ToString());
                lwi.SubItems.Add(gpxf.WayPoints.ToString());

                lw_Tracks.Items.Add(lwi);
                idx++;
            }
        }

        private void menuItem_Close_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void menuItem_LoadTrack_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void menuItem_delete_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Trace.Assert(lw_Tracks.SelectedIndices.Count == 1, "Numero elementi selezionati non valido: " + lw_Tracks.SelectedIndices.Count);
            try
            {
                int selected = lw_Tracks.SelectedIndices[0];
                GPXFile gpxf = (GPXFile)lw_Tracks.Items[selected].Tag;
                string file = gpxf.FileName;
                if (MessageBox.Show("Delete " + Path.GetFileName(file) + " and associated data?", "Warning!", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                {
                    File.Delete(file);
                    string datadir = WaypointNames.DataDir(file);
                    if (Directory.Exists(datadir))
                        Directory.Delete(datadir, true);
                    lw_Tracks.Items.RemoveAt(selected);
                    mTracks.Remove(gpxf);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Delete error");
            }
        }

        private void lw_Tracks_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lw_Tracks.SelectedIndices.Count == 0)
                lw_Tracks.ContextMenu = null;
            else
                lw_Tracks.ContextMenu = contextMenu_track;
        }
    }
}