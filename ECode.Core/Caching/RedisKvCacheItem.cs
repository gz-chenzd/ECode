
namespace ECode.Caching
{
    public class RedisKvCacheItem
    {
        public string Key { get; set; }

        public long Revision { get; set; }

        public byte[] ValueBytes { get; set; }

        public string StringValue { get; set; }
    }
}
