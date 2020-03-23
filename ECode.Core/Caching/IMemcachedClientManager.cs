using System;

namespace ECode.Caching
{
    public interface IMemcachedClientManager : IDisposable
    {
        MemcachedClient GetClient(object shardObject, bool writable);
    }
}
