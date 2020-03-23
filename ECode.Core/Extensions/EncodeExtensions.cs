using System.Text;
using ECode.Encoder;

namespace ECode.Core
{
    public static class EncodeExtensions
    {
        static readonly HexEncoder              HEX                 = new HexEncoder();
        static readonly HexEncoder              HEX_UPPERCASE       = new HexEncoder(true);
        static readonly Base64Encoder           BASE64              = new Base64Encoder();
        static readonly QuotedPrintableEncoder  QUOTED_PRINTABLE    = new QuotedPrintableEncoder();


        public static string ToHex(this byte[] bytes, bool upperCase = false)
        {
            if (bytes == null)
            { return string.Empty; }

            return upperCase ? HEX_UPPERCASE.Encode(bytes) : HEX.Encode(bytes);
        }

        public static string ToBase64(this byte[] bytes)
        {
            if (bytes == null)
            { return string.Empty; }

            return BASE64.Encode(bytes);
        }

        public static string ToQuotedPrintable(this byte[] bytes)
        {
            if (bytes == null)
            { return string.Empty; }

            return QUOTED_PRINTABLE.Encode(bytes);
        }


        public static byte[] FromHex(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            { return new byte[0]; }

            return HEX.Decode(Encoding.UTF8.GetBytes(str));
        }

        public static byte[] FromHex(this byte[] bytes)
        {
            if (bytes == null)
            { return new byte[0]; }

            return HEX.Decode(bytes);
        }


        public static byte[] FromBase64(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            { return new byte[0]; }

            return BASE64.Decode(Encoding.UTF8.GetBytes(str));
        }

        public static byte[] FromBase64(this byte[] bytes)
        {
            if (bytes == null)
            { return new byte[0]; }

            return BASE64.Decode(bytes);
        }


        public static byte[] FromQuotedPrintable(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            { return new byte[0]; }

            return QUOTED_PRINTABLE.Decode(Encoding.UTF8.GetBytes(str));
        }

        public static byte[] FromQuotedPrintable(this byte[] bytes)
        {
            if (bytes == null)
            { return new byte[0]; }

            return QUOTED_PRINTABLE.Decode(bytes);
        }
    }
}
