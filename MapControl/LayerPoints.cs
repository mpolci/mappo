using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace MapsLibrary
{
    public class PointsCollection
    {
        public interface Iterator
        {
            void process(ProjectedGeoPoint p);
        }

        protected class QTreeNode
        {
            ProjectedGeoPoint pMin, pMax;

            public QTreeNode[,] q;
            const int BUCKSIZE = 4;
            ProjectedGeoPoint[] points;
            Int16 count;

            public QTreeNode(ProjectedGeoPoint min, ProjectedGeoPoint max)
            {
                pMin = min;
                pMax = max;
                points = new ProjectedGeoPoint[BUCKSIZE];
                count = 0;
            }

            public ProjectedGeoArea area
            {
                get
                {
                    return new ProjectedGeoArea(pMin, pMax);
                }
            }
        
            private void createQ1(ProjectedGeoPoint middle)
            {
                q[0,0] = new QTreeNode(pMin, middle);
            }
            private void createQ2(ProjectedGeoPoint middle)
            {
                q[1,0] = new QTreeNode(new ProjectedGeoPoint(pMin.nLat, middle.nLon), new ProjectedGeoPoint(middle.nLat, pMax.nLon));
            }
            private void createQ3(ProjectedGeoPoint middle)
            {
                q[0,1] = new QTreeNode(new ProjectedGeoPoint(middle.nLat, pMin.nLon), new ProjectedGeoPoint(pMax.nLat, middle.nLon));
            }
            private void createQ4(ProjectedGeoPoint middle)
            {
                q[1,1] = new QTreeNode(middle, pMax);
            }

            public void addPoint(ProjectedGeoPoint point)
            {
                if (q == null && count < BUCKSIZE) {
                    // aggiunge il punto al bucket se non è già presente
                    bool present = false;
                    for (int i = 0; i < BUCKSIZE; i++) {
                        if (points[i] == point) {
                            present = true;
                            break;
                        }
                    }
                    if (!present) 
                        points[count++] = point;
                } else {
                    ProjectedGeoPoint middle = ProjectedGeoPoint.middle(pMin, pMax);
                    // se middle == pMin abbiamo raggiunto la profondità massima, 
                    // non possiamo avere altri livelli né aggiungerli, quindi il nuovo 
                    // punto viene scartato.
                    if (middle != pMin)
                    {
                        int x, y;
                        if (count == BUCKSIZE)
                        {
                            // Il bucket è già pieno, quindi bisogna dividere 
                            // il nodo in 4 aggiungendo un livello
                            this.q = new QTreeNode[2, 2];
                            createQ1(middle);
                            createQ2(middle);
                            createQ3(middle);
                            createQ4(middle);

                            foreach (ProjectedGeoPoint cgp in points)
                            {
                                x = cgp.nLon < middle.nLon ? 0 : 1;
                                y = cgp.nLat < middle.nLat ? 0 : 1;
                                q[x, y].addPoint(cgp);
                            }
                            points = null;
                            count = 0;
                        }
                        x = point.nLon < middle.nLon ? 0 : 1;
                        y = point.nLat < middle.nLat ? 0 : 1;
                        q[x, y].addPoint(point);
                    }
                }
            }

            public QTreeNode expand(ProjectedGeoPoint direction) {
                ProjectedGeoPoint newMin, newMiddle, newMax;
                QTreeNode newNode;
                if (direction.nLon < pMin.nLon && direction.nLat < pMin.nLat) 
                {   // quadrante 4
                    newMin = new ProjectedGeoPoint(2 * pMin.nLat - pMax.nLat, 2 * pMin.nLon - pMax.nLon);
                    newMiddle = pMin;
                    newMax = pMax;
                    newNode = new QTreeNode(newMin, newMax);
                    newNode.q = new QTreeNode[2, 2];
                    newNode.createQ1(newMiddle);
                    newNode.createQ2(newMiddle);
                    newNode.createQ3(newMiddle);
                    newNode.q[1,1] = this;
                }
                else if (direction.nLon > pMin.nLon && direction.nLat < pMin.nLat) 
                {   // quadrante 3
                    newMin = new ProjectedGeoPoint(2 * pMin.nLat - pMax.nLat, pMin.nLon);
                    newMiddle = new ProjectedGeoPoint(pMin.nLat, pMax.nLon);
                    newMax = new ProjectedGeoPoint(pMax.nLat, 2 * pMax.nLon - pMin.nLon);
                    newNode = new QTreeNode(newMin, newMax);
                    newNode.q = new QTreeNode[2, 2];
                    newNode.createQ1(newMiddle);
                    newNode.createQ2(newMiddle);
                    newNode.createQ4(newMiddle);
                    newNode.q[0,1] = this;
                }
                else if (direction.nLon < pMin.nLon && direction.nLat > pMin.nLat) 
                {   // quadrante 2
                    newMin = new ProjectedGeoPoint(pMin.nLat, 2 * pMin.nLon - pMax.nLon);
                    newMiddle = new ProjectedGeoPoint(pMax.nLat, pMin.nLon);
                    newMax = new ProjectedGeoPoint(2 * pMax.nLat - pMin.nLat, pMax.nLon);
                    newNode = new QTreeNode(newMin, newMax);
                    newNode.q = new QTreeNode[2, 2];
                    newNode.createQ1(newMiddle);
                    newNode.createQ3(newMiddle);
                    newNode.createQ4(newMiddle);
                    newNode.q[1,0] = this;
                }
                else if (direction.nLon > pMin.nLon && direction.nLat > pMin.nLat) 
                {   // quadrante 1
                    newMax = new ProjectedGeoPoint(2 * pMax.nLat - pMin.nLat, 2 * pMax.nLon - pMin.nLon);
                    newMiddle = pMax;
                    newMin = pMin;
                    newNode = new QTreeNode(newMin, newMax);
                    newNode.q = new QTreeNode[2, 2];
                    newNode.createQ2(newMiddle);
                    newNode.createQ3(newMiddle);
                    newNode.createQ4(newMiddle);
                    newNode.q[0,0] = this;
                } else newNode = this;
                return newNode;
            }

            public bool contains(ProjectedGeoPoint point)
            {
                return (pMin.nLat <= point.nLat && point.nLat <= pMax.nLat) && 
                       (pMin.nLon <= point.nLon && point.nLon <= pMax.nLon);
            }

            /// <param name="maxdepth">Massima profondità da raggiungere. Se il valore è negativo arriva fino alle foglie.</param>
            public void iterate(Iterator iterator, int maxdepth)
            {
                if (maxdepth == 0)
                {
                    // profondità massima raggiunta, elabora l'angolo superiore sinistro in sostituzione di tutti quelli contenuti
                    iterator.process(this.pMin);
                }
                else if (q == null)
                {
                    for (int i = 0; i < this.count; i++)
                        iterator.process(points[i]);
                }
                else 
                {
                    foreach (QTreeNode n in q)
                        n.iterate(iterator, maxdepth - 1);
                }
            }

            /// <param name="fa">Elabora solo i punti interni a quest'area</param>
            /// <param name="maxdepth">Massima profondità da raggiungere. Se il valore è negativo arriva fino alle foglie.</param>
            public void iterate(ProjectedGeoArea fa, Iterator iterator, int maxdepth)
            {
                if (maxdepth == 0) 
                {
                    // profondità massima raggiunta, elabora l'angolo superiore sinistro in sostituzione di tutti quelli contenuti
                    iterator.process(this.pMin);
                }
                else if (q == null) {
                    for (int i = 0; i < this.count; i++)
                        iterator.process(points[i]);
                }
                else 
                {
                    switch (fa.testIntersection(new ProjectedGeoArea(pMin, pMax)))
                    {
                        case AreaIntersectionType.fullContains:
                            this.iterate(iterator, maxdepth);
                            break;
                        case AreaIntersectionType.partialIntersection:
                        case AreaIntersectionType.fullContained:
                            foreach (QTreeNode n in q)
                                n.iterate(fa, iterator, maxdepth - 1);
                            break;
                        case AreaIntersectionType.noItersection:
                            // niente da fare
                            break;
                    }
                }
            }
        }


        QTreeNode root;

        public ProjectedGeoArea ContainigArea
        {
            get
            {
                if (root == null) return new ProjectedGeoArea(new ProjectedGeoPoint(0, 0), new ProjectedGeoPoint(0, 0));
                else return root.area;
            }
        }
        //double roothalfsize;

        public void addPoint(ProjectedGeoPoint point)
        {
            if (root == null)
            {
                // ATTENZIONE nelle vecchie versioni c'era un errore!
//                GeoPoint min = new GeoPoint(tc.dLat - 0.005, tc.dLon - 0.005),
//                         max = new GeoPoint(tc.dLat + 0.014, tc.dLon + 0.014);
                ProjectedGeoPoint min, max;

                min = point;
                min.nLat /= 0x10000; min.nLat *= 0x10000;
                min.nLon /= 0x10000; min.nLon *= 0x10000;
                max = min;    max.nLat += 0x10000;  max.nLon += 0x10000;
                this.root = new QTreeNode(min, max);
            }
            while (!root.contains(point)) {
                root = root.expand(point);
            }
            root.addPoint(point);
        }

        /// <param name="maxdepth">Massima profondità da raggiungere. Se il valore è negativo arriva fino alle foglie.</param>
        public void Iterate(ProjectedGeoArea filterarea, Iterator iterator, int maxdepth)
        {
            if (root != null) root.iterate(filterarea, iterator, maxdepth);
        }

    }



    public class LayerPoints : IMap
    {
        protected PointsCollection points;
        //System.Drawing.Pen pen;
        //protected System.Drawing.Brush brush;
        protected MercatorProjectionMapSystem mapsys;
        protected static readonly double ln2 = Math.Log(2);
        
        public event MapChangedEventHandler MapChanged;

        public MercatorProjectionMapSystem mapsystem
        {
            get { return mapsys; }
        }

        
        public delegate void CustomDrawPointFn(Graphics g, Point p, object arg);
        protected CustomDrawPointFn customdrawfn;
        protected object customdrawarg;
        
        public void SetDrawPointFunction(CustomDrawPointFn fn, object fnarg)
        {
            customdrawfn = fn;
            customdrawarg = fnarg;
        }
        
        public static void DrawFilledSquare(Graphics g, Point p, object brush)
        {
            g.FillRectangle((Brush) brush, p.X - 1, p.Y - 1, 3, 3);
        }

        public static void DrawEmptySquare(Graphics g, Point p, object pen)
        {
            g.DrawRectangle((Pen) pen, p.X - 1, p.Y - 1, 3, 3);
        }

        protected class DrawPointIterator : PointsCollection.Iterator
        {
            System.Drawing.Graphics gDst;
            Point pDestDelta;
            /// <summary>
            /// angolo corrispondente al pixel di coordinate (0, 0) dell'area del Graphics
            /// </summary>
            PxCoordinates pxCorner;  
            uint zoom;
            //System.Drawing.Size szSize;
            //Pen pen;
            //Brush brush;
            CustomDrawPointFn drawfn;
            object drawfnarg;
            private MercatorProjectionMapSystem mapsys;

            public DrawPointIterator(System.Drawing.Graphics dst, CustomDrawPointFn fn, object fnarg, PxCoordinates corner, uint z, MercatorProjectionMapSystem mapsystem)
            {
                mapsys = mapsystem;
                this.gDst = dst;
                //pDestDelta = delta;
                pxCorner = corner;
                this.zoom = z;
                //pen = p;
                //brush = b;
                drawfn = fn;
                drawfnarg = fnarg;
            }

            #region Iterator Members

            public void process(ProjectedGeoPoint pgp)
            {
                PxCoordinates pxc = mapsys.PointToPx(pgp, this.zoom);
                Point p = new Point((int)(pxc.xpx - pxCorner.xpx),
                                    (int)(pxc.ypx - pxCorner.ypx));
                drawfn(gDst, p, drawfnarg);
            }

            #endregion
        }



        public LayerPoints(MercatorProjectionMapSystem mapsystem)
        {
            mapsys = mapsystem;
            points = new PointsCollection();
            //pen = new Pen(Color.Blue);
            //brush = new System.Drawing.SolidBrush(Color.Blue);
        }

        public virtual void addPoint(ProjectedGeoPoint point)
        {
            points.addPoint(point);
            if (MapChanged != null)
                MapChanged(this, new ProjectedGeoArea(point, point));
        }

        #region ILayer Members

        public virtual void drawImageMapAt(Graphics dst, Point delta, ProjectedGeoArea area, uint zoom)
        {
            // calcola la profondità massima per la visita dell'albero
            PxCoordinates pxcsize = mapsys.PointToPx(area.pMax - area.pMin, zoom);
            int depth = calcMaxDepth(area, new Size((int)pxcsize.xpx, (int)pxcsize.ypx));
            PxCoordinates pxGraphCorner = mapsys.PointToPx(area.pMin, zoom);
            pxGraphCorner.xpx -= delta.X;
            pxGraphCorner.ypx -= delta.Y;
            if (this.customdrawfn != null)
            {
                DrawPointIterator pi = new DrawPointIterator(dst, customdrawfn, customdrawarg, pxGraphCorner, zoom, this.mapsys);
                points.Iterate(area, pi, depth);
            }
            else using (Brush brush = new SolidBrush(Color.Blue))
            {
                DrawPointIterator pi = new DrawPointIterator(dst, DrawFilledSquare, brush, pxGraphCorner, zoom, this.mapsys);
                points.Iterate(area, pi, depth);
            }
        }

        /*
        public virtual void drawImageMapAt(Graphics dst, ProjectedGeoPoint center, uint zoom, Size size)
        {
            PxCoordinates pxCorner; 
            pxCorner = mapsys.PointToPx(center, zoom);
            pxCorner.xpx -= size.Width / 2;
            pxCorner.ypx -= size.Height / 2;   
            // calcola la profondità massima per la visita dell'albero            
            ProjectedGeoArea area = calcArea(center, zoom, size);
            int depth = calcMaxDepth(area, size);

            if (this.customdrawfn != null)
            {
                DrawPointIterator pi = new DrawPointIterator(dst, customdrawfn, customdrawarg, pxCorner, zoom, this.mapsys);
                points.Iterate(area, pi, depth);
            }
            else using (Brush brush = new SolidBrush(Color.Blue))
            {
                DrawPointIterator pi = new DrawPointIterator(dst, DrawFilledSquare, brush, pxCorner, zoom, this.mapsys);
                points.Iterate(area, pi, depth);
            }            
        }
        */
        #endregion

        protected int calcMaxDepth(ProjectedGeoArea drawarea, Size wsize)
        {
            // calcola la profondità massima per la visita dell'albero            
            double dContaining = (double) Math.Max(points.ContainigArea.height, points.ContainigArea.width),
                   dWindow = (double) Math.Max(drawarea.height, drawarea.width);
            // int depth = (int)Math.Ceiling(Math.Log(dContaining / dWindow) / ln2) + (int)Math.Ceiling(Math.Log(size.Width) / ln2); 
            int depth = (int)Math.Ceiling(Math.Log(dContaining / dWindow) / ln2 + Math.Log(Math.Max(wsize.Width, wsize.Height)) / ln2);
            // a quanto pare se dContaining è 0 alla fine il risultato di depth è 0, anche se (int) di -infinito non è ovvio che dia 0
            return depth;
        }

        protected ProjectedGeoArea calcArea(ProjectedGeoPoint center, uint zoom, Size size)
        {
            PxCoordinates p1, p2;
            p1 = mapsys.PointToPx(center, zoom);
            p1.xpx -= size.Width / 2;
            p1.ypx -= size.Height / 2;   
            p2 = p1;
            p2.xpx += size.Width;   // forse bisognerebbe aumentare di un pixel e poi scalare di uno il punto nella geoarea
            p2.ypx += size.Height;  
            return new ProjectedGeoArea(mapsys.PxToPoint(p1, zoom), mapsys.PxToPoint(p2, zoom));
        }
    }

    /*
    public class LayerBufferedPoints : LayerPoints
    {
        protected Bitmap buffer;
        protected uint buffer_zoom;
        protected ProjectedGeoArea buffer_area;

        public LayerBufferedPoints(MercatorProjectionMapSystem mapsystem)
            : base(mapsystem)
        {
        }

        // forse questa classe posso eliminarla quindi non è necessario reimplementare questo metodo
        public virtual void drawImageMapAt(Graphics dst, Point delta, ProjectedGeoArea area, uint zoom)
        {
            throw new NotImplementedException();
        }

        // forse questa classe posso eliminarla quindi non è necessario reimplementare questo metodo
        public override void drawImageMapAt(Graphics dst, ProjectedGeoPoint center, uint zoom, Size size)
        {
            PxCoordinates corner = mapsystem.PointToPx(center, zoom);
            corner.xpx -= size.Width / 2;
            corner.ypx -= size.Height / 2;   // la coordinata y sale al decrescere della latitudine
            ProjectedGeoArea drawarea = calcArea(center, zoom, size);
            int maxdepth = calcMaxDepth(drawarea, size);
            ProjectedGeoArea[] zones;

            CustomDrawPointFn fn;
            object arg;
            if (customdrawfn != null) {
                fn = customdrawfn;
                arg = customdrawarg;
            } else {
                fn = DrawFilledSquare;
                arg = new SolidBrush(Color.Blue);
            }
            try
            {

                Bitmap newbuffer;
                Graphics outg;
                PxCoordinates bufpxpos = mapsystem.PointToPx(buffer_area.pMin, zoom);
                Point bufpos = new Point((int)(bufpxpos.xpx - corner.xpx), (int)(bufpxpos.ypx - corner.ypx));
                int movecx = Math.Abs(bufpos.X) + Math.Abs(bufpos.Y); //è una valutazione dello spostamento
                if (zoom == buffer_zoom && movecx < 50) // questo è un valore empirico
                {
                    // per piccoli spostamenti non rimpiazzo il buffer

                    System.Drawing.Imaging.ImageAttributes attr = new System.Drawing.Imaging.ImageAttributes();
                    // determina la parte effettiva della cache da copiare in output (in pratica serve quando l'area coperta dalla collezione di punti non copre tutto il buffer)
                    PxCoordinates cont_px1 = mapsystem.PointToPx(points.ContainigArea.pMin, zoom),
                                  cont_px2 = mapsystem.PointToPx(points.ContainigArea.pMax, zoom);
                    int cont_sx = (int)(cont_px2.xpx - cont_px1.xpx),
                        cont_sy = (int)(cont_px2.ypx - cont_px1.ypx);
                    cont_px1.xpx -= corner.xpx; cont_px1.ypx -= corner.ypx;
                    int effx = Math.Max(bufpos.X, (int)cont_px1.xpx),
                        effy = Math.Max(bufpos.Y, (int)cont_px1.ypx),
                        sx = Math.Min(size.Width - (effx - bufpos.X), cont_sx),
                        sy = Math.Min(size.Height - (effy - bufpos.Y), cont_sy);
                    Rectangle dstRect = new Rectangle(effx, effy, sx, sy);
                    // imposta la trasparenza
                    attr.SetColorKey(Color.Black, Color.Black);
                    dst.DrawImage(buffer, dstRect, effx - bufpos.X, effy - bufpos.Y, sx, sy, GraphicsUnit.Pixel, attr);
                    zones = drawarea.difference(this.buffer_area);
                    DrawPointIterator pit = new DrawPointIterator(dst, fn, arg, corner, zoom, this.mapsys);
                    foreach (ProjectedGeoArea pr_area in zones)
                        points.Iterate(pr_area, pit, maxdepth);
                }
                else
                {
                    newbuffer = new Bitmap(size.Width, size.Height);
                    using (outg = Graphics.FromImage(newbuffer))
                    {
                        if (zoom != buffer_zoom)
                            zones = new ProjectedGeoArea[] { drawarea };
                        else
                        {
                            outg.DrawImage(buffer, bufpos.X, bufpos.Y);
                            zones = drawarea.difference(this.buffer_area);
                        }
                        DrawPointIterator pit = new DrawPointIterator(outg, fn, arg, corner, zoom, this.mapsys);
                        foreach (ProjectedGeoArea pr_area in zones)
                            points.Iterate(pr_area, pit, maxdepth);
                    }
                    if (buffer != null) buffer.Dispose();
                    buffer = newbuffer;
                    buffer_area = drawarea;
                    buffer_zoom = zoom;

                    // determina la parte effettiva della cache da copiare in output (in pratica serve quando l'area coperta dalla collezione di punti non copre tutto il buffer)
                    PxCoordinates cont_px1 = mapsystem.PointToPx(points.ContainigArea.pMin, zoom),
                                  cont_px2 = mapsystem.PointToPx(points.ContainigArea.pMax, zoom);
                    int cont_sx = (int)(cont_px2.xpx - cont_px1.xpx),
                        cont_sy = (int)(cont_px2.ypx - cont_px1.ypx);
                    cont_px1.xpx -= corner.xpx; cont_px1.ypx -= corner.ypx;
                    int effx = (cont_px1.xpx > 0) ? (int)cont_px1.xpx : 0,
                        effy = (cont_px1.ypx > 0) ? (int)cont_px1.ypx : 0,
                        sx = Math.Min(size.Width - effx, cont_sx),
                        sy = Math.Min(size.Height - effy, cont_sy);
                    Rectangle dstRect = new Rectangle(effx, effy, sx, sy);
                    // imposta la trasparenza
                    System.Drawing.Imaging.ImageAttributes attr = new System.Drawing.Imaging.ImageAttributes();
                    attr.SetColorKey(Color.Black, Color.Black);
                    // output definitivo del layer
                    dst.DrawImage(buffer, dstRect, effx, effy, sx, sy, GraphicsUnit.Pixel, attr);
                }
            }
            finally
            {
                if (customdrawfn == null)
                    ((Brush)arg).Dispose();
            }
        }

        public override void addPoint(ProjectedGeoPoint point)
        {
            base.addPoint(point);
            if (buffer_area.contains(point))
            {
                // devo disegnare il punto nel buffer
                Point pos = (Point) (mapsys.PointToPx(point, buffer_zoom) -
                                     mapsys.PointToPx(buffer_area.pMin, buffer_zoom));
                using (Graphics gDst = Graphics.FromImage(buffer))
                {
                    if (customdrawfn != null)
                        customdrawfn(gDst, pos, customdrawarg);
                    else using (Brush b = new SolidBrush(Color.Blue))
                        DrawFilledSquare(gDst, pos, b);
                }
            }
        }

    }
    */
}
