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
using System.Collections.Generic;
using System.Text;

namespace MapsLibrary
{
    public class GoogleMapsTileMapSystem : TileMapSystem
    {
        private static int serveridx = 0;
        private const string urlFormatString = "http://mt{0}.google.com/vt/x={1}&y={2}&z={3}";

        public override string identifier
        {
            get { return "GoogleMaps"; }
            set { throw new InvalidOperationException(); }
        }

        public override string TileUrl(TileNum tn)
        {
            serveridx = ++serveridx % 4;
            string result = string.Format(urlFormatString, serveridx, tn.X, tn.Y, tn.uZoom);
            return result;
        }

        public override uint MaxZoom
        {
            get { return 21; }
            set { throw new InvalidOperationException(); }
        }

        public override uint PixelZoomFactor
        {
            get { return 8; }
            set { throw new InvalidOperationException(); }
        }
    }
}
