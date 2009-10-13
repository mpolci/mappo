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
using System.Net;
using System.IO;
using System.Xml.Serialization;
using System.Globalization;

namespace GMapsDataAPI
{
    public class GMAPSService
    {
        private static string uriLogin = "https://www.google.com/accounts/ClientLogin";

        /// <summary>
        /// Login to Google Maps and create a GMAPSService object.
        /// </summary>
        /// <param name="email">Google account (email)</param>
        /// <param name="password">Password</param>
        /// <returns></returns>
        /// <exception cref="System.Net.WebException"></exception>
        public static GMAPSService Login(string email, string password)
        {
            HttpWebRequest webreq = (HttpWebRequest)WebRequest.Create(uriLogin);
            
            // FIXME: AllowWriteStreamBuffering richiede un grosso uso di memoria
            webreq.AllowWriteStreamBuffering = true;

            if (webreq.ServicePoint != null)
                webreq.ServicePoint.Expect100Continue = false;
            else
                ServicePointManager.FindServicePoint(new Uri(uriLogin)).Expect100Continue = false;
            
            webreq.ContentType = "application/x-www-form-urlencoded";
            webreq.Method = "POST";

            string postContent = "accountType=GOOGLE&Email=" + email + "&Passwd=" + password + "&service=local&source=test_app";
            byte[] postContentBytes = Encoding.ASCII.GetBytes(postContent);

            //webreq.ContentLength = postContentBytes.Length;
            using (Stream requestStream = webreq.GetRequestStream())
                requestStream.Write(postContentBytes, 0, postContentBytes.Length);

            // get the response
            using (WebResponse webResponse = webreq.GetResponse())
            using (StreamReader sr = new StreamReader(webResponse.GetResponseStream()))
                while (sr.Peek() >= 0)
                {
                    string result_item = sr.ReadLine().Trim();
                    string[] data = result_item.Split(new char[] { '=' });
                    if (data.Length == 2 && data[0] == "Auth")
                        return new GMAPSService(data[1]);
                }
            return null;
        }

        private GMAPSService(string authcode)
        {
            Authorization = authcode;
        }

        public string Authorization { get; private set; }

        public string AuthorizationHeaderValue { get { return "GoogleLogin auth=\"" + Authorization + '"'; } }

    }

    public class GMAPSMap {
        public string id { get; private set; }
        public GMAPSService service { get; private set; }

        private static string uriCreateMap = "http://maps.google.com/maps/feeds/maps/default/full";

        public GMAPSMap( string name, string description, GMAPSService srv)
        {
            service = srv;

            entry e = new entry(name, description);

            HttpWebRequest webreq = (HttpWebRequest)WebRequest.Create(uriCreateMap);
            webreq.ContentType = "application/atom+xml";
            webreq.Method = "POST";
            webreq.Headers.Add("Authorization", srv.AuthorizationHeaderValue);

            // FIXME: AllowWriteStreamBuffering richiede un grosso uso di memoria
            webreq.AllowWriteStreamBuffering = true;

            if (webreq.ServicePoint != null)
                webreq.ServicePoint.Expect100Continue = false;
            else
                ServicePointManager.FindServicePoint(new Uri(uriCreateMap)).Expect100Continue = false;

            XmlSerializer serializer = new XmlSerializer(typeof(entry), entry.xmlns);
            // send request
            using (Stream requestStream = webreq.GetRequestStream()) {
                serializer.Serialize(requestStream, e);
            }
            // get the response
            using (WebResponse webResponse = webreq.GetResponse())
            //using (StreamReader sr = new StreamReader(webResponse.GetResponseStream()))
            {
                e = (entry) serializer.Deserialize(webResponse.GetResponseStream());
            }
            id = e.id;
        }


        public void AddPoint(double lat, double lon, double ele, string name)
        {
            string c1 = @"<atom:entry xmlns='http://www.opengis.net/kml/2.2' xmlns:atom='http://www.w3.org/2005/Atom'> 
<atom:title type='text'>";
            string c2 = @"</atom:title>
   <atom:content type='application/vnd.google-earth.kml+xml'>
     <Placemark>
       <name>";
            string c3 = @"</name>
       <description/>
       <Point>
         <coordinates>";
            string c4 = @"</coordinates>
       </Point>
     </Placemark>
   </atom:content>
</atom:entry>";
            //CultureInfo ci = CultureInfo.InvariantCulture new CultureInfo("en-us");

            string postContent = c1 + name + c2 + name + c3 + lon.ToString(CultureInfo.InvariantCulture) + ',' + lat.ToString(CultureInfo.InvariantCulture) + ',' + ele.ToString(CultureInfo.InvariantCulture) + c4;
            int i = id.LastIndexOf("maps");
            string uri = id.Substring(0, i) + "features" + id.Substring(i + 4) + "/full";

            HttpWebRequest webreq = (HttpWebRequest)WebRequest.Create(uri);
            webreq.ContentType = "application/atom+xml";
            webreq.Method = "POST";
            webreq.Headers.Add("Authorization", service.AuthorizationHeaderValue);

            // FIXME: AllowWriteStreamBuffering richiede un grosso uso di memoria
            webreq.AllowWriteStreamBuffering = true;

            if (webreq.ServicePoint != null)
                webreq.ServicePoint.Expect100Continue = false;
            else
                ServicePointManager.FindServicePoint(new Uri(uri)).Expect100Continue = false;

            XmlSerializer serializer = new XmlSerializer(typeof(entry), entry.xmlns);
            // send request
            using (Stream requestStream = webreq.GetRequestStream())
            using (StreamWriter sw = new StreamWriter(requestStream))
            {
                sw.Write(postContent);
            }
            // get the response
            using (WebResponse webResponse = webreq.GetResponse())
            using (StreamReader sr = new StreamReader(webResponse.GetResponseStream()))
            {
                string result = sr.ReadToEnd();
            }


        }
    }

    [Serializable]
    public class entry
    {
        public entry() { idSpecified = false; }
        public entry(string t, string s)
        {
            title = t;
            summary = s;
            idSpecified = false;
        }

        [XmlIgnore]
        public const string xmlns = "http://www.w3.org/2005/Atom";

        public string title;
        public string summary;

        public string id { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idSpecified  { get; set; }
    }


}
