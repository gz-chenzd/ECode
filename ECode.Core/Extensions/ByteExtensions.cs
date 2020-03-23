using System.Text;
using ECode.Utility;

namespace ECode.Core
{
    public static class ByteExtensions
    {
        public static string ToString(this byte[] bytes, Encoding encoding)
        {
            AssertUtil.ArgumentNotNull(encoding, nameof(encoding));

            if (bytes == null)
            { return string.Empty; }

            return encoding.GetString(bytes);
        }

        public static string ToUtf8String(this byte[] bytes)
        {
            return ToString(bytes, Encoding.UTF8);
        }

        public static string ToAsciiString(this byte[] bytes)
        {
            return ToString(bytes, Encoding.ASCII);
        }
    }
}
