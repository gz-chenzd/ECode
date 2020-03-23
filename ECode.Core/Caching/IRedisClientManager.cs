using System;

namespace ECode.Caching
{
    public interface IRedisClientManager : IDisposable
    {
        RedisClient GetClient(object shardObject, bool writable);
    }
}
