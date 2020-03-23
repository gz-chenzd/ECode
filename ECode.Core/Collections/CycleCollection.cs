using System;
using System.Collections.Generic;
using ECode.Utility;

namespace ECode.Collections
{
    public class CycleCollection<T>
    {
        private int         index       = 0;
        private List<T>     items       = new List<T>();


        /// <summary>
        /// Gets the number of items contained in the collection.
        /// </summary>
        public int Count
        {
            get { return items.Count; }
        }


        /// <summary>
        /// Adds specified items to the collection.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Is raised when <b>items</b> is null.</exception>
        public void Add(IEnumerable<T> items)
        {
            AssertUtil.ArgumentNotNull(items, nameof(items));

            lock (this)
            {
                this.items.AddRange(items);
            }
        }

        /// <summary>
        /// Adds specified item to the collection.
        /// </summary>
        public void Add(T item)
        {
            lock (this)
            {
                items.Add(item);
            }
        }

        /// <summary>
        /// Removes the first occurrence of the specified item from the collection.
        /// </summary>
        public void Remove(T item)
        {
            lock (this)
            {
                items.Remove(item);

                // Update loop index.
                if (index >= items.Count)
                { index = 0; }
            }
        }

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        public void Clear()
        {
            lock (this)
            {
                items.Clear();

                // Reset loop index.
                index = 0;
            }
        }

        /// <summary>
        /// Determines whether specified item is in the collection.
        /// </summary>
        public bool Contains(T item)
        {
            lock (this)
            {
                return items.Contains(item);
            }
        }


        /// <summary>
        /// Gets next item from the collection. This method is thread-safe.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Is raised when thre is no items in the collection.</exception>
        public T Next()
        {
            lock (this)
            {
                if (items.Count == 0)
                { throw new InvalidOperationException("There is no items in the collection."); }

                T item = items[index++];
                if (index >= items.Count)
                { index = 0; }

                return item;
            }
        }

        /// <summary>
        /// Copies all items to new array, all items will be listed in the order they were added. This method is thread-safe.
        /// </summary>
        public T[] ToArray()
        {
            lock (this)
            {
                return items.ToArray();
            }
        }

        /// <summary>
        /// Copies all items to new array, all items will be listed in current cycle order. This method is thread-safe.
        /// </summary>
        public T[] ToCurrentOrderArray()
        {
            lock (this)
            {
                int idx = this.index;
                T[] retVal = new T[items.Count];
                for (int i = 0; i < items.Count; i++)
                {
                    retVal[i] = items[idx++];
                    if (idx >= items.Count)
                    { idx = 0; }
                }

                return retVal;
            }
        }
    }
}