using ECode.Checksums;
using ECode.Core;

namespace ECode.Caching
{
    public class CrcShardStrategy : IShardStrategy
    {
        public string GetShardNo(object target)
        {
            if (target == null)
            { return string.Empty; }

            var str = target.ToString();
            if (string.IsNullOrWhiteSpace(str))
            { return string.Empty; }

            var checksum = new Crc32_IEEE();
            checksum.Update(str.ToBytes());

            return checksum.Value.ToString();
        }
    }
}
