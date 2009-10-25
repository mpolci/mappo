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

namespace MapsLibrary
{
    public class LRUQueue<T>
    {
        private Hashtable listnode = new Hashtable();
        private LinkedList<T> keys = new LinkedList<T>();

        public bool Enqueue(T k)
        {
            if (!listnode.Contains(k))
            {
                LinkedListNode<T> node = keys.AddFirst(k);
                listnode.Add(k, node);
                return true;
            }
            else
            {
                MoveToFirst(k);
                return false;
            }
        }

        public T Dequeue()
        {
            System.Diagnostics.Debug.Assert(keys.Count == listnode.Count, "listnode and keys count inconsistency");
            T older = keys.Last.Value;
            System.Diagnostics.Debug.Assert(listnode.Contains(older), "listnode doesn't contain lru.Last.Value");
            keys.RemoveLast();
            listnode.Remove(older);
            return older;
        }

        public bool Remove(T k)
        {
            if (listnode.Contains(k))
            {
                LinkedListNode<T> node = (LinkedListNode<T>)listnode[k];
                System.Diagnostics.Debug.Assert(node != null, "Remove(): list node is null");
                keys.Remove(node);
                listnode.Remove(k);
                return true;
            }
            else
                return false;
        }

        public bool Contains(T k)
        {
            bool c = listnode.Contains(k);
            if (c)
                MoveToFirst(k);
            return c;
        }

        public int Count
        {
            get
            {
                System.Diagnostics.Debug.Assert(keys.Count == listnode.Count, "data and keys count inconsistency");
                return keys.Count;
            }
        }

        private void MoveToFirst(T k)
        {
            LinkedListNode<T> node = (LinkedListNode<T>) listnode[k];
            if (node != keys.First)
            {
                keys.Remove(node);
                keys.AddFirst(node);
            }
            System.Diagnostics.Debug.Assert(node == keys.First);
        }

    }

    //-------------------------------------------------------------------------------

    // LRUQueue<TKey, TData> : Questa coda LRU associa una chiave ad un dato. L'elemento 
    // è riportato in cima alla coda (rotazione LRU) solo quando è acceduto il dato, non
    // quando è testata la sua presenza.
    public class LRUQueue<TKey, TData>
    {
        protected struct ListItem
        {
            public TKey key;
            public TData data;
            public ListItem(TKey k, TData d) {
                key = k;
                data = d;
            }
        }
        private Hashtable listnode = new Hashtable();
        private LinkedList<ListItem> keys = new LinkedList<ListItem>();
        private object accesslock = new object();

        public void Enqueue(TKey k, TData v)
        {
            lock (accesslock)
            {
                if (!listnode.Contains(k))
                {
                    LinkedListNode<ListItem> node = keys.AddFirst(new ListItem(k, v));
                    listnode.Add(k, node);
                }
                else
                {
                    throw new ArgumentException("Key already present in the queue: " + k.ToString());
                }
            }
        }

        public TData Dequeue()
        {
            lock (accesslock)
            {
                System.Diagnostics.Debug.Assert(keys.Count == listnode.Count, "data and keys count inconsistency");
                ListItem older = keys.Last.Value;
                System.Diagnostics.Debug.Assert(listnode.Contains(older.key), "data doesn't contain lru.Last.Value");
                keys.RemoveLast();
                listnode.Remove(older.key);
                return older.data;
            }
        }

        public TData Remove(TKey k)
        {
            lock (accesslock)
            {
                System.Diagnostics.Debug.Assert(keys.Count == listnode.Count, "listnode and keys count inconsistency");
                if (listnode.Contains(k))
                {
                    LinkedListNode<ListItem> node = (LinkedListNode<ListItem>)listnode[k];
                    System.Diagnostics.Debug.Assert(node != null, "Remove(): list node is null");
                    keys.Remove(node);
                    listnode.Remove(k);
                    return node.Value.data;
                }
                else
                    return default(TData);
            }
        }

        public bool Contains(TKey k)
        {
            lock (accesslock)
            {
                bool c = listnode.Contains(k);
                //if (c)
                //    MoveToFirst(k);
                return c;
            }
        }

        public int Count
        {
            get
            {
                System.Diagnostics.Debug.Assert(keys.Count == listnode.Count, "data and keys count inconsistency");
                return keys.Count;
            }
        }

        private void MoveToFirst(TKey k)
        {
            // il lock è preso nel metodo chiamante
            LinkedListNode<ListItem> node = (LinkedListNode<ListItem>)listnode[k];
            if (node != keys.First)
            {
                keys.Remove(node);
                keys.AddFirst(node);
            }
            System.Diagnostics.Debug.Assert(node == keys.First);
        }

        public void Clear()
        {
            lock (accesslock)
            {
                if (typeof(IDisposable).IsAssignableFrom(typeof(TData)))
                {
                    //foreach (LinkedListNode<ListItem> entry in keys)
                    for (LinkedListNode<ListItem> entry = keys.First; entry != null; entry = entry.Next) 
                        ((IDisposable) entry.Value.data).Dispose();
                }
                listnode.Clear();
                keys.Clear();
            }
        }

        public TData this[TKey k]
        {
            get
            {
                lock (accesslock)
                {
                    if (listnode.Contains(k))
                    {
                        LinkedListNode<ListItem> node = (LinkedListNode<ListItem>)listnode[k];
                        System.Diagnostics.Debug.Assert(node != null, "Remove(): list node is null");
                        MoveToFirst(k);
                        return node.Value.data;
                    }
                    else
                        return default(TData);
                }
            }
        }


    }
}
