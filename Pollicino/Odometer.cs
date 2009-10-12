using System;
using System.Text;

namespace MapperTools.Pollicino
{
    public class Odometer
    {
        MapsLibrary.GeoPoint? mLast;

        public Odometer()
        {
            mLast = null;
            distance = 0;
        }

        public double distance { get; set; }

        //public void start()
        //{
        //}

        public void stop()
        {
            mLast = null;
        }

        public void HandleGPSEvent(GPSControl.GPSPosition gpsdata)
        {
            double d = 0;
            if (mLast == null)
                mLast = gpsdata.position;
            else
            {
                distance += mLast.Value.Distance(gpsdata.position);
                mLast = gpsdata.position;
            }
        }
        override public string ToString()
        {
            double d = distance;
            string format;
            if (d > 2000)
            {
                d /= 1000;
                format = "0.0 km";
            }
            else
                format = "0 m";
            return d.ToString(format);
        }
    }
}
