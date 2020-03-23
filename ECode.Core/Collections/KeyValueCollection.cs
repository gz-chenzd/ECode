using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ECode.Utility;

namespace ECode.Collections
{
    /// <summary>
    /// Represents a collection that can be accessed either with the key or with the index. 
    /// </summary>
    public class KeyValueCollection<K, V> : IEnumerable
    {
        class ValueItem
        {
            static int  serialNo    = 0;


            public int ID
            { get; }

            public V Value
            { get; }


            public ValueItem(V value)
            {
                ID = ++serialNo;
                Value = value;
            }
        }


        private SortedList<int, ValueItem>      valueById       = new SortedList<int, ValueItem>();
        private Dictionary<K, ValueItem>        valueByKey      = new Dictionary<K, ValueItem>();


        /// <summary>
        /// Gets the number of items contained in the collection.
        /// </summary>
        public int Count
        {
            get { return valueById.Count; }
        }

        /// <summary>
        /// Gets value with the specified key.
        /// </summary>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Is raised when <b>key</b> not exist in the collection.</exception>
        public V this[K key]
        {
            get
            {
                lock (this)
                {
                    return valueByKey[key].Value;
                }
            }
        }


        /// <summary>
        /// Adds specified key and value to the collection.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Is raised when <b>key</b> is null.</exception>
        /// <exception cref="System.ArgumentException">Is raised when <b>key</b> already exists in the collection.</exception>
        public void Add(K key, V value)
        {
            AssertUtil.ArgumentNotNull(key, nameof(key));

            lock (this)
            {
                var item = new ValueItem(value);

                valueById.Add(item.ID, item);
                valueByKey.Add(key, item);
            }
        }

        /// <summary>
        /// Removes the item with the specified key from the collection.
        /// </summary>
        /// <returns>true if the item is successfully found and removed; otherwise, false.</returns>
        /// <exception cref="System.ArgumentNullException">Is raised when <b>key</b> is null.</exception>
        public bool Remove(K key)
        {
            AssertUtil.ArgumentNotNull(key, nameof(key));

            lock (this)
            {
                if (valueByKey.TryGetValue(key, out ValueItem item))
                {
                    valueById.Remove(item.ID);
                    valueByKey.Remove(key);

                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        public void Clear()
        {
            lock (this)
            {
                valueById.Clear();
                valueByKey.Clear();
            }
        }

        /// <summary>
        /// Determines whether the collection contains the specified key.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Is raised when <b>key</b> is null.</exception>
        public bool ContainsKey(K key)
        {
            AssertUtil.ArgumentNotNull(key, nameof(key));

            lock (this)
            {
                return valueByKey.ContainsKey(key);
            }
        }


        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Is raised when <b>key</b> is null.</exception>
        public bool TryGetValue(K key, out V value)
        {
            AssertUtil.ArgumentNotNull(key, nameof(key));

            lock (this)
            {
                if (valueByKey.TryGetValue(key, out ValueItem item))
                {
                    value = item.Value;
                    return true;
                }

                value = default(V);
                return false;
            }
        }

        /// <summary>
        /// Gets the value at the specified index.
        /// </summary>
        public bool TryGetValueAt(int index, out V value)
        {
            value = default(V);

            if (index < 0)
            { return false; }

            lock (this)
            {
                if (index >= valueById.Count)
                { return false; }

                value = valueById.Values[index].Value;
                return true;
            }
        }


        /// <summary>
        /// Copies all items to new array, all items will be listed in the order they were added. This method is thread-safe.
        /// </summary>
        public V[] ToArray()
        {
            lock (this)
            {
                return valueById.Values.Select(t => t.Value).ToArray();
            }
        }


        public IEnumerator GetEnumerator()
        {
            lock (this)
            {
                return valueById.Values.Select(t => t.Value).GetEnumerator();
            }
        }
    }
}
