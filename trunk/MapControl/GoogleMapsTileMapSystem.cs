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
