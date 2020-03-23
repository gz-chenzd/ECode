using System;
using System.IO;
using ECode.IO;
using ECode.Utility;

namespace ECode.Cryptography
{
    public sealed class SymmetricCrypto : ISymmetricCrypto, IDisposable
    {
        static System.Security.Cryptography.CipherMode ToSysCipherMode(CipherMode mode)
        {
            switch (mode)
            {
                case CipherMode.CBC:
                    return System.Security.Cryptography.CipherMode.CBC;

                case CipherMode.ECB:
                    return System.Security.Cryptography.CipherMode.ECB;

                case CipherMode.OFB:
                    return System.Security.Cryptography.CipherMode.OFB;

                case CipherMode.CFB:
                    return System.Security.Cryptography.CipherMode.CFB;

                case CipherMode.CTS:
                    return System.Security.Cryptography.CipherMode.CTS;

                default:
                    throw new Exception();
            }
        }

        static CipherMode FromSysCipherMode(System.Security.Cryptography.CipherMode mode)
        {
            switch (mode)
            {
                case System.Security.Cryptography.CipherMode.CBC:
                    return CipherMode.CBC;

                case System.Security.Cryptography.CipherMode.ECB:
                    return CipherMode.ECB;

                case System.Security.Cryptography.CipherMode.OFB:
                    return CipherMode.OFB;

                case System.Security.Cryptography.CipherMode.CFB:
                    return CipherMode.CFB;

                case System.Security.Cryptography.CipherMode.CTS:
                    return CipherMode.CTS;

                default:
                    throw new Exception();
            }
        }

        static System.Security.Cryptography.PaddingMode ToSysPaddingMode(PaddingMode padding)
        {
            switch (padding)
            {
                case PaddingMode.None:
                    return System.Security.Cryptography.PaddingMode.None;

                case PaddingMode.PKCS7:
                    return System.Security.Cryptography.PaddingMode.PKCS7;

                case PaddingMode.Zeros:
                    return System.Security.Cryptography.PaddingMode.Zeros;

                case PaddingMode.ANSIX923:
                    return System.Security.Cryptography.PaddingMode.ANSIX923;

                case PaddingMode.ISO10126:
                    return System.Security.Cryptography.PaddingMode.ISO10126;

                default:
                    throw new Exception();
            }
        }

        static PaddingMode FromSysPaddingMode(System.Security.Cryptography.PaddingMode padding)
        {
            switch (padding)
            {
                case System.Security.Cryptography.PaddingMode.None:
                    return PaddingMode.None;

                case System.Security.Cryptography.PaddingMode.PKCS7:
                    return PaddingMode.PKCS7;

                case System.Security.Cryptography.PaddingMode.Zeros:
                    return PaddingMode.Zeros;

                case System.Security.Cryptography.PaddingMode.ANSIX923:
                    return PaddingMode.ANSIX923;

                case System.Security.Cryptography.PaddingMode.ISO10126:
                    return PaddingMode.ISO10126;

                default:
                    throw new Exception();
            }
        }


        System.Security.Cryptography.SymmetricAlgorithm     provider    = null;


        private bool IsDisposed
        { get; set; }

        public SymmetricAlgName AlgorithmName
        { get; private set; }


        public byte[] Key
        {
            get
            {
                ThrowIfObjectDisposed();

                return this.provider.Key;
            }

            set
            {
                ThrowIfObjectDisposed();

                this.provider.Key = value;
            }
        }

        public byte[] IV
        {
            get
            {

                ThrowIfObjectDisposed();

                return this.provider.IV;
            }

            set
            {
                ThrowIfObjectDisposed();

                this.provider.IV = value;
            }
        }

        public int KeySize
        {
            get
            {
                ThrowIfObjectDisposed();

                return this.provider.KeySize;
            }

            set
            {
                ThrowIfObjectDisposed();

                this.provider.KeySize = value;
            }
        }

        public int BlockSize
        {
            get
            {
                ThrowIfObjectDisposed();

                return this.provider.BlockSize;
            }

            set
            {
                ThrowIfObjectDisposed();

                this.provider.BlockSize = value;
            }
        }

        public CipherMode Mode
        {
            get
            {
                ThrowIfObjectDisposed();

                return FromSysCipherMode(this.provider.Mode);
            }

            set
            {
                ThrowIfObjectDisposed();

                this.provider.Mode = ToSysCipherMode(value);
            }
        }

        public PaddingMode Padding
        {
            get
            {
                ThrowIfObjectDisposed();

                return FromSysPaddingMode(this.provider.Padding);
            }

            set
            {
                ThrowIfObjectDisposed();

                this.provider.Padding = ToSysPaddingMode(value);
            }
        }


        public SymmetricCrypto(SymmetricAlgName algorithmName)
        {
            this.AlgorithmName = algorithmName;
            this.provider = CreateProvider(algorithmName);
        }

        private System.Security.Cryptography.SymmetricAlgorithm CreateProvider(SymmetricAlgName algorithmName)
        {
            switch (algorithmName)
            {
                case SymmetricAlgName.AES:
                    return System.Security.Cryptography.Aes.Create();

                case SymmetricAlgName.DES:
                    return System.Security.Cryptography.DES.Create();

                case SymmetricAlgName.TripleDES:
                    return System.Security.Cryptography.TripleDES.Create();

                default:
                    throw new NotSupportedException($"Unsupported symmetric algorithm '{algorithmName}'.");
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


        public byte[] Encrypt(byte[] bytes)
        {
            AssertUtil.ArgumentNotNull(bytes, nameof(bytes));

            return Encrypt(bytes, 0, bytes.Length);
        }

        public byte[] Encrypt(byte[] bytes, int index, int count)
        {
            ThrowIfObjectDisposed();

            if (bytes == null)
            { throw new ArgumentNullException(nameof(bytes)); }

            if (index < 0)
            { throw new ArgumentOutOfRangeException(nameof(index), $"Argument '{nameof(index)}' value must be >= 0."); }

            if (index >= bytes.Length)
            { throw new ArgumentOutOfRangeException(nameof(index), $"Argument '{nameof(index)}' value exceeds the maximum length of argument '{nameof(bytes)}'."); }

            if (count < 0)
            { throw new ArgumentOutOfRangeException(nameof(count), $"Argument '{nameof(count)}' value must be >= 0."); }

            if (index + count > bytes.Length)
            { throw new ArgumentOutOfRangeException(nameof(count), $"Argument '{nameof(index)} + {nameof(count)}' value exceeds the maximum length of argument '{nameof(bytes)}'."); }


            #region 注释的也是一种方法，效果一样

            //using (var encryptor = this.provider.CreateEncryptor())
            //{
            //    return encryptor.TransformFinalBlock(bytes, index, count);
            //}

            #endregion

            using (var outStream = new MemoryStream())
            using (var encryptor = this.provider.CreateEncryptor())
            using (var writer = new System.Security.Cryptography.CryptoStream(outStream, encryptor, System.Security.Cryptography.CryptoStreamMode.Write))
            {
                writer.Write(bytes, index, count);
                writer.FlushFinalBlock();

                return outStream.ToArray();
            }
        }

        public void Encrypt(Stream inStream, Stream outStream)
        {
            ThrowIfObjectDisposed();

            AssertUtil.ArgumentNotNull(inStream, nameof(inStream));
            AssertUtil.ArgumentNotNull(outStream, nameof(outStream));

            if (!inStream.CanRead)
            { throw new ArgumentException($"Argument '{nameof(inStream)}' cannot be read."); }

            if (!outStream.CanWrite)
            { throw new ArgumentException($"Argument '{nameof(outStream)}' not supports writing."); }


            using (var encryptor = this.provider.CreateEncryptor())
            using (var writer = new System.Security.Cryptography.CryptoStream(outStream, encryptor, System.Security.Cryptography.CryptoStreamMode.Write, true))
            {
                StreamUtil.StreamCopy(inStream, writer);
                writer.FlushFinalBlock();
            }
        }


        public byte[] Decrypt(byte[] bytes)
        {
            AssertUtil.ArgumentNotNull(bytes, nameof(bytes));

            return Decrypt(bytes, 0, bytes.Length);
        }

        public byte[] Decrypt(byte[] bytes, int index, int count)
        {
            ThrowIfObjectDisposed();

            if (bytes == null)
            { throw new ArgumentNullException(nameof(bytes)); }

            if (index < 0)
            { throw new ArgumentOutOfRangeException(nameof(index), $"Argument '{nameof(index)}' value must be >= 0."); }

            if (index >= bytes.Length)
            { throw new ArgumentOutOfRangeException(nameof(index), $"Argument '{nameof(index)}' value exceeds the maximum length of argument '{nameof(bytes)}'."); }

            if (count < 0)
            { throw new ArgumentOutOfRangeException(nameof(count), $"Argument '{nameof(count)}' value must be >= 0."); }

            if (index + count > bytes.Length)
            { throw new ArgumentOutOfRangeException(nameof(count), $"Argument '{nameof(index)} + {nameof(count)}' value exceeds the maximum length of argument '{nameof(bytes)}'."); }


            #region 注释的也是一种方法，效果一样

            //using (var decryptor = this.provider.CreateDecryptor())
            //{
            //    return decryptor.TransformFinalBlock(bytes, index, count);
            //}

            #endregion

            using (var outStream = new MemoryStream())
            using (var inStream = new MemoryStream(bytes, index, count))
            using (var decryptor = this.provider.CreateDecryptor())
            using (var reader = new System.Security.Cryptography.CryptoStream(inStream, decryptor, System.Security.Cryptography.CryptoStreamMode.Read))
            {
                reader.CopyTo(outStream);

                return outStream.ToArray();
            }
        }

        public void Decrypt(Stream inStream, Stream outStream)
        {
            ThrowIfObjectDisposed();

            AssertUtil.ArgumentNotNull(inStream, nameof(inStream));
            AssertUtil.ArgumentNotNull(outStream, nameof(outStream));

            if (!inStream.CanRead)
            { throw new ArgumentException($"Argument '{nameof(inStream)}' cannot be read."); }

            if (!outStream.CanWrite)
            { throw new ArgumentException($"Argument '{nameof(outStream)}' not supports writing."); }


            using (var decryptor = this.provider.CreateDecryptor())
            using (var reader = new System.Security.Cryptography.CryptoStream(inStream, decryptor, System.Security.Cryptography.CryptoStreamMode.Read, true))
            {
                reader.CopyTo(outStream);
                outStream.Flush();
            }
        }
    }
}