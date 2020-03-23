using System;
using System.IO;
using ECode.Utility;

namespace ECode.Cryptography
{
    public sealed class HashCrypto : IHashCrypto, IDisposable
    {
        System.Security.Cryptography.HashAlgorithm      provider    = null;


        private bool IsDisposed
        { get; set; }

        public HashAlgName AlgorithmName
        { get; private set; }


        public HashCrypto(HashAlgName algorithmName)
        {
            this.AlgorithmName = algorithmName;
            this.provider = CreateProvider(algorithmName);
        }

        private System.Security.Cryptography.HashAlgorithm CreateProvider(HashAlgName algorithmName)
        {
            switch (algorithmName)
            {
                case HashAlgName.MD5:
                    return System.Security.Cryptography.MD5.Create();

                case HashAlgName.SHA1:
                    return System.Security.Cryptography.SHA1.Create();

                case HashAlgName.SHA256:
                    return System.Security.Cryptography.SHA256.Create();

                case HashAlgName.SHA384:
                    return System.Security.Cryptography.SHA384.Create();

                case HashAlgName.SHA512:
                    return System.Security.Cryptography.SHA512.Create();

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
            AssertUtil.ArgumentNotNull(stream, nameof(stream));

            if (!stream.CanRead)
            { throw new ArgumentException($"Argument '{nameof(stream)}' cannot be read."); }


            return this.provider.ComputeHash(stream);
        }
    }
}
