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
