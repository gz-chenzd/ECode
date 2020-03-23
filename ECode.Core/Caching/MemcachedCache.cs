using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ECode.Core;
using ECode.Json;
using ECode.TypeConversion;
using ECode.Utility;

namespace ECode.Caching
{
    public class MemcachedCache : ICacheManager, IShardCacheManager
    {
        static readonly Type                TYPE_STRING         = typeof(string);

        protected const int                 FLAG_JSON           = 0;
        protected const int                 FLAG_STRING         = 1;
        protected const int                 FLAG_BINARY         = 2;

        private IMemcachedClientManager     ClientManager       = null;


        public MemcachedCache(IMemcachedClientManager clientManager)
        {
            AssertUtil.ArgumentNotNull(clientManager, nameof(clientManager));

            ClientManager = clientManager;
        }


        protected virtual string ResolveKey(string key)
        {
            var bytes = Encoding.UTF8.GetBytes(key);
            for (int i = 0; i < bytes.Length; i++)
            {
                if (bytes[i] < 32 || bytes[i] > 126)
                { throw new Exception($"Invalid key: contains invisible chars '0x{bytes[i].ToString("X2")}'."); }

                if (bytes[i] == 32)
                { bytes[i] = (byte)'_'; }
            }

            return Encoding.UTF8.GetString(bytes);
        }

        protected virtual void ResolveInput(MemcachedCacheItem item, object value)
        {
            if (value is string || value.GetType().IsPrimitive)
            {
                item.Flags = FLAG_STRING;
                item.ValueBytes = Encoding.UTF8.GetBytes(value.ToString());
            }
            else if (value is DateTime || value is DateTime?)
            {
                item.Flags = FLAG_STRING;
                item.ValueBytes = Encoding.UTF8.GetBytes(((DateTime)value).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz"));
            }
            else if (value is ICollection<byte>)
            {
                item.Flags = FLAG_BINARY;
                item.ValueBytes = (value as ICollection<byte>).ToArray();
            }
            else
            {
                item.Flags = FLAG_JSON;
                item.ValueBytes = Encoding.UTF8.GetBytes(JsonUtil.Serialize(value));
            }
        }

        protected virtual object ResolveOutput(MemcachedCacheItem item, Type targetType)
        {
            if (item.Flags == FLAG_JSON)
            {
                return JsonUtil.Deserialize(Encoding.UTF8.GetString(item.ValueBytes), targetType);
            }
            else if (item.Flags == FLAG_STRING)
            {
                return TypeConversionUtil.ConvertValueIfNecessary(targetType, Encoding.UTF8.GetString(item.ValueBytes));
            }
            else
            {
                if (targetType == TYPE_STRING)
                { return item.ValueBytes.ToBase64(); }

                return TypeConversionUtil.ConvertValueIfNecessary(targetType, item.ValueBytes);
            }
        }

        protected MemcachedClient GetMemcachedClient(object shardObject, bool writable = true)
        {
            return ClientManager.GetClient(shardObject, writable);
        }


        public bool Contains(string key)
        {
            return Contains(null, key);
        }

        public bool Contains(object shardObject, string key)
        {
            AssertUtil.ArgumentNotEmpty(key, nameof(key));

            using (var client = GetMemcachedClient(shardObject, false))
            {
                return client.Get(ResolveKey(key)) != null;
            }
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

        public object Get(object shardObject, string key, Type objectType)
        {
            AssertUtil.ArgumentNotEmpty(key, nameof(key));

            MemcachedCacheItem item = null;
            using (var client = GetMemcachedClient(shardObject, false))
            {
                item = client.Get(ResolveKey(key));
            }

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

        public bool Add(object shardObject, string key, object value, int ttl)
        {
            AssertUtil.ArgumentNotEmpty(key, nameof(key));
            AssertUtil.ArgumentNotNull(value, nameof(value));

            if (ttl <= 0)
            { throw new ArgumentException($"Expire time must be > 0."); }


            var item = new MemcachedCacheItem(){ Key = ResolveKey(key) };

            ResolveInput(item, value);
            if (item.ValueBytes == null)
            { throw new ArgumentException($"Argument '{value}' cannot be empty."); }

            using (var client = GetMemcachedClient(shardObject, true))
            {
                return client.Add(item.Key, item.ValueBytes, item.Flags, ttl);
            }
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

        public bool Set(object shardObject, string key, object value, int ttl)
        {
            AssertUtil.ArgumentNotEmpty(key, nameof(key));
            AssertUtil.ArgumentNotNull(value, nameof(value));

            if (ttl <= 0)
            { throw new ArgumentException($"Expire time must be > 0."); }


            var item = new MemcachedCacheItem(){ Key = ResolveKey(key) };

            ResolveInput(item, value);
            if (item.ValueBytes == null)
            { throw new ArgumentException($"Argument '{value}' cannot be empty."); }

            using (var client = GetMemcachedClient(shardObject, true))
            {
                return client.Set(item.Key, item.ValueBytes, item.Flags, ttl);
            }
        }

        public bool Set(object shardObject, string key, object value, TimeSpan expired)
        {
            return Set(shardObject, key, value, (int)expired.TotalSeconds);
        }

        public bool Set(object shardObject, string key, object value, DateTime expired)
        {
            return Set(shardObject, key, value, (int)(expired - DateTime.Now).TotalSeconds);
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

        public bool Replace(object shardObject, string key, object value, int ttl)
        {
            AssertUtil.ArgumentNotEmpty(key, nameof(key));
            AssertUtil.ArgumentNotNull(value, nameof(value));

            if (ttl <= 0)
            { throw new ArgumentException($"Expire time must be > 0."); }


            var item = new MemcachedCacheItem(){ Key = ResolveKey(key) };

            ResolveInput(item, value);
            if (item.ValueBytes == null)
            { throw new ArgumentException($"Argument '{value}' cannot be empty."); }

            using (var client = GetMemcachedClient(shardObject, true))
            {
                return client.Replace(item.Key, item.ValueBytes, item.Flags, ttl);
            }
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

        public bool Remove(object shardObject, string key)
        {
            AssertUtil.ArgumentNotEmpty(key, nameof(key));

            using (var client = GetMemcachedClient(shardObject, true))
            {
                return client.Delete(ResolveKey(key));
            }
        }


        public long Increase(string key, int delta = 1)
        {
            return Increase(null, key, delta);
        }

        public long Decrease(string key, int delta = 1)
        {
            return Increase(null, key, (-1) * delta);
        }

        public long Increase(object shardObject, string key, int delta = 1)
        {
            AssertUtil.ArgumentNotEmpty(key, nameof(key));

            using (var client = GetMemcachedClient(shardObject, true))
            {
                if (delta < 0)
                { return client.Decr(ResolveKey(key), (-1) * delta); }

                return client.Incr(ResolveKey(key), delta);
            }
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

        public bool Touch(object shardObject, string key, int ttl)
        {
            AssertUtil.ArgumentNotEmpty(key, nameof(key));

            if (ttl <= 0)
            { throw new ArgumentException($"Expire time must be > 0."); }

            using (var client = GetMemcachedClient(shardObject, true))
            {
                return client.Touch(ResolveKey(key), ttl);
            }
        }

        public bool Touch(object shardObject, string key, TimeSpan expired)
        {
            return Touch(shardObject, key, (int)expired.TotalSeconds);
        }

        public bool Touch(object shardObject, string key, DateTime expired)
        {
            return Touch(null, key, (int)(expired - DateTime.Now).TotalSeconds);
        }
    }
}
