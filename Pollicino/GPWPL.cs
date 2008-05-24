using System;
using SharpGis.SharpGps.NMEA;
using System.Text;

namespace MapperTool
{
    class GPWPL
    {
        string _name;
        double _lat, _lon;
        string _nmeasentence;

        public string NMEASentence
        {
            get
            {
                return _nmeasentence;
            }
        }
        public double latitude
        {
            get
            {
                return _lat;
            }
        }
        public double longitude
        {
            get
            {
                return _lon;
            }
        }
        public string name
        {
            get
            {
                return _name;
            }
        }
    

        public GPWPL(string nmea_sentence)
        {
            string[] split = nmea_sentence.Split(new Char[] { ',' });
            // To do: check the checksum validity

            _lat = GPSToDecimalDegrees(split[1], split[2]);
            _lon = GPSToDecimalDegrees(split[3], split[4]);
            _name = split[5].Trim();
            if (_name[_name.Length - 3] == '*')
                _name = _name.Substring(0, _name.Length - 3);
            _nmeasentence = nmea_sentence;
        }

        public GPWPL(string name, double lat, double lon)
        {
            // Mancano i controlli dei valori corretti

            _lat = lat;
            _lon = lon;
            _name = name;

            string NordSud = (lat > 0) ? ",N," : ",S,",
                   EstOvest = (lon > 0) ? ",E," : ",W,";
            lat = Math.Abs(lat);
            lon = Math.Abs(lon);
            int latdeg = (int)Math.Floor(lat),
                londeg = (int)Math.Floor(lon);
            lat = (lat - latdeg) * 60;
            lon = (lon - londeg) * 60;

            string sentence = "$GPWPL," + latdeg.ToString("D2") + lat.ToString("00.#######", numberFormat_EnUS) + NordSud
                            + londeg.ToString("D3") + lon.ToString("00.#######", numberFormat_EnUS) + EstOvest
                            + name + '*';

            _nmeasentence = sentence + getNMEAChecksum(sentence);
        }

        internal static System.Globalization.NumberFormatInfo numberFormat_EnUS = new System.Globalization.CultureInfo("en-US", false).NumberFormat;

        /// <summary>
        /// Converts GPS position in d"dd.ddd' to decimal degrees ddd.ddddd
        /// </summary>
        /// <remarks>Code from  SharpGPS <see cref="http://www.codeplex.com/SharpGPS"/>.</remarks>
        /// <param name="DM"></param>
        /// <param name="Dir"></param>
        /// <returns></returns>
        internal static double GPSToDecimalDegrees(string DM, string Dir)
        {
            try
            {
                if (DM == "" || Dir == "")
                {
                    return 0.0;
                }
                //Get the fractional part of minutes
                //DM = '5512.45',  Dir='N'
                //DM = '12311.12', Dir='E'

                string t = DM.Substring(DM.IndexOf("."));
                double FM = double.Parse(DM.Substring(DM.IndexOf(".")), numberFormat_EnUS);

                //Get the minutes.
                t = DM.Substring(DM.IndexOf(".") - 2, 2);
                double Min = double.Parse(DM.Substring(DM.IndexOf(".") - 2, 2), numberFormat_EnUS);

                //Degrees
                t = DM.Substring(0, DM.IndexOf(".") - 2);
                double Deg = double.Parse(DM.Substring(0, DM.IndexOf(".") - 2), numberFormat_EnUS);

                if (Dir == "S" || Dir == "W")
                    Deg = -(Deg + (Min + FM) / 60);
                else
                    Deg = Deg + (Min + FM) / 60;
                return Deg;
            }
            catch
            {
                return 0.0;
            }
        }

        // Calculates the checksum for a sentence
        private static string getNMEAChecksum(string sentence)
        {
            //start with first Item
            int checksum = Convert.ToByte(sentence[sentence.IndexOf('$') + 1]);
            // Loop through all chars to get a checksum
            for (int i = sentence.IndexOf('$') + 2; i < sentence.IndexOf('*'); i++)
            {
                // No. XOR the checksum with this character's value
                checksum ^= Convert.ToByte(sentence[i]);
            }
            // Return the checksum formatted as a two-character hexadecimal
            return checksum.ToString("X2");
        }

    }
}
