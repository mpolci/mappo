using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Threading;

namespace MapsLibrary
{
    using TileIdxType = Int32;

    public class ReadAheadCachedTilesMap : CachedTilesMap, IDisposable
    {
        private uint oldZoom;
        private ProjectedGeoPoint oldCenter;
        private PxCoordinates raareasize;
        private bool threadrun;
        private Thread thrReadAhead;
        private AutoResetEvent jobEvent;
        private object joblock;
        private ProjectedGeoArea jobArea;
        private uint jobZoom;


        public ReadAheadCachedTilesMap(string tileCachePath, TileMapSystem ms, uint cachelen, Size ReadAheadAreaSize) :
            base(tileCachePath, ms, cachelen)
        {
            raareasize = new PxCoordinates(ReadAheadAreaSize.Width, ReadAheadAreaSize.Height);
            oldZoom = 0;
            oldCenter = new ProjectedGeoPoint(0, 0);
            joblock = new object();
            jobEvent = new AutoResetEvent(false);
            threadrun = true;
            thrReadAhead = new Thread(new ThreadStart(this.RedAheadThreadProc));
            thrReadAhead.Priority = ThreadPriority.BelowNormal;
            thrReadAhead.Name = "Tiles preloader";
            thrReadAhead.Start();
        }

        public override void drawImageMapAt(MapsLibrary.ProjectedGeoPoint map_center, uint zoom, MapsLibrary.ProjectedGeoArea area, System.Drawing.Graphics g, System.Drawing.Point delta)
        {
            if (zoom == oldZoom && map_center != oldCenter)
            {
                PxCoordinates pxCenter = mMapsys.PointToPx(map_center, zoom),
                              shift = pxCenter - mMapsys.PointToPx(oldCenter, zoom),
                              raC1 = pxCenter + shift * 4 - raareasize / 2,
                              raC2 = raC1 + raareasize;
                ProjectedGeoArea raArea = new ProjectedGeoArea(mMapsys.PxToPoint(raC1, zoom), mMapsys.PxToPoint(raC2, zoom));
                lock (joblock)
                {
                    jobArea = raArea;
                    jobZoom = zoom;
                    jobEvent.Set();
                }
            }
            oldZoom = zoom; 
            oldCenter = map_center;                
            base.drawImageMapAt(map_center, zoom, area, g, delta);
        }

        public void RedAheadThreadProc()
        {
            while (true)
            {
                jobEvent.WaitOne();
                if (!threadrun) return;
                ProjectedGeoArea area;
                uint zoom;
                lock (joblock)
                {
                    area = jobArea;
                    zoom = jobZoom;
                }
                TileNum tn1 = mMapsys.PointToTileNum(area.pMin, zoom),
                        tn2 = mMapsys.PointToTileNum(area.pMax, zoom);
                TileIdxType x1 = Math.Min(tn1.X, tn2.X),
                            x2 = Math.Max(tn1.X, tn2.X),
                            y1 = Math.Min(tn1.Y, tn2.Y),
                            y2 = Math.Max(tn1.Y, tn2.Y);
                TileNum i = new TileNum();
                i.uZoom = zoom;
                for (i.X = x1; i.X <= x2; i.X++)
                    for (i.Y = y1; i.Y <= y2; i.Y++)
                        getImageTile(i);
            }
        }


        #region IDisposable Members

        public override void Dispose()
        {
            threadrun = false;
            jobEvent.Set();
            thrReadAhead.Join();
        }

        #endregion
    }
}
