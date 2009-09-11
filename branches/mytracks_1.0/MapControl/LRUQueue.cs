/*******************************************************************************
 *  MyTracks - A tool for gps mapping.
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

namespace MapsLibrary
{
    public class LRUQueue<TKey, TData>
    {
        private Hashtable data = new Hashtable();
        private LinkedList<TKey> keys = new LinkedList<TKey>();
        private object accesslock = new object();

        public bool Add(TKey k, TData v)
        {
            lock (accesslock)
            {
                if (!keys.Contains(k))
                {
                    keys.AddFirst(k);
                    data.Add(k, v);
                    return true;
                }
                return false;
            }
        }

        public TData Remove(TKey to_remove)
        {
            lock (accesslock)
            {
                System.Diagnostics.Debug.Assert(keys.Count == data.Count, "data and keys count inconsistency");
                if (Contains(to_remove))
                {
                    keys.Remove(to_remove);
                    TData v = (TData)data[to_remove];
                    data.Remove(to_remove);
                    System.Diagnostics.Debug.Assert(!data.Contains(to_remove), "data contains removed item");
                    System.Diagnostics.Debug.Assert(keys.Count == data.Count, "data and keys count inconsistency after element removing");
                    return v;
                }
                else
                    return default(TData);
            }
        }

        public TData RemoveOlder()
        {
            lock (accesslock)
            {
                System.Diagnostics.Debug.Assert(keys.Count == data.Count, "data and keys count inconsistency");
                TKey older = keys.Last.Value;
                System.Diagnostics.Debug.Assert(data.Contains(older), "data doesn't contain lru.Last.Value");
                keys.RemoveLast();
                TData v = (TData)data[older];
                data.Remove(older);
                System.Diagnostics.Debug.Assert(!data.Contains(older), "data contains removed item");
                System.Diagnostics.Debug.Assert(keys.Count == data.Count, "data and keys count inconsistency after element removing");
                return v;
            }
        }

        public bool Contains(TKey k)
        {
            lock (accesslock)
            {
                System.Diagnostics.Debug.Assert(keys.Contains(k) == data.Contains(k), "data and keys collections inconsistency");
                return data.Contains(k);
            }
        }

        public void Clear()
        {
            lock (accesslock)
            {
                if (typeof(IDisposable).IsAssignableFrom(typeof(TData)))
                {
                    foreach (DictionaryEntry entry in data)
                        ((IDisposable) entry.Value).Dispose();
                }
                data.Clear();
                keys.Clear();                
            }
        }

        public TData this[TKey idx]
        {
            get
            {
                lock (accesslock)
                {
                    TData value = (TData)data[idx];
                    LinkedListNode<TKey> node = keys.Find(idx);
                    System.Diagnostics.Debug.Assert(node != null, "lru.Find returns null");
                    keys.Remove(node);
                    keys.AddFirst(node);
                    return value;
                }
            }
        }

        public int Count
        {
            get
            {
                lock (accesslock)
                {
                    System.Diagnostics.Debug.Assert(keys.Count == data.Count, "data and keys count inconsistency");
                    return data.Count;
                }
            }
        }

    }
}
