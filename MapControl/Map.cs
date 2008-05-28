using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Net;
using System.IO;
using System.Drawing;

namespace MapsLibrary
{
    public delegate void MapChangedEventHandler(IMap map, ProjectedGeoArea area);

    public interface IMap
    {
        event MapChangedEventHandler MapChanged;
    
        MercatorProjectionMapSystem mapsystem
        {
            get;
        }

        void drawImageMapAt(Graphics g, ProjectedGeoPoint center, uint zoom, Size size);

    }

    public struct TileNum
    {
        public long lX, lY;
        public uint uZoom;

        public TileNum(long x, long y, uint zoom)
        {
            lX = x; lY = y; uZoom = zoom;
        }

        public string ToString(char separator)
        {
            return uZoom.ToString() + separator + lX.ToString() + separator + lY.ToString();
        }

        public override string ToString()
        {
            return this.ToString('/');
        }
    }

    public class TileCoordinates
    {
        //protected double dLat, dLon;
        protected GeoPoint gp;
        protected double dTileFracX, dTileFracY;  // frazione del tile indicante il punto preciso corrispondente alla lat. e long.
        protected TileNum tn;

        public TileCoordinates(GeoPoint gp, uint zoom)
        {
            tn.uZoom = zoom;
            this.setLatLon(gp.dLat, gp.dLon);
        }

        public TileCoordinates()
        {

        }

        /// <summary>
        /// Costruisce l'oggetto con dei dati precalcolati
        /// </summary>
        protected internal TileCoordinates(GeoPoint gpData, TileNum tnData, double tfxData, double tfyData)
        {
            gp = gpData;
            tn = tnData;
            dTileFracX = tfxData;
            dTileFracY = tfyData;
        }

        public GeoPoint geopoint
        {
            get
            {
                return gp;
            }
        }

        public TileNum tilenum
        {
            get
            {
                return tn;
            }
        }

        protected void setLatLon(double lat, double lon)
        {
            double X, Y, tileX, tileY;
            //X = (lon + 180) / 360 * Math.Pow(2, tn.uZoom);
            //Y = (1 - Math.Log(Math.Tan(lat * Math.PI / 180) + 1 / Math.Cos(lat * Math.PI / 180)) / Math.PI) / 2 * Math.Pow(2, tn.uZoom);
            Int32 pow = (Int32)1 << (Int32)tn.uZoom;    // equivale a Math.Pow(2, Zoom). Questa istruzione limita Zoom a 32.
            X = (lon + 180) / 360 * pow;
            Y = (1 - Math.Log(Math.Tan(lat * Math.PI / 180) + 1 / Math.Cos(lat * Math.PI / 180)) / Math.PI) / 2 * (double) pow;
            tileX = Math.Floor(X);
            tileY = Math.Floor( Y );
       
            tn.lX = (long) tileX;
            tn.lY = (long) tileY;
            this.dTileFracX = X - tileX; 
            this.dTileFracY = Y - tileY; 

            gp.dLat = lat;
            gp.dLon = lon;
        }

        public double getIntDX() { return dTileFracX; } // restituisce lo scarto all'interno del tile 
        public double getIntDY() { return dTileFracY; } // restituisce lo scarto all'interno del tile 

        public override string ToString()
        {
            return this.gp.ToString();
        }

    }

    public abstract class TilesMap : IMap
    {
        protected string strTileCachePath;
        //protected string strTileServerUrl;
        protected TileMapSystem mapsys;


        /// <summary>
        /// Evento chiamato quando non viene trovato un tile nella cache su disco.
        /// </summary>
        /// <returns>False per indicare che il problema è stato risolto, True per indicare che il tile continua a non essere presente.</returns>
        public delegate bool TileNotFoundHandler(object sender, TileNum tn);
        public event TileNotFoundHandler TileNotFound;

        public event MapChangedEventHandler MapChanged;

        public TilesMap(string tileCachePath, TileMapSystem ms)
        {
            this.mapsys = ms;
            if (string.IsNullOrEmpty(tileCachePath))
                tileCachePath = ".";
            else if (!Directory.Exists(tileCachePath))
            {
                Directory.CreateDirectory(tileCachePath);
            }
            this.strTileCachePath = tileCachePath.EndsWith("\\") ? tileCachePath : tileCachePath + "\\";
        }

        public MercatorProjectionMapSystem mapsystem
        {
            get {
                return mapsys;
            }
        }

        protected TileMapSystem GetMapSystem()
        {
            return mapsys;
        }

        public abstract void drawImageMapAt(Graphics g, TileCoordinates tc, Size size);

        public void drawImageMapAt(Graphics g, ProjectedGeoPoint center, uint zoom, Size size)
        {
            drawImageMapAt(g, mapsys.PointToTileCoo(center, zoom), size);
        }

        protected static string TileNumToString(long x, long y, uint zoom, string separator) {
            return zoom.ToString() + separator + x.ToString() + separator + y.ToString();
        }


        /// <remarks>Può essere utilizzato come Handler per l'evento TileNotFound</remarks>
        public void downloadTile(TileNum tn) 
        {
            downloadTile(tn, false);
        }

        /// <summary>
        /// Scarica il tile indicato
        /// </summary>
        public void downloadTile(TileNum tn, bool overwrite) 
        {
            //string url = strTileServerUrl + tn.ToString('/') + ".png";
            string url = mapsys.TileUrl(tn);

            FileInfo file = new FileInfo(strTileCachePath + mapsys.TileFile(tn));
            if (!file.Directory.Exists) 
                file.Directory.Create();

            try {
                //if (!File.Exists(path) || overwrite)
                if (!file.Exists || overwrite)
                {
                    // può lanciare l'eccezzione System.Net.WebException
                    Tools.downloadHttpToFile(url, file.FullName);
                }
            }
            catch (WebException we)
            {
                throw we;
            }
        }

        public bool tileInCache(TileNum tn)
        {
            return File.Exists(strTileCachePath + mapsys.TileFile(tn));
        }

        /// <summary>
        /// Scarica il tile indicato e i tile che coprono la stessa area con Zoom superiore
        /// </summary>
        public void downloadTileDepth(TileNum tn, uint depth, bool overwrite)
        {
            downloadTile(tn, overwrite);
            if (depth > 0) 
            {
                downloadTileDepth(new TileNum(tn.lX * 2, tn.lY * 2, tn.uZoom+1), depth - 1, overwrite);
                downloadTileDepth(new TileNum(tn.lX * 2 + 1, tn.lY * 2, tn.uZoom+1), depth - 1, overwrite);
                downloadTileDepth(new TileNum(tn.lX * 2, tn.lY * 2 + 1, tn.uZoom+1), depth - 1, overwrite);
                downloadTileDepth(new TileNum(tn.lX * 2 + 1, tn.lY * 2 + 1, tn.uZoom+1), depth - 1, overwrite);
            }
        }

        
        /// <summary>
        /// Scarica il tile ad una certa coordinata geografica
        /// </summary>
        public virtual void downloadAt(ProjectedGeoPoint p, uint zoom, bool overwrite)
        {
            downloadTile(mapsys.PointToTileCoo(p, zoom).tilenum, overwrite);
        }

        /// <summary>
        /// Restituisce il bitmap del tile contenente le coordinate indicate
        /// </summary>
        public Bitmap getImageTileAt(TileCoordinates tc)
        {
            return this.getImageTile(tc.tilenum);
        }

        /// <summary>
        /// Restituisce il bitmap del tile indicato
        /// </summary>
        public virtual Bitmap getImageTile(TileNum tn)
        {
            throw new NotImplementedException("Abstract method");
        }
        /*
        public class TileNotFoundException: Exception 
        {
            private TileNum tn;
            public TileNum tilenum { get { return tn; } }
            public TileNotFoundException(TileNum n, Exception inner): base("", inner)
            {
                this.tn = n;
            }

        }
        */
        /// <summary>
        /// Crea e restituisce il bitmap del tile indicato
        /// </summary>
        public Bitmap createImageTile(TileNum tn) 
        {
            Bitmap img;
            string file = strTileCachePath + tn.ToString('\\') + ".png";
            do {
                if (File.Exists(file)) {
                    try
                    {
                        return new Bitmap(file);
                    }
                    catch (Exception e)
                    {
                        File.Delete(file);  // file non valido
                    }
                }
            } while (TileNotFound != null && !TileNotFound(this, tn));
            // crea un bitmap nero
            img = new Bitmap(mapsys.tilesize, mapsys.tilesize);
            using (Graphics g = Graphics.FromImage(img))
            using (Font font = new Font(FontFamily.GenericSerif, 10, FontStyle.Regular))
            using (Brush brush = new SolidBrush(Color.White))
            using (Pen pen = new Pen(Color.White))
            {
                g.DrawString("Tile Not Available", font, brush, 10, 10);
                g.DrawRectangle(pen, 0, 0, mapsys.tilesize, mapsys.tilesize);
            }
            return img;
        }

        /// <summary>
        /// Scarica i tile che comprendono l'area indicata
        /// </summary>
        /// <param name="area">Coordinate geografiche dell'area rettangolare</param>
        /// <param name="Zoom">livello di Zoom dei tile da scaricare</param>
        public void downloadArea(ProjectedGeoArea area, uint zoom, bool overwrite)
        {
            TileCoordinates tc1 = mapsys.PointToTileCoo(area.pMin, zoom),
                            tc2 = mapsys.PointToTileCoo(area.pMax, zoom);
            TileNum tn1 = tc1.tilenum,
                    tn2 = tc2.tilenum;
            long x1 = Math.Min(tn1.lX, tn2.lX),
                 x2 = Math.Max(tn1.lX, tn2.lX),
                 y1 = Math.Min(tn1.lY, tn2.lY),
                 y2 = Math.Max(tn1.lY, tn2.lY);
            TileNum i = new TileNum();
            i.uZoom = zoom;
            for (i.lX = x1; i.lX <= x2; i.lX++)
                for (i.lY = y1; i.lY <= y2; i.lY++)
                    downloadTile(i, overwrite);
        }

        /// <summary>
        /// Aggiorna i tile già in cache che sono compresi nell'area indicata
        /// </summary>
        /// <remarks>L'implementazione attuale è poco efficiente, piuttosto che controllare se esiste ogni possibile file relativo all'area indicata, forse sarebbe meglio partire dai file in cache e vedere se sono relativi all'area indicata.</remarks>
        /// <param name="area">Coordinate geografiche dell'area rettangolare</param>
        /// <param name="Zoom">livello di Zoom dei tile da scaricare</param>
        public virtual void updateArea(ProjectedGeoArea area, uint zoom)
        {
            TileCoordinates tc1 = mapsys.PointToTileCoo(area.pMin, zoom),
                            tc2 = mapsys.PointToTileCoo(area.pMax, zoom);
            TileNum tn1 = tc1.tilenum,
                    tn2 = tc2.tilenum;
            long x1 = Math.Min(tn1.lX, tn2.lX),
                 x2 = Math.Max(tn1.lX, tn2.lX),
                 y1 = Math.Min(tn1.lY, tn2.lY),
                 y2 = Math.Max(tn1.lY, tn2.lY);
            TileNum i = new TileNum();
            i.uZoom = zoom;
            for (i.lX = x1; i.lX <= x2; i.lX++)
                for (i.lY = y1; i.lY <= y2; i.lY++)
                    if (tileInCache(i))
                        downloadTile(i, true);
        }
    }

    /// <summary>
    /// Permette l'estrazione di un pezzo di mappa di dimensione inferiore o uguale Y quella di un tile
    /// </summary>
    /// <remarks></remarks>
    public class MapTS : TilesMap
    {
        public MapTS(string tileCachePath, TileMapSystem ms)
            : base(tileCachePath, ms)
        {
            
        }
    
        public Image createImageMosaic(TileCoordinates tc)
        {
            TileNum tn = tc.tilenum;
            long x = tn.lX,
                 y = tn.lY;
            long[] xb = new long[2],
                   yb = new long[2]; 
            long max;
            uint zoom = tn.uZoom;
            double cx = tc.getIntDX(),
                   cy = tc.getIntDY();
            max = ((int)1 << (int)zoom) - 1;

            // determina i tile per i vari quadranti
            if (cx > 0.5)  
            {
                xb[0] = x;
                xb[1] = (x == max) ? 0 : x + 1;
            }
            else
            {
                xb[0] = (x == 0) ? max : x - 1;
                xb[1] = x;
            }

            if (cy > 0.5)
            {
                yb[0] = y;
                yb[1] = (y == max) ? 0 : y + 1;
            }
            else
            {
                yb[0] = (y == 0) ? max : y - 1;
                yb[1] = y;
            }

            Bitmap imgCompose = new Bitmap(512, 512);
            using (Graphics g = Graphics.FromImage(imgCompose))
            {
                for (int iX = 0; iX <= 1; iX++)
                    for (int iY = 0; iY <= 1; iY++) {
                        Bitmap tile = getImageTile(new TileNum(xb[iX], yb[iY], zoom));
                        g.DrawImage(tile, iX * 256, iY * 256);
                    }
            }

            return imgCompose;
        }

        public virtual Image createImageMapAt(TileCoordinates tc, uint sx, uint sy)
        {
            // ATTENZIONE in caso di eccezioni bisognerebbe liberare questa bitmap
            Bitmap img = new Bitmap((int)sx, (int)sy);
            using (Graphics g = Graphics.FromImage(img))
            {
                drawImageMapAt(g, tc, new Size((int)sx, (int)sy));
            }
            return img;
        }
            

        public override void drawImageMapAt(Graphics g, TileCoordinates tc, Size size)
        {
            Int32 px = (Int32)(tc.getIntDX() * 256),
                  py = (Int32)(tc.getIntDY() * 256);
            Rectangle r = new Rectangle(px - (int)size.Width / 2, py - (int)size.Height / 2, (int) size.Width, (int) size.Height);

            int cornDeltaX = (r.Left < 0)? -1 : 0,
                cornDeltaY = (r.Top < 0) ? -1 : 0;

            TileNum tnCorner = new TileNum(tc.tilenum.lX + cornDeltaX, tc.tilenum.lY + cornDeltaY, tc.tilenum.uZoom);
            r.Offset(-256 * cornDeltaX, -256 * cornDeltaY);
            Int32 q1width = (r.Right > 256) ? 256 - r.Left : r.Width,
                  q1height = (r.Bottom > 256) ? 256 - r.Top : r.Height;
            Bitmap imgsrc;

            // quadrante 1
            imgsrc = this.getImageTile(tnCorner);
            Rectangle r1 = new Rectangle(r.Left, r.Top, q1width, q1height);
            g.DrawImage(imgsrc, 0, 0, r1, GraphicsUnit.Pixel);

            // quadrante 2
            if (r.Right > 256)
            {
                imgsrc = this.getImageTile(new TileNum(tnCorner.lX+1,tnCorner.lY,tnCorner.uZoom));
                Rectangle r2 = new Rectangle(0, r.Top, (int) size.Width - q1width, q1height);
                g.DrawImage(imgsrc, q1width, 0, r2, GraphicsUnit.Pixel);
            }

            // quadrante 3
            if (r.Bottom > 256)
            {
                imgsrc = this.getImageTile(new TileNum(tnCorner.lX, tnCorner.lY + 1, tnCorner.uZoom));
                Rectangle r3 = new Rectangle(r.Left, 0, q1width, (int) size.Height - q1height);
                g.DrawImage(imgsrc, 0, q1height, r3, GraphicsUnit.Pixel);
            }

            // quadrante 4
            if (r.Right > 256 && r.Bottom > 256)
            {
                imgsrc = this.getImageTile(new TileNum(tnCorner.lX + 1, tnCorner.lY + 1, tnCorner.uZoom));
                Rectangle r4 = new Rectangle(0, 0, (int) size.Width - q1width, (int) size.Height - q1height);
                g.DrawImage(imgsrc, q1width, q1height, r4, GraphicsUnit.Pixel);
            }
        }

        /// <summary>
        /// Scarica il tile ad una certa coordinata geografica
        /// </summary>
        /// <remarks>Rispetto al metodo della classe base vengono scaricati anche i tile più vicini al punto indicato in modo da avere l'intero mosaico.</remarks>
        public override void downloadAt(ProjectedGeoPoint point, uint z, bool overwrite)
        {
            // QUESTO METODO E' DA RISCRIVERE
            TileCoordinates tc = mapsys.PointToTileCoo(point, z);
            TileNum tn = tc.tilenum;
            long x = tn.lX,
                 y = tn.lY,
                 x1, x2, y1, y2, ix, iy,
                 max;
            uint zoom = tn.uZoom;
            double cx = tc.getIntDX(),
                   cy = tc.getIntDY();

            if (zoom == 0) {
                downloadTile(new TileNum(x, y, zoom));
            } else {
                // determina i tile per i vari quadranti
                if (cx > 0.5)
                {
                    x1 = x;
                    x2 = x + 1;
                }
                else
                {
                    x1 = x - 1;
                    x2 = x;
                }

                if (cy > 0.5)
                {
                    y1 = y;
                    y2 = y + 1;
                }
                else
                {
                    y1 = y - 1;
                    y2 = y;
                }
                max = ((int) 1 << (int)zoom) -1;
                //scarica i tile
                for (ix = x1; ix <= x2; ix++)
                    for (iy = y1; iy <= y2; iy++)
                    {
                        long effx = (ix < 0) ? max : (ix > max ? 0 : ix),
                             effy = (iy < 0) ? max : (iy > max ? 0 : iy);
                        downloadTile(new TileNum(effx, effy, zoom));
                    }
            }
        }
    }

    public struct GeoPoint
    {
        public double dLat;
        public double dLon;

        public GeoPoint(double lat, double lon)
        {
            dLat = lat;
            dLon = lon;
        }

        public static GeoPoint middle(GeoPoint p1, GeoPoint p2)
        {
            return new GeoPoint((p1.dLat + p2.dLat) / 2, (p1.dLon + p2.dLon) / 2);
        }

        public override string ToString()
        {
            return "Lat: " + dLat.ToString("F7") + " Lon: " + dLon.ToString("F7");
        }
    }

    public class BufferedMapTS: MapTS
    {
        TileNum tnBufferCorner;
        Bitmap[,] buffer = new Bitmap[2,2];

        public BufferedMapTS(string tileCachePath, TileMapSystem ms)
            : base(tileCachePath, ms)
        {
            tnBufferCorner.lX = -2; tnBufferCorner.lY = -2; // dato impossibile che obbliga Y ricaricare il primo blocco
        }

        public override Bitmap getImageTile(TileNum tn)
        {
            long dX = tn.lX - tnBufferCorner.lX,
                 dY = tn.lY - tnBufferCorner.lY;
            if (tn.uZoom != tnBufferCorner.uZoom || dX < -1 || dX > 2 || dY < -1 || dY > 2)
            {
                bufferFullRefresh(tn);
                return buffer[0, 0];
            } 
            else if ((dX == 0 || dX == 1) && (dY == 0 || dY == 1))
            {
                return buffer[dX, dY];
            }
            else
            {
                bufferTranslate(dX, dY);
                // ricalcola dX e dY;
                dX = tn.lX - tnBufferCorner.lX;
                dY = tn.lY - tnBufferCorner.lY;
                return buffer[dX, dY];
            }
        }


        /// <summary>
        /// reinquadra il buffer
        /// </summary>
        /// <remarks>i valori dei parametri devono essere fra -1 e 2</remarks>
        private void bufferTranslate(long dX, long dY) 
        {
            int tX, tY; // spostamento
            tX = (dX == -1) ? -1 : dX == 2 ? 1 : 0;
            tY = (dY == -1) ? -1 : dY == 2 ? 1 : 0;
            /*
            int startX = dX < 0 ? 1 : 0, 
                startY = dY < 0 ? 1 : 0, // angolo dal quale partire
            */

            Bitmap[,] newcache = new Bitmap[2, 2],
                      oldcache = this.buffer;
            long newCornX = tnBufferCorner.lX + tX,
                 newCornY = tnBufferCorner.lY + tY;
            int iX, iY;
            TileNum tnSrc = new TileNum(); 
            tnSrc.uZoom = tnBufferCorner.uZoom;
            for (tnSrc.lX = newCornX, iX = 0; iX <= 1; iX++, tnSrc.lX++)
            {
                for (tnSrc.lY = newCornY, iY = 0; iY <= 1; iY++, tnSrc.lY++)
                {
                    int srcX = tX + iX,
                        srcY = tY + iY;
                    if (srcX < 0 || srcX > 1 || srcY < 0 || srcY > 1)
                        newcache[iX, iY] = createImageTile(tnSrc);  // potrebbe lanciare un'eccezione
                    else
                    {
                        newcache[iX, iY] = oldcache[srcX, srcY];
                        oldcache[srcX, srcY] = null;
                    }
                }
            }
            // nuova cachelen costruita. Imposto i dati della classe solo ora perché potevo avere delle 
            // eccezioni che avrebbero interrotto il lavoro Y metà
            this.tnBufferCorner = new TileNum(newCornX, newCornY, tnBufferCorner.uZoom);
            this.buffer = newcache;
            // elimina i bitmap non più utilizzati
            freeArray(ref oldcache);
        }

        static private void freeArray(ref Bitmap[,] a)
        {
            if (a == null) return;
            for (int iX = 0; iX <= 1; iX++)
                for (int iY = 0; iY <= 1; iY++)
                    if (a[iX, iY] != null)
                        a[iX, iY].Dispose();
        }
        protected void bufferFullRefresh(TileNum tn)
        {
            Bitmap [,] newcache= new Bitmap[2,2];
            try
            {
                newcache[0, 0] = createImageTile(tn);
                newcache[1, 0] = createImageTile(new TileNum(tn.lX + 1, tn.lY, tn.uZoom));
                newcache[0, 1] = createImageTile(new TileNum(tn.lX, tn.lY + 1, tn.uZoom));
                newcache[1, 1] = createImageTile(new TileNum(tn.lX + 1, tn.lY + 1, tn.uZoom));
                freeArray(ref this.buffer);
                this.tnBufferCorner = tn;
                this.buffer = newcache;
            }
            catch (Exception e)
            {
                freeArray(ref newcache);
                throw e;
            }
        }

    }

    public class LayerCrossCenter : IMap
    {
        private int halflinelen;

        public LayerCrossCenter(uint size)
        {
            halflinelen = (int) size / 2;
        }

        public MercatorProjectionMapSystem mapsystem
        {
            get {
                return null;
            }
        }

        #region ILayer Members

        public event MapChangedEventHandler MapChanged;

        public void drawImageMapAt(Graphics dst, ProjectedGeoPoint center, uint zoom, Size size)
        {
            int x = size.Width / 2,
                y = size.Height / 2;
            using (Pen pen = new Pen(Color.Black)) {
            dst.DrawLine(pen, x - halflinelen, y, x + halflinelen, y);
            dst.DrawLine(pen, x, y - halflinelen, x, y + halflinelen);
            }
        }

        #endregion
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

        public void drawImageMapAt(Graphics g, ProjectedGeoPoint center, uint zoom, Size size)
        {
            // CODICE DI DEBUG
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            // FINE CODICE DI DEBUG
            foreach (LayerItem layer in this.aLayers)
            {
                if (layer.visible) 
                    layer.map.drawImageMapAt(g, center, zoom, size);
            }
            // CODICE DI DEBUG
            watch.Stop();
            string msg = "Paint time: " + watch.Elapsed.TotalMilliseconds.ToString() + " ms";
            using (Font drawFont = new Font("Arial", 8, FontStyle.Regular))
            using (SolidBrush drawBrush = new SolidBrush(Color.Black))
                g.DrawString(msg, drawFont, drawBrush, 0, 0);
            // FINE CODICE DI DEBUG
        }

        #endregion

        public int addLayerOnTop(IMap newLayer)
        {
            newLayer.MapChanged += new MapChangedEventHandler(mapchangedhandler);
            return this.aLayers.Add(new LayerItem(newLayer));            
        }

        protected void mapchangedhandler(IMap map, ProjectedGeoArea area)
        {
            if (MapChanged != null)
                MapChanged(this, area);
        }

        public void setVisibility(int idx, bool v)
        {
            LayerItem layer =  (LayerItem) aLayers[idx];
            layer.visible = v;
            aLayers[idx] = layer;
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

    public enum AreaIntersectionType
    {
        noItersection,
        partialIntersection,
        fullContains,
        fullContained,
    }

    public struct GeoArea
    {

        public GeoPoint pMin;
        public GeoPoint pMax;

        public GeoArea(GeoPoint min, GeoPoint max)
        {
            double minLat, maxLat, minLon, maxLon;
            if (min.dLat <= max.dLat) {
                minLat = min.dLat;
                maxLat = max.dLat;
            } else {
                minLat = max.dLat;
                maxLat = min.dLat;
            }
            if (min.dLon <= max.dLon) {
                minLon = min.dLon;
                maxLon = max.dLon;
            } else {
                minLon = max.dLon;
                maxLon = min.dLon;
            }
            this.pMin = new GeoPoint(minLat, minLon);
            this.pMax = new GeoPoint(maxLat, maxLon);
        }

        public GeoPoint center
        {
            get
            {
                return GeoPoint.middle(pMin, pMax);
            }
        }

        public bool contains(GeoPoint point)
        {
            return (pMin.dLat <= point.dLat && point.dLat <= pMax.dLat) &&
                   (pMin.dLon <= point.dLon && point.dLon <= pMax.dLon);
        }

        public AreaIntersectionType testIntersection(GeoArea testarea)
        {
            if (pMin.dLat <= testarea.pMin.dLat)
            {
                if (pMax.dLat >= testarea.pMax.dLat)
                {   //pieno contenimento sulla latitudine
                    switch (testLonInt(testarea)) {
                        case AreaIntersectionType.fullContains:
                            return AreaIntersectionType.fullContains;
                        case AreaIntersectionType.noItersection:
                            return AreaIntersectionType.noItersection;
                        default:
                            return AreaIntersectionType.partialIntersection;
                    }
                } 
                else if (pMax.dLat >= testarea.pMin.dLat) 
                {   // intersezione sulla latitudine
                    return (testLonInt(testarea) == AreaIntersectionType.noItersection) ? 
                           AreaIntersectionType.noItersection : AreaIntersectionType.partialIntersection;
                } 
                else 
                { // nessuna intersezione sulla latitudine
                    return AreaIntersectionType.noItersection;
                }
            }
            else
            {
                if (pMax.dLat <= testarea.pMax.dLat)
                {   // pieno contenimento sulla latitudine da parte di testarea
                    switch (testLonInt(testarea)) {
                        case AreaIntersectionType.fullContained:
                            return AreaIntersectionType.fullContained;
                        case AreaIntersectionType.noItersection:
                            return AreaIntersectionType.noItersection;
                        default:
                            return AreaIntersectionType.partialIntersection;
                    }
                }
                else if (pMin.dLat <= testarea.pMax.dLat)
                {   // intersezione sulla latitudine
                    return (testLonInt(testarea) == AreaIntersectionType.noItersection) ? 
                           AreaIntersectionType.noItersection : AreaIntersectionType.partialIntersection;
                } 
                else 
                { // nessuna intersezione sulla latitudine
                    return AreaIntersectionType.noItersection;
                }
            }
        }

        private AreaIntersectionType testLonInt(GeoArea testarea)
        {
            if (pMin.dLon <= testarea.pMin.dLon)
            {
                if (pMax.dLon >= testarea.pMax.dLon)
                {   //pieno contenimento sulla longitudine
                    return AreaIntersectionType.fullContains;
                } 
                else if (pMax.dLon >= testarea.pMin.dLon)
                {   // intersezione sulla longitudine
                    return AreaIntersectionType.partialIntersection;
                } 
                else
                { // nessuna intersezione sulla longitudine
                    return AreaIntersectionType.noItersection;
                }
            }
            else
            {
                if (pMax.dLon <= testarea.pMax.dLon)
                {   // pieno contenimento sulla longitudine da parte di testarea
                            return AreaIntersectionType.fullContained;
                }
                else if (pMin.dLon <= testarea.pMax.dLon)
                {   // intersezione sulla longitudine
                    return AreaIntersectionType.partialIntersection;
                } 
                else
                { // nessuna intersezione sulla longitudine
                    return AreaIntersectionType.noItersection;
                }
            }
            
        }

        public override string ToString()
        {
            return "[(" + this.pMin.ToString() + ") (" + pMax.ToString() + ")]";
        }


    }

    public struct ProjectedGeoArea
    {

        public ProjectedGeoPoint pMin;
        public ProjectedGeoPoint pMax;

        public ProjectedGeoArea(ProjectedGeoPoint min, ProjectedGeoPoint max)
        {
            Int32 minLat, maxLat, minLon, maxLon;
            if (min.nLat <= max.nLat)
            {
                minLat = min.nLat;
                maxLat = max.nLat;
            }
            else
            {
                minLat = max.nLat;
                maxLat = min.nLat;
            }
            if (min.nLon <= max.nLon)
            {
                minLon = min.nLon;
                maxLon = max.nLon;
            }
            else
            {
                minLon = max.nLon;
                maxLon = min.nLon;
            }
            this.pMin = new ProjectedGeoPoint(minLat, minLon);
            this.pMax = new ProjectedGeoPoint(maxLat, maxLon);
        }

        public ProjectedGeoPoint center
        {
            get
            {
                return ProjectedGeoPoint.middle(pMin, pMax);
            }
        }

        public Int32 width
        {
            get
            {
                return pMax.nLon - pMin.nLon;
            }
        }

        public Int32 height
        {
            get
            {
                return pMax.nLat - pMin.nLat;
            }
        }

        /// <summary>
        /// </summary>
        public bool contains(ProjectedGeoPoint point)
        {
            return (pMin.nLat <= point.nLat && point.nLat <= pMax.nLat) &&
                   (pMin.nLon <= point.nLon && point.nLon <= pMax.nLon);
        }

        public AreaIntersectionType testIntersection(ProjectedGeoArea testarea)
        {
            if (pMin.nLat <= testarea.pMin.nLat)
            {
                if (pMax.nLat >= testarea.pMax.nLat)
                {   //pieno contenimento sulla latitudine
                    switch (testLonInt(testarea))
                    {
                        case AreaIntersectionType.fullContains:
                            return AreaIntersectionType.fullContains;
                        case AreaIntersectionType.noItersection:
                            return AreaIntersectionType.noItersection;
                        default:
                            return AreaIntersectionType.partialIntersection;
                    }
                }
                else if (pMax.nLat >= testarea.pMin.nLat)
                {   // intersezione sulla latitudine
                    return (testLonInt(testarea) == AreaIntersectionType.noItersection) ?
                           AreaIntersectionType.noItersection : AreaIntersectionType.partialIntersection;
                }
                else
                { // nessuna intersezione sulla latitudine
                    return AreaIntersectionType.noItersection;
                }
            }
            else
            {
                if (pMax.nLat <= testarea.pMax.nLat)
                {   // pieno contenimento sulla latitudine da parte di testarea
                    switch (testLonInt(testarea))
                    {
                        case AreaIntersectionType.fullContained:
                            return AreaIntersectionType.fullContained;
                        case AreaIntersectionType.noItersection:
                            return AreaIntersectionType.noItersection;
                        default:
                            return AreaIntersectionType.partialIntersection;
                    }
                }
                else if (pMin.nLat <= testarea.pMax.nLat)
                {   // intersezione sulla latitudine
                    return (testLonInt(testarea) == AreaIntersectionType.noItersection) ?
                           AreaIntersectionType.noItersection : AreaIntersectionType.partialIntersection;
                }
                else
                { // nessuna intersezione sulla latitudine
                    return AreaIntersectionType.noItersection;
                }
            }
        }

        private AreaIntersectionType testLonInt(ProjectedGeoArea testarea)
        {
            if (pMin.nLon <= testarea.pMin.nLon)
            {
                if (pMax.nLon >= testarea.pMax.nLon)
                {   //pieno contenimento sulla longitudine
                    return AreaIntersectionType.fullContains;
                }
                else if (pMax.nLon >= testarea.pMin.nLon)
                {   // intersezione sulla longitudine
                    return AreaIntersectionType.partialIntersection;
                }
                else
                { // nessuna intersezione sulla longitudine
                    return AreaIntersectionType.noItersection;
                }
            }
            else
            {
                if (pMax.nLon <= testarea.pMax.nLon)
                {   // pieno contenimento sulla longitudine da parte di testarea
                    return AreaIntersectionType.fullContained;
                }
                else if (pMin.nLon <= testarea.pMax.nLon)
                {   // intersezione sulla longitudine
                    return AreaIntersectionType.partialIntersection;
                }
                else
                { // nessuna intersezione sulla longitudine
                    return AreaIntersectionType.noItersection;
                }
            }

        }

        public override string ToString()
        {
            return '(' + pMin.ToString() + ' ' + pMax.ToString() + ')';
        }

        /// <summary>
        /// Calcola la differenza fra l'area dell'oggetto e l'area indicata come parametro.
        /// </summary>
        public ProjectedGeoArea[] difference(ProjectedGeoArea area)
        {
            ArrayList zones = new ArrayList();
            
            if (area.pMax.nLat < this.pMin.nLat || area.pMin.nLat > this.pMax.nLat || area.pMax.nLon < this.pMin.nLon || area.pMin.nLon > this.pMax.nLon) {
                // nessuna intersezione
                zones.Add(this);
            } else {
                Int32 top = Math.Min(area.pMin.nLat-1, this.pMax.nLat),
                      bottom = Math.Max(area.pMax.nLat + 1, this.pMin.nLat);
                if (area.pMin.nLat > pMin.nLat) {
                    // fascia superiore
                    zones.Add( new ProjectedGeoArea(pMin, new ProjectedGeoPoint(top, this.pMax.nLon)) );
                }
                if (area.pMax.nLat < pMax.nLat) {
                    // fascia inferiore
                    zones.Add( new ProjectedGeoArea(new ProjectedGeoPoint(bottom, pMin.nLon), this.pMax) );
                }
                if (area.pMin.nLon > this.pMin.nLon) {
                    // fascia di sinistra
                    zones.Add( new ProjectedGeoArea(new ProjectedGeoPoint(top, this.pMin.nLon), new ProjectedGeoPoint(bottom, Math.Min(area.pMin.nLon - 1, this.pMax.nLon))) );
                }
                if (area.pMax.nLon < this.pMax.nLon) {
                    // fascia di destra
                    zones.Add( new ProjectedGeoArea(new ProjectedGeoPoint(top, Math.Max(area.pMax.nLon + 1, this.pMin.nLon)), new ProjectedGeoPoint(bottom, this.pMax.nLon)) );
                }
            }
            ProjectedGeoArea[] retval = new ProjectedGeoArea[zones.Count];
            zones.CopyTo(retval);
            return retval;
        }

        public void translate(ProjectedGeoPoint delta)
        {
            this.pMin += delta;
            this.pMax += delta;
        }
    }

    public struct PxCoordinates
    {
        public long xpx;
        public long ypx;

        public PxCoordinates(long x, long y)
        {
            xpx = x;
            ypx = y;
        }

        public static PxCoordinates operator -(PxCoordinates a, PxCoordinates b)
        {
            return new PxCoordinates(a.xpx - b.xpx, a.ypx - b.ypx);
        }
        public static PxCoordinates operator +(PxCoordinates a, PxCoordinates b)
        {
            return new PxCoordinates(a.xpx + b.xpx, a.ypx + b.ypx);
        }

        public static explicit operator System.Drawing.Point(PxCoordinates p)
        {
            return new Point((int)p.xpx, (int)p.ypx);
        }
    }

    public abstract class TileMapSystem : MercatorProjectionMapSystem
    {
        /// <summary>
        /// URL di base dove scaricare i tile
        /// </summary>
        private string sTileServer;
        /// <summary>
        /// Lunghezza in pixel del lato di un tile
        /// </summary>
        private uint uTileSize;

        public TileMapSystem(string server, uint tilesize)
        {
            sTileServer = server;
            uTileSize = tilesize;
        }

        public int tilesize
        {
            get
            {
                return (int) uTileSize;
            }
        }

        public PxCoordinates GeoToPx(GeoPoint gp, uint zoom)
        {
            PxCoordinates px;
            double X, Y;
            //X = (gp.dLon + 180) / 360 * Math.Pow(2, Zoom);
            //Y = (1 - Math.Log(Math.Tan(gp.dLat * Math.PI / 180) + 1 / Math.Cos(gp.dLat * Math.PI / 180)) / Math.PI) / 2 * Math.Pow(2, Zoom);
            Int32 pow = (Int32)1 << (Int32)zoom;    // equivale a Math.Pow(2, Zoom). Questa istruzione limita Zoom a 32.
            X = (gp.dLon + 180) / 360 * pow;
            Y = (1 - Math.Log(Math.Tan(gp.dLat * Math.PI / 180) + 1 / Math.Cos(gp.dLat * Math.PI / 180)) / Math.PI) / 2 * pow;
            px.xpx = (long)(X * uTileSize);
            px.ypx = (long)(Y * uTileSize);
            return px;
        }

        public PxCoordinates TileCooToPx(TileCoordinates tc)
        {
            PxCoordinates px;
            px.xpx = (long)((tc.tilenum.lX + tc.getIntDX()) * uTileSize);
            px.ypx = (long)((tc.tilenum.lY + tc.getIntDY()) * uTileSize);
            return px;
        }

        public GeoPoint PxToGeo(PxCoordinates px, uint zoom)
        {
            GeoPoint p;
            double X, Y;

            X = ((double)px.xpx /*+ 0.5*/) / (double)this.uTileSize;
            Y = ((double)px.ypx /*+ 0.5*/) / (double)this.uTileSize;
            //p.dLon = X * 360 / Math.Pow(2, Zoom) - 180;
            Int32 pow = (Int32) 1 << (Int32)zoom;    // equivale a Math.Pow(2, Zoom). Questa istruzione limita Zoom a 32.
            p.dLon = X * 360 / pow - 180;
            //double v = Math.PI * (1 - (Y * 2 / Math.Pow(2, Zoom)));
            double v = Math.PI * (1 - (Y * 2 / pow));
            p.dLat = Math.Atan(Math.Sinh(v)) * 180 / Math.PI;

            return p;
        }

        public TileCoordinates PxToTileCoo(PxCoordinates px, uint zoom)
        {
            GeoPoint gp;
            TileNum tn;
            double X, Y, tileX, tileY;

            X = ((double)px.xpx /*+ 0.5*/) / (double)this.uTileSize;
            Y = ((double)px.ypx /*+ 0.5*/) / (double)this.uTileSize;
            //gp.dLon = X * 360 / Math.Pow(2, Zoom) - 180;
            //double v = Math.PI * (1 - (Y * 2 / Math.Pow(2, Zoom)));
            Int32 pow = (Int32)1 << (Int32)zoom;    // equivale a Math.Pow(2, Zoom). Questa istruzione limita Zoom a 32.
            gp.dLon = X * 360 / pow - 180;
            double v = Math.PI * (1 - (Y * 2 / pow));
            gp.dLat = Math.Atan(Math.Sinh(v)) * 180 / Math.PI;
            tileX = Math.Floor(X);
            tileY = Math.Floor(Y);
            tn.lX = (long)tileX;
            tn.lY = (long)tileY;
            tn.uZoom = zoom;

            return new TileCoordinates(gp, tn, X - tileX, Y - tileY);
        }

        /// <summary>
        /// Calcola lo scarto interno al tile in pixel
        /// </summary>
        public PxCoordinates InternalPx(TileCoordinates tc)
        {
            PxCoordinates delta;
            delta.xpx = (long)(tc.getIntDX() * this.uTileSize);
            delta.ypx = (long)(tc.getIntDY() * this.uTileSize);
            return delta;
        }

        public virtual string TileUrl(TileNum tn)
        {
            return sTileServer + TileFile(tn);
        }

        /// <summary>
        /// Restituisce il nome del file (con path relativo) utilizzato per rappresentare il tile. 
        /// </summary>
        public abstract string TileFile(TileNum tn);

        public TileCoordinates PointToTileCoo(ProjectedGeoPoint p, uint zoom)
        {
            // NON EFFICIENTE! DA RISCRIVERE
            return new TileCoordinates(CalcInverseProjection(p), zoom);
        }

        public override PxCoordinates PointToPx(ProjectedGeoPoint pgp, uint zoom)
        {
            PxCoordinates px;
            Int32 factor = (1 << (30 - (int)zoom));
            px.xpx = (long)pgp.nLon * (long)uTileSize / factor;
            px.ypx = (long)pgp.nLat * (long)uTileSize / factor;
            return px;
        }

        public override ProjectedGeoPoint PxToPoint(PxCoordinates px, uint zoom)
        {
            Int32 factor = (1 << (30 - (int)zoom)) / 256;
            //INIZIO DEBUG
            //ProjectedGeoPoint ok = this.CalcProjection(PxToGeo(px, Zoom)),
            //                  test = new ProjectedGeoPoint((Int32)px.ypx * factor, (Int32)px.xpx * factor);
            //if (ok != test) throw new Exception();
            // FINE DEBUG
            return new ProjectedGeoPoint((Int32)px.ypx * factor, (Int32)px.xpx * factor);

        }

    }

    public class OSMTileMapSystem : TileMapSystem
    {
        public OSMTileMapSystem()
            : base("http://tile.openstreetmap.org/", 256)
        {

        }
        public OSMTileMapSystem(string tileserver)
            : base(tileserver, 256)
        {

        }
        /// <summary>
        /// Restituisce il nome del file (con path relativo) utilizzato per rappresentare il tile
        /// </summary>
        public override string TileFile(TileNum tn)
        {
            return tn.uZoom.ToString() + '/' + tn.lX.ToString() + '/' + tn.lY.ToString() + ".png";
        }

        public override uint MaxZoom
        {
            get
            {
                return 19;
            }
        }
    }

    public class CachedMapTS : MapTS
    {
        private Hashtable cache;
        private Queue<TileNum> q;
        uint maxitems;

        public CachedMapTS(string tileCachePath, TileMapSystem ms, uint cachelen)
            : base(tileCachePath, ms)
        {
            cache = new Hashtable((int)cachelen);
            q = new Queue<TileNum>((int)cachelen);
            maxitems = cachelen;
        }

        public override Bitmap getImageTile(TileNum tn)
        {
            if (cache.Contains(tn))
            {
                Bitmap bmp = (System.Drawing.Bitmap)cache[tn];
                return bmp;
            }
            else
            {
                if (cache.Count >= maxitems)
                {
                    TileNum oldertn = q.Dequeue();
                    Bitmap olderbmp = (System.Drawing.Bitmap)cache[oldertn];
                    olderbmp.Dispose();
                    cache.Remove(oldertn);
                }
                Bitmap bmp = base.createImageTile(tn);
                cache.Add(tn, bmp);
                q.Enqueue(tn);
                return bmp;
            }
        }

        public override void updateArea(ProjectedGeoArea area, uint zoom)
        {
            this.cache.Clear();
            base.updateArea(area, zoom);
        }
    }

    /// <summary>
    /// Rappresentazione con numeri interi a 32 bit della proiezione delle coordinate geografiche
    /// </summary>
    public struct ProjectedGeoPoint
    {
        public Int32 nLat;
        public Int32 nLon;

        public ProjectedGeoPoint(Int32 lat, Int32 lon)
        {
            nLat = lat;
            nLon = lon;
        }

        public static ProjectedGeoPoint middle(ProjectedGeoPoint p1, ProjectedGeoPoint p2)
        {
            return new ProjectedGeoPoint((p1.nLat + p2.nLat) / 2, (p1.nLon + p2.nLon) / 2);
        }

        public override string ToString()
        {
            return "(nLat: " + nLat.ToString("X8") + " nLon: " + nLon.ToString("X8") + ")";
        }

        public static bool operator ==(ProjectedGeoPoint p1, ProjectedGeoPoint p2)
        {
            return p1.nLat == p2.nLat && p1.nLon == p2.nLon;
        }
        public static bool operator !=(ProjectedGeoPoint p1, ProjectedGeoPoint p2)
        {
            return p1.nLat != p2.nLat || p1.nLon != p2.nLon;
        }

        public static Int32 distanceXY(ProjectedGeoPoint p1, ProjectedGeoPoint p2)
        {
            return Math.Max(Math.Abs(p1.nLat - p2.nLat), Math.Abs(p1.nLon - p2.nLon));
        }

        public static ProjectedGeoPoint operator -(ProjectedGeoPoint a, ProjectedGeoPoint b)
        {
            return new ProjectedGeoPoint(a.nLat - b.nLat, a.nLon - b.nLon);
        }
        public static ProjectedGeoPoint operator +(ProjectedGeoPoint a, ProjectedGeoPoint b)
        {
            return new ProjectedGeoPoint(a.nLat + b.nLat, a.nLon + b.nLon);
        }

    }

    /// <summary>
    /// Classe per la gestione delle coordinate e della loro proiezione.
    /// </summary>
    /// <remarks>
    /// Utilizza la proiezione di mercatore per la trasformazione di latitudine/longitudine in coordinate planari X/Y.
    /// Concetto di zoom: utilizzato nella trasforamazione delle coordinate in pixel. Si considera la mappa del globo un quadrato (zoom=0) che viene suddiviso in 4 quadranti uguali in modo ricorsivo tante volte quanto è il fattore di zoom.
    /// Da fare: inglobare la trasformazione delle coordinate in pixel, ora lasciata alle classi figlie ma implementata praticamente allo stesso modo.
    /// </remarks>
    public abstract class MercatorProjectionMapSystem
    {
        public abstract uint MaxZoom { get; }
    
        public GeoPoint CalcInverseProjection(ProjectedGeoPoint pgp)
        {
            GeoPoint p;
            double X, Y;

            X = (double)pgp.nLon;
            Y = (double)pgp.nLat;
            Int32 pow = (Int32)1 << (Int32)30;    // equivale a Math.Pow(2, 30)
            p.dLon = X * 360 / pow - 180;
            double v = Math.PI * (1 - (Y * 2 / pow));
            /*
            double t = Math.Sinh(v);
            t = Math.Atan(t);
            t *= 180;
            t /= Math.PI;
            */
            p.dLat = Math.Atan(Math.Sinh(v)) * 180 / Math.PI;
            
            return p;
        }

        public ProjectedGeoPoint CalcProjection(GeoPoint gp)
        {
            ProjectedGeoPoint pgp;
            double X, Y;
            //X = (gp.dLon + 180) / 360 * Math.Pow(2, 30);
            //Y = (1 - Math.Log(Math.Tan(gp.dLat * Math.PI / 180) + 1 / Math.Cos(gp.dLat * Math.PI / 180)) / Math.PI) / 2 * Math.Pow(2, 30);
            Int32 pow = (Int32)1 << (Int32)30;    // equivale a Math.Pow(2, Zoom). Questa istruzione limita Zoom a 32.
            X = (gp.dLon + 180) / 360 * pow;
            Y = (1 - Math.Log(Math.Tan(gp.dLat * Math.PI / 180) + 1 / Math.Cos(gp.dLat * Math.PI / 180)) / Math.PI) / 2 * pow;
            pgp.nLon = (Int32)X;
            pgp.nLat = (Int32)Y;
            return pgp;
        }

        public abstract PxCoordinates PointToPx(ProjectedGeoPoint pgp, uint zoom);
        public abstract ProjectedGeoPoint PxToPoint(PxCoordinates px, uint zoom);

        //public ProjectedGeoArea AreaToPx(
    }

    public class SparseImagesMapSystem : MercatorProjectionMapSystem
    {
        /// <summary>
        /// Restituisce il nome del file che deve
        /// </summary>
        public string ImageFile(ProjectedGeoPoint center, uint zoom)
        {
            return zoom.ToString() + '/' + center.nLat.ToString("X8") + '_' + center.nLon.ToString("X8");
        }

        // COPIATO DA TileMapSystem. REIMPLEMENTARE o raggruppare!
        public override PxCoordinates PointToPx(ProjectedGeoPoint pgp, uint zoom)
        {
            PxCoordinates px;
            Int32 factor = (1 << (30 - (int)zoom));
            px.xpx = (long)pgp.nLon * (long)256 / factor;
            px.ypx = (long)pgp.nLat * (long)256 / factor;
            return px;
        }

        // COPIATO DA TileMapSystem. REIMPLEMENTARE o raggruppare!
        public override ProjectedGeoPoint PxToPoint(PxCoordinates px, uint zoom)
        {
            //  DA REIMPLEMENTARE suppone i tile da 256 pixel.
            Int32 factor = (1 << (30 - (int)zoom)) / 256;
            return new ProjectedGeoPoint((Int32)px.ypx * factor, (Int32)px.xpx * factor);
        }

        public override uint MaxZoom
        {
            get
            {
                return 19;
            }
        }
    }
    

    public class SparseImagesMap : IMap
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

        protected Hashtable images;  // ImgID => nomefile
        protected SparseImagesMapSystem msys;
        string cachedir;

        ImgID currentID;
        Bitmap currentImg;

        public event MapChangedEventHandler MapChanged;

        public SparseImagesMap(SparseImagesMapSystem ms, string cachepath)
        {
            msys = ms;
            images = new Hashtable();
            if (string.IsNullOrEmpty(cachepath))
                cachepath = "./";
            else if (!Directory.Exists(cachepath))
                Directory.CreateDirectory(cachepath);
            if (!cachepath.EndsWith("/") || ! cachepath.EndsWith("\\"))
                cachepath += '/';
            cachedir = cachepath;
            parseCache();
        }

        public MercatorProjectionMapSystem mapsystem
        {
            get {
                return msys;
            }
        }

        protected void parseCache()
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
                    }
                }
            }

        }

        public virtual void downloadAt(ProjectedGeoPoint point, uint zoom, bool overwrite)
        {
            GeoPoint gp = msys.CalcInverseProjection(point);
            System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("en-us");
            string url = "http://maps.google.com/staticmap?center="
                         + gp.dLat.ToString("F6", System.Globalization.CultureInfo.InvariantCulture) + ',' + gp.dLon.ToString("F6", ci)
                         + "&size=500x500&key=ABQIAAAAh1PZgzRL2L5R0KjFvh5Q8RSpRh2vAE_NR2zRhX5ZJ3rp2ZZDExTpIUr3qM9vMNqCFi7gF1XEEyaRAA"
                         + "&zoom=" + zoom.ToString();
            string filename = cachedir + msys.ImageFile(point, zoom);
            FileInfo fileinfo = new FileInfo(filename);
            if (!fileinfo.Exists || overwrite)
            {
                if (!fileinfo.Directory.Exists)
                    fileinfo.Directory.Create();
                // bisognerebbe controllare che non ci sia una mappa troppo vicina
                try {
                    Int32 mindist = msys.PxToPoint(new PxCoordinates(180, 0), zoom).nLon;  // dipende dalla dimensione massima di un'immagine di mappa
                    findNearest(point, zoom, mindist);
                    // mappa trovata, non c'è altro da fare
                } catch (Exception) {
                    // mappa non trovata, bisogna scaricarla
                    Tools.downloadHttpToFile(url, filename);
                    images.Add(new ImgID(point, zoom), filename);
                }
            } // altrimenti si suppone che il file sia già stato caricato
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
                throw new Exception("Empty images set");
            return found;
        }

        protected virtual Bitmap getMap(ImgID id)
        {
            if (currentImg == null || currentID.zoom != id.zoom || currentID.point != id.point)
            {
                //string filename = msys.ImageFile(id.point, id.Zoom);
                string filename = (string)images[id];
                if (currentImg != null) currentImg.Dispose();
                currentImg = new Bitmap(filename);
                currentID = id;
            }
            return currentImg;
        }

        #region IMap Members

        public void drawImageMapAt(Graphics g, ProjectedGeoPoint center, uint zoom, Size size)
        {
            using (Brush blackbrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black))
            {
                try
                {
                    Int32 maxdist = msys.PxToPoint(new PxCoordinates(512, 0), zoom).nLon;  // dipende dalla dimensione massima di un'immagine di mappa
                    ImgID imgid = findNearest(center, zoom, maxdist);
                    Bitmap bmp = getMap(imgid);
                    PxCoordinates corner = msys.PointToPx(center, zoom),
                                  imgcorner = msys.PointToPx(imgid.point, imgid.zoom);
                    corner.xpx -= size.Width / 2; corner.ypx -= size.Height / 2;
                    imgcorner.xpx -= bmp.Width / 2; imgcorner.ypx -= bmp.Height / 2;
                    System.Drawing.Point outpoint = new Point((int)imgcorner.xpx - (int)corner.xpx, (int)imgcorner.ypx - (int)corner.ypx);
                    if (outpoint.X < size.Width && outpoint.Y < size.Height)
                    {
                        Rectangle rect_outimg = new Rectangle(outpoint.X, outpoint.Y, bmp.Width, bmp.Height),
                                  rect_screen = new Rectangle(0, 0, size.Width, size.Height);
                        using (Region blackregion = new Region(rect_screen))
                        {
                            blackregion.Xor(rect_outimg);
                            g.FillRegion(blackbrush, blackregion);
                        }
                        g.DrawImage(bmp, outpoint.X, outpoint.Y);
                    }
                }
                catch (Exception)
                {
                    g.FillRectangle(blackbrush, 0, 0, size.Width, size.Height);
                    using (Font drawFont = new Font("Arial", 12, FontStyle.Regular))
                    using (SolidBrush drawBrush = new SolidBrush(Color.White))
                        g.DrawString("mappa non disponibile", drawFont, drawBrush, 0, size.Height / 2 - 6);
                }
            }
        }

        #endregion
    }

    internal static class Tools
    {
        /// <summary>
        /// Scarica un file da un server http e lo salva su disco
        /// </summary>
        public static void downloadHttpToFile(string url, string file)
        {
            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(url);
            using (HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse())
            {
                // se il file già esiste legge le informazioni di download per determinare se è necessario
                // riscaricare il file
                bool download;
                System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(typeof(FileDownloadInfo));
                try {
                    FileDownloadInfo fdi;
                    using (FileStream ins = new FileStream(file + ".info", FileMode.Open))
                        fdi = (FileDownloadInfo)ser.Deserialize(ins);
                    download = fdi.modifiedtime < httpResponse.LastModified
                               || fdi.lenght != httpResponse.ContentLength;
                } catch (Exception) {
                    download = true;
                }

                if (download) {  // da completare
                    using (System.IO.Stream dataStream = httpResponse.GetResponseStream())
                    using (System.IO.FileStream outstream = new FileStream(file, FileMode.Create))
                    {
                        const int BUFFSIZE = 8192;
                        byte[] buffer = new byte[BUFFSIZE];
                        int count = dataStream.Read(buffer, 0, BUFFSIZE);
                        while (count > 0)
                        {
                            outstream.Write(buffer, 0, count);
                            count = dataStream.Read(buffer, 0, BUFFSIZE);
                        }
                        outstream.Close();
                        dataStream.Close();
                        httpResponse.Close();
                    }
                    // imposta la data di ultima modifica al file
                    using (FileStream outs = new FileStream(file + ".info", FileMode.Create))
                        ser.Serialize(outs, new FileDownloadInfo(httpResponse));
                }
            }
        }


        public struct FileDownloadInfo
        {
            public string uri;
            public DateTime modifiedtime;
            public long lenght;

            public FileDownloadInfo(HttpWebResponse response)
            {
                uri = response.ResponseUri.ToString();
                modifiedtime = response.LastModified;
                lenght = response.ContentLength;
            }
        }
    }




}
