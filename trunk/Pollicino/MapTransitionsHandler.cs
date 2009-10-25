using System;
using System.Drawing;
using System.Windows.Forms;
using MapsLibrary;

namespace MapperTools.Pollicino
{
    /// <summary>
    /// Questa classe serve per gestire le transizioni delle mappe in MainForm.
    /// </summary>
    public class MapTransitionsHandler
    {
        private MapControl mapcontrol;
        protected CachedTilesMapDL map;
        private System.Windows.Forms.Timer trans_timer;
        private Bitmap trans_buffer_pre;
        private Bitmap trans_buffer_post;
        private int trans_progress = 0;
        private PaintEventHandler current_transition;

        public MapTransitionsHandler(MapControl mc, CachedTilesMapDL m)
        {
            mapcontrol = mc;
            map = m;
            trans_timer = new System.Windows.Forms.Timer();
            trans_timer.Interval = 40;
            trans_timer.Tick += new EventHandler((Object sender, EventArgs e) => mapcontrol.Invalidate());
        }

        private void initPreBuffer()
        {
            System.Diagnostics.Trace.Assert(trans_buffer_pre == null, "buffer già inizializzato");
            trans_buffer_pre = new Bitmap(mapcontrol.MapViewportSize.Width, mapcontrol.MapViewportSize.Height);
            bool oldAsyncState = map.AsyncLoad;
            map.AsyncLoad = false;
            using (Graphics gpre = Graphics.FromImage(trans_buffer_pre))
                mapcontrol.Map.drawImageMapAt(mapcontrol.Center, mapcontrol.Zoom, mapcontrol.VisibleArea, gpre, new Point(0, 0));
            map.AsyncLoad = oldAsyncState;
        }

        /// <summary>
        /// Inizializza il buffer di fine transizione verso un nuovo mapsystem.
        /// </summary>
        /// <param name="new_msys">Mapsystem verso il quale transitare.</param>
        private void initPostBuffer(MercatorProjectionMapSystem new_msys)
        {
            System.Diagnostics.Trace.Assert(trans_buffer_post == null, "buffer già inizializzato");
            trans_buffer_post = new Bitmap(mapcontrol.MapViewportSize.Width, mapcontrol.MapViewportSize.Height);
            bool oldAsyncState = map.AsyncLoad;
            map.AsyncLoad = false;
            map.mapsystem = new_msys;
            using (Graphics gpost = Graphics.FromImage(trans_buffer_post))
                mapcontrol.Map.drawImageMapAt(mapcontrol.Center, mapcontrol.Zoom, mapcontrol.VisibleArea, gpost, new Point(0, 0));
            map.AsyncLoad = oldAsyncState;
        }

        /// <summary>
        /// Inizializza il buffer di fine transizione verso un nuovo livello di zoom.
        /// </summary>
        /// <param name="zoominc">Incremento del livello di zoom.</param>
        private void initPostBuffer(int zoominc)
        {
            System.Diagnostics.Trace.Assert(trans_buffer_post == null, "buffer già inizializzato");
            trans_buffer_post = new Bitmap(mapcontrol.MapViewportSize.Width, mapcontrol.MapViewportSize.Height);
            bool oldAsyncState = map.AsyncLoad;
            map.AsyncLoad = false;
            mapcontrol.Zoom = (uint)((int)mapcontrol.Zoom + zoominc);
            using (Graphics gpost = Graphics.FromImage(trans_buffer_post))
                mapcontrol.Map.drawImageMapAt(mapcontrol.Center, mapcontrol.Zoom, mapcontrol.VisibleArea, gpost, new Point(0, 0));
            map.AsyncLoad = oldAsyncState;
        }

        /// <summary>
        /// Inizializza il buffer di fine transizione per lo zoom out di un livello. Contrariamente al metodo generico,
        /// questo non impone il caricamento dei tile in modo sincrono e la parte centrale viene generata
        ///  ridimensionando il buffer di inizio transizione, se inizializzato.
        /// </summary>
        private void initPostBufferFastZO()
        {
            System.Diagnostics.Trace.Assert(trans_buffer_post == null, "buffer già inizializzato");
            trans_buffer_post = new Bitmap(mapcontrol.MapViewportSize.Width, mapcontrol.MapViewportSize.Height);
            mapcontrol.Zoom--;
            using (Graphics gpost = Graphics.FromImage(trans_buffer_post))
            {
                mapcontrol.Map.drawImageMapAt(mapcontrol.Center, mapcontrol.Zoom, mapcontrol.VisibleArea, gpost, new Point(0, 0));
                if (map.AsyncLoad && trans_buffer_pre != null)
                {
                    int x = trans_buffer_post.Width / 4,
                        y = trans_buffer_post.Height / 4,
                        w = trans_buffer_post.Width / 2,
                        h = trans_buffer_post.Height / 2;
                    StretchBlt(gpost, new Rectangle(x, y, w, h), trans_buffer_pre, new Rectangle(0, 0, trans_buffer_pre.Width, trans_buffer_pre.Height));
                }
            }
        }

        private void fixBuffersSize(ref Bitmap buffer)
        {
            if (buffer != null && buffer.Width != mapcontrol.Width)
            {
                Bitmap finalbuff = new Bitmap(mapcontrol.Width, mapcontrol.Height);
                using (Graphics dst = Graphics.FromImage(finalbuff))
                    StretchBlt(dst, new Rectangle(0, 0, mapcontrol.Width, mapcontrol.Height),
                               buffer, new Rectangle(0, 0, buffer.Width, buffer.Height));
                buffer.Dispose();
                buffer = finalbuff;
            }
        }

        private void initTransition(PaintEventHandler transition_handler)
        {
            if (current_transition != null)
            {
                trans_terminate();
                trans_freebuffers();
            }
            current_transition = transition_handler;
            mapcontrol.Paint += current_transition;
            trans_timer.Enabled = true;
        }

        private void trans_terminate()
        {
            trans_timer.Enabled = false;
            trans_progress = 0;
            mapcontrol.Paint -= current_transition;
            current_transition = null;
            mapcontrol.Invalidate();
        }

        private void trans_freebuffers()
        {
            if (trans_buffer_pre != null)
                trans_buffer_pre.Dispose();
            trans_buffer_pre = null;
            if (trans_buffer_post != null)
                trans_buffer_post.Dispose();
            trans_buffer_post = null;
        }

        public void AnimateMapRL(MercatorProjectionMapSystem new_msys)
        {
            PaintEventHandler paint_fn = new PaintEventHandler(this.mapcontrol_paint_RL);
            AnimateMap(new_msys, paint_fn);
        }

        public void AnimateMapLR(MercatorProjectionMapSystem new_msys)
        {
            PaintEventHandler paint_fn = new PaintEventHandler(this.mapcontrol_paint_LR);
            AnimateMap(new_msys, paint_fn);
        }

        private void AnimateMap(MercatorProjectionMapSystem new_msys, PaintEventHandler paint_fn)
        {
            // inizializza la transizione
            initTransition(paint_fn);
            // prepara il buffer di inizio transizione
            initPreBuffer();
            // prepara il buffer di fine transizione
            initPostBuffer(new_msys);
            // se i buffer sono più piccoli dell'area del controllo vengono ridimensionati
            fixBuffersSize(ref trans_buffer_pre);
            fixBuffersSize(ref trans_buffer_post);
        }

        public void AnimateMapZoomIn()
        {
            // Inizializza la transizione
            initTransition(new PaintEventHandler(this.mapcontrol_paint_ZI));
            // Prepara il buffer di inizio transizione
            initPreBuffer();
            // Il buffer di fine transizione non serve durante la transizione, quindi verrà generato alla fine.
            mapcontrol.Zoom++;
        }

        public void AnimateMapZoomOut()
        {
             // inizializza la transizione
            initTransition(new PaintEventHandler(this.mapcontrol_paint_ZO));
            // prepara il buffer di fine transizione
#if (PocketPC || Smartphone || WindowsCE)
            // Il caricamento dei tile dopo lo zoom out è un'operazione lenta, quindi il post buffer viene generato dal pre buffer.
            initPreBuffer();
            initPostBufferFastZO();
#else 
            initPostBuffer(-1);
#endif

        }
        
        void mapcontrol_paint_RL(object sender, PaintEventArgs e)
        {
            trans_progress += 8;
            if (trans_progress >= 100)
            {
                trans_terminate();
                trans_freebuffers();
            }
            else
            {
                int x = trans_progress * mapcontrol.Width / 100;
                e.Graphics.DrawImage(trans_buffer_pre, 0, 0, new Rectangle(0, 0, mapcontrol.Width - x, mapcontrol.Height), GraphicsUnit.Pixel);
                e.Graphics.DrawImage(trans_buffer_post, mapcontrol.Width - x, 0, new Rectangle(0, 0, x, mapcontrol.Height), GraphicsUnit.Pixel);
            }
        }

        void mapcontrol_paint_LR(object sender, PaintEventArgs e)
        {
            trans_progress += 8;
            if (trans_progress >= 100)
            {
                trans_terminate();
                trans_freebuffers();
            }
            else
            {
                int x = trans_progress * mapcontrol.Width / 100;
                e.Graphics.DrawImage(trans_buffer_pre, x, 0, new Rectangle(x, 0, mapcontrol.Width - x, mapcontrol.Height), GraphicsUnit.Pixel);
                e.Graphics.DrawImage(trans_buffer_post, 0, 0, new Rectangle(mapcontrol.Width - x, 0, x, mapcontrol.Height), GraphicsUnit.Pixel);
            }
        }

        void mapcontrol_paint_ZI(object sender, PaintEventArgs e)
        {
            trans_progress += 8;
            if (trans_progress >= 100)
            {
                trans_terminate();
                // Disegna la mappa finale
                // Il post buffer non era ancora stato inizializzato
                initPostBuffer(0);
                fixBuffersSize(ref trans_buffer_post);
                e.Graphics.DrawImage(trans_buffer_post, 0, 0);
                trans_freebuffers();
            }
            else
            {
                int w = trans_buffer_pre.Width * 100 / (100 + trans_progress);
                int h = trans_buffer_pre.Height * 100 / (100 + trans_progress);
                int x = (trans_buffer_pre.Width - w) / 2;
                int y = (trans_buffer_pre.Height - h) / 2;
                Rectangle src_rec = new Rectangle(x, y, w, h);
                StretchBlt(e.Graphics, new Rectangle(0, 0, mapcontrol.Width, mapcontrol.Height),
                           trans_buffer_pre, src_rec);
            }
        }

        void mapcontrol_paint_ZO(object sender, PaintEventArgs e)
        {
            trans_progress += 8;
            if (trans_progress >= 100)
            {
                trans_terminate();
#if (PocketPC || Smartphone || WindowsCE)
                // Si stava utilizzando un post buffer fasullo generato dal pre buffer. Ora viene generato e disegnato quello vero.
                trans_buffer_post.Dispose();
                trans_buffer_post = null;
                initPostBuffer(0);
                fixBuffersSize(ref trans_buffer_post);
                e.Graphics.DrawImage(trans_buffer_post, 0, 0);
#endif
                trans_freebuffers();
            }
            else
            {
                int w = trans_buffer_post.Width * 100 / (200 - trans_progress);
                int h = trans_buffer_post.Height * 100 / (200 - trans_progress);
                int x = (trans_buffer_post.Width - w) / 2;
                int y = (trans_buffer_post.Height - h) / 2;
                Rectangle src_rec = new Rectangle(x, y, w, h);
                StretchBlt(e.Graphics, new Rectangle(0, 0, mapcontrol.Width, mapcontrol.Height),
                           trans_buffer_post, src_rec);
            }
        }

#if (PocketPC || Smartphone || WindowsCE)
       private void StretchBlt(Graphics dst, Rectangle dst_rec, Bitmap src, Rectangle src_rec)
        {
            using (Graphics ig = Graphics.FromImage(src))
            {
                IntPtr hdcimg = ig.GetHdc();
                IntPtr hdcout = dst.GetHdc();
                StretchBlt(hdcout, dst_rec.Left, dst_rec.Top, dst_rec.Width, dst_rec.Height,
                           hdcimg, src_rec.Left, src_rec.Top, src_rec.Width, src_rec.Height,
                           TernaryRasterOperations.SRCCOPY);
                dst.ReleaseHdc(hdcout);
                ig.ReleaseHdc(hdcimg);
            }
        }
#else
        private void StretchBlt(Graphics dst, Rectangle dst_rec, Bitmap src, Rectangle src_rec)
        {
            dst.DrawImage(src, dst_rec, src_rec, GraphicsUnit.Pixel);
        }
#endif
 
#if (PocketPC || Smartphone || WindowsCE)
        [System.Runtime.InteropServices.DllImport("coredll.dll")]
#else 
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
#endif
        static extern bool StretchBlt(IntPtr hdcDest, int nXOriginDest, int nYOriginDest,
            int nWidthDest, int nHeightDest,
            IntPtr hdcSrc, int nXOriginSrc, int nYOriginSrc, int nWidthSrc, int nHeightSrc,
            TernaryRasterOperations dwRop);

        enum TernaryRasterOperations
        {
            SRCCOPY = 0x00CC0020, /* dest = source*/
        };
    }
}
