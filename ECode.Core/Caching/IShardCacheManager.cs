using System;
using System.Threading.Tasks;

namespace ECode.Caching
{
    public interface IShardCacheManager
    {
        /// <summary>
        /// Returns true if the given key stored in cache.
        /// </summary>
        /// <param name="shardObject">Object for computing shard.</param>
        /// <param name="key">Key of item to check for.</param>
        /// <exception cref="ArgumentNullException">Provided key is null.</exception>
        /// <exception cref="ArgumentException">Provided key is an empty string.</exception>
        bool Contains(object shardObject, string key);

        /// <summary>
        /// Returns true if the given key stored in cache.
        /// </summary>
        /// <param name="shardObject">Object for computing shard.</param>
        /// <param name="key">Key of item to check for.</param>
        /// <exception cref="ArgumentNullException">Provided key is null.</exception>
        /// <exception cref="ArgumentException">Provided key is an empty string.</exception>
        Task<bool> ContainsAsync(object shardObject, string key);


        /// <summary>
        /// Returns the value associated with the given key.
        /// </summary>
        /// <param name="shardObject">Object for computing shard.</param>
        /// <param name="key">Key of item to return from cache.</param>
        /// <exception cref="ArgumentNullException">Provided key is null.</exception>
        /// <exception cref="ArgumentException">Provided key is an empty string.</exception>
        string Get(object shardObject, string key);

        /// <summary>
        /// Returns the value of specified type associated with the given key.
        /// </summary>
        /// <param name="shardObject">Object for computing shard.</param>
        /// <param name="key">Key of item to return from cache.</param>
        /// <exception cref="ArgumentNullException">Provided key is null.</exception>
        /// <exception cref="ArgumentException">Provided key is an empty string.</exception>
        T Get<T>(object shardObject, string key);

        /// <summary>
        /// Returns the value of specified type associated with the given key.
        /// </summary>
        /// <param name="shardObject">Object for computing shard.</param>
        /// <param name="key">Key of item to return from cache.</param>
        /// <param name="objectType">The <see cref="System.Type"/> of value being get.</param>
        /// <exception cref="ArgumentNullException">Provided key is null.</exception>
        /// <exception cref="ArgumentException">Provided key is an empty string.</exception>
        object Get(object shardObject, string key, Type objectType);

        /// <summary>
        /// Returns the value associated with the given key.
        /// </summary>
        /// <param name="shardObject">Object for computing shard.</param>
        /// <param name="key">Key of item to return from cache.</param>
        /// <exception cref="ArgumentNullException">Provided key is null.</exception>
        /// <exception cref="ArgumentException">Provided key is an empty string.</exception>
        Task<string> GetAsync(object shardObject, string key);

        /// <summary>
        /// Returns the value of specified type associated with the given key.
        /// </summary>
        /// <param name="shardObject">Object for computing shard.</param>
        /// <param name="key">Key of item to return from cache.</param>
        /// <exception cref="ArgumentNullException">Provided key is null.</exception>
        /// <exception cref="ArgumentException">Provided key is an empty string.</exception>
        Task<T> GetAsync<T>(object shardObject, string key);

        /// <summary>
        /// Returns the value of specified type associated with the given key.
        /// </summary>
        /// <param name="shardObject">Object for computing shard.</param>
        /// <param name="key">Key of item to return from cache.</param>
        /// <param name="objectType">The <see cref="System.Type"/> of value being get.</param>
        /// <exception cref="ArgumentNullException">Provided key is null.</exception>
        /// <exception cref="ArgumentException">Provided key is an empty string.</exception>
        Task<object> GetAsync(object shardObject, string key, Type objectType);


        /// <summary>
        /// Adds new item to cache while the specified key not exists.
        /// </summary>
        /// <param name="shardObject">Object for computing shard.</param>
        /// <param name="key">Identifier for this item.</param>
        /// <param name="value">Value to be stored in cache. May be null.</param>
        /// <param name="ttl">Time to live by seconds.</param>
        /// <exception cref="ArgumentNullException">Provided key is null.</exception>
        /// <exception cref="ArgumentException">Provided key is an empty string.</exception>
        bool Add(object shardObject, string key, object value, int ttl);

        /// <summary>
        /// Adds new item to cache while the specified key not exists.
        /// </summary>
        /// <param name="shardObject">Object for computing shard.</param>
        /// <param name="key">Identifier for this item.</param>
        /// <param name="value">Value to be stored in cache. May be null.</param>
        /// <param name="expired">The expiration time of this item.</param>
        /// <exception cref="ArgumentNullException">Provided key is null.</exception>
        /// <exception cref="ArgumentException">Provided key is an empty string.</exception>
        bool Add(object shardObject, string key, object value, TimeSpan expired);

        /// <summary>
        /// Adds new item to cache while the specified key not exists.
        /// </summary>
        /// <param name="shardObject">Object for computing shard.</param>
        /// <param name="key">Identifier for this item.</param>
        /// <param name="value">Value to be stored in cache. May be null.</param>
        /// <param name="expired">The expiration time of this item.</param>
        /// <exception cref="ArgumentNullException">Provided key is null.</exception>
        /// <exception cref="ArgumentException">Provided key is an empty string.</exception>
        bool Add(object shardObject, string key, object value, DateTime expired);

        /// <summary>
        /// Adds new item to cache while the specified key not exists.
        /// </summary>
        /// <param name="shardObject">Object for computing shard.</param>
        /// <param name="key">Identifier for this item.</param>
        /// <param name="value">Value to be stored in cache. May be null.</param>
        /// <param name="ttl">Time to live by seconds.</param>
        /// <exception cref="ArgumentNullException">Provided key is null.</exception>
        /// <exception cref="ArgumentException">Provided key is an empty string.</exception>
        Task<bool> AddAsync(object shardObject, string key, object value, int ttl);

        /// <summary>
        /// Adds new item to cache while the specified key not exists.
        /// </summary>
        /// <param name="shardObject">Object for computing shard.</param>
        /// <param name="key">Identifier for this item.</param>
        /// <param name="value">Value to be stored in cache. May be null.</param>
        /// <param name="expired">The expiration time of this item.</param>
        /// <exception cref="ArgumentNullException">Provided key is null.</exception>
        /// <exception cref="ArgumentException">Provided key is an empty string.</exception>
        Task<bool> AddAsync(object shardObject, string key, object value, TimeSpan expired);

        /// <summary>
        /// Adds new item to cache while the specified key not exists.
        /// </summary>
        /// <param name="shardObject">Object for computing shard.</param>
        /// <param name="key">Identifier for this item.</param>
        /// <param name="value">Value to be stored in cache. May be null.</param>
        /// <param name="expired">The expiration time of this item.</param>
        /// <exception cref="ArgumentNullException">Provided key is null.</exception>
        /// <exception cref="ArgumentException">Provided key is an empty string.</exception>
        Task<bool> AddAsync(object shardObject, string key, object value, DateTime expired);


        /// <summary>
        /// Adds new item to cache or updates existing one.
        /// </summary>
        /// <param name="shardObject">Object for computing shard.</param>
        /// <param name="key">Identifier for this item.</param>
        /// <param name="value">Value to be stored in cache. May be null.</param>
        /// <param name="ttl">Time to live by seconds.</param>
        /// <exception cref="ArgumentNullException">Provided key is null.</exception>
        /// <exception cref="ArgumentException">Provided key is an empty string.</exception>
        bool Set(object shardObject, string key, object value, int ttl);

        /// <summary>
        /// Adds new item to cache or updates existing one.
        /// </summary>
        /// <param name="shardObject">Object for computing shard.</param>
        /// <param name="key">Identifier for this item.</param>
        /// <param name="value">Value to be stored in cache. May be null.</param>
        /// <param name="expired">The expiration time of this item.</param>
        /// <exception cref="ArgumentNullException">Provided key is null.</exception>
        /// <exception cref="ArgumentException">Provided key is an empty string.</exception>
        bool Set(object shardObject, string key, object value, TimeSpan expired);

        /// <summary>
        /// Adds new item to cache or updates existing one.
        /// </summary>
        /// <param name="shardObject">Object for computing shard.</param>
        /// <param name="key">Identifier for this item.</param>
        /// <param name="value">Value to be stored in cache. May be null.</param>
        /// <param name="expired">The expiration time of this item.</param>
        /// <exception cref="ArgumentNullException">Provided key is null.</exception>
        /// <exception cref="ArgumentException">Provided key is an empty string.</exception>
        bool Set(object shardObject, string key, object value, DateTime expired);

        /// <summary>
        /// Adds new item to cache or updates existing one.
        /// </summary>
        /// <param name="shardObject">Object for computing shard.</param>
        /// <param name="key">Identifier for this item.</param>
        /// <param name="value">Value to be stored in cache. May be null.</param>
        /// <param name="ttl">Time to live by seconds.</param>
        /// <exception cref="ArgumentNullException">Provided key is null.</exception>
        /// <exception cref="ArgumentException">Provided key is an empty string.</exception>
        Task<bool> SetAsync(object shardObject, string key, object value, int ttl);

        /// <summary>
        /// Adds new item to cache or updates existing one.
        /// </summary>
        /// <param name="shardObject">Object for computing shard.</param>
        /// <param name="key">Identifier for this item.</param>
        /// <param name="value">Value to be stored in cache. May be null.</param>
        /// <param name="expired">The expiration time of this item.</param>
        /// <exception cref="ArgumentNullException">Provided key is null.</exception>
        /// <exception cref="ArgumentException">Provided key is an empty string.</exception>
        Task<bool> SetAsync(object shardObject, string key, object value, TimeSpan expired);

        /// <summary>
        /// Adds new item to cache or updates existing one.
        /// </summary>
        /// <param name="shardObject">Object for computing shard.</param>
        /// <param name="key">Identifier for this item.</param>
        /// <param name="value">Value to be stored in cache. May be null.</param>
        /// <param name="expired">The expiration time of this item.</param>
        /// <exception cref="ArgumentNullException">Provided key is null.</exception>
        /// <exception cref="ArgumentException">Provided key is an empty string.</exception>
        Task<bool> SetAsync(object shardObject, string key, object value, DateTime expired);


        /// <summary>
        /// Replace cache item with new value while the specified key exists.
        /// </summary>
        /// <param name="shardObject">Object for computing shard.</param>
        /// <param name="key">Identifier for this item.</param>
        /// <param name="value">Value to be stored in cache. May be null.</param>
        /// <param name="ttl">Time to live by seconds.</param>
        /// <exception cref="ArgumentNullException">Provided key is null.</exception>
        /// <exception cref="ArgumentException">Provided key is an empty string.</exception>
        bool Replace(object shardObject, string key, object value, int ttl);

        /// <summary>
        /// Replace cache item with new value while the specified key exists.
        /// </summary>
        /// <param name="shardObject">Object for computing shard.</param>
        /// <param name="key">Identifier for this item.</param>
        /// <param name="value">Value to be stored in cache. May be null.</param>
        /// <param name="expired">The expiration time of this item.</param>
        /// <exception cref="ArgumentNullException">Provided key is null.</exception>
        /// <exception cref="ArgumentException">Provided key is an empty string.</exception>
        bool Replace(object shardObject, string key, object value, TimeSpan expired);

        /// <summary>
        /// Replace cache item with new value while the specified key exists.
        /// </summary>
        /// <param name="shardObject">Object for computing shard.</param>
        /// <param name="key">Identifier for this item.</param>
        /// <param name="value">Value to be stored in cache. May be null.</param>
        /// <param name="expired">The expiration time of this item.</param>
        /// <exception cref="ArgumentNullException">Provided key is null.</exception>
        /// <exception cref="ArgumentException">Provided key is an empty string.</exception>
        bool Replace(object shardObject, string key, object value, DateTime expired);

        /// <summary>
        /// Replace cache item with new value while the specified key exists.
        /// </summary>
        /// <param name="shardObject">Object for computing shard.</param>
        /// <param name="key">Identifier for this item.</param>
        /// <param name="value">Value to be stored in cache. May be null.</param>
        /// <param name="ttl">Time to live by seconds.</param>
        /// <exception cref="ArgumentNullException">Provided key is null.</exception>
        /// <exception cref="ArgumentException">Provided key is an empty string.</exception>
        Task<bool> ReplaceAsync(object shardObject, string key, object value, int ttl);

        /// <summary>
        /// Replace cache item with new value while the specified key exists.
        /// </summary>
        /// <param name="shardObject">Object for computing shard.</param>
        /// <param name="key">Identifier for this item.</param>
        /// <param name="value">Value to be stored in cache. May be null.</param>
        /// <param name="expired">The expiration time of this item.</param>
        /// <exception cref="ArgumentNullException">Provided key is null.</exception>
        /// <exception cref="ArgumentException">Provided key is an empty string.</exception>
        Task<bool> ReplaceAsync(object shardObject, string key, object value, TimeSpan expired);

        /// <summary>
        /// Replace cache item with new value while the specified key exists.
        /// </summary>
        /// <param name="shardObject">Object for computing shard.</param>
        /// <param name="key">Identifier for this item.</param>
        /// <param name="value">Value to be stored in cache. May be null.</param>
        /// <param name="expired">The expiration time of this item.</param>
        /// <exception cref="ArgumentNullException">Provided key is null.</exception>
        /// <exception cref="ArgumentException">Provided key is an empty string.</exception>
        Task<bool> ReplaceAsync(object shardObject, string key, object value, DateTime expired);


        /// <summary>
        /// Removes the given item from cache.
        /// </summary>
        /// <param name="shardObject">Object for computing shard.</param>
        /// <param name="key">Key of item to remove from cache.</param>
        /// <exception cref="ArgumentNullException">Provided key is null.</exception>
        /// <exception cref="ArgumentException">Provided key is an empty string.</exception>
        bool Remove(object shardObject, string key);

        /// <summary>
        /// Removes the given item from cache.
        /// </summary>
        /// <param name="shardObject">Object for computing shard.</param>
        /// <param name="key">Key of item to remove from cache.</param>
        /// <exception cref="ArgumentNullException">Provided key is null.</exception>
        /// <exception cref="ArgumentException">Provided key is an empty string.</exception>
        Task<bool> RemoveAsync(object shardObject, string key);


        /// <summary>
        /// Increases cache item with specified value.
        /// </summary>
        /// <param name="shardObject">Object for computing shard.</param>
        /// <param name="key">Identifier for this item.</param>
        /// <param name="delta">Delta value to increase.</param>
        /// <returns>Value after increased.</returns>
        /// <exception cref="ArgumentNullException">Provided key is null.</exception>
        /// <exception cref="ArgumentException">Provided key is an empty string.</exception>
        /// <exception cref="InvalidOperationException">Value isnot an integer or out of range.</exception>
        long Increase(object shardObject, string key, int delta = 1);

        /// <summary>
        /// Increases cache item with specified value.
        /// </summary>
        /// <param name="shardObject">Object for computing shard.</param>
        /// <param name="key">Identifier for this item.</param>
        /// <param name="delta">Delta value to increase.</param>
        /// <returns>Value after increased.</returns>
        /// <exception cref="ArgumentNullException">Provided key is null.</exception>
        /// <exception cref="ArgumentException">Provided key is an empty string.</exception>
        /// <exception cref="InvalidOperationException">Value isnot an integer or out of range.</exception>
        Task<long> IncreaseAsync(object shardObject, string key, int delta = 1);

        /// <summary>
        /// Decreases cache item with specified value.
        /// </summary>
        /// <param name="shardObject">Object for computing shard.</param>
        /// <param name="key">Identifier for this item.</param>
        /// <param name="delta">Delta value to decrease.</param>
        /// <returns>Value after decreased.</returns>
        /// <exception cref="ArgumentNullException">Provided key is null.</exception>
        /// <exception cref="ArgumentException">Provided key is an empty string.</exception>
        /// <exception cref="InvalidOperationException">Value isnot an integer or out of range.</exception>
        long Decrease(object shardObject, string key, int delta = 1);

        /// <summary>
        /// Decreases cache item with specified value.
        /// </summary>
        /// <param name="shardObject">Object for computing shard.</param>
        /// <param name="key">Identifier for this item.</param>
        /// <param name="delta">Delta value to decrease.</param>
        /// <returns>Value after decreased.</returns>
        /// <exception cref="ArgumentNullException">Provided key is null.</exception>
        /// <exception cref="ArgumentException">Provided key is an empty string.</exception>
        /// <exception cref="InvalidOperationException">Value isnot an integer or out of range.</exception>
        Task<long> DecreaseAsync(object shardObject, string key, int delta = 1);


        /// <summary>
        /// Refresh the expiration time of cache item.
        /// </summary>
        /// <param name="shardObject">Object for computing shard.</param>
        /// <param name="key">Identifier for cache item.</param>
        /// <param name="ttl">Time to live by seconds.</param>
        /// <exception cref="ArgumentNullException">Provided key is null.</exception>
        /// <exception cref="ArgumentException">Provided key is an empty string.</exception>
        bool Touch(object shardObject, string key, int ttl);

        /// <summary>
        /// Refresh the expiration time of cache item.
        /// </summary>
        /// <param name="shardObject">Object for computing shard.</param>
        /// <param name="key">Identifier for cache item.</param>
        /// <param name="expired">The expiration time.</param>
        /// <exception cref="ArgumentNullException">Provided key is null.</exception>
        /// <exception cref="ArgumentException">Provided key is an empty string.</exception>
        bool Touch(object shardObject, string key, TimeSpan expired);

        /// <summary>
        /// Refresh the expiration time of cache item.
        /// </summary>
        /// <param name="shardObject">Object for computing shard.</param>
        /// <param name="key">Identifier for cache item.</param>
        /// <param name="expired">The expiration time.</param>
        /// <exception cref="ArgumentNullException">Provided key is null.</exception>
        /// <exception cref="ArgumentException">Provided key is an empty string.</exception>
        bool Touch(object shardObject, string key, DateTime expired);

        /// <summary>
        /// Refresh the expiration time of cache item.
        /// </summary>
        /// <param name="shardObject">Object for computing shard.</param>
        /// <param name="key">Identifier for cache item.</param>
        /// <param name="ttl">Time to live by seconds.</param>
        /// <exception cref="ArgumentNullException">Provided key is null.</exception>
        /// <exception cref="ArgumentException">Provided key is an empty string.</exception>
        Task<bool> TouchAsync(object shardObject, string key, int ttl);

        /// <summary>
        /// Refresh the expiration time of cache item.
        /// </summary>
        /// <param name="shardObject">Object for computing shard.</param>
        /// <param name="key">Identifier for cache item.</param>
        /// <param name="expired">The expiration time.</param>
        /// <exception cref="ArgumentNullException">Provided key is null.</exception>
        /// <exception cref="ArgumentException">Provided key is an empty string.</exception>
        Task<bool> TouchAsync(object shardObject, string key, TimeSpan expired);

        /// <summary>
        /// Refresh the expiration time of cache item.
        /// </summary>
        /// <param name="shardObject">Object for computing shard.</param>
        /// <param name="key">Identifier for cache item.</param>
        /// <param name="expired">The expiration time.</param>
        /// <exception cref="ArgumentNullException">Provided key is null.</exception>
        /// <exception cref="ArgumentException">Provided key is an empty string.</exception>
        Task<bool> TouchAsync(object shardObject, string key, DateTime expired);
    }
}
