using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace MapsLibrary
{
    public partial class MapControl : UserControl
    {
        private IMap map;
        private ProjectedGeoPoint pgpCenter;
        private uint uZoom;
        //private Bitmap buffer;

        protected Bitmap buffer;
        protected uint buffer_zoom;
        protected ProjectedGeoArea buffer_area;

        // per le operazioni di dragging
        private int drag_lastx, drag_lasty;
        private bool dragging;

        public delegate void MapControlEventHandler(MapControl sender);
        public event MapControlEventHandler PrePaint;
        public event MapControlEventHandler PositionChanged;
        public event MapControlEventHandler ZoomChanged;

        public MapControl()
        {
            InitializeComponent();
            uZoom = 0;
            pgpCenter = new ProjectedGeoPoint();
            dragging = false;
        }


        /// <summary>
        /// Mappa disegnata dal controllo
        /// </summary>
        /// <remarks>bla bla bla</remarks>
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
                    map.MapChanged += new MapChangedEventHandler(mapchanged);
                    Invalidate();
                }
            }
        }

        void mapchanged(IMap map, ProjectedGeoArea area)
        {
            //ProjectedGeoPoint delta = area.center - this.pgpCenter;
            //area.pMin -= delta;
            //area.pMax -= delta;
            PxCoordinates c = map.mapsystem.PointToPx(pgpCenter, uZoom);
            c.xpx -= this.Size.Width / 2;
            c.ypx -= this.Size.Height / 2;
            PxCoordinates px1 = map.mapsystem.PointToPx(area.pMin, uZoom),
                          px2 = map.mapsystem.PointToPx(area.pMax, uZoom);
            Rectangle rect = new Rectangle((int)(px1.xpx - c.xpx - 1), (int)(px1.ypx - c.ypx - 1), (int)(px2.xpx - px1.xpx + 3), (int)(px2.ypx - px1.ypx + 3));
            this.Invalidate(rect);
        }

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

        public ProjectedGeoArea VisibleArea
        {
            get
            {
                PxCoordinates c1, c2;
                c1 = this.map.mapsystem.PointToPx(this.Center, this.Zoom);
                c1.xpx -= this.Size.Width / 2;
                c1.ypx -= this.Size.Height / 2;
                c2 = c1;
                c2.xpx += this.Size.Width;
                c2.ypx += this.Size.Height;
                return new ProjectedGeoArea(map.mapsystem.PxToPoint(c1, this.Zoom), map.mapsystem.PxToPoint(c2, this.Zoom));
            }
        }
        /*
        protected override void OnPaint(PaintEventArgs e)
        {
            //base.OnPaint(e);
            if (map != null)
            {
                if (PrePaint != null) PrePaint(this);  // genera l'evento PrePaint
                if (buffer == null) buffer = new Bitmap(this.Size.Width, this.Size.Height);
                using (Graphics goff = Graphics.FromImage(buffer))
                    this.map.drawImageMapAt(goff, new Point(0, 0), this.VisibleArea, this.Zoom);
                //this.map.drawImageMapAt(goff, this.Center, this.Zoom, this.Size);
                e.Graphics.DrawImage(buffer, 0, 0);
            }
            else
            {
                // Nessuna mappa. Colora tutto di rosso.
                using (Brush b = new SolidBrush(Color.Red))
                    e.Graphics.FillRectangle(b, e.ClipRectangle);
            }
        }
        */
        protected override void OnPaint(PaintEventArgs e)
        {

            if (map != null)
            {
                if (PrePaint != null) PrePaint(this);  // genera l'evento PrePaint

                ProjectedGeoArea drawarea = this.VisibleArea;
                ProjectedGeoArea[] zones;
                PxCoordinates pxcWinCorner = map.mapsystem.PointToPx(drawarea.pMin, this.Zoom);

                Bitmap newbuffer = new Bitmap(this.Size.Width, this.Size.Height);
                // CODICE DI DEBUG
                System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
                watch.Start();
                // FINE CODICE DI DEBUG
                using (Graphics outg = Graphics.FromImage(newbuffer))
                {
                    if (this.Zoom != buffer_zoom)
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
                        PxCoordinates pxccorn = map.mapsystem.PointToPx(pr_area.pMin, Zoom);
                        PxCoordinates pxcCorner = map.mapsystem.PointToPx(pr_area.pMin, Zoom) - pxcWinCorner;

                        this.map.drawImageMapAt(outg, new Point((int)pxcCorner.xpx, (int)pxcCorner.ypx), pr_area, this.Zoom);
                    }
                }
                // CODICE DI DEBUG
                watch.Stop();
                // FINE CODICE DI DEBUG
                if (buffer != null) buffer.Dispose();
                buffer = newbuffer;
                buffer_area = drawarea;
                buffer_zoom = Zoom;

                e.Graphics.DrawImage(buffer, 0, 0);
                // CODICE DI DEBUG
                string msg = "Paint time: " + watch.Elapsed.TotalMilliseconds.ToString() + " ms";
                using (Font drawFont = new Font("Arial", 8, FontStyle.Regular))
                using (SolidBrush drawBrush = new SolidBrush(Color.Black))
                    e.Graphics.DrawString(msg, drawFont, drawBrush, 0, 0);
                // FINE CODICE DI DEBUG
            }
            else
            {
                // Nessuna mappa. Colora tutto di rosso.
                using (Brush b = new SolidBrush(Color.Red))
                    e.Graphics.FillRectangle(b, e.ClipRectangle);
            }
        }

        // Questo metodo vuoto evita il flickering quando non c'è double-buffering e comunque 
        // evita l'inutile riempimento dello sfondo
        protected override void OnPaintBackground(PaintEventArgs e)
        { }


        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            this.drag_lastx = e.X;
            this.drag_lasty = e.Y;
            this.dragging = true;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            this.dragging = false;
            base.OnMouseUp(e);
        }

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

    }
}