using System;
using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
//using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;

namespace MapperTools.Pollicino
{
    public partial class FormTracksManager : Form
    {
        public FormTracksManager()
        {
            InitializeComponent();
        }

        public ApplicationOptions AppOptions { get;  set; }

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
                lwi.Tag = gpxf;
                lwi.ImageIndex = (gpxf.getUploaded()) ? 1 : 0;
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





        private string OSMAPIUploadGPX(string uploadfile, string param_description, string param_tags, bool pblc,
                               string username, string password)
        {
            string uri = "http://www.openstreetmap.org/api/0.5/gpx/create";
            //string uri = "http://127.0.0.1:8080/sviluppo/test_post/test1.php";
            string param_public = pblc ? "1" : "0";
            string boundary = "----------" + DateTime.Now.Ticks.ToString("x");
            NetworkCredential netcred = new NetworkCredential(username, password);
            HttpWebRequest webreq = (HttpWebRequest)WebRequest.Create(uri);
            webreq.Credentials = netcred;
            webreq.PreAuthenticate = true;

            // FIXME: AllowWriteStreamBuffering richiede un grosso uso di memoria
            webreq.AllowWriteStreamBuffering = true;

            if (webreq.ServicePoint != null)
                webreq.ServicePoint.Expect100Continue = false;
            else
                ServicePointManager.FindServicePoint(new Uri(uri)).Expect100Continue = false;

            webreq.ContentType = "multipart/form-data; boundary=" + boundary;
            webreq.Method = "POST";

            string postHeader = prepareMimeHeader_filepart("file", uploadfile, boundary);
            byte[] postHeaderBytes = Encoding.UTF8.GetBytes(postHeader);

            // Build the trailing boundary string as a byte array ensuring the boundary appears on a line by itself
            string postFile = prepareMimeHeader_data("description", boundary) + param_description + "\r\n" +
                              prepareMimeHeader_data("tags", boundary) + param_tags + "\r\n" +
                              prepareMimeHeader_data("public", boundary) + param_public +
                              "\r\n--" + boundary + "--\r\n";
            byte[] postFileBytes = Encoding.ASCII.GetBytes(postFile);

            using (FileStream fileStream = new FileStream(uploadfile, FileMode.Open, FileAccess.Read))
            {
                long length = postHeaderBytes.Length + fileStream.Length + postFileBytes.Length;

                webreq.ContentLength = length;

                using (Stream requestStream = webreq.GetRequestStream())
                {
                    // Write out our post header
                    requestStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);

                    // Write out the file contents
                    byte[] buffer = new Byte[checked((uint)Math.Min(4096,
                                             (int)fileStream.Length))];
                    int bytesRead = 0;
                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                        requestStream.Write(buffer, 0, bytesRead);

                    // Write out the other parameters and boundary
                    requestStream.Write(postFileBytes, 0, postFileBytes.Length);
                }

                // get the response
                using (WebResponse webResponse = webreq.GetResponse())
                using (StreamReader sr = new StreamReader(webResponse.GetResponseStream()))
                    return sr.ReadToEnd().Trim();
            }
        }

        private string prepareMimeHeader_filepart(string paramname, string filename, string boundary)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("--");
            sb.Append(boundary);
            sb.Append("\r\n");
            sb.Append("Content-Disposition: form-data; name=\"");
            sb.Append(paramname);
            sb.Append("\"; filename=\"");
            sb.Append(Path.GetFileName(filename));
            sb.Append("\"");
            sb.Append("\r\n");
            sb.Append("Content-Type: ");
            //sb.Append(contenttype);
            sb.Append("application/octet-stream");
            sb.Append("\r\n");
            sb.Append("\r\n");

            return sb.ToString();
        }

        private string prepareMimeHeader_data(string name, string boundary)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("--");
            sb.Append(boundary);
            sb.Append("\r\n");
            sb.Append("Content-Disposition: form-data; name=\"");
            sb.Append(name);
            sb.Append("\"");
            sb.Append("\r\n");
            sb.Append("\r\n");

            return sb.ToString();
        }

        private void menuItem_Upload_Click(object sender, EventArgs ev)
        {
            System.Diagnostics.Trace.Assert(lw_Tracks.SelectedIndices.Count == 1, "Numero elementi selezionati non valido: " + lw_Tracks.SelectedIndices.Count);

            if (AppOptions == null || string.IsNullOrEmpty(AppOptions.Application.OSMUsername))
            {
                MessageBox.Show("OSM username and password not configured");
                return;
            }

            string username = AppOptions.Application.OSMUsername;
            string password = AppOptions.Application.OSMPassword;

            int selected = lw_Tracks.SelectedIndices[0];
            GPXFile gpxf = (GPXFile)lw_Tracks.Items[selected].Tag;

            string msg = "This track is already uploaded. Do you want to continue?";
            if (gpxf.getUploaded() &&
                MessageBox.Show(msg, "", MessageBoxButtons.YesNo, MessageBoxIcon.None, MessageBoxDefaultButton.Button2) == DialogResult.No)
            {
                return;
            }

            FormGPXDetails propertiesdlg = new FormGPXDetails();
            propertiesdlg.FullDetailsMode = false;
            propertiesdlg.gpxfile = gpxf;

            if (propertiesdlg.ShowDialog() == DialogResult.OK)
            {
                bool updatedb = propertiesdlg.PropertiesModified;
                try
                {
                    string filename = gpxf.FileName;

                    string s_id = OSMAPIUploadGPX(filename, gpxf.Description, gpxf.TagsString, gpxf.getPublic(),
                                                  username, password);
                    try { 
                        int id = int.Parse(s_id);
                        MessageBox.Show("Successfull uploaded");
                        gpxf.OSMId = id;
                        //gpxf.UploadedToOSM = true;
                        updatedb = true;
                        lw_Tracks.Items[selected].ImageIndex = 1;
                    }
                    catch (Exception e) 
                    {
                        throw new Exception("OSM Server returns error: " + s_id, e);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine("GPX Upload error - " + ex);
                    #if DEBUG
                    MessageBox.Show(ex.Message, "Upload Error",
                       MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                    #else 
                    MessageBox.Show("Upload Error");
                    #endif
                }
                if (updatedb)
                   mTracks.SaveCollection();
            }
        }

        private void menuItem_Info_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Trace.Assert(lw_Tracks.SelectedIndices.Count == 1, "Numero elementi selezionati non valido: " + lw_Tracks.SelectedIndices.Count);
                int selected = lw_Tracks.SelectedIndices[0];
                GPXFile gpxf = (GPXFile)lw_Tracks.Items[selected].Tag;

            FormGPXDetails propertiesdlg = new FormGPXDetails();
            propertiesdlg.FullDetailsMode = true;
            propertiesdlg.gpxfile = gpxf;

            if (propertiesdlg.ShowDialog() == DialogResult.OK && propertiesdlg.PropertiesModified)
            {
                mTracks.SaveCollection();
            }
        }

        private void menuItem_ImportGPX_Click(object sender, EventArgs e)
        {

        }
    }
}