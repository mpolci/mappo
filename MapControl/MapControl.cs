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
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace MapsLibrary
{
    public partial class MapControl : UserControl
    {
        private IMap map;
        private ProjectedGeoPoint pgpCenter;
        private uint uZoom;

        protected Bitmap buffer;
        protected uint buffer_zoom;
        protected ProjectedGeoArea buffer_area;

        // per le operazioni di dragging
        private int drag_lastx, drag_lasty;
        private bool dragging;

        private bool _showcross;
        private bool _showscaleref;

        public delegate void MapControlEventHandler(MapControl sender);
        public event MapControlEventHandler PrePaint;
        public event MapControlEventHandler PositionChanged;
        public event MapControlEventHandler ZoomChanged;

        public MapControl()
        {
            map = null;
            InitializeComponent();
            uZoom = 0;
            pgpCenter = new ProjectedGeoPoint();
            dragging = false;
            _showcross = false;
            _showscaleref = true;

            using (Font f = getDrawingFont())
            {
                drawingfont_height = Tools.GetFontHeight(f);
                if (drawingfont_height == 0)
                {
                    drawingfont_height = 12;
                    System.Diagnostics.Trace.WriteLine("Warning: cannot determine drawing font height");
                }
                crossline_hlen = drawingfont_height * 3 / 2;
            }
        }

        #region campi e metodi utilizzati dal metodo paint

        private static Font getDrawingFont()
        {
            return new Font("Arial", 8, FontStyle.Regular);
        }

        private readonly int drawingfont_height, crossline_hlen;

        #endregion

        /// <summary>
        /// Mappa disegnata dal controllo
        /// </summary>
        /// <remarks></remarks>
        /// <value>oggetto di tipo IMap</value>
        public IMap Map
        {
            get
            {
                return map;
            }
            set
            {
                map = value;
                if (value != null)
                {
                    map.MapChanged += new MapChangedEventHandler(invokemapchanged);
                    Invalidate();
                }
            }
        }

        private delegate void mapchangeddelegate(IMap map, ProjectedGeoArea area);

        void invokemapchanged(IMap map, ProjectedGeoArea area)
        {
            mapchangeddelegate mydelegate = this.mapchanged;
            BeginInvoke(mydelegate, new object[] { map, area });
        }

        void mapchanged(IMap map, ProjectedGeoArea area)
        {
            // aggiorna il buffer
            if (buffer != null && buffer_area.testIntersection(area) != AreaIntersectionType.noItersection)
            {
                // TODO: ATTENZIONE! qui rigenero il buffer causando delle nuove chiamate a drawImageMapAt. Sarebbe meglio 
                // semplicemente trovare un meccanismo per invalidare la parte del buffer interessata e lasciare
                // la rigenerazione al momento del Paint del controllo. In particolare in alcuni casi posso avere
                // più invalidazioni su aree parzialmente sovrapposte prima che venga elaborato un Paint, e in tale
                // situazione può capitare di rigenerare più volte la stessa area del buffer inutilmente
                ProjectedGeoArea redrawarea = ProjectedGeoArea.Intersection(buffer_area, area);
                using (Graphics g = Graphics.FromImage(buffer))
                {
                    PxCoordinates pxcBufCorn = map.mapsystem.PointToPx(buffer_area.pMin, buffer_zoom),
                                  pxcInvAreaCorn = map.mapsystem.PointToPx(redrawarea.pMin, buffer_zoom),
                                  outpos = pxcInvAreaCorn - pxcBufCorn;
                    // bisognerebbe tenere in considerazione il margine anche qui
                    this.map.drawImageMapAt(this.Center, this.Zoom, redrawarea, g, (Point)outpos);
                }
            }
            // invalida l'area del controllo (tenendo un margine di 2 pixel)
            const int marg = 2;
            PxCoordinates c = map.mapsystem.PointToPx(pgpCenter, uZoom);
            c.xpx -= this.MapViewportSize.Width / 2;
            c.ypx -= this.MapViewportSize.Height / 2;
            ProjectedGeoArea invalidarea = ProjectedGeoArea.Intersection(this.VisibleArea, area);
            PxCoordinates px1 = map.mapsystem.PointToPx(invalidarea.pMin, uZoom),
                          px2 = map.mapsystem.PointToPx(invalidarea.pMax, uZoom);  // CONTROLLARE è compreso nell'area da invalidare ????
            Rectangle rect = new Rectangle((int)(px1.xpx - c.xpx - marg), (int)(px1.ypx - c.ypx - marg),
                                           (int)(px2.xpx - px1.xpx + 1 + 2 * marg), (int)(px2.ypx - px1.ypx + 1 + 2 * marg));
            if (HiResMode)
            {
                rect.X *= 2;
                rect.Y *= 2;
                rect.Width *= 2;
                rect.Height *= 2;
            }
            this.Invalidate(rect);
        }

        /// <summary>
        /// Restituisce o imposta il fattore di zoom per la visualizzazione della mappa.
        /// </summary>
        public uint Zoom
        {
            get
            {
                return uZoom;
            }
            set
            {
                if (uZoom != value)
                {
                    uZoom = value;
                    Invalidate();
                    if (ZoomChanged != null) ZoomChanged(this); // evento
                }
            }
        }

        /// <summary>
        /// Restituisce o imposta le coordinate geografiche corrispondenti al centro dell'area visibile del controllo.
        /// </summary>
        public ProjectedGeoPoint Center
        {
            get
            {
                return pgpCenter;
            }
            set
            {
                if (pgpCenter != value)
                {
                    pgpCenter = value;
                    Invalidate();
                    if (PositionChanged != null) 
                        PositionChanged(this); // evento
                }
            }
        }

        /// <summary>
        /// Restituisce l'area geografica corrispondente all'area visibile del controllo. 
        /// </summary>
        public ProjectedGeoArea VisibleArea
        {
            get
            {
                if (map == null)
                {
                    ProjectedGeoPoint zero = new ProjectedGeoPoint(0, 0);
                    return new ProjectedGeoArea(zero, zero);
                }
                else
                {
                    PxCoordinates c1, c2;
                    c1 = this.map.mapsystem.PointToPx(this.Center, this.Zoom);
                    c1.xpx -= this.MapViewportSize.Width / 2;
                    c1.ypx -= this.MapViewportSize.Height / 2;
                    c2 = c1;
                    c2.xpx += this.MapViewportSize.Width;
                    c2.ypx += this.MapViewportSize.Height;
                    return new ProjectedGeoArea(map.mapsystem.PxToPoint(c1, this.Zoom), map.mapsystem.PxToPoint(c2, this.Zoom));
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {

            if (map != null)
            {
                if (PrePaint != null) PrePaint(this);  // genera l'evento PrePaint

                ProjectedGeoArea drawarea = this.VisibleArea;
                ProjectedGeoArea[] zones;
                PxCoordinates pxcWinCorner = map.mapsystem.PointToPx(drawarea.pMin, this.Zoom);

                Bitmap newbuffer = new Bitmap(this.MapViewportSize.Width, this.MapViewportSize.Height);
#if DEBUG
                System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
                watch.Start();
#endif
                using (Graphics outg = Graphics.FromImage(newbuffer))
                {
                    if (this.Zoom != buffer_zoom || buffer == null)
                        zones = new ProjectedGeoArea[] { drawarea };
                    else
                    {
                        // disegna il buffer attuale dentro il nuovo 
                        PxCoordinates bufpxpos = map.mapsystem.PointToPx(buffer_area.pMin, buffer_zoom);
                        Point bufpos = new Point((int)(bufpxpos.xpx - pxcWinCorner.xpx), (int)(bufpxpos.ypx - pxcWinCorner.ypx));
                        outg.DrawImage(buffer, bufpos.X, bufpos.Y);
                        zones = drawarea.difference(this.buffer_area);
                    }
                    // disegna nel buffer le parti mancanti
                    foreach (ProjectedGeoArea pr_area in zones)
                    {
                        PxCoordinates pxcCorner = map.mapsystem.PointToPx(pr_area.pMin, Zoom) - pxcWinCorner;

                        this.map.drawImageMapAt(this.Center, this.Zoom, pr_area, outg, (Point) pxcCorner);
                    }
                }
#if DEBUG
                watch.Stop();
#endif
                if (buffer != null) buffer.Dispose();
                buffer = newbuffer;
                buffer_area = drawarea;
                buffer_zoom = Zoom;

                // Il buffer viene disegnato il più vicino possibile alla croce e alle coordinate per minimizzare il flickering
                //e.Graphics.DrawImage(buffer, 0, 0);

                bool _showpos = this.ShowPosition,
                     _showscale = this.ShowScaleRef && (this.Zoom >= 7);

                // TODO: e se il font lo tenessi sempre, invece di rigenerarlo ad ogni paint?
                Font drawFont = getDrawingFont();

                // FIXME: la lunghezza del riferimento di scala varia se HiResMode
                // croce centrale con coordinate - preparazione 
                // HACK: devo assegnare le seguenti variabili anche se non mi servono sempre. Il problema è causato dalla divisione in 2 dell'if
                int scr_cent_x = 0, scr_cent_y = 0;
                if (_showpos)
                {
                    scr_cent_x = this.Size.Width / 2;
                    scr_cent_y = this.Size.Height / 2;
                }
                // riferimento di scala - preparazione
                // scalerefhlen è la lunghezza del riferimento di scala. Viene inizialmente in base all'altezza del font e poi approssimato
                // alla lunghezza inferiore più vicina che corrisponde ad una lunghezza in metri che è multiplo di una potenza di 10
                int scalerefhlen = drawingfont_height * 3;
                // HACK: devo assegnare le seguenti variabili anche se non mi servono sempre. Il problema è causato dalla divisione in 2 dell'if
                string strRefLen = null;
                if (_showscale)
                {
                    // p1 e p2 sono gli estremi di un ipotetico riferimento di scala preso al centro della finestra di visualizazione
                    PxCoordinates p1 = this.map.mapsystem.PointToPx(this.Center, this.Zoom),
                                  p2 = p1;
                    p1.xpx -= scalerefhlen; p2.xpx += scalerefhlen;
                    GeoPoint g1 = map.mapsystem.CalcInverseProjection(map.mapsystem.PxToPoint(p1, Zoom)),
                             g2 = map.mapsystem.CalcInverseProjection(map.mapsystem.PxToPoint(p2, Zoom));
                    double len = g1.Distance(g2);
                    if (HiResMode)
                        len /= 2;
                    int reflen_meters = (len < int.MaxValue) ? (int)len : 0;
                    // calcolo di una lunghezza in metri "comoda" che sia vicina a quella ottenuta
                    int aprox = ((int)Math.Log10(len));
                    if (aprox > 0)
                    {
                        int coeff = (int)Math.Pow(10, aprox);
                        reflen_meters /= coeff;
                        reflen_meters *= coeff;
                    }
                    scalerefhlen = (int)((double)reflen_meters / len * (double)scalerefhlen);
                    // sceglie unità di misura e prepara stringa
                    if (reflen_meters >= 1000)
                        strRefLen = ((double)reflen_meters / 1000.0).ToString() + " km";
                    else
                        strRefLen = reflen_meters.ToString() + " m";

                }

#if DEBUG
                    System.Diagnostics.Stopwatch watch2 = new System.Diagnostics.Stopwatch();
                    watch2.Start();
#endif
                //----- Procede con il disegno effettivo. Prima il buffer con la mappa, poi le scritte sovrapposte.
                if (HiResMode)
                {
                    if (HiResModeCustomDraw)
                    {
                        if (HiResModeBuffer == null) 
                            HiResModeBuffer = new Bitmap(Size.Width, Size.Height);
                        ResizeImage(buffer, HiResModeBuffer);
                        e.Graphics.DrawImage(HiResModeBuffer, 0, 0);
                    }
                    else
                    {
                        // FIXME: Non c'è bisogno di ricalcolare questi rettangoli ogni volta
                        Rectangle dst_r = new Rectangle(0, 0, Size.Width, Size.Height);
                        Rectangle src_r = new Rectangle(0, 0, MapViewportSize.Width, MapViewportSize.Height);
                        e.Graphics.DrawImage(buffer, dst_r, src_r, GraphicsUnit.Pixel);
                    }
                }
                else
                    e.Graphics.DrawImage(buffer, 0, 0);
#if DEBUG
                watch2.Stop();
#endif
                // croce centrale con coordinate - disegno 
                if (_showpos)
                {
                    GeoPoint gpCenter = map.mapsystem.CalcInverseProjection(this.Center);
                    using (Pen pen = new Pen(Color.Black))
                    using (SolidBrush blackBrush = new SolidBrush(Color.Black))
                    using (SolidBrush whiteBrush = new SolidBrush(Color.White))
                    {
                        e.Graphics.DrawLine(pen, scr_cent_x - crossline_hlen, scr_cent_y, scr_cent_x + crossline_hlen, scr_cent_y);
                        e.Graphics.DrawLine(pen, scr_cent_x, scr_cent_y - crossline_hlen, scr_cent_x, scr_cent_y + crossline_hlen);
                        e.Graphics.DrawString(gpCenter.ToString(), drawFont, whiteBrush, 1, 1);
                        e.Graphics.DrawString(gpCenter.ToString(), drawFont, blackBrush, 0, 0);
                    }
                }
                // riferimento di scala - disegno
                if (_showscale)
                {
                    int base_y_txt = drawingfont_height,
                        base_y_line = drawingfont_height + drawingfont_height / 2,
                        y1 = base_y_line - drawingfont_height / 2,
                        y2 = base_y_line + drawingfont_height / 2,
                        ref_x1 = drawingfont_height/2,
                        ref_x2 = ref_x1 + 2*scalerefhlen;
                    using (Pen pen = new Pen(Color.Black))
                    //using (Font drawFont = new Font("Arial", 8, FontStyle.Regular))
                    using (SolidBrush blackBrush = new SolidBrush(Color.Black))
                    using (SolidBrush whiteBrush = new SolidBrush(Color.White))
                    {
                        e.Graphics.DrawLine(pen, ref_x1, base_y_line, ref_x2, base_y_line);
                        e.Graphics.DrawLine(pen, ref_x1, y1, ref_x1, y2);
                        e.Graphics.DrawLine(pen, ref_x2, y1, ref_x2, y2);
                        e.Graphics.DrawString(strRefLen, drawFont, blackBrush, ref_x2 + drawingfont_height/2, base_y_txt);
                    }
                }

#if DEBUG
                int ptime_base_y = drawingfont_height * 2;
                string msg = "Paint time: " + watch.Elapsed.TotalMilliseconds.ToString() + " ms" +
                             " / " + watch2.ElapsedMilliseconds + " ms"; 
                //using (Font drawFont = new Font("Arial", 8, FontStyle.Regular))
                using (SolidBrush drawBrush = new SolidBrush(Color.Black))
                    e.Graphics.DrawString(msg, drawFont, drawBrush, 0, ptime_base_y);
#endif
                drawFont.Dispose();
            }
            else
            {
                // Nessuna mappa. Colora tutto di rosso.
                using (Brush b = new SolidBrush(Color.Red))
                    e.Graphics.FillRectangle(b, e.ClipRectangle);
            }
        }

        /// <remarks>Questo metodo vuoto evita il flickering quando non c'è double-buffering e comunque 
        /// evita l'inutile riempimento dello sfondo.
        /// </remarks>
        protected override void OnPaintBackground(PaintEventArgs e)
        { }

        /// <summary>
        /// Avvia lo spostamento dell'area visibile della mappa
        /// </summary>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            this.drag_lastx = e.X;
            this.drag_lasty = e.Y;
            this.dragging = true;
        }
        /// <summary>
        /// Termina lo spostamento dell'area visibile della mappa
        /// </summary>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            this.dragging = false;
            base.OnMouseUp(e);
        }
        /// <summary>
        /// Effettua lo spostamento dell'area visibile della mappa calcolando il nuovo centro.
        /// </summary>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (dragging)
            {
                PxCoordinates pxc = map.mapsystem.PointToPx(Center, Zoom);
                pxc.xpx -= e.X - drag_lastx;
                pxc.ypx -= e.Y - drag_lasty;

                this.drag_lastx = e.X;
                this.drag_lasty = e.Y;

                this.Center = map.mapsystem.PxToPoint(pxc, this.Zoom);
            }
        }

#if !(PocketPC || Smartphone || WindowsCE)
        protected override void OnResize(EventArgs e)
        {
            Invalidate();
            base.OnResize(e);
            if (HiResModeBuffer != null) 
            {
                HiResModeBuffer.Dispose();
                HiResModeBuffer = null;
            }
        }
#endif
        /// <summary>
        /// Indica/imposta la visualizzazione di una croce centrale e le corrispondenti coordinate geografiche
        /// </summary>
        public bool ShowPosition
        {
            get
            {
                return _showcross;
            }
            set
            {
                if (_showcross != value)
                {
                    _showcross = value;
                    Invalidate();
                }
            }
        }
        /// <summary>
        /// Indica/imposta la visualizzazione di un riferimento di scala
        /// </summary>
        public bool ShowScaleRef
        {
            get
            {
                return _showscaleref;
            }
            set
            {
                if (_showscaleref != value)
                {
                    _showscaleref = value;
                    Invalidate();
                }
            }
        }

        private bool _hiresmode;
        /// <summary>
        /// Modalità schermo ad alta risoluzione
        /// </summary>
        /// <remarks>Nella modalità ad alta risoluzione le mappe vengono ingrandite di un fattore 2 per migliorare la leggibilità</remarks>
        public bool HiResMode
        {
            get { return _hiresmode; }
            set
            {
                _hiresmode = value;
                Invalidate();
                if (value == false)
                {
                    HiResModeBuffer.Dispose();
                    HiResModeBuffer = null;
                }
            }
        }

        public bool HiResModeCustomDraw { get; set; }

        private Bitmap HiResModeBuffer;

        /// <summary>
        /// Dimensione in pixel dell'aria di visualizzazione della mappa. 
        /// Normalmente corrisponde alla dimensione del controllo stesso, più piccola se HiResMode è true.
        /// </summary>
        public Size MapViewportSize
        {
            get
            {
                return HiResMode ? new Size(this.Size.Width / 2, this.Size.Height / 2) : this.Size;
            }
        }

        public static void ResizeImage(Bitmap originalBitmap, Bitmap resultBitmap)
        {
            int originalWidth = originalBitmap.Width;
            int originalHeight = originalBitmap.Height;

            BitmapData originalData = originalBitmap.LockBits(
                new Rectangle(0, 0, originalWidth, originalHeight),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppRgb);
            BitmapData resultData = resultBitmap.LockBits(
                new Rectangle(0, 0, resultBitmap.Width, resultBitmap.Height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppRgb);

            int resultStride = resultData.Stride;
            int originalStride = originalData.Stride;

            unsafe
            {
                Int32* originalPointer = (Int32*)originalData.Scan0.ToPointer();
                Int32* resultPointer = (Int32*)resultData.Scan0.ToPointer();
                Int32* endimg = (Int32*)((byte*)originalPointer + originalStride * originalHeight);
                while (originalPointer < endimg) 
                {
                    Int32* endline = (Int32 *)((byte*)originalPointer + originalStride);
                    Int32* line2 = (Int32*)((byte*)resultPointer + resultStride);
                    while(originalPointer < endline) 
                    {
                        //*resultPointer++ = *originalPointer;
                        //*resultPointer++ = *originalPointer;
                        //*line2++ = *originalPointer;
                        //*line2++ = *originalPointer++;
                        *resultPointer++ = *resultPointer++ = *line2++ = *line2++ = *originalPointer++;

                    }
                    resultPointer = line2;
                }
                originalBitmap.UnlockBits(originalData);
                resultBitmap.UnlockBits(resultData);
            }
        }

    }
}