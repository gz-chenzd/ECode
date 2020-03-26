using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ECode.Core;
using ECode.Json;
using ECode.TypeConversion;
using ECode.Utility;

namespace ECode.Caching
{
    public class MemoryCache : ICacheManager, IShardCacheManager, IDisposable
    {
        static readonly Type                            TYPE_STRING     = typeof(string);

        private bool                                    IsDisposed      = false;
        private int                                     MaxCapacity     = 100000;
        private Dictionary<string, MemoryCacheItem>     ItemsByKey      = null;
        private TimerEx                                 ClearTimer      = null;

        private MemoryCacheItem                         First           = null;
        private MemoryCacheItem                         Last            = null;


        public MemoryCache(int capacity = 100000)
        {
            if (capacity < 1000)
            { throw new ArgumentException($"Argument '{nameof(capacity)}' value must be >= 1000."); }

            MaxCapacity = capacity;
            ItemsByKey = new Dictionary<string, MemoryCacheItem>(capacity);

            First = Last = new MemoryCacheItem();

            ClearTimer = new TimerEx(60 * 1000);
            ClearTimer.Elapsed += Clear_Elapsed;
            ClearTimer.Start();
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Dispose()
        {
            if (IsDisposed)
            { return; }

            IsDisposed = true;

            ClearTimer.Stop();
            ClearTimer = null;

            ItemsByKey.Clear();
            ItemsByKey = null;
        }

        protected void ThrowIfObjectDisposed()
        {
            if (IsDisposed)
            { throw new ObjectDisposedException(this.GetType().Name); }
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        private void Clear_Elapsed(object sender, EventArgs e)
        {
            if (IsDisposed)
            { return; }

            Clear();
        }

        private void Clear()
        {
            int clearCount = (int)Math.Ceiling(MaxCapacity * 0.1);  // remove 10%
            var clearItems = new List<MemoryCacheItem>(clearCount);
            var itemsByKey = new Dictionary<string, MemoryCacheItem>(clearCount);

            var item = Last.Previous;
            while (clearCount > 0 && item != null)
            {
                if (item.IsExpired)
                {
                    clearItems.Add(item);
                    itemsByKey[item.Key] = item;

                    clearCount--;
                }

                item = item.Previous;
            }

            if (ItemsByKey.Count - clearItems.Count == MaxCapacity)
            {
                item = Last.Previous;
                while (clearCount > 0 && item != null)
                {
                    if (!itemsByKey.ContainsKey(item.Key))
                    {
                        clearItems.Add(item);
                        itemsByKey[item.Key] = item;

                        clearCount--;
                    }

                    item = item.Previous;
                }
            }

            foreach (var item2 in clearItems)
            {
                Remove(null, item2.Key);
            }
        }


        private MemoryCacheItem GetItem(string key)
        {
            AssertUtil.ArgumentNotEmpty(key, nameof(key));

            if (ItemsByKey.TryGetValue(key, out MemoryCacheItem item))
            {
                if (item.IsExpired)
                {
                    Remove(null, key);
                    return null;
                }

                MoveToFirst(item);

                return item;
            }

            return null;
        }

        private void MoveToFirst(MemoryCacheItem item)
        {
            if (First == item)
            { return; }

            if (item.Previous != null)
            { item.Previous.Next = item.Next; }

            item.Next.Previous = item.Previous;

            item.Next = First;
            item.Previous = null;

            First = item;
        }


        protected virtual void ResolveInput(MemoryCacheItem item, object value)
        {
            if (value is string || value.GetType().IsPrimitive)
            {
                item.ValueType = CacheValueType.Plain;
                item.StringValue = value.ToString();
            }
            else if (value is DateTime || value is DateTime?)
            {
                item.ValueType = CacheValueType.Plain;
                item.StringValue = ((DateTime)value).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz");
            }
            else if (value is ICollection<byte>)
            {
                item.ValueType = CacheValueType.Binary;
                item.BinaryValue = new List<byte>(value as ICollection<byte>).ToArray();
            }
            else
            {
                item.ValueType = CacheValueType.Json;
                item.StringValue = JsonUtil.Serialize(value);
            }
        }

        protected virtual object ResolveOutput(MemoryCacheItem item, Type targetType)
        {
            if (item.ValueType == CacheValueType.Json)
            {
                return JsonUtil.Deserialize(item.StringValue, targetType);
            }
            else if (item.ValueType == CacheValueType.Plain)
            {
                return TypeConversionUtil.ConvertValueIfNecessary(targetType, item.StringValue);
            }
            else if (item.ValueType == CacheValueType.Binary)
            {
                if (targetType == TYPE_STRING)
                { return item.BinaryValue.ToBase64(); }

                return TypeConversionUtil.ConvertValueIfNecessary(targetType, item.BinaryValue);
            }
            else
            {
                throw new Exception();
            }
        }


        public bool Contains(string key)
        {
            return Contains(null, key);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool Contains(object shardObject, string key)
        {
            ThrowIfObjectDisposed();

            return GetItem(key) != null;
        }


        public string Get(string key)
        {
            return (string)Get(null, key, TYPE_STRING);
        }

        public T Get<T>(string key)
        {
            return (T)Get(null, key, typeof(T));
        }

        public object Get(string key, Type objectType)
        {
            return Get(null, key, objectType);
        }

        public string Get(object shardObject, string key)
        {
            return (string)Get(shardObject, key, TYPE_STRING);
        }

        public T Get<T>(object shardObject, string key)
        {
            return (T)Get(shardObject, key, typeof(T));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public object Get(object shardObject, string key, Type objectType)
        {
            ThrowIfObjectDisposed();

            AssertUtil.ArgumentNotEmpty(key, nameof(key));
            AssertUtil.ArgumentNotNull(objectType, nameof(objectType));


            var item = GetItem(key);
            if (item == null)
            { throw new KeyNotFoundException(); }

            return ResolveOutput(item, objectType);
        }


        public bool Add(string key, object value, int ttl)
        {
            return Add(null, key, value, ttl);
        }

        public bool Add(string key, object value, TimeSpan expired)
        {
            return Add(null, key, value, (int)expired.TotalSeconds);
        }

        public bool Add(string key, object value, DateTime expired)
        {
            return Add(null, key, value, (int)(expired - DateTime.Now).TotalSeconds);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool Add(object shardObject, string key, object value, int ttl)
        {
            ThrowIfObjectDisposed();

            AssertUtil.ArgumentNotEmpty(key, nameof(key));
            AssertUtil.ArgumentNotNull(value, nameof(value));

            if (ttl <= 0)
            { throw new ArgumentException($"Expire time must be > 0."); }


            if (GetItem(key) != null)
            { return false; }

            var item = new MemoryCacheItem()
            {
                Key = key,
                ExpireTime = DateTime.Now.AddSeconds(ttl)
            };

            ResolveInput(item, value);
            if (item.StringValue == null && item.BinaryValue == null)
            { throw new ArgumentException($"Argument '{nameof(value)}' cannot be empty."); }

            if (ItemsByKey.Count == MaxCapacity)
            { Clear(); }

            ItemsByKey[key] = item;

            item.Next = First;
            First.Previous = item;
            First = item;

            return true;
        }

        public bool Add(object shardObject, string key, object value, TimeSpan expired)
        {
            return Add(shardObject, key, value, (int)expired.TotalSeconds);
        }

        public bool Add(object shardObject, string key, object value, DateTime expired)
        {
            return Add(shardObject, key, value, (int)(expired - DateTime.Now).TotalSeconds);
        }


        public bool Set(string key, object value, int ttl)
        {
            return Set(null, key, value, ttl);
        }

        public bool Set(string key, object value, TimeSpan expired)
        {
            return Set(null, key, value, (int)expired.TotalSeconds);
        }

        public bool Set(string key, object value, DateTime expired)
        {
            return Set(null, key, value, (int)(expired - DateTime.Now).TotalSeconds);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool Set(object shardObject, string key, object value, int ttl)
        {
            ThrowIfObjectDisposed();

            AssertUtil.ArgumentNotEmpty(key, nameof(key));
            AssertUtil.ArgumentNotNull(value, nameof(value));

            if (ttl <= 0)
            { throw new ArgumentException($"Expire time must be > 0."); }


            if (GetItem(key) == null)
            { return Add(shardObject, key, value, ttl); }

            return Replace(shardObject, key, value, ttl);
        }

        public bool Set(object shardObject, string key, object value, TimeSpan expired)
        {
            return Set(shardObject, key, value, (int)expired.TotalSeconds);
        }

        public bool Set(object shardObject, string key, object value, DateTime expired)
        {
            return Set(null, key, value, (int)(expired - DateTime.Now).TotalSeconds);
        }


        public bool Replace(string key, object value, int ttl)
        {
            return Replace(null, key, value, ttl);
        }

        public bool Replace(string key, object value, TimeSpan expired)
        {
            return Replace(null, key, value, (int)expired.TotalSeconds);
        }

        public bool Replace(string key, object value, DateTime expired)
        {
            return Replace(null, key, value, (int)(expired - DateTime.Now).TotalSeconds);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool Replace(object shardObject, string key, object value, int ttl)
        {
            ThrowIfObjectDisposed();

            AssertUtil.ArgumentNotEmpty(key, nameof(key));
            AssertUtil.ArgumentNotNull(value, nameof(value));

            if (ttl <= 0)
            { throw new ArgumentException($"Expire time time must be > 0."); }


            var item = GetItem(key);
            if (item == null)
            { return false; }

            var temp = new MemoryCacheItem();
            ResolveInput(temp, value);
            if (temp.StringValue == null && temp.BinaryValue == null)
            { throw new ArgumentException($"Argument '{nameof(value)}' cannot be empty."); }

            item.StringValue = temp.StringValue;
            item.BinaryValue = temp.BinaryValue;
            item.ExpireTime = DateTime.Now.AddSeconds(ttl);

            return true;
        }

        public bool Replace(object shardObject, string key, object value, TimeSpan expired)
        {
            return Replace(shardObject, key, value, (int)expired.TotalSeconds);
        }

        public bool Replace(object shardObject, string key, object value, DateTime expired)
        {
            return Replace(shardObject, key, value, (int)(expired - DateTime.Now).TotalSeconds);
        }


        public bool Remove(string key)
        {
            return Remove(null, key);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool Remove(object shardObject, string key)
        {
            ThrowIfObjectDisposed();

            AssertUtil.ArgumentNotEmpty(key, nameof(key));


            if (ItemsByKey.TryGetValue(key, out MemoryCacheItem item))
            {
                ItemsByKey.Remove(key);

                if (item == First)
                {
                    First = item.Next;
                    First.Previous = null;
                }
                else
                {
                    if (item.Previous != null)
                    { item.Previous.Next = item.Next; }

                    item.Next.Previous = item.Previous;
                }

                return !item.IsExpired;
            }

            return false;
        }


        public long Increase(string key, int delta = 1)
        {
            return Increase(null, key, delta);
        }

        public long Decrease(string key, int delta = 1)
        {
            return Increase(null, key, (-1) * delta);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public long Increase(object shardObject, string key, int delta = 1)
        {
            ThrowIfObjectDisposed();

            AssertUtil.ArgumentNotEmpty(key, nameof(key));


            var item = GetItem(key);
            if (item == null)
            { throw new KeyNotFoundException(); }

            if (!long.TryParse(item.StringValue, out long num))
            { throw new InvalidOperationException("Value is not an integer."); }

            long value = num + delta;
            if ((delta > 0 && value < num) || (delta < 0 && value > num))
            { throw new InvalidOperationException("Value is not an integer or out of range."); }

            item.StringValue = value.ToString();

            return value;
        }

        public long Decrease(object shardObject, string key, int delta = 1)
        {
            return Increase(shardObject, key, (-1) * delta);
        }


        public bool Touch(string key, int ttl)
        {
            return Touch(null, key, ttl);
        }

        public bool Touch(string key, TimeSpan expired)
        {
            return Touch(null, key, (int)expired.TotalSeconds);
        }

        public bool Touch(string key, DateTime expired)
        {
            return Touch(null, key, (int)(expired - DateTime.Now).TotalSeconds);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool Touch(object shardObject, string key, int ttl)
        {
            ThrowIfObjectDisposed();

            AssertUtil.ArgumentNotEmpty(key, nameof(key));

            if (ttl <= 0)
            { throw new ArgumentException($"Expire time must be > 0."); }


            var item = GetItem(key);
            if (item == null)
            { return false; }

            item.ExpireTime = DateTime.Now.AddSeconds(ttl);

            return true;
        }

        public bool Touch(object shardObject, string key, TimeSpan expired)
        {
            return Touch(shardObject, key, (int)expired.TotalSeconds);
        }

        public bool Touch(object shardObject, string key, DateTime expired)
        {
            return Touch(shardObject, key, (int)(expired - DateTime.Now).TotalSeconds);
        }
    }
}
