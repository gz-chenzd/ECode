using System;
using System.IO;
using ECode.Utility;

namespace ECode.Cryptography
{
    public class HmacHashCrypto : IHashCrypto, IDisposable
    {
        private byte[]                              key         = null;
        private System.Security.Cryptography.HMAC   provider    = null;


        private bool IsDisposed
        { get; set; }

        public HashAlgName AlgorithmName
        { get; private set; }


        public byte[] Key
        {
            get
            {
                ThrowIfObjectDisposed();

                return (byte[])this.key.Clone();
            }

            set
            {
                ThrowIfObjectDisposed();

                AssertUtil.ArgumentNotEmpty(value, nameof(Key));

                this.key = value;
            }
        }


        public HmacHashCrypto(HashAlgName algorithmName, byte[] key)
        {
            AssertUtil.ArgumentNotEmpty(key, nameof(key));

            this.AlgorithmName = algorithmName;
            this.Key = key;
            this.provider = CreateProvider(algorithmName, key);
        }

        private System.Security.Cryptography.HMAC CreateProvider(HashAlgName algorithmName, byte[] key)
        {
            switch (algorithmName)
            {
                case HashAlgName.MD5:
                    return new System.Security.Cryptography.HMACMD5(this.Key);

                case HashAlgName.SHA1:
                    return new System.Security.Cryptography.HMACSHA1(this.Key);

                case HashAlgName.SHA256:
                    return new System.Security.Cryptography.HMACSHA256(this.Key);

                case HashAlgName.SHA384:
                    return new System.Security.Cryptography.HMACSHA384(this.Key);

                case HashAlgName.SHA512:
                    return new System.Security.Cryptography.HMACSHA512(this.Key);

                default:
                    throw new NotSupportedException($"Unsupported hash algorithm '{algorithmName}'.");
            }
        }


        public void Dispose()
        {
            if (this.IsDisposed)
            { return; }

            this.IsDisposed = true;

            this.provider.Dispose();
            this.provider = null;
        }

        private void ThrowIfObjectDisposed()
        {
            if (this.IsDisposed)
            { throw new ObjectDisposedException(this.GetType().Name); }
        }


        public byte[] ComputeHash(byte[] bytes)
        {
            AssertUtil.ArgumentNotNull(bytes, nameof(bytes));

            return ComputeHash(bytes, 0, bytes.Length);
        }

        public byte[] ComputeHash(byte[] bytes, int index, int count)
        {
            ThrowIfObjectDisposed();

            if (bytes == null)
            { throw new ArgumentNullException(nameof(bytes)); }

            if (index < 0)
            { throw new ArgumentOutOfRangeException(nameof(index), $"Argument '{nameof(index)}' value must be >= 0."); }

            if (index > bytes.Length)
            { throw new ArgumentOutOfRangeException(nameof(index), $"Argument '{nameof(index)}' value exceeds the maximum length of argument '{nameof(bytes)}'."); }

            if (count < 0)
            { throw new ArgumentOutOfRangeException(nameof(count), $"Argument '{nameof(count)}' value must be >= 0."); }

            if (index + count > bytes.Length)
            { throw new ArgumentOutOfRangeException(nameof(count), $"Argument '{nameof(index)} + {nameof(count)}' value exceeds the maximum length of argument '{nameof(bytes)}'."); }


            return this.provider.ComputeHash(bytes, index, count);
        }

        public byte[] ComputeHash(Stream stream)
        {
            ThrowIfObjectDisposed();

            AssertUtil.ArgumentNotNull(stream, nameof(stream));

            if (!stream.CanRead)
            { throw new ArgumentException($"Argument '{nameof(stream)}' cannot be read."); }


            return this.provider.ComputeHash(stream);
        }
    }
}
