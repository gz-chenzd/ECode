
namespace ECode.Caching
{
    public class MemcachedCacheItem
    {
        public string Key { get; set; }

        public int Flags { get; set; }

        public int Length { get; set; }

        public long Revision { get; set; }

        public byte[] ValueBytes { get; set; }
    }
}
