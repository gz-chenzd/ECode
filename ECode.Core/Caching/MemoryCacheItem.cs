using System;

namespace ECode.Caching
{
    public enum CacheValueType
    {
        Unknown,

        Json,

        Plain,

        Binary,
    }


    public class MemoryCacheItem
    {
        internal MemoryCacheItem Previous
        { get; set; }

        internal MemoryCacheItem Next
        { get; set; }


        public string Key
        { get; set; }

        public CacheValueType ValueType
        { get; set; }

        public string StringValue
        { get; set; }

        public byte[] BinaryValue
        { get; set; }

        public DateTime ExpireTime
        { get; set; }


        public bool IsExpired
        {
            get
            {
                return DateTime.Now > this.ExpireTime;
            }
        }
    }
}
