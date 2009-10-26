using System;
using System.Collections.Generic;
using System.Drawing;

namespace MapsLibrary
{
    public interface IDrawableObject 
    {
        void Draw(System.Drawing.Graphics dst, PxCoordinates corner, uint zoom, MercatorProjectionMapSystem mapsystem);
    }

    public class LayerSpatialObjects : IMap
    {
        protected SpatialObjectCollection objects;
        protected MercatorProjectionMapSystem mapsys;
        protected static readonly double ln2 = Math.Log(2);

        public event MapChangedEventHandler MapChanged;

        public MercatorProjectionMapSystem mapsystem
        {
            get { return mapsys; }
        }

        public LayerSpatialObjects(MercatorProjectionMapSystem mapsystem)
        {
            mapsys = mapsystem;
            objects = new SpatialObjectCollection();
        }

        /// <summary>
        /// Aggiunge l'oggetto al contenitore. Solo gli oggetti che implementano IDrawableObject saranno
        /// renderizzati.
        /// </summary>
        /// <param name="o"></param>
        public virtual void addObject(SpatialObject o)
        {
            objects.AddObject(o);

            if (MapChanged != null && typeof(IDrawableObject).IsAssignableFrom(o.GetType()) )
                MapChanged(this, o.ContainingArea);
        }

        public System.Collections.IEnumerable Iterate(ProjectedGeoPoint p)
        {
            return objects.Iterate(p, -1);
        }

        /// <summary>
        /// Elimina tutti i punti dal layer
        /// </summary>
        public virtual void clear()
        {
            ProjectedGeoArea area = objects.ContainigArea;
            objects = new SpatialObjectCollection();
            if (MapChanged != null)
                MapChanged(this, area);
        }

        public virtual void drawImageMapAt(ProjectedGeoPoint map_center, uint zoom, ProjectedGeoArea area, Graphics dst, Point delta)
        {
            // calcola la profondità massima per la visita dell'albero
            PxCoordinates pxcsize = mapsys.PointToPx(area.pMax - area.pMin, zoom) + new PxCoordinates(1, 1); ;
            int depth = calcMaxDepth(area, new Size((int)pxcsize.xpx, (int)pxcsize.ypx));
            PxCoordinates pxGraphCorner = mapsys.PointToPx(area.pMin, zoom);
            pxGraphCorner.xpx -= delta.X;
            pxGraphCorner.ypx -= delta.Y;

            foreach (SpatialObject so in objects.Iterate(area, depth))
            {
                IDrawableObject dso = so as IDrawableObject;
                if (dso != null)
                    dso.Draw(dst, pxGraphCorner, zoom, this.mapsys);
            }
        }

        /// <summary>
        /// Calcola la profondità massima necessaria per la visita dell'albero, supponendo di avere 
        /// una certa area da rappresentare su un'immagine di una certa grandezza in pixel.
        /// </summary>
        /// <param name="drawarea">Area da rappresentare.</param>
        /// <param name="wsize">Dimensione in pixel dell'immagine rappresentante l'area indicata.</param>
        /// <returns></returns>
        protected int calcMaxDepth(ProjectedGeoArea drawarea, Size wsize)
        {
            // calcola la profondità massima per la visita dell'albero            
            double dContaining = (double)Math.Max(objects.ContainigArea.height, objects.ContainigArea.width),
                   dWindow = (double)Math.Max(drawarea.height, drawarea.width);
            // int depth = (int)Math.Ceiling(Math.Log(dContaining / dWindow) / ln2) + (int)Math.Ceiling(Math.Log(size.Width) / ln2); 
            int depth = (int)Math.Ceiling(Math.Log(dContaining / dWindow) / ln2 + Math.Log(Math.Max(wsize.Width, wsize.Height)) / ln2);
            // a quanto pare se dContaining è 0 alla fine il risultato di depth è 0, anche se (int) di -infinito non è ovvio che dia 0
            return depth;
        }
    }
}
