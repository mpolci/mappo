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

        /// <summary>
        /// Indica l'ulteriore fattore di zoom per determinare la dimensione dei pixel.
        /// </summary>
        /// <remarks>
        /// Ad un qualsiasi livello di zoom, i pixel della mappa corrispondono ad un certo numero di livelli di suddivisione ulteriore, in pratica i pixel corrispondono ad un fattore di zoom uguale a:
        /// zoom + PixelZoomFactor.
        /// Dal punto di vista delle TilesMap i pezzi in cui è suddivisa la mappa hanno una dimensionde in pixel pari a 2^PixelZoomFactor.
        /// Vedere anche il concetto di zoom nella descrizione della classe.
        /// </remarks>
        public abstract uint PixelZoomFactor { get; }

        public readonly ProjectedGeoArea FullMapArea = new ProjectedGeoArea(new ProjectedGeoPoint(0, 0), new ProjectedGeoPoint((Int32)1 << (Int32)30, (Int32)1 << (Int32)30));

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


        public PxCoordinates PointToPx(ProjectedGeoPoint pgp, uint zoom)
        {
            // Equivalente a: 
            //Int32 factor = (1 << (30 - (int)zoom));
            ////px.xpx = (long)pgp.nLon * (long)tilesize / factor;
            ////px.ypx = (long)pgp.nLat * (long)tilesize / factor;
            int factor = 30 - (int)zoom - (int)PixelZoomFactor;
            return new PxCoordinates(pgp.nLon >> factor, pgp.nLat >> factor);

        }

        public ProjectedGeoPoint PxToPoint(PxCoordinates px, uint zoom)
        {
            // Equivalente a:
            //Int32 factor = (1 << (30 - (int)zoom)) >> (int) PixelZoomFactor;
            //return new ProjectedGeoPoint((Int32)px.ypx * factor, (Int32)px.xpx * factor);
            int factor = 30 - (int)zoom - (int)PixelZoomFactor;
            return new ProjectedGeoPoint((Int32)px.ypx << factor, (Int32)px.xpx << factor);

        }

    }

    public abstract class TileMapSystem : MercatorProjectionMapSystem
    {
        //public TileMapSystem() {}

        /// <summary>
        /// Lunghezza in pixel del lato di un tile
        /// </summary>
        public int tilesize
        {
            get
            {
                return (int) 1 << (int) PixelZoomFactor;
            }
        }

        /// <summary>
        /// Stringa che identifica il Tile Map System
        /// </summary>
        /// <remarks>
        /// I differenti Tile Map System, solitamente sono distinti e identificati attraverso il server che ospita i tile, questo rappresenta un identificatore più leggibile e sintetico.
        /// Può essere utile anche per associare fra di loro diversi server che ospitano gli stessi tile (mirror).
        /// </remarks>
        public abstract string identifier
        {
            get;
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
            // TODO: GeoToPx può essere resa indipendente dal concetto di tile usando il pixelzoomfactor anziché tilesize
            px.xpx = (PxType)(X * tilesize);
            px.ypx = (PxType)(Y * tilesize);
            return px;
        }

        // coordinate in pixel dell'angolo superiore sinistro del tile
        public PxCoordinates TileNumToPx(TileNum tn)
        {
            //PxCoordinates px;
            //px.xpx = (PxType)(tn.X * tilesize);
            //px.ypx = (PxType)(tn.Y * tilesize);
            //return px;
            return new PxCoordinates((PxType)(tn.X << (int)PixelZoomFactor),
                                     (PxType)(tn.Y << (int)PixelZoomFactor));
        }

        public TileNum PxToTileNum(PxCoordinates px, uint zoom)
        {
            //return new TileNum(px.xpx / tilesize, px.ypx / tilesize, zoom);
            return new TileNum((TileIdxType) (px.xpx >> (int)PixelZoomFactor), 
                               (TileIdxType) (px.ypx >> (int)PixelZoomFactor), 
                               zoom); 
        }

        public TileNum PointToTileNum(ProjectedGeoPoint p, uint zoom)
        {
            // REIMPLEMENTARE
            //return PxToTileNum(PointToPx(p, zoom), zoom);

            int factor = 30 - (int)zoom;
            return new TileNum(p.nLon >> factor, p.nLat >> factor, zoom);
        }

        public ProjectedGeoPoint TileNumToPoint(TileNum tn) {
            int factor = 30 - (int)tn.uZoom;
            return new ProjectedGeoPoint(tn.Y << factor, tn.X << factor);
        }

        public ProjectedGeoArea TileNumToArea(TileNum tn)
        {
            PxCoordinates corner = TileNumToPx(tn),
                          limit = corner + new PxCoordinates(tilesize, tilesize);
            ProjectedGeoArea tilearea = new ProjectedGeoArea(PxToPoint(corner, tn.uZoom), PxToPoint(limit, tn.uZoom));
            tilearea.pMax.nLat--; tilearea.pMax.nLon--;
            return tilearea;
        }

        public GeoPoint PxToGeo(PxCoordinates px, uint zoom)
        {
            GeoPoint p;
            double X, Y;
            // TODO: GeoToPx può essere resa indipendente dal concetto di tile usando il pixelzoomfactor anziché tilesize
            X = ((double)px.xpx /*+ 0.5*/) / (double)this.tilesize;
            Y = ((double)px.ypx /*+ 0.5*/) / (double)this.tilesize;
            //p.dLon = X * 360 / Math.Pow(2, Zoom) - 180;
            Int32 pow = (Int32) 1 << (Int32)zoom;    // equivale a Math.Pow(2, Zoom). Questa istruzione limita Zoom a 32.
            p.dLon = X * 360 / pow - 180;
            //double v = Math.PI * (1 - (Y * 2 / Math.Pow(2, Zoom)));
            double v = Math.PI * (1 - (Y * 2 / pow));
            p.dLat = Math.Atan(Math.Sinh(v)) * 180 / Math.PI;

            return p;
        }

        public abstract string TileUrl(TileNum tn);

        /// <summary>
        /// Salva l'immagine del tile sul file indicato.
        /// </summary>
        /// <remarks>Può lanciare l'eccezione WebException.</remarks>
        /// <returns>true se il file è stato aggiornato.</returns>
        public bool SaveTileToFile(TileNum tn, string filename)
        {
            //HTTPFileDownloader.downloadToFile(this.TileUrl(tn), filename, true);

            bool saved = false;
            try
            {
                TileInfo tileinfosaved = getSavedTileInfo(filename);
                if (tileinfosaved == null || !tileinfosaved.isRecent())  // procede solo se il tile non è stato scaricato di recente
                {
                    HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(this.TileUrl(tn));
                    //httpRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                    httpRequest.UserAgent = "Pollicino";  
                    using (HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse())
                    {
                        TileInfo tileinfonew = getTileInfo(tn, httpResponse);
                        if (tileinfonew.wasUpdated(tileinfosaved)) // scarica solo se il tile è stato aggiornato
                        {
                            using (System.IO.Stream dataStream = httpResponse.GetResponseStream())
                            using (System.IO.FileStream outstream = new FileStream(filename, FileMode.Create))
                            {
                                const int BUFFSIZE = 8192;
                                byte[] buffer = new byte[BUFFSIZE];
                                int count = dataStream.Read(buffer, 0, BUFFSIZE);
                                while (count > 0)
                                {
                                    outstream.Write(buffer, 0, count);
                                    count = dataStream.Read(buffer, 0, BUFFSIZE);
                                }
                                tileinfonew.lenght = outstream.Length;
                                outstream.Close();
                                dataStream.Close();
                                httpResponse.Close();
                            }
                            // salva le informazioni di download
                            System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(typeof(TileInfo));
                            using (FileStream outs = new FileStream(filename + ".info", FileMode.Create))
                            {
                                ser.Serialize(outs, tileinfonew);
                                outs.Close();
                            }
                            saved = true;
                        }
                    }
                }
            }
            catch (Exception e) 
            {
                System.Diagnostics.Debug.WriteLine("Download Error --- " + e.Message);
            }
            return saved;
        }

        private TileInfo getSavedTileInfo(string filename)
        {
            TileInfo fdi = null;
            System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(typeof(TileInfo));
            try
            {
                using (FileStream ins = new FileStream(filename + ".info", FileMode.Open))
                    fdi = (TileInfo)ser.Deserialize(ins);
            }
            catch (System.IO.FileNotFoundException) { }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine("Impossibile caricare il file di info relativo a " + filename + " - " + e.ToString());
            }
            return fdi;
        }

        /// <summary>
        /// Restituisce le informazioni (aggiornate) relative ad un tile
        /// </summary>
        /// <param name="httpResponse">relativa alla connessione con il server per il download del tile</param>
        /// <returns>un oggetto di tipo TileInfo o un suo derivato contenente le informazioni sul tile</returns>
        protected virtual TileInfo getTileInfo(TileNum tn, HttpWebResponse httpResponse)
        {
            return new TileInfo(httpResponse);
        }

        /// <summary>
        /// Classe per la memorizzazione delle informazioni relative ad un tile
        /// </summary>
        /// <remarks>La classe è pubblica con campi pubblici per poter essere usata con XmlSerializer</remarks>
        public class TileInfo  
        {
            public string uri;
            public DateTime modifiedtime;
            public DateTime downloadedtime;
            public long lenght;

            public TileInfo()  // necessario, altrimenti non posso usare XMLSerializer
            { }

            public TileInfo(HttpWebResponse response)
            {
                uri = response.ResponseUri.ToString();
                modifiedtime = response.LastModified;
                downloadedtime = DateTime.Now;
                lenght = response.ContentLength;
            }

            public virtual bool wasUpdated(TileInfo tileinfosaved)
            {
                return tileinfosaved == null 
                       || this.modifiedtime > tileinfosaved.modifiedtime
                       || this.lenght != tileinfosaved.lenght;
            }

            public bool isRecent()
            {
                return (DateTime.Now - downloadedtime).TotalMinutes < 360;
            }
        }

        /// <summary>
        /// Restituisce il nome del file (con path relativo) utilizzato per rappresentare il tile.
        /// </summary>
        //public abstract string TileFile(TileNum tn);
    }

    public class OSMTileMapSystem : TileMapSystem
    {
        /// <summary>
        /// URL di base dove scaricare i tile
        /// </summary>
        protected string sTileServer;
        
        public OSMTileMapSystem() : this("http://tile.openstreetmap.org/")
        {}

        public OSMTileMapSystem(string tileserver)
        {
            sTileServer = tileserver;
        }
        /// <summary>
        /// Restituisce l'URL dell'immagine del tile
        /// </summary>
        public override string TileUrl(TileNum tn)
        {
            //TODO: sistemare l'utilizzo di sTileServer in modo che non includa i path per i comamdi delle query
            return sTileServer + tn.uZoom.ToString() + '/' + tn.X.ToString() + '/' + tn.Y.ToString() + ".png";
        }

        /// <summary>
        /// Crea l'oggetto di tipo OSMTileMapSystem o discendente più adeguato per il tile server indicato
        /// </summary>
        public static OSMTileMapSystem CreateOSMMapSystem(string serverurl)
        {
            switch (serverurl)
            {
                case "http://tile.openstreetmap.org/":
                    return new MapnikMapSystem(serverurl); 
                case "http://tah.openstreetmap.org/Tiles/tile/":
                    return new TAHMapSystem(serverurl);
                //case "http://dev.openstreetmap.org/~random/no-names/":
                //    return new NoNameMapSystem();
                default:
                    return new OSMTileMapSystem(serverurl);
            }
        }

        public override uint MaxZoom
        {
            get
            {
                return 19;
            }
        }
        public override uint PixelZoomFactor
        {
            get
            {
                return 8;
            }
        }

        public override string identifier
        {
            get
            {
                Uri srvuri = new Uri(sTileServer);
                return srvuri.Host + '_' + sTileServer.GetHashCode().ToString("X8");
            }
        }
    }

    public class MapnikMapSystem : OSMTileMapSystem
    {
        public MapnikMapSystem(string tileserver)
            : base(tileserver)
        { }

        public override uint MaxZoom {
            get {
                return 19;
            }
        }

        public override string identifier
        {
            get
            {
                return "OSM_mapnik";
            }
        }

        /// <param name="httpResponse">relativa alla connessione con il server per il download del tile</param>
        /// <returns>un oggetto di tipo TileInfo o un suo derivato contenente le informazioni sul tile</returns>
        protected override TileInfo getTileInfo(TileNum tn, System.Net.HttpWebResponse httpResponse)
        {
            TileInfo tinfo = base.getTileInfo(tn, httpResponse);

            string tileinfourl = TileUrl(tn) + "/status";
            try
            {
                string htmlpage;
                HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(tileinfourl);
                using (HttpWebResponse httpResp = (HttpWebResponse)httpRequest.GetResponse())
                using (System.IO.Stream dataStream = httpResp.GetResponseStream())
                using (StreamReader reader = new StreamReader(dataStream))
                    htmlpage = reader.ReadToEnd();

                int pos = htmlpage.IndexOf("Last rendered at ");
                if (pos == -1) throw new Exception("formato pagina non valido");
                pos += "Last rendered at ".Length;
                string renderedtime = htmlpage.Substring(pos).TrimEnd(new char[] { '\n', '.' });
                tinfo.modifiedtime = DateTime.ParseExact(renderedtime, "ddd MMM dd HH:mm:ss yyyy", System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (WebException)
            {
                System.Diagnostics.Debug.WriteLine("Impossibile scaricare le informazioni del tile - " + tileinfourl);
            }
            catch (FormatException)
            {
                System.Diagnostics.Trace.WriteLine("Errore: formato data non valido nell'ottenere le informazioni sul tile  " + tn.ToString() );
                // HACK: il messaggio qui sotto deve essere messo solo in fase di DEBUG
                //#if DEBUG
                System.Windows.Forms.MessageBox.Show("Errore! Formato data non valido nel download delle informazioni del tile");
                //#endif
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine("Errore nel determinare le informazioni sul tile " + tn.ToString() + ": " + e.ToString());
            }

            return tinfo;
        }
    }

    public class TAHMapSystem : OSMTileMapSystem
    {
        public TAHMapSystem(string tileserver) : base(tileserver)
        { }

        public override uint MaxZoom {
            get {
                return 18;
            }
        }

        public override string identifier
        {
            get
            {
                return "tiles@home";
            }
        }
        /// <param name="httpResponse">relativa alla connessione con il server per il download del tile</param>
        /// <returns>un oggetto di tipo TileInfo o un suo derivato contenente le informazioni sul tile</returns>
        protected override MapsLibrary.TileMapSystem.TileInfo getTileInfo(MapsLibrary.TileNum tn, System.Net.HttpWebResponse httpResponse)
        {
            TileInfo tinfo = base.getTileInfo(tn, httpResponse);

            Uri uriserver = new Uri(this.sTileServer);
            //TODO: una volta che sTileServer contiene solo la base e non il path dei comandi posso evitare l'estrazione dell'host qui sotto
            string tileinfourl = uriserver.Scheme + "://" + uriserver.Host + "/Browse/details/tile/" 
                                 + tn.uZoom.ToString() + '/' + tn.X.ToString() + '/' + tn.Y.ToString() + '/';
            try
            {
                string htmlpage;
                HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(tileinfourl);
                using (HttpWebResponse httpResp = (HttpWebResponse)httpRequest.GetResponse())
                using (System.IO.Stream dataStream = httpResp.GetResponseStream())
                using (StreamReader reader = new StreamReader(dataStream))
                    htmlpage = reader.ReadToEnd();

                int pos = htmlpage.IndexOf("Base Tileset");
                if (pos == -1) throw new Exception("formato pagina non valido");
                pos += "Base Tileset".Length;
                pos = htmlpage.IndexOf("Last modified:", pos);
                if (pos == -1) throw new Exception("formato pagina non valido");
                pos += "Last modified:".Length;
                int endpos = htmlpage.IndexOf("<br", pos);
                if (endpos == -1) throw new Exception("formato pagina non valido");
                tinfo.modifiedtime = DateTime.Parse(htmlpage.Substring(pos, endpos - pos));
            }
            catch (WebException)
            {
                System.Diagnostics.Debug.WriteLine("Impossibile scaricare le informazioni del tile - " + tileinfourl);
            }
            catch (Exception e) 
            {
                System.Diagnostics.Trace.WriteLine("Errore nel determinare le informazioni sul tile " + tn.ToString() + ": " + e.ToString());
            }

            return tinfo;
        }
    }

    public class NoNameMapSystem : OSMTileMapSystem
    {
        public NoNameMapSystem()
            : base("http://dev.openstreetmap.org/~random/no-names/")
        { }

        public override string identifier
        {
            get
            {
                return "noname_" + sTileServer.GetHashCode().ToString("X8");
            }
        }

        /// <summary>
        /// Restituisce l'URL dell'immagine del tile
        /// </summary>
        public override string TileUrl(TileNum tn)
        {
            return sTileServer + "?zoom=" + tn.uZoom.ToString() + "&lat=" + tn.X.ToString() + "&lon=" + tn.Y.ToString() + "&layers=0B000";
        }

    }

    public abstract class SparseImagesMapSystem : MercatorProjectionMapSystem
    {
        /// <summary>
        /// Restituisce il nome del file che deve
        /// </summary>
        public string ImageFile(ProjectedGeoPoint center, uint zoom)
        {
            return zoom.ToString() + '/' + center.nLat.ToString("X8") + '_' + center.nLon.ToString("X8");
        }

        public abstract void RetrieveMap(GeoPoint center, uint zoom, string outfile);



        public abstract int imagemapsize { get; }
    }
    
    public static class HTTPFileDownloader
    {
        /// <summary>
        /// Scarica un file da un server http e lo salva su disco
        /// </summary>
        public static void downloadToFile(string url, string file, bool saveinfo)
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
                    if (saveinfo) 
                        using (FileStream outs = new FileStream(file + ".info", FileMode.Create))
                            ser.Serialize(outs, new FileDownloadInfo(httpResponse));
                }
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

    public class GoogleMapsSystem : SparseImagesMapSystem
    {
        public GoogleMapsSystem(string apikey)
            : base()
        {
            APIKey = apikey;
        }

        public string APIKey { get; set; }

        public override uint MaxZoom         { get { return 19; } }
        public override uint PixelZoomFactor { get { return 8;  } }
        public override int imagemapsize     { get { return 500; } }

        public override void RetrieveMap(GeoPoint center, uint zoom, string outfilename)
        {
            System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("en-us");
            string url = "http://maps.google.com/staticmap?center="
                         + center.dLat.ToString("F6", System.Globalization.CultureInfo.InvariantCulture) + ',' + center.dLon.ToString("F6", ci)
                         + "&size=" + imagemapsize.ToString() + 'x' + imagemapsize.ToString()
                         + "&key=" + APIKey
                         + "&sensor = true"
                         + "&zoom=" + zoom.ToString();

            FileInfo fileinfo = new FileInfo(outfilename);
            if (!fileinfo.Directory.Exists)
                fileinfo.Directory.Create();

            HTTPFileDownloader.downloadToFile(url, outfilename, false);
        }
    }
}
