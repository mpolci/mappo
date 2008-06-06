﻿/*******************************************************************************
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

    public struct TileNum
    {
        public TileIdxType X, Y;
        public uint uZoom;

        public TileNum(TileIdxType x, TileIdxType y, uint zoom)
        {
            X = x; Y = y; uZoom = zoom;
        }

        public string ToString(char separator)
        {
            return uZoom.ToString() + separator + X.ToString() + separator + Y.ToString();
        }

        public override string ToString()
        {
            return this.ToString('/');
        }
    }

    public abstract class TilesMap : IMap, IDownloadableMap
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

        public abstract void drawImageMapAt(ProjectedGeoPoint map_center, uint zoom, ProjectedGeoArea area, Graphics g, Point delta);

        protected static string TileNumToString(TileIdxType x, TileIdxType y, uint zoom, string separator)
        {
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
                if (!file.Exists || overwrite)
                {
                    // può lanciare l'eccezzione System.Net.WebException
                    Tools.downloadHttpToFile(url, file.FullName, true);
                    // invalida l'area relativa al tile
                    if (this.MapChanged != null)
                    {
                        PxCoordinates corner = mapsys.TileNumToPx(tn),
                                      limit = corner + new PxCoordinates(mapsys.tilesize, mapsys.tilesize);
                        ProjectedGeoArea tilearea = new ProjectedGeoArea(mapsys.PxToPoint(corner, tn.uZoom), mapsys.PxToPoint(limit, tn.uZoom));
                        MapChanged(this, tilearea);
                    }
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
                downloadTileDepth(new TileNum(tn.X * 2, tn.Y * 2, tn.uZoom+1), depth - 1, overwrite);
                downloadTileDepth(new TileNum(tn.X * 2 + 1, tn.Y * 2, tn.uZoom+1), depth - 1, overwrite);
                downloadTileDepth(new TileNum(tn.X * 2, tn.Y * 2 + 1, tn.uZoom+1), depth - 1, overwrite);
                downloadTileDepth(new TileNum(tn.X * 2 + 1, tn.Y * 2 + 1, tn.uZoom+1), depth - 1, overwrite);
            }
        }

        
        /// <summary>
        /// Scarica il tile ad una certa coordinata geografica
        /// </summary>
        public virtual void downloadAt(ProjectedGeoPoint p, uint zoom, bool overwrite)
        {
            TileNum tn = mapsys.PointToTileNum(p, zoom);
            downloadTile(tn, overwrite);
        }

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
            //Bitmap img;
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
            Bitmap img = new Bitmap(mapsys.tilesize, mapsys.tilesize);
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
        /// Scarica, se necessario, i tile relativi all'area indicata
        /// </summary>
        public void DownloadMapArea(ProjectedGeoArea area, uint zoom)
        {
            downloadArea(area, zoom, false);
        }
        
        /// <summary>
        /// Scarica i tile che comprendono l'area indicata
        /// </summary>
        /// <param name="area">Coordinate geografiche dell'area rettangolare</param>
        /// <param name="Zoom">livello di Zoom dei tile da scaricare</param>
        public void downloadArea(ProjectedGeoArea area, uint zoom, bool overwrite)
        {
            TileNum tn1 = mapsys.PointToTileNum(area.pMin, zoom),
                    tn2 = mapsys.PointToTileNum(area.pMax, zoom);
            TileIdxType x1 = Math.Min(tn1.X, tn2.X),
                        x2 = Math.Max(tn1.X, tn2.X),
                        y1 = Math.Min(tn1.Y, tn2.Y),
                        y2 = Math.Max(tn1.Y, tn2.Y);
            TileNum i = new TileNum();
            i.uZoom = zoom;
            for (i.X = x1; i.X <= x2; i.X++)
                for (i.Y = y1; i.Y <= y2; i.Y++)
                    downloadTile(i, overwrite);
        }

        /// <summary>
        /// Aggiorna i tile già in cache che sono compresi nell'area indicata
        /// </summary>
        /// <remarks>L'implementazione attuale è poco efficiente, piuttosto che controllare se esiste ogni possibile file relativo all'area indicata, forse sarebbe meglio partire dai file in cache e vedere se sono relativi all'area indicata.</remarks>
        /// <param name="area">Coordinate geografiche dell'area rettangolare</param>
        /// <param name="Zoom">livello di Zoom dei tile da scaricare</param>
        public virtual void updateTilesInArea(ProjectedGeoArea area, uint zoom)
        {
            TileNum tn1 = mapsys.PointToTileNum(area.pMin, zoom),
                    tn2 = mapsys.PointToTileNum(area.pMax, zoom);
            TileIdxType x1 = Math.Min(tn1.X, tn2.X),
                        x2 = Math.Max(tn1.X, tn2.X),
                        y1 = Math.Min(tn1.Y, tn2.Y),
                        y2 = Math.Max(tn1.Y, tn2.Y);
            TileNum i = new TileNum();
            i.uZoom = zoom;
            for (i.X = x1; i.X <= x2; i.X++)
                for (i.Y = y1; i.Y <= y2; i.Y++)
                    if (tileInCache(i))
                        downloadTile(i, true);
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

        public static ProjectedGeoArea Intersection(ProjectedGeoArea a, ProjectedGeoArea b)
        {
            Int32 minlat = Math.Max(a.pMin.nLat, b.pMin.nLat),
                  minLon = Math.Max(a.pMin.nLon, b.pMin.nLon),
                  maxLat = Math.Min(a.pMax.nLat, b.pMax.nLat),
                  maxLon = Math.Min(a.pMax.nLon, b.pMax.nLon);
            if (minlat > maxLat || minLon > maxLon)
                return new ProjectedGeoArea(new ProjectedGeoPoint(0, 0), new ProjectedGeoPoint(0, 0));
            else
                return new ProjectedGeoArea(new ProjectedGeoPoint(minlat, minLon), new ProjectedGeoPoint(maxLat, maxLon));
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
                Int32 top = Math.Min(area.pMin.nLat, this.pMax.nLat),
                      bottom = Math.Max(area.pMax.nLat, this.pMin.nLat);
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
                    zones.Add( new ProjectedGeoArea(new ProjectedGeoPoint(top, this.pMin.nLon), new ProjectedGeoPoint(bottom, Math.Min(area.pMin.nLon, this.pMax.nLon))) );
                }
                if (area.pMax.nLon < this.pMax.nLon) {
                    // fascia di destra
                    zones.Add( new ProjectedGeoArea(new ProjectedGeoPoint(top, Math.Max(area.pMax.nLon, this.pMin.nLon)), new ProjectedGeoPoint(bottom, this.pMax.nLon)) );
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
        public PxType xpx;
        public PxType ypx;

        public PxCoordinates(PxType x, PxType y)
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

        public static implicit operator PxCoordinates(System.Drawing.Point p)
        {
            return new PxCoordinates(p.X, p.Y);
        }
        public override string ToString()
        {
            return "(x: " + this.xpx.ToString() + " y: " + this.ypx.ToString() + ")"; 
        }
    }

    public abstract class TileMapSystem : MercatorProjectionMapSystem
    {
        /// <summary>
        /// URL di base dove scaricare i tile
        /// </summary>
        private string sTileServer;
        //private uint uTileSize;

        //public TileMapSystem(string server, uint tilesize)
        public TileMapSystem(string server)
        {
            sTileServer = server;
            //uTileSize = tilesize;
        }
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

        public PxCoordinates GeoToPx(GeoPoint gp, uint zoom)
        {
            PxCoordinates px;
            double X, Y;
            //X = (gp.dLon + 180) / 360 * Math.Pow(2, Zoom);
            //Y = (1 - Math.Log(Math.Tan(gp.dLat * Math.PI / 180) + 1 / Math.Cos(gp.dLat * Math.PI / 180)) / Math.PI) / 2 * Math.Pow(2, Zoom);
            Int32 pow = (Int32)1 << (Int32)zoom;    // equivale a Math.Pow(2, Zoom). Questa istruzione limita Zoom a 32.
            X = (gp.dLon + 180) / 360 * pow;
            Y = (1 - Math.Log(Math.Tan(gp.dLat * Math.PI / 180) + 1 / Math.Cos(gp.dLat * Math.PI / 180)) / Math.PI) / 2 * pow;
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


        public GeoPoint PxToGeo(PxCoordinates px, uint zoom)
        {
            GeoPoint p;
            double X, Y;

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

        public virtual string TileUrl(TileNum tn)
        {
            return sTileServer + TileFile(tn);
        }

        /// <summary>
        /// Restituisce il nome del file (con path relativo) utilizzato per rappresentare il tile. 
        /// </summary>
        public abstract string TileFile(TileNum tn);
    }

    public class OSMTileMapSystem : TileMapSystem
    {
        public OSMTileMapSystem()
            : base("http://tile.openstreetmap.org/")
        {

        }
        public OSMTileMapSystem(string tileserver)
            : base(tileserver)
        {

        }
        /// <summary>
        /// Restituisce il nome del file (con path relativo) utilizzato per rappresentare il tile
        /// </summary>
        public override string TileFile(TileNum tn)
        {
            return tn.uZoom.ToString() + '/' + tn.X.ToString() + '/' + tn.Y.ToString() + ".png";
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

    }

    public class CachedMapTS : TilesMap
    {
        private Hashtable cache;
        //private Queue<TileNum> q;
        private LinkedList<TileNum> lru;
        uint maxitems;
        protected Bitmap _imgTileNotFound;

        public CachedMapTS(string tileCachePath, TileMapSystem ms, uint cachelen)
            : base(tileCachePath, ms)
        {
            cache = new Hashtable((int)cachelen);
            //q = new Queue<TileNum>((int)cachelen);
            lru = new LinkedList<TileNum>();
            maxitems = cachelen;
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

        /// <summary>
        /// Restituisce il bitmap del tile indicato
        /// </summary>
        public Bitmap getImageTile(TileNum tn)
        {
            if (cache.Contains(tn))
            {
                LinkedListNode<TileNum> node = lru.Find(tn);
                lru.Remove(node);
                lru.AddFirst(node);

                Bitmap bmp = (System.Drawing.Bitmap)cache[tn];
                return bmp;
            }
            else
            {
                if (cache.Count >= maxitems)
                {
                    // rimuove un elemento dalla coda
                    //TileNum oldertn = q.Dequeue();
                    //Bitmap olderbmp = (System.Drawing.Bitmap)cache[oldertn];
                    //olderbmp.Dispose();
                    //cache.Remove(oldertn);
                    // rimuove un elemento dalla coda
                    TileNum oldertn = lru.Last.Value;
                    lru.RemoveLast();
                    Bitmap olderbmp = (System.Drawing.Bitmap)cache[oldertn];
                    olderbmp.Dispose();
                    cache.Remove(oldertn);
                }
                Bitmap bmp;
                try
                {
                    bmp = base.createImageTile(tn);
                    cache.Add(tn, bmp);
                    //q.Enqueue(tn);
                    lru.AddFirst(tn);
                }
                catch (Exception e)
                {
                    bmp = ImgTileNotFound;
                }
                return bmp;
            }
        }

        public override void updateTilesInArea(ProjectedGeoArea area, uint zoom)
        {
            this.cache.Clear();
            base.updateTilesInArea(area, zoom);
        }

        public override void drawImageMapAt(ProjectedGeoPoint map_center, uint zoom, ProjectedGeoArea area, Graphics g, Point delta)
        {
            PxCoordinates pxcMin = mapsys.PointToPx(area.pMin, zoom),
                          pxcMax = mapsys.PointToPx(area.pMax, zoom);
            //PxCoordinates pxAreaSize = pxcMax - pxcMin + new PxCoordinates(1, 1);
            TileNum tnMin = mapsys.PxToTileNum(pxcMin, zoom),
                    tnMax = mapsys.PxToTileNum(pxcMax, zoom);
            pxcMax += new PxCoordinates(1, 1);  // credo vada bene giusto per l'utilizzo che ne faccio dopo

            TileNum tn;
            tn.uZoom = zoom;
            for (tn.X = tnMin.X; tn.X <= tnMax.X; tn.X++)
            {
                for (tn.Y = tnMin.Y; tn.Y <= tnMax.Y; tn.Y++)
                {
                    PxCoordinates pxcTileCorner = mapsys.TileNumToPx(tn),
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
                    PxType pxTileMaxX = pxcTileCorner.xpx + mapsys.tilesize;

                    srcsx = mapsys.tilesize - srcx;
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
                    PxType pxTileMaxY = pxcTileCorner.ypx + mapsys.tilesize;

                    srcsy = mapsys.tilesize - srcy;
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
        public static ProjectedGeoPoint operator /(ProjectedGeoPoint num, int den)
        {
            return new ProjectedGeoPoint(num.nLat / den, num.nLon / den);
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
        /// <summary>
        /// Indica l'ulteriore fattore di zoom per determinare la dimensione dei pixel.
        /// </summary>
        /// <remarks>
        /// Ad un qualsiasi livello di zoom i pixel della mappa sono dati un certo numero di livelli di suddivisione ulteriore, in pratica i pixel corrispondono ad un fattore di zoom uguale a:
        /// zoom + PixelZoomFactor
        /// Da un altro punto di vista i pezzi in cui è suddivisa la mappa hanno una dimensionde in pixel pari a 2^PixelZoomFactor
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
                        //cf.Delete();  // cancello il file non valido
                    }
                }
            }
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
                maxdist = msys.PxToPoint(new PxCoordinates(step, 0), zoom).nLon;  

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
            //if (zoom != currentID.zoom) throw new Exception("Area non preparata");
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
                    if (pximgcorner.xpx < pxareamin.xpx) {
                        srcx = (int)pxareamin.xpx - (int)pximgcorner.xpx;
                        outx = 0;
                    } else {
                        srcx = 0;
                        outx = (int)pximgcorner.xpx - (int)pxareamin.xpx;
                    }
                    srcsx = bmp.Width - srcx;
                    if (pximgsup.xpx > pxareamax.xpx + 1) {
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
                    //dst.FillRectangle(blackbrush, 0, 0, size.Width, size.Height);
                    dst.FillRegion(blackbrush, dst.Clip);
                    using (Font drawFont = new Font("Arial", 12, FontStyle.Regular))
                    using (SolidBrush drawBrush = new SolidBrush(Color.White))
                        dst.DrawString("mappa non disponibile", drawFont, drawBrush, 5, 5);
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
        public static void downloadHttpToFile(string url, string file, bool saveinfo)
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

    public interface IDownloadableMap
    {
        /// <summary>
        /// Metodo utilizzato per scaricare i dati relativi ad un'area 
        /// </summary>
        void DownloadMapArea(ProjectedGeoArea area, uint zoom);
    }

    public class GoogleMapsSystem : SparseImagesMapSystem
    {
        public GoogleMapsSystem(string apikey)
            : base()
        {
            _gmapskey = apikey;
        }

        private string _gmapskey;
        public string APIKey { get { return _gmapskey; } set { _gmapskey = value; } }

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
                         + "&zoom=" + zoom.ToString();

            FileInfo fileinfo = new FileInfo(outfilename);
            if (!fileinfo.Directory.Exists)
                fileinfo.Directory.Create();

            Tools.downloadHttpToFile(url, outfilename, false);
        }
    }
}
