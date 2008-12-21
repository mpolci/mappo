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
using System.Collections;
using System.Text;
using System.Net;
using System.IO;
using System.Drawing;
using System.Diagnostics;

namespace MapsLibrary
{
    using PxType = Int32;
    using TileIdxType = Int32;

    public delegate void MapChangedEventHandler(IMap map, ProjectedGeoArea area);

    public interface IMap
    {
        event MapChangedEventHandler MapChanged;
    
        MercatorProjectionMapSystem mapsystem
        {
            get;
        }

        /// <summary>
        /// Disegna un'area della mappa.
        /// </summary>
        /// <remarks>
        /// Disegna un'area di mappa su un oggetto Graphics ad una posizione indicata. 
        /// La mappa è univocamente identificata dal suo centro e dal livello di zoom.
        /// </remarks>
        /// <param name="map_center">Centro della mappa. Serve per identificarla e non è correlato all'area che effettivamente va disegnata. Di solito corrisponde alle coordinate geografiche del centro della finestra di visualizzazione.</param>
        /// <param name="zoom">Livello di zoom della mappa.</param>
        /// <param name="area">Area della mappa da disegnare.</param>
        /// <param name="g">Oggetto Graphics dove disegnare la mappa.</param>
        /// <param name="delta">Coordinate all'interno dell'oggetto "g" dove disegnare l'area "area".</param>
        void drawImageMapAt(ProjectedGeoPoint map_center, uint zoom, ProjectedGeoArea area, Graphics g, Point delta);

    }

    public interface IDownloadableMap
    {
        /// <summary>
        /// Metodo utilizzato per scaricare i dati relativi ad un'area 
        /// </summary>
        void DownloadMapArea(ProjectedGeoArea area, uint zoom);

        /// <summary>
        /// Confronta due aree per determinare se sono equivalenti ai fini del download
        /// </summary>
        /// <remarks>Utilizzato per evitare di tentare il download più volte su aree che sono equivalenti.</remarks>
        /// <returns>true se le aree sono equivalenti ai fini del download</returns>
        bool CompareAreas(ProjectedGeoArea area1, ProjectedGeoArea area2, uint zoom);

        /// <summary>
        /// Semplifica l'area per il download
        /// </summary>
        /// <remarks>Metodo utilizzato per evitare che un processo di download di un'area sia troppo lungo. Deve restituire un array di aree il cui download sia equivalente all'area indicata ma che singolarmente non rappresentano un processo di download troppo grande.</remarks>
        /// <returns>Array di aree equivalenti per il download</returns>
        ProjectedGeoArea[] SimplifyDownloadArea(ProjectedGeoArea area, uint zoom);
    }

    public abstract class TilesMap : IMap, IDownloadableMap
    {
        private string mTileCacheBasePath;
        protected string mTileCachePath;
        //protected string strTileServerUrl;
        protected TileMapSystem mMapsys;

        public enum DownloadMode
        {
            NoOverwrite,
            Overwrite,
            Refresh
        }

        /// <summary>
        /// Evento chiamato quando non viene trovato un tile nella cache su disco.
        /// </summary>
        /// <returns>False per indicare che il problema è stato risolto, True per indicare che il tile continua a non essere presente.</returns>
        public delegate bool TileNotFoundHandler(object sender, TileNum tn);
        public event TileNotFoundHandler TileNotFound;

        public event MapChangedEventHandler MapChanged;

        public TilesMap(string tileCachePath, TileMapSystem ms)
        {
            if (string.IsNullOrEmpty(tileCachePath))
                tileCachePath = ".";
            else if (!Directory.Exists(tileCachePath))
            {
                Directory.CreateDirectory(tileCachePath);
            }
            mTileCacheBasePath = tileCachePath.EndsWith("\\") ? tileCachePath : tileCachePath + "\\";
            mapsystem = ms;
        }

        public virtual MercatorProjectionMapSystem mapsystem
        {
            get {
                return mMapsys;
            }
            set
            {
                mMapsys = (TileMapSystem) value;
                mTileCachePath = mTileCacheBasePath + mMapsys.identifier + '\\';
                if (!Directory.Exists(mTileCachePath))
                {
                    Directory.CreateDirectory(mTileCachePath);
                }
                if (MapChanged != null)
                    MapChanged(this, this.mapsystem.FullMapArea);
            }
        }

        public abstract void drawImageMapAt(ProjectedGeoPoint map_center, uint zoom, ProjectedGeoArea area, Graphics g, Point delta);

        protected static string TileNumToString(TileIdxType x, TileIdxType y, uint zoom, string separator)
        {
            return zoom.ToString() + separator + x.ToString() + separator + y.ToString();
        }
        
        /// <remarks>Può essere utilizzato come Handler per l'evento TileNotFound</remarks>
        public void downloadTile(TileNum tn) 
        {
            //downloadTile(tn, DownloadMode.NoOverwrite);
            downloadTile(tn, DownloadMode.NoOverwrite);
        }
        
        /// <summary>
        /// Scarica il tile indicato
        /// </summary>
        public void downloadTile(TileNum tn, DownloadMode downmode) 
        {
            //string url = mMapsys.TileUrl(tn);

            FileInfo file = new FileInfo(this.TileFile(tn));
            if (!file.Directory.Exists) 
                file.Directory.Create();

            try {
                //Espressione logica: r x + !r (!x + o)
                //if (!file.Exists || overwrite)
                if (downmode == DownloadMode.Overwrite ||
                    file.Exists == (downmode == DownloadMode.Refresh))
                {
                    // può lanciare l'eccezzione System.Net.WebException
                    // HTTPFileDownloader.downloadToFile(url, file.FullName, true);
                    bool changed = mMapsys.SaveTileToFile(tn, file.FullName);
                    // se il tile è stato scaricato invalida l'area relativa al tile
                    if (changed)
                        onTileChanged(tn);
                    /*
                    if (changed && this.MapChanged != null)
                    {
                        PxCoordinates corner = mMapsys.TileNumToPx(tn),
                                      limit = corner + new PxCoordinates(mMapsys.tilesize, mMapsys.tilesize);
                        ProjectedGeoArea tilearea = new ProjectedGeoArea(mMapsys.PxToPoint(corner, tn.uZoom), mMapsys.PxToPoint(limit, tn.uZoom));
                        MapChanged(this, tilearea);
                    }
                    */
                }
            }
            catch (WebException we)
            {
                throw we;
            }
        }

        public bool tileInCache(TileNum tn)
        {
            return File.Exists(this.TileFile(tn));
        }

        /// <summary>
        /// Scarica il tile indicato e i tile che coprono la stessa area con Zoom superiore
        /// </summary>
        public void downloadTileDepth(TileNum tn, uint depth, DownloadMode mode)
        {
            downloadTile(tn, mode);
            if (depth > 0) 
            {
                downloadTileDepth(new TileNum(tn.X * 2, tn.Y * 2, tn.uZoom+1), depth - 1, mode);
                downloadTileDepth(new TileNum(tn.X * 2 + 1, tn.Y * 2, tn.uZoom+1), depth - 1, mode);
                downloadTileDepth(new TileNum(tn.X * 2, tn.Y * 2 + 1, tn.uZoom+1), depth - 1, mode);
                downloadTileDepth(new TileNum(tn.X * 2 + 1, tn.Y * 2 + 1, tn.uZoom+1), depth - 1, mode);
            }
        }

        /*
        /// <summary>
        /// Scarica il tile ad una certa coordinata geografica
        /// </summary>
        public virtual void downloadAt(ProjectedGeoPoint p, uint zoom, DownloadMode mode)
        {
            TileNum tn = mMapsys.PointToTileNum(p, zoom);
            downloadTile(tn, mode);
        }
        */
        public class TileNotFoundException: Exception 
        {
            private TileNum tn;
            public TileNum tilenum { get { return tn; } }
            public TileNotFoundException(TileNum n, Exception inner): base("", inner)
            {
                this.tn = n;
            }

        }
        
        /// <summary>
        /// Crea e restituisce il bitmap del tile indicato
        /// </summary>
        public Bitmap createImageTile(TileNum tn) 
        {
            Bitmap img = null;
            string file = TileFile(tn);
            do {
                if (File.Exists(file)) {
                    try
                    {
                        img = new Bitmap(file);
                    }
                    catch (Exception) { }

                    if (img != null)
                        return img;
                    else
                    {
                        try
                        {
                            File.Delete(file);  // file non valido, lo cancello
                        }
                        catch (Exception e) { System.Diagnostics.Trace.WriteLine("\n----\n" + e.ToString() + "\n----\n"); }
                    }
                }
                // lancia l'evento TileNotFound e, se questo lo indica, tenta nuovamente di caricare il tile
            } while (TileNotFound != null && !TileNotFound(this, tn));

            throw new TileNotFoundException(tn, null);
        }

        /// <summary>
        /// Crea e restituisce il bitmap del tile indicato
        /// </summary>
        public Bitmap createNotFoundImageTile()
        {
            // crea un bitmap nero
            Bitmap img = new Bitmap(mMapsys.tilesize, mMapsys.tilesize);
            using (Graphics g = Graphics.FromImage(img))
            using (Font font = new Font(FontFamily.GenericSerif, 10, FontStyle.Regular))
            using (Brush brush = new SolidBrush(Color.White))
            using (Pen pen = new Pen(Color.White))
            {
                Rectangle r = new Rectangle(0, 0, mMapsys.tilesize, mMapsys.tilesize);
#if !(PocketPC || Smartphone || WindowsCE)
                using (Brush bb = new SolidBrush(Color.Black))
                    g.FillRectangle(bb, r);
#endif
                g.DrawString("Tile Not Available", font, brush, 10, 10);
                g.DrawRectangle(pen, r);
            }
            return img;
        }

        #region IDownloadableMap Members
        /// <summary>
        /// Confronta due aree per determinare se sono equivalenti ai fini del download, cioè corrispondo agli stessi tile
        /// </summary>
        /// <remarks>Utilizzato per evitare di tentare il download più volte su aree che sono equivalenti.</remarks>
        /// <returns>true se le aree sono equivalenti ai fini del download</returns>
        public bool CompareAreas(ProjectedGeoArea area1, ProjectedGeoArea area2, uint zoom)
        {
            TileNum min1 = mMapsys.PointToTileNum(area1.pMin, zoom),
                    min2 = mMapsys.PointToTileNum(area2.pMin, zoom);
            if (min1.uZoom == min2.uZoom && min1.X == min2.X && min1.Y == min2.Y) 
            {
                TileNum max1 = mMapsys.PointToTileNum(area1.pMax, zoom),
                        max2 = mMapsys.PointToTileNum(area2.pMax, zoom);
                return max1.uZoom == max2.uZoom && max1.X == max2.X && max1.Y == max2.Y;
            } 
            else 
                return false;
        }
        /// <summary>
        /// Semplifica l'area per il download
        /// </summary>
        /// <remarks>Se l'area è di lato maggiore di 2 tile, viene spezzata in blocchi 2x2.</remarks>
        /// <returns>Array di aree equivalenti per il download</returns>
        public ProjectedGeoArea[] SimplifyDownloadArea(ProjectedGeoArea area, uint zoom)
        {
            TileNum tn1 = mMapsys.PointToTileNum(area.pMin, zoom),
                    tn2 = mMapsys.PointToTileNum(area.pMax, zoom);
            TileIdxType x1 = Math.Min(tn1.X, tn2.X),
                        x2 = Math.Max(tn1.X, tn2.X) + 1,
                        y1 = Math.Min(tn1.Y, tn2.Y),
                        y2 = Math.Max(tn1.Y, tn2.Y) + 1;
            //TODO: ottimizzare questo ciclo
            ArrayList result = new ArrayList();
            for (TileIdxType xi = x1; xi < x2; xi += 2) {
                TileIdxType xmax = Math.Min(xi + 2, x2);
                for (TileIdxType yi = y1; yi < y2; yi += 2)
                {
                    TileIdxType ymax = Math.Min(yi + 2, y2);
                    TileNum tnmin = new TileNum(xi, yi, zoom),
                            tnmax = new TileNum(xmax, ymax, zoom);
                    ProjectedGeoPoint pmax = mMapsys.TileNumToPoint(tnmax) - new ProjectedGeoPoint(1, 1);
                    ProjectedGeoArea sa = new ProjectedGeoArea(mMapsys.TileNumToPoint(tnmin), pmax);
                    result.Add(sa);
                }
            }
            return (ProjectedGeoArea[]) result.ToArray(typeof(ProjectedGeoArea));
        }
        

        /// <summary>
        /// Scarica, se necessario, i tile relativi all'area indicata
        /// </summary>
        public void DownloadMapArea(ProjectedGeoArea area, uint zoom)
        {
            //DownloadMapArea(area, zoom, DownloadMode.NoOverwrite);
            DownloadMapArea(area, zoom, DownloadMode.Overwrite);
        }
        #endregion
        /// <summary>
        /// Scarica i tile che comprendono l'area indicata
        /// </summary>
        /// <param name="area">Coordinate geografiche dell'area rettangolare</param>
        /// <param name="Zoom">livello di Zoom dei tile da scaricare</param>
        public void DownloadMapArea(ProjectedGeoArea area, uint zoom, DownloadMode mode)
        {
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
                    downloadTile(i, mode);
        }

        /// <summary>
        /// Aggiorna i tile già in cache che sono compresi nell'area indicata
        /// </summary>
        /// <remarks>L'implementazione attuale è poco efficiente, piuttosto che controllare se esiste ogni possibile file relativo all'area indicata, forse sarebbe meglio partire dai file in cache e vedere se sono relativi all'area indicata.</remarks>
        /// <param name="area">Coordinate geografiche dell'area rettangolare</param>
        /// <param name="Zoom">livello di Zoom dei tile da scaricare</param>
/*        public virtual void updateTilesInArea(ProjectedGeoArea area, uint zoom)
        {
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
                    downloadTile(i, DownloadMode.Refresh);
                    //if (tileInCache(i))
                    //    downloadTile(i, true);
        }
*/
        public virtual void updateTilesInArea(ProjectedGeoArea area, uint zoom)
        {
            // Struttura directory: zoom/x/y.png
            string sZoomDir = mTileCachePath + zoom + '\\';
            if (!Directory.Exists(sZoomDir))
                return;

            TileNum tn1 = mMapsys.PointToTileNum(area.pMin, zoom),
                    tn2 = mMapsys.PointToTileNum(area.pMax, zoom);
            TileIdxType x1 = Math.Min(tn1.X, tn2.X),
                        x2 = Math.Max(tn1.X, tn2.X),
                        y1 = Math.Min(tn1.Y, tn2.Y),
                        y2 = Math.Max(tn1.Y, tn2.Y);
            TileNum i = new TileNum();
            i.uZoom = zoom;
            for (i.X = x1; i.X <= x2; i.X++)
            {
                string sXDir = sZoomDir + i.X.ToString() + '\\';
                if (!Directory.Exists(sXDir)) continue;
                for (i.Y = y1; i.Y <= y2; i.Y++)
                    downloadTile(i, DownloadMode.Refresh);
                    //if (File.Exists(sXDir + i.Y.ToString() + ".png"))
                    //    downloadTile(i, true);
                
            }
        }

        /// <summary>
        /// Restituisce il nome del file (con path relativo) utilizzato per rappresentare il tile
        /// </summary>
        protected string TileFile(TileNum tn)
        {
            return mTileCachePath + tn.uZoom.ToString() + '\\' + tn.X.ToString() + '\\' + tn.Y.ToString() + ".png";
        }

        public string TileCachePath
        {
            get
            {
                return mTileCachePath;
            }
        }

        protected virtual void onTileChanged(TileNum tn)
        {
            RaiseMapChangedEv(tn);
        }

        protected void RaiseMapChangedEv(TileNum tn)
        {
            // Lancia l'evento MapChanged sull'area relativa al tile per invalidarla
            if (MapChanged != null)
            {
                MapChanged(this, mMapsys.TileNumToArea(tn));
            }
        }


        /*
        public virtual void updateTilesInArea2(ProjectedGeoArea area, uint zoom)
        {
            TileNum tn1 = mMapsys.PointToTileNum(area.pMin, zoom),
                    tn2 = mMapsys.PointToTileNum(area.pMax, zoom);
            TileIdxType x1 = Math.Min(tn1.X, tn2.X),
                        x2 = Math.Max(tn1.X, tn2.X),
                        y1 = Math.Min(tn1.Y, tn2.Y),
                        y2 = Math.Max(tn1.Y, tn2.Y),
                        ix, iy;
            // Struttura directory: zoom/x/y.png
            try {
                string sZoomDir = mTileCacheBasePath + zoom + '\\';
                DirectoryInfo diZoom = new DirectoryInfo(sZoomDir);
                if (!diZoom.Exists) throw new DirectoryNotFoundException();
                foreach (DirectoryInfo diCurrX in diZoom.GetDirectories())
                {
                    try
                    {
                        ix = TileIdxType.Parse(diCurrX.Name);
                        if (ix < x1 || ix > x2) continue;

                        for (iy = y1; iy <= y2; iy++)
                        {
                            TileNum i = new TileNum(ix, iy, zoom);
                            if (tileInCache(i))
                                downloadTile(i, true);
                        }
                        //foreach (FileInfo fiCurrLon in diCurrLat.GetFiles())
                    }
                    catch (FormatException) { }
                }
                
            } catch (DirectoryNotFoundException) {}
        }
        */

    }

    public class CachedTilesMap : TilesMap, IDisposable
    {
        protected LRUQueue<TileNum, Bitmap> lruqueue;
        protected uint maxitems;
        private Bitmap _imgTileNotFound;

        public CachedTilesMap(string tileCachePath, TileMapSystem ms, uint cachelen)
            : base(tileCachePath, ms)
        {
            lruqueue = new LRUQueue<TileNum, Bitmap>();
            maxitems = cachelen;
        }

        public uint CacheLen
        {
            get
            {
                return maxitems;
            }
            set
            {
                if (value == 0)
                    throw new InvalidOperationException("Cache lenght cannot be 0");
                maxitems = value;
                while (lruqueue.Count > maxitems)
                    lruqueue.RemoveOlder().Dispose();
            }
        }

        protected Bitmap ImgTileNotFound
        {
            get
            {
                if (_imgTileNotFound == null)
                    _imgTileNotFound = createNotFoundImageTile();
                return _imgTileNotFound;
            }
        }

        public override MapsLibrary.MercatorProjectionMapSystem mapsystem
        {
            set
            {
                if (lruqueue != null) lruqueue.Clear();
                base.mapsystem = value;
            }
        }

        /// <summary>
        /// Restituisce il bitmap del tile indicato
        /// </summary>
        public virtual Bitmap getImageTile(TileNum tn)
        {
            Debug.Assert(lruqueue != null, "null lruqueue");
            if (lruqueue.Contains(tn))
            {
                Bitmap bmp = lruqueue[tn];
                Trace.Assert(bmp != null, "lruqueue contains null bitmap");
                return bmp;
            }
            else
            {
                if (lruqueue.Count >= maxitems)
                {
                    // rimuove un elemento dalla coda
                    Bitmap olderbmp = lruqueue.RemoveOlder();
                    Trace.Assert(olderbmp != null, "old bitmap in cache is null");
                    olderbmp.Dispose();
                }
                Bitmap bmp;
                try
                {
                    bmp = base.createImageTile(tn);
                    Trace.Assert(bmp != null, "getImageTile(): createImageTile(tn) returns null");
                    lruqueue.Add(tn, bmp);
                }
                catch (TileNotFoundException)
                {
                    bmp = ImgTileNotFound;
                }
                Trace.Assert(bmp != null, "getImageTile will returns null");
                return bmp;
            }
        }

        public override void updateTilesInArea(ProjectedGeoArea area, uint zoom)
        {
            lruqueue.Clear();
            base.updateTilesInArea(area, zoom);
        }

        public override void drawImageMapAt(ProjectedGeoPoint map_center, uint zoom, ProjectedGeoArea area, Graphics g, Point delta)
        {
            PxCoordinates pxcMin = mMapsys.PointToPx(area.pMin, zoom),
                          pxcMax = mMapsys.PointToPx(area.pMax, zoom);
            //PxCoordinates pxAreaSize = pxcMax - pxcMin + new PxCoordinates(1, 1);
            TileNum tnMin = mMapsys.PxToTileNum(pxcMin, zoom),
                    tnMax = mMapsys.PxToTileNum(pxcMax, zoom);
            pxcMax += new PxCoordinates(1, 1);  // credo vada bene giusto per l'utilizzo che ne faccio dopo

            TileNum tn;
            tn.uZoom = zoom;
            for (tn.X = tnMin.X; tn.X <= tnMax.X; tn.X++)
            {
                for (tn.Y = tnMin.Y; tn.Y <= tnMax.Y; tn.Y++)
                {
                    PxCoordinates pxcTileCorner = mMapsys.TileNumToPx(tn),
                                  tileareaoffset = pxcTileCorner - pxcMin;

                    //------------------------- X ------------------
                    int outx, // coordinata x su g dove piazzare il tile;
                        srcx, srcsx;
                    // se il tile inizia prima dell'area da disegnare
                    if (tileareaoffset.xpx < 0)
                    {
                        outx = delta.X;
                        srcx = (int)-tileareaoffset.xpx;
                    }
                    else
                    {
                        outx = (int)(delta.X + tileareaoffset.xpx);
                        srcx = 0;
                    }
                    PxType pxTileMaxX = pxcTileCorner.xpx + mMapsys.tilesize;

                    srcsx = mMapsys.tilesize - srcx;
                    // se il tile va oltre l'area d disegnare
                    if (pxTileMaxX > pxcMax.xpx)
                        srcsx -= (int)(pxTileMaxX - pxcMax.xpx);

                    //------------------------- Y ------------------
                    int outy, // coordinata y su g dove piazzare il tile;
                        srcy, srcsy;
                    // se il tile inizia prima dell'area da disegnare
                    if (tileareaoffset.ypx < 0)
                    {
                        outy = delta.Y;
                        srcy = (int)-tileareaoffset.ypx;
                    }
                    else
                    {
                        outy = (int)(delta.Y + tileareaoffset.ypx);
                        srcy = 0;
                    }
                    PxType pxTileMaxY = pxcTileCorner.ypx + mMapsys.tilesize;

                    srcsy = mMapsys.tilesize - srcy;
                    // se il tile va oltre l'area d disegnare
                    if (pxTileMaxY > pxcMax.ypx)
                        srcsy -= (int)(pxTileMaxY - pxcMax.ypx);

                    //----------------------------------------------

                    Rectangle src_rect = new Rectangle(srcx, srcy, srcsx, srcsy);
                    Bitmap imgsrc = this.getImageTile(tn);
                    g.DrawImage(imgsrc, outx, outy, src_rect, GraphicsUnit.Pixel);
                }
            }
        }

        #region IDisposable Members

        public virtual void Dispose()
        {
            lruqueue.Clear();
        }

        #endregion

        protected override void onTileChanged(TileNum tn)
        {
            if (lruqueue.Contains(tn))
            {
                Bitmap bmp = lruqueue.Remove(tn);
                Debug.Assert(bmp != null, "Il bitmap rimosso per tile cambiato è null");
                bmp.Dispose();
            }
            base.onTileChanged(tn);
        }
    }

    /// <remarks>Rispetto alla classe padre, vengono disegnati solo i tile già presenti nella cache. Quando un tile non è presente viene disegnato un tile vuoto mentre il tile vero e proprio viene fatto caricare nella cache da un thread che lavora in background.</remarks>
    public class CachedTilesMapDL : CachedTilesMap, IDisposable
    {
        private System.Threading.Thread thrLoader;
        private System.Threading.AutoResetEvent jobEvent;
        //private Queue<TileNum> mTilesToLoad = new Queue<TileNum>();
        private LRUQueue<TileNum> mTilesToLoad = new LRUQueue<TileNum>();


        public CachedTilesMapDL( string tileCachePath, TileMapSystem ms, uint cachelen) :
            base(tileCachePath, ms, cachelen)
        {
            jobEvent = new System.Threading.AutoResetEvent(false);
            thrLoader = new System.Threading.Thread(new System.Threading.ThreadStart(this.LoadTileToCacheProc));
            thrLoader.Name = "Image loader";
            //thrLoader.Priority = System.Threading.ThreadPriority.AboveNormal;
            thrLoader.Start();
        }

        #region IDisposable Members

        public override void Dispose()
        {
            thrLoader.Abort();
            base.Dispose();
        }

        #endregion

        /// <summary>
        /// Restituisce il bitmap del tile indicato
        /// </summary>
        public override Bitmap getImageTile(TileNum tn)
        {
            Bitmap retbmp = null;
            Debug.Assert(lruqueue != null, "null lruqueue");
            bool incache;
            lock (lruqueue)
            {
                incache = lruqueue.Contains(tn);
                if (incache)
                {
                    retbmp = lruqueue[tn];
                    Trace.Assert(retbmp != null, "lruqueue contains null bitmap");
                }
            }
            if (!incache)
            {
                lock (mTilesToLoad)
                {
                    if (!mTilesToLoad.Contains(tn))
                    {
                        // Elimina il tile più vecchio che non servirebbe più e 
                        // finirebbe per entrare e uscire subito dalla coda
                        if (mTilesToLoad.Count == maxitems)
                            mTilesToLoad.Dequeue();
                        mTilesToLoad.Enqueue(tn);
                        jobEvent.Set();
                    }
                    else
                        System.Diagnostics.Debug.WriteLine("Richiesta ripetuta del tile " + tn);
                }

                retbmp = ImgTileNotFound;
                Trace.Assert(retbmp != null, "getImageTile will returns null");
            }
            return retbmp;
        }

        private void LoadTileToCacheProc()
        {
            //TODO: sfruttare il codice dell classe base
            TileNum tn = new TileNum();
            while (true)
            {
                bool avail;
                lock (mTilesToLoad)
                {
                    avail = mTilesToLoad.Count > 0;
                    if (avail)
                        tn = mTilesToLoad.Dequeue();
                }
                if (!avail)
                    jobEvent.WaitOne();
                else if (!lruqueue.Contains(tn))
                {
                    if (lruqueue.Count >= maxitems)
                    {
                        // rimuove un elemento dalla coda
                        Bitmap olderbmp;
                        lock (lruqueue)
                            olderbmp = lruqueue.RemoveOlder();
                        Trace.Assert(olderbmp != null, "old bitmap in cache is null");
                        olderbmp.Dispose();
                    }
                    Bitmap bmp;
                    try
                    {
                        bmp = base.createImageTile(tn);
                        Trace.Assert(bmp != null, "getImageTile(): createImageTile(tn) returns null");
#if DEBUG && !(PocketPC || Smartphone || WindowsCE)
                        // simula un caricamento lento
                        System.Threading.Thread.Sleep(200);
#endif
                        lock (lruqueue)
                            lruqueue.Add(tn, bmp);
                        RaiseMapChangedEv(tn);
                    }
                    catch (TileNotFoundException)
                    { }
                } else {
                    System.Diagnostics.Debug.WriteLine("LoadTileToCacheProc() - tile già in cache: " + tn.ToString());
                }
            }
        }

    }

    public class LayeredMap : IMap
    {
        private struct LayerItem
        {
            public bool visible;
            public IMap map;
            public LayerItem(IMap m) {
                map = m;
                visible = true;
            }
            public override bool Equals(object obj)
            {
                return Object.ReferenceEquals(((LayerItem)obj).map, map);
            }

        }

        private ArrayList aLayers = new ArrayList();

        #region IMap Members

        public event MapChangedEventHandler MapChanged;

        public MercatorProjectionMapSystem mapsystem
        {
            get
            {
                return (aLayers.Count > 0) ? ((LayerItem)aLayers[0]).map.mapsystem : null;
            }
        }

        public void drawImageMapAt(ProjectedGeoPoint map_center, uint zoom, ProjectedGeoArea area, Graphics g, Point delta)
        {
            foreach (LayerItem layer in this.aLayers)
            {
                if (layer.visible)
                    layer.map.drawImageMapAt(map_center, zoom, area, g, delta);
            }
        }

        #endregion

        public int addLayerOnTop(IMap newLayer)
        {
            newLayer.MapChanged += new MapChangedEventHandler(mapchangedhandler);
            return this.aLayers.Add(new LayerItem(newLayer));            
        }

        /// <summary>
        /// cattura l'evento mapchanged dei vari layer
        /// </summary>
        protected void mapchangedhandler(IMap map, ProjectedGeoArea area)
        {
            if (MapChanged != null)
            {
                int idx = aLayers.IndexOf(new LayerItem(map));
                if ( ((LayerItem) aLayers[idx]).visible )
                    MapChanged(this, area);
            }
        }

        public void setVisibility(int idx, bool v)
        {
            LayerItem layer =  (LayerItem) aLayers[idx];
            if (layer.visible != v)
            {
                layer.visible = v;
                aLayers[idx] = layer;
                if (MapChanged != null)
                    MapChanged(this, this.mapsystem.FullMapArea);
            }
        }

        public bool isVisible(int idx)
        {
            return ((LayerItem)aLayers[idx]).visible;
        }

        public IMap this[int idx]
        {
            get
            {
                return ((LayerItem)aLayers[idx]).map;
            }
        }

    }

    public class SparseImagesMap : IMap, IDownloadableMap
    {
        protected struct ImgID
        {
            public uint zoom;
            public ProjectedGeoPoint point;

            public ImgID(ProjectedGeoPoint p, uint z)
            {
                zoom = z;
                point = p;
            }
        }

        private int _downloadimageoverlap;
        public int downloadimageoverlap { get { return _downloadimageoverlap; } set { _downloadimageoverlap = value; } }


        protected Hashtable images;  // ImgID => nomefile
        protected SparseImagesMapSystem msys;
        protected string cachedir;

        ImgID currentID;
        Bitmap currentImg;
        ProjectedGeoArea currentImgArea;

        public event MapChangedEventHandler MapChanged;

        public SparseImagesMap(SparseImagesMapSystem ms, string cachepath, int maps_overlap )
        {
            msys = ms;
            images = new Hashtable();
            if (string.IsNullOrEmpty(cachepath))
                cachepath = "./";
            else if (!Directory.Exists(cachepath))
                Directory.CreateDirectory(cachepath);
            if (!cachepath.EndsWith("/") || !cachepath.EndsWith("\\"))
                cachepath += '/';
            cachedir = cachepath;

            _downloadimageoverlap = maps_overlap;
           
            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(this.parseCache));
        }

        public MercatorProjectionMapSystem mapsystem
        {
            get {
                return msys;
            }
        }

        protected void parseCache(object nullparam)
        {
            string[] dirs = Directory.GetDirectories(this.cachedir);
            foreach (string dn in dirs)
            {
                DirectoryInfo cwd = new DirectoryInfo(dn);
                uint current_zoom = uint.Parse(cwd.Name);
                FileInfo[] files = cwd.GetFiles();
                foreach (FileInfo cf in files)
                {
                    string[] data = cf.Name.Split('_');
                    try
                    {
                        Int32 lat = Int32.Parse(data[0], System.Globalization.NumberStyles.HexNumber),
                              lon = Int32.Parse(data[1], System.Globalization.NumberStyles.HexNumber);
                        images.Add(new ImgID(new ProjectedGeoPoint(lat, lon), current_zoom), cf.FullName);
                    }
                    catch (Exception e) 
                    {
                        System.Windows.Forms.MessageBox.Show("GMaps cache error", e.Message);
                        System.Diagnostics.Trace.WriteLine("GMaps cache error - Not valid file: " + cf.FullName);
                        //cf.Delete();  // cancello il file non valido --> NO! perché non so che file sia.
                    }
                }
            }
        }

        #region IDownloadable Members
        public bool CompareAreas(ProjectedGeoArea area1, ProjectedGeoArea area2, uint zoom)
        {
            //return area1 == area2;
            return false; // HACK: il confronto fra aree non è implementato
        }

        public ProjectedGeoArea[] SimplifyDownloadArea(ProjectedGeoArea area, uint zoom)
        {
            return new ProjectedGeoArea[] { area };
        }

        public void DownloadMapArea(ProjectedGeoArea area, uint zoom)
        {
            PxCoordinates cursorstart = msys.PointToPx(area.pMin, zoom) + new PxCoordinates(msys.imagemapsize / 2, msys.imagemapsize / 2),
                          areacenter = msys.PointToPx(area.center, zoom),
                          areasuplimit = msys.PointToPx(area.pMax, zoom);
            if (cursorstart.xpx > areacenter.xpx) 
                cursorstart.xpx = areacenter.xpx;
            if (cursorstart.ypx > areacenter.ypx)
                cursorstart.ypx = areacenter.ypx;

            int step = msys.imagemapsize - downloadimageoverlap,
                maxdist = msys.PxToPoint(new PxCoordinates(msys.imagemapsize / 2, 0), zoom).nLon;  

            PxCoordinates cursor;
            for (cursor.xpx = cursorstart.xpx; cursor.xpx <= areasuplimit.xpx; cursor.xpx += step)
            {
                for (cursor.ypx = cursorstart.ypx; cursor.ypx <= areasuplimit.ypx; cursor.ypx += step)
                {
                    ProjectedGeoPoint center = msys.PxToPoint(cursor, zoom);
                    // se esiste una mappa nelle vicinanze non è necessario scaricarne un'altra
                    try
                    {
                        findNearest(center, zoom, maxdist);
                    }
                    catch (ImageMapNotFoundException)
                    {
                        downloadAt(center, zoom, false);
                    }
                }
            }
        }
        #endregion

        protected void downloadAt(ProjectedGeoPoint center, uint zoom, bool overwrite)
        {
            string filename = cachedir + msys.ImageFile(center, zoom);

            if (!File.Exists(filename) || overwrite)
            {
                msys.RetrieveMap(msys.CalcInverseProjection(center), zoom, filename);
                images.Add(new ImgID(center, zoom), filename);
                // invalida l'area
                ProjectedGeoArea imgarea = ImgArea(new Size(msys.imagemapsize, msys.imagemapsize), center, zoom);
                if (MapChanged != null)
                    MapChanged(this, imgarea);
            } // altrimenti si suppone che il file sia già stato caricato
        }

        public class ImageMapNotFoundException : Exception
        {
            public ImageMapNotFoundException(string msg) : base(msg) { }
        }

        protected ImgID findNearest(ProjectedGeoPoint point, uint zoom, Int32 maxdist)
        {
            Int32 mindist = maxdist; 
            ImgID found = new ImgID(new ProjectedGeoPoint(0,0), 0);
            foreach (ImgID id in images.Keys)
            {
                if (id.zoom == zoom)
                {
                    Int32 dist = Math.Max(Math.Abs(id.point.nLat - point.nLat), Math.Abs(id.point.nLon - point.nLon));
                    if (dist < mindist)
                    {
                        found = id;
                        mindist = dist;
                    }
                }
            }
            if (found.zoom == 0)
                throw new ImageMapNotFoundException("Empty images set");
            return found;
        }

        protected ProjectedGeoArea ImgArea(Size imgsize, ProjectedGeoPoint center, uint zoom)
        {
                ProjectedGeoPoint size = mapsystem.PxToPoint(new PxCoordinates(imgsize.Width, imgsize.Height), zoom),
                                  c1 = center - size / 2,
                                  c2 = c1 + size;
                return new ProjectedGeoArea(c1, c2);
        }

        protected virtual Bitmap getMap(ImgID id)
        {
            if (currentImg == null || currentID.zoom != id.zoom || currentID.point != id.point)
            {
                bool currentimgpresent = currentImg != null;
                string filename = (string)images[id];
                Bitmap newimg = new Bitmap(filename);
                System.Diagnostics.Trace.Assert(newimg != null, "getMap(): new Bitmap() returns null");
                
                ProjectedGeoArea newarea = ImgArea(newimg.Size, id.point, id.zoom), 
                                 oldarea = currentImgArea;
                
                if (currentimgpresent) currentImg.Dispose();
                currentID = id;
                currentImg = newimg;
                currentImgArea = newarea;

                // notificare qui il cambiamento di mappa altrimenti può inescare un'ulteriore cambiamento di mappa,
                // forse è meglio spostarlo alla fine di drawImageMapAt
                if (MapChanged != null)
                {
                    if (currentimgpresent) 
                        MapChanged(this, oldarea);
                    MapChanged(this, newarea);
                }
            }
            return currentImg;
        }

        #region IMap Members

        /// <summary>
        /// Disegna l'area indicata presa dall'immagine più vicina al centro della mappa indicato.
        /// </summary>
        /// <remarks>Attenzione: questo metodo può generare uno o più eventi MapChanged in modo sincrono. Può essere un errore elaborare questi eventi causando delle nuove chiamate a drawImageMapAt prima che la chiamata iniziale sia terminata.</remarks>
        public virtual void drawImageMapAt(ProjectedGeoPoint map_center, uint zoom, ProjectedGeoArea area, Graphics dst, Point delta)
        {
            using (Brush blackbrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black))
            {
                try
                {
                    PxCoordinates pxareamax = msys.PointToPx(area.pMax, zoom),           // CONTROLLARE: forse va incrementato di 1, specialmente per il calcolo di pxareasize
                                  pxareamin = msys.PointToPx(area.pMin, zoom);
                    //pxwincenter = pxareamin - delta + this.center_offset;  // coordinate supposte del centro del Graphics dst
                    Size pxareasize = new Size((int)pxareamax.xpx - (int)pxareamin.xpx, (int)pxareamax.ypx - (int)pxareamin.ypx);

                    //ImgID oldId = currentID;
                    // seleziona l'immagine da utilizzare
                    //ProjectedGeoPoint center = msys.PxToPoint(pxwincenter, zoom);
                    Int32 maxdist = msys.PxToPoint(new PxCoordinates(msys.imagemapsize, 0), zoom).nLon;
                    ImgID imgid = findNearest(map_center, zoom, maxdist);
                    Bitmap bmp = getMap(imgid);

                    PxCoordinates pximgcorner = msys.PointToPx(imgid.point, imgid.zoom), // angolo di coordinate minime dell'immagine
                                  pximgsup; // limite superiore dell'area dell'immagine
                    pximgcorner.xpx -= bmp.Width / 2; pximgcorner.ypx -= bmp.Height / 2;
                    pximgsup = pximgcorner + new PxCoordinates(bmp.Width, bmp.Height); // pximgsup non fa parte dell'immagine
                    int outx, outy,
                        srcx, srcy, srcsx, srcsy;
                    // l'immagine inizia prima dell'area
                    if (pximgcorner.xpx < pxareamin.xpx)
                    {
                        srcx = (int)pxareamin.xpx - (int)pximgcorner.xpx;
                        outx = 0;
                    }
                    else
                    {
                        srcx = 0;
                        outx = (int)pximgcorner.xpx - (int)pxareamin.xpx;
                    }
                    srcsx = bmp.Width - srcx;
                    if (pximgsup.xpx > pxareamax.xpx + 1)
                    {
                        // considero pxareamax facente parte dell'area da disegnare
                        srcsx -= (int)pximgsup.xpx - (int)pxareamax.xpx;
                    }
                    // l'immagine inizia prima dell'area
                    if (pximgcorner.ypx < pxareamin.ypx)
                    {
                        srcy = (int)pxareamin.ypx - (int)pximgcorner.ypx;
                        outy = 0;
                    }
                    else
                    {
                        srcy = 0;
                        outy = (int)pximgcorner.ypx - (int)pxareamin.ypx;
                    }
                    srcsy = bmp.Height - srcy;
                    if (pximgsup.ypx > pxareamax.ypx + 1)
                    {
                        // considero pxareamax facente parte dell'area da disegnare
                        srcsy -= (int)pximgsup.ypx - (int)pxareamax.ypx;
                    }

                    outx += delta.X; outy += delta.Y;

                    // riempie di nero la zona non coperta
                    using (Region blackregion = new Region(new Rectangle(delta.X, delta.Y, pxareasize.Width, pxareasize.Height)))
                    {
                        if (srcsx > 0 && srcsy > 0)
                        {
                            // disegna l'immagine della mappa
                            Rectangle srcrect = new Rectangle(srcx, srcy, srcsx, srcsy);
                            dst.DrawImage(bmp, outx, outy, srcrect, GraphicsUnit.Pixel);
                            // prepara l'area da annerire
                            Rectangle rect_outimg = new Rectangle(outx, outy, srcsx, srcsy);
                            blackregion.Exclude(rect_outimg);
                        }
                        // riempie di nero l'area non coperta dalla mappa
                        dst.FillRegion(blackbrush, blackregion);
                    }
                    //if (oldId.point != currentID.point && MapChanged != null) MapChanged(this, msys.FullMapArea);
                }
                catch (ImageMapNotFoundException)
                {
                    DrawNoMap(dst, blackbrush);
                }
                catch (Exception generic)
                {
                    DrawNoMap(dst, blackbrush);
                    System.Diagnostics.Trace.WriteLine("-- SparseImagesMap.DrawImageMapAt() - Unexpected exception: \n" + generic.ToString());
                }
            }
        }

        private static void DrawNoMap(Graphics dst, Brush bckgnd_brush)
        {
            dst.FillRegion(bckgnd_brush, dst.Clip);
            using (Font drawFont = new Font("Arial", 12, FontStyle.Regular))
            using (SolidBrush drawBrush = new SolidBrush(Color.White))
                dst.DrawString("mappa non disponibile", drawFont, drawBrush, 5, 5);
        }

        #endregion


    }

}
