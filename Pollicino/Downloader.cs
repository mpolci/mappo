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
using System.Text;
using System.Threading;
using MapsLibrary;

namespace MapperTools.Pollicino
{
    public class Downloader
    {
        private struct AreaMapItem
        {
            public IDownloadableMap map;
            public ProjectedGeoArea area;
            public uint zoom;
            public AreaMapItem(IDownloadableMap m, ProjectedGeoArea a, uint z) { map = m; area = a; zoom = z; }
        }

        //private Queue<AreaMapItem> q;
        private Stack<AreaMapItem> q;
        ManualResetEvent mre;

        bool runthread;
        Thread thr;

        IWorkNotifier notifier;

        public Downloader(IWorkNotifier wnotifier)
        {
            //q = new Queue<AreaMapItem>();
            q = new Stack<AreaMapItem>();
            thr = new Thread(new ThreadStart(this.threadproc));
            thr.Priority = ThreadPriority.BelowNormal;
            thr.Name = "Map Downloader Thread";
            runthread = false;
            mre = new ManualResetEvent(false);
            notifier = wnotifier;
        }

        public void startThread()
        {
            if (runthread) throw new Exception("thread already running");
            runthread = true;
            q.Clear();
            thr.Start();
        }

        public void stopThread()
        {
            if (!runthread) throw new Exception("thread not running");
            runthread = false;
            q.Clear();
            mre.Set();
            thr.Join();
        }

        private void threadproc()
        {
            #if DEBUG
            long count = 0;
            #endif
            while (runthread)
            {
                mre.WaitOne();
                notifier.WorkBegin();
                while (q.Count > 0 && runthread)
                {
                    #if DEBUG
                    count++;
                    #endif
                    AreaMapItem item;
                    lock (q)
                        //item = q.Dequeue();
                        item = q.Pop();
                    try
                    {
                        item.map.DownloadMapArea(item.area, item.zoom);
                    }
                    catch (System.Net.WebException) { }
                    catch (Exception ex)
                    {
                        #if DEBUG
                        throw ex;
                        #endif
                    }
                }
                notifier.WorkEnd();
                mre.Reset();
            }
        }

        public void addDownloadArea(IDownloadableMap map, ProjectedGeoArea area, uint zoom)
        {
            if (runthread)
            {
                lock (q)
                    q.Push(new AreaMapItem(map, area, zoom));
                    //q.Enqueue(new AreaMapItem(map, area, zoom));
                mre.Set();
            }
        }


            
    }
}
