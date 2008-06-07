/*******************************************************************************
 *  Pollicino - A tool for gps mapping.
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
//using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace MapperTool
{
    public class ApplicationOptions
    {
        public struct GPSOptions
        {
            public string PortName;
            public int PortSpeed;
            public bool Simulation;
            public string SimulationFile;
            public string LogsDir;
        }
        public struct MapsOptions
        {
            public struct OSMOptions {
                public string OSMTileServer;
                public string TileCachePath;
                public int DownloadDepth;
            }
            public struct GMAPSOptions {
                public string CachePath;
            }

            public OSMOptions OSM;
            public GMAPSOptions GMaps;
            public bool AutoDownload;
        }
        public struct InterfaceOptions
        {
            public bool WaypointSoundPlay;
            public string WaypointSoundFile;
            public bool WaypointRecordAudio;
            public int WaypointRecordAudioSeconds;
            public int RecordAudioDevice;
            public OpenNETCF.Media.WaveAudio.SoundFormats RecordAudioFormat;
            public bool AutoCentreMap;
            public MapsLibrary.GeoPoint InitialMapPosition;
        }

        public GPSOptions GPS;
        public MapsOptions Maps;
        public InterfaceOptions Application;

        public void SaveToFile(string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ApplicationOptions));
#if DEBUG
            using (TextWriter wdebug = new StreamWriter(filename + ".txt"))
            {
                serializer.Serialize(wdebug, this);
                wdebug.Close();
            }
#endif
            using (TextWriter writer = new StreamWriter(filename))
            {
                serializer.Serialize(writer, this);            
                writer.Close();
            }
        }

        public static ApplicationOptions FromFile(string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ApplicationOptions));
            ApplicationOptions opts;
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                opts = (ApplicationOptions)serializer.Deserialize(fs);
                fs.Close();
            }
            return opts;
        }

        public ApplicationOptions()
        {
            GPS.PortName = "";
            GPS.SimulationFile = "";
            GPS.LogsDir = "";
            Maps.OSM.OSMTileServer = "";
            Maps.OSM.TileCachePath = "";
            Maps.GMaps.CachePath = "";
            Application.WaypointSoundFile = "";
        }
    }
}
