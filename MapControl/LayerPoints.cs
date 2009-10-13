/*******************************************************************************
 *  Mappo! - A tool for gps mapping.
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
            //Point pDestDelta;
            /// <summary>
            /// angolo corrispondente al pixel di coordinate (0, 0) dell'area del Graphics
            /// </summary>
            PxCoordinates pxCorner;  
            uint zoom;
            CustomDrawPointFn drawfn;
            object drawfnarg;
            private MercatorProjectionMapSystem mapsys;

            public DrawPointIterator(System.Drawing.Graphics dst, CustomDrawPointFn fn, object fnarg, PxCoordinates corner, uint z, MercatorProjectionMapSystem mapsystem)
            {
                mapsys = mapsystem;
                this.gDst = dst;
                pxCorner = corner;
                this.zoom = z;
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
        }

        public virtual void addPoint(ProjectedGeoPoint point)
        {
            points.addPoint(point);
            if (MapChanged != null)
                MapChanged(this, new ProjectedGeoArea(point, new ProjectedGeoPoint(point.nLat + 1, point.nLon +1)));
        }

        /// <summary>
        /// Elimina tutti i punti dal layer
        /// </summary>
        public virtual void clear()
        {
            points = new PointsCollection();
            if (MapChanged != null)
                MapChanged(this, this.mapsystem.FullMapArea);
        }

        #region ILayer Members

        public virtual void drawImageMapAt(ProjectedGeoPoint map_center, uint zoom, ProjectedGeoArea area, Graphics dst, Point delta)
        {
            // calcola la profondità massima per la visita dell'albero
            PxCoordinates pxcsize = mapsys.PointToPx(area.pMax - area.pMin, zoom) + new PxCoordinates(1, 1); ;
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

}
