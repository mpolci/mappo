using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace MapsLibrary
{
    public abstract class SpatialObject
    {
        public abstract ProjectedGeoArea ContainigArea { get; }
        public abstract bool Contains(ProjectedGeoPoint point);
        //public abstract bool Intersect(ProjectedGeoArea area);
    }

    public class SpatialObjectCollection
    {

        protected class QTreeNode
        {
            ProjectedGeoPoint pMin, pMax;

            public QTreeNode[,] q;
            // TODO: determinare il valore ideale di BUCKSIZE
            const int BUCKSIZE = 4;
            LinkedList<SpatialObject> bucket;

            public QTreeNode(ProjectedGeoPoint min, ProjectedGeoPoint max)
            {
                pMin = min;
                pMax = max;
                bucket = new LinkedList<SpatialObject>();
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

            private void transferObjectToChild(ProjectedGeoPoint middle)
            {
                LinkedListNode<SpatialObject> current = bucket.First,
                                              next;
                while (current != null)
                {
                    ProjectedGeoPoint c = current.Value.ContainigArea.center;
                    int x = c.nLon < middle.nLon ? 0 : 1;
                    int y = c.nLat < middle.nLat ? 0 : 1;
                    next = current.Next;
                    if (q[x, y].area.testIntersection(current.Value.ContainigArea) == AreaIntersectionType.fullContains)
                    {
                        q[x, y].AddObject(current.Value);
                        //TODO: controllare che quest'operazione non mandi in tilt il ciclo
                        bucket.Remove(current);
                    }
                    current = next;
                }
            }

            public void AddObject(SpatialObject sobj)
            {
                System.Diagnostics.Debug.Assert(this.area.testIntersection(sobj.ContainigArea) == AreaIntersectionType.fullContains, "QTreeNode.AddObject(): object too big");
                ProjectedGeoPoint middle = ProjectedGeoPoint.middle(pMin, pMax);
                // Se middle == pMin abbiamo raggiunto la profondità massima, non possiamo avere
                // altri livelli né aggiungerli. L'oggetto va inserito per forza nel livello corrente.
                if ((bucket.Count < BUCKSIZE && q == null) || middle == pMin)
                {
                    bucket.AddLast(sobj);
                }
                else
                {
                    if (q == null)
                    {
                        // Creo i 4 sottonodi e cerco di spostarci gli elementi del bucket
                        this.q = new QTreeNode[2, 2];
                        createQ1(middle);
                        createQ2(middle);
                        createQ3(middle);
                        createQ4(middle);

                        transferObjectToChild(middle);
                    }
                    // Aggiunge il nuovo oggetto in uno dei sottonodi se in grado di contenerlo, altrimenti nel bucket.
                    ProjectedGeoPoint c = sobj.ContainigArea.center;
                    int x = c.nLon < middle.nLon ? 0 : 1;
                    int y = c.nLat < middle.nLat ? 0 : 1;
                    if (q[x, y].area.testIntersection(sobj.ContainigArea) == AreaIntersectionType.fullContains)
                        q[x, y].AddObject(sobj);
                    else
                        bucket.AddLast(sobj);
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
            
            //public bool contains(ProjectedGeoPoint point)
            //{
            //    return (pMin.nLat <= point.nLat && point.nLat <= pMax.nLat) && 
            //           (pMin.nLon <= point.nLon && point.nLon <= pMax.nLon);
            //}

            /// <summary>
            /// Visita dell'albero. Tutti gli oggetti contenuti vengono elaborati.
            /// </summary>
            /// <param name="maxdepth">Massima profondità da raggiungere. Se il valore è negativo arriva fino alle foglie.</param>
            public IEnumerable iterate(int maxdepth)
            {
                foreach (SpatialObject o in bucket)
                    yield return o;

                if (q != null && maxdepth != 0)
                    foreach (QTreeNode n in q) 
                        foreach (SpatialObject o in n.iterate(maxdepth - 1))
                            yield return o;
                        //n.iterate(iterator, maxdepth - 1);
            }

            /// <summary>
            /// Visita l'albero elaborando gli oggetti che intersecano un'area.
            /// </summary>
            /// <param name="filterarea">Elabora solo gli oggetti che intersecano quest'area.</param>
            /// <param name="maxdepth">Massima profondità da raggiungere. Se il valore è negativo arriva fino alle foglie.</param>
            public IEnumerable iterate(ProjectedGeoArea filterarea, int maxdepth)
            {
                switch (filterarea.testIntersection(this.area))
                {
                    case AreaIntersectionType.fullContains:
                        foreach (SpatialObject o in this.iterate(maxdepth))
                            yield return o;
                        break;
                    case AreaIntersectionType.partialIntersection:
                    case AreaIntersectionType.fullContained:
                        foreach (SpatialObject o in bucket)
                            if (filterarea.testIntersection(o.ContainigArea) != AreaIntersectionType.noItersection)
                                yield return o;
                        if (q != null && maxdepth != 0)
                            foreach (QTreeNode n in q)
                                foreach (SpatialObject o in n.iterate(filterarea, maxdepth - 1))
                                    yield return o;
                        break;
                    case AreaIntersectionType.noItersection:
                        // niente da fare
                        break;
                }
            }

            /// <summary>
            /// Visita l'albero elaborando gli oggetti che contengono un determinato punto.
            /// </summary>
            /// <param name="point">Elabora solo gli oggetti che contengono questo punto. 
            /// Questo punto deve essere interno all'area del nodo, altrimenti si raggiunge inutilmente una foglia dell'albero
            /// </param>
            /// <param name="iterator"></param>
            /// <param name="maxdepth">Massima profondità da raggiungere. Se il valore è negativo arriva fino alle foglie.</param>
            public IEnumerable iterate(ProjectedGeoPoint point, int maxdepth)
            {
                //if (!this.area.contains(point))
                //    return;

                foreach (SpatialObject o in bucket)
                    if (o.ContainigArea.contains(point))
                        yield return o;

                if (q != null && maxdepth != 0)
                {
                    ProjectedGeoPoint middle = this.area.center;
                    int x = point.nLon < middle.nLon ? 0 : 1;
                    int y = point.nLat < middle.nLat ? 0 : 1;
                    foreach (SpatialObject o in q[x, y].iterate(point, maxdepth - 1))
                        yield return o;
                }
            }

            public bool remove(SpatialObject o)
            {
                if (area.testIntersection(o.ContainigArea) != AreaIntersectionType.fullContains)
                    return false;
                else
                {
                    // prima cerca di rimuovere l'oggetto nei sottorami e solo nel caso non venga trovato cerca nel bucket
                    if (q != null)
                    {
                        ProjectedGeoPoint point = o.ContainigArea.center;
                        ProjectedGeoPoint middle = this.area.center;
                        int x = point.nLon < middle.nLon ? 0 : 1;
                        int y = point.nLat < middle.nLat ? 0 : 1;
                        if (q[x, y].remove(o))
                            return true;
                    }

                    return bucket.Remove(o);
                }
            }


        }  // end QTreeNode


        QTreeNode root;

        public ProjectedGeoArea ContainigArea
        {
            get
            {
                if (root == null) return new ProjectedGeoArea(new ProjectedGeoPoint(0, 0), new ProjectedGeoPoint(0, 0));
                else return root.area;
            }
        }

        public void AddObject(SpatialObject o)
        {
            ProjectedGeoPoint objcenter = o.ContainigArea.center;
            if (root == null)
                InitRoot(objcenter);
            
            // Espande l'albero finché non contiene completamente l'oggetto da inserire.
            while (root.area.testIntersection(o.ContainigArea) != AreaIntersectionType.fullContains)
                root = root.expand(objcenter);
            root.AddObject(o);
        }

        /// <summary>
        /// Inizializza l'albero con un primo nodo di una dimensione predefinita e con una 
        /// suddivizione del sistema di coordinate compatibile con il meccanismo dei tile. 
        /// </summary>
        /// <param name="center">Riferimento per determinare il primon nodo. Tale punto sarà interno al nodo.</param>
        private void InitRoot(ProjectedGeoPoint internalpoint)
        {
            ProjectedGeoPoint min, max;

            min = internalpoint;
            // Le istruzioni qui sotto equivalgono ad un AND sui bit con 0xFFFF0000 
            //min.nLat /= 0x10000; min.nLat *= 0x10000;
            //min.nLon /= 0x10000; min.nLon *= 0x10000;
            const int mask = -1 << 16;  // 0xFFFF0000
            const int size = 1 << 16;  // 0x00010000
            System.Diagnostics.Debug.Assert(mask == 0xFFFF0000 && size == 0x00010000, "addPoint: incorrect mask and size");
            min.nLat &= mask;
            min.nLon &= mask;
            max = min; max.nLat += size; max.nLon += size;
            this.root = new QTreeNode(min, max);
        }

        /// <summary>
        /// Elabora gli oggetti che intersecano una determinata area.
        /// </summary>
        /// <param name="maxdepth">Massima profondità da raggiungere. Se il valore è negativo arriva fino alle foglie.</param>
        public IEnumerable Iterate(ProjectedGeoArea filterarea, int maxdepth)
        {
            if (root != null)
                return root.iterate(filterarea, maxdepth);
            else
                return emptyenum();
        }

        /// <summary>
        /// Elabora gli oggetti che contengono un determinato punto.
        /// </summary>
        /// <param name="point">Elabora solo gli oggetti che contengono questo punto.</param>
        /// <param name="maxdepth">Massima profondità da raggiungere. Se il valore è negativo arriva fino alle foglie.</param>
        public IEnumerable Iterate(ProjectedGeoPoint point, int maxdepth)
        {
            if (root != null && root.area.contains(point))
                return root.iterate(point, maxdepth);
            else
                return emptyenum();
        }

        private IEnumerable emptyenum()
        {
            yield break;
        }

        /// <summary>
        /// Rimuove un elemento.
        /// </summary>
        /// <param name="o"></param>
        /// <returns>true se l'elemento è stato rimosso. false se non trovato.</returns>
        public bool Remove(SpatialObject o)
        {
            if (root != null)
                return root.remove(o);
            else
                return false;
        }
    }
}
