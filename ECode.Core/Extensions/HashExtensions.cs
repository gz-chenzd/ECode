using System.Text;
using ECode.Cryptography;

namespace ECode.Core
{
    public static class HashExtensions
    {
        private static byte[] ComputeHash(HashAlgName algorithmName, byte[] bytes)
        {
            using (var provider = new HashCrypto(algorithmName))
            {
                return provider.ComputeHash(bytes);
            }
        }

        private static byte[] ComputeHmacHash(HashAlgName algorithmName, byte[] bytes, byte[] key)
        {
            using (var provider = new HmacHashCrypto(algorithmName, key))
            {
                return provider.ComputeHash(bytes);
            }
        }


        public static byte[] ComputeMD5(this byte[] bytes)
        {
            return ComputeHash(HashAlgName.MD5, bytes);
        }

        public static byte[] ComputeSHA1(this byte[] bytes)
        {
            return ComputeHash(HashAlgName.SHA1, bytes);
        }

        public static byte[] ComputeSHA256(this byte[] bytes)
        {
            return ComputeHash(HashAlgName.SHA256, bytes);
        }

        public static byte[] ComputeSHA384(this byte[] bytes)
        {
            return ComputeHash(HashAlgName.SHA384, bytes);
        }

        public static byte[] ComputeSHA512(this byte[] bytes)
        {
            return ComputeHash(HashAlgName.SHA512, bytes);
        }


        public static byte[] ComputeMD5(this string str)
        {
            return ComputeHash(HashAlgName.MD5, Encoding.UTF8.GetBytes(str));
        }

        public static byte[] ComputeSHA1(this string str)
        {
            return ComputeHash(HashAlgName.SHA1, Encoding.UTF8.GetBytes(str));
        }

        public static byte[] ComputeSHA256(this string str)
        {
            return ComputeHash(HashAlgName.SHA256, Encoding.UTF8.GetBytes(str));
        }

        public static byte[] ComputeSHA384(this string str)
        {
            return ComputeHash(HashAlgName.SHA384, Encoding.UTF8.GetBytes(str));
        }

        public static byte[] ComputeSHA512(this string str)
        {
            return ComputeHash(HashAlgName.SHA512, Encoding.UTF8.GetBytes(str));
        }


        public static byte[] ComputeHmacMD5(this byte[] bytes, byte[] key)
        {
            return ComputeHmacHash(HashAlgName.MD5, bytes, key);
        }

        public static byte[] ComputeHmacSHA1(this byte[] bytes, byte[] key)
        {
            return ComputeHmacHash(HashAlgName.SHA1, bytes, key);
        }

        public static byte[] ComputeHmacSHA256(this byte[] bytes, byte[] key)
        {
            return ComputeHmacHash(HashAlgName.SHA256, bytes, key);
        }

        public static byte[] ComputeHmacSHA384(this byte[] bytes, byte[] key)
        {
            return ComputeHmacHash(HashAlgName.SHA384, bytes, key);
        }

        public static byte[] ComputeHmacSHA512(this byte[] bytes, byte[] key)
        {
            return ComputeHmacHash(HashAlgName.SHA512, bytes, key);
        }


        public static byte[] ComputeHmacMD5(this string str, string key)
        {
            return ComputeHmacHash(HashAlgName.MD5, Encoding.UTF8.GetBytes(str), Encoding.UTF8.GetBytes(key));
        }

        public static byte[] ComputeHmacSHA1(this string str, string key)
        {
            return ComputeHmacHash(HashAlgName.SHA1, Encoding.UTF8.GetBytes(str), Encoding.UTF8.GetBytes(key));
        }

        public static byte[] ComputeHmacSHA256(this string str, string key)
        {
            return ComputeHmacHash(HashAlgName.SHA256, Encoding.UTF8.GetBytes(str), Encoding.UTF8.GetBytes(key));
        }

        public static byte[] ComputeHmacSHA384(this string str, string key)
        {
            return ComputeHmacHash(HashAlgName.SHA384, Encoding.UTF8.GetBytes(str), Encoding.UTF8.GetBytes(key));
        }

        public static byte[] ComputeHmacSHA512(this string str, string key)
        {
            return ComputeHmacHash(HashAlgName.SHA512, Encoding.UTF8.GetBytes(str), Encoding.UTF8.GetBytes(key));
        }
    }
}
