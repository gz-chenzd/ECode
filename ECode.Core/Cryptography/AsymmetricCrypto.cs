using System;
using System.IO;
using ECode.Utility;

namespace ECode.Cryptography
{
    public sealed class AsymmetricCrypto : IAsymmetricCrypto, IDisposable
    {
        static System.Security.Cryptography.HashAlgorithmName ToSysHashAlgorithmName(HashAlgName hashAlgorithm)
        {
            switch (hashAlgorithm)
            {
                case HashAlgName.MD5:
                    return System.Security.Cryptography.HashAlgorithmName.MD5;

                case HashAlgName.SHA1:
                    return System.Security.Cryptography.HashAlgorithmName.SHA1;

                case HashAlgName.SHA256:
                    return System.Security.Cryptography.HashAlgorithmName.SHA256;

                case HashAlgName.SHA384:
                    return System.Security.Cryptography.HashAlgorithmName.SHA384;

                case HashAlgName.SHA512:
                    return System.Security.Cryptography.HashAlgorithmName.SHA512;

                default:
                    throw new NotSupportedException($"Unsupported hash algorithm '{hashAlgorithm}'.");
            }
        }

        static System.Security.Cryptography.RSAEncryptionPadding ToSysEncryptionPaddingMode(EncryptionPaddingMode padding)
        {
            switch (padding)
            {
                case EncryptionPaddingMode.Pkcs1:
                    return System.Security.Cryptography.RSAEncryptionPadding.Pkcs1;

                case EncryptionPaddingMode.OaepSHA1:
                    return System.Security.Cryptography.RSAEncryptionPadding.OaepSHA1;

                case EncryptionPaddingMode.OaepSHA256:
                    return System.Security.Cryptography.RSAEncryptionPadding.OaepSHA256;

                case EncryptionPaddingMode.OaepSHA384:
                    return System.Security.Cryptography.RSAEncryptionPadding.OaepSHA384;

                case EncryptionPaddingMode.OaepSHA512:
                    return System.Security.Cryptography.RSAEncryptionPadding.OaepSHA512;

                default:
                    throw new Exception();
            }
        }

        static System.Security.Cryptography.RSASignaturePadding ToSysSignaturePaddingMode(SignaturePaddingMode padding)
        {
            switch (padding)
            {
                case SignaturePaddingMode.Pss:
                    return System.Security.Cryptography.RSASignaturePadding.Pss;

                case SignaturePaddingMode.Pkcs1:
                    return System.Security.Cryptography.RSASignaturePadding.Pkcs1;

                default:
                    throw new Exception();
            }
        }


        System.Security.Cryptography.AsymmetricAlgorithm    provider    = null;


        private bool IsDisposed
        { get; set; }

        public AsymmetricAlgName AlgorithmName
        { get; private set; }


        public AsymmetricCrypto(AsymmetricAlgName algorithmName)
        {
            this.AlgorithmName = algorithmName;
            this.provider = CreateProvider(algorithmName);
        }

        private System.Security.Cryptography.AsymmetricAlgorithm CreateProvider(AsymmetricAlgName algorithmName)
        {
            switch (algorithmName)
            {
                case AsymmetricAlgName.RSA:
                    return System.Security.Cryptography.RSA.Create();

                case AsymmetricAlgName.DSA:
                    return System.Security.Cryptography.DSA.Create();

                case AsymmetricAlgName.ECDsa:
                    return System.Security.Cryptography.ECDsa.Create();

                default:
                    throw new NotSupportedException($"Unsupported asymmetric algorithm '{algorithmName}'.");
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


        public void ImportParameters(string xmlString)
        {
            ThrowIfObjectDisposed();

            this.provider.FromXmlString(xmlString);
        }

        public void ImportParameters(System.Security.Cryptography.RSAParameters parameters)
        {
            ThrowIfObjectDisposed();

            if (this.AlgorithmName != AsymmetricAlgName.RSA)
            { throw new InvalidOperationException("Only supported by RSA."); }

            (this.provider as System.Security.Cryptography.RSA).ImportParameters(parameters);
        }

        public void ImportParameters(System.Security.Cryptography.DSAParameters parameters)
        {
            ThrowIfObjectDisposed();

            if (this.AlgorithmName != AsymmetricAlgName.DSA)
            { throw new InvalidOperationException("Only supported by DSA."); }

            (this.provider as System.Security.Cryptography.DSA).ImportParameters(parameters);
        }

        public void ImportParameters(System.Security.Cryptography.ECParameters parameters)
        {
            ThrowIfObjectDisposed();

            if (this.AlgorithmName != AsymmetricAlgName.ECDsa)
            { throw new InvalidOperationException("Only supported by ECDsa."); }

            (this.provider as System.Security.Cryptography.ECDsa).ImportParameters(parameters);
        }


        public byte[] SignData(byte[] bytes, HashAlgName hashAlgorithm)
        {
            AssertUtil.ArgumentNotNull(bytes, nameof(bytes));

            return SignData(bytes, 0, bytes.Length, hashAlgorithm);
        }

        public byte[] SignData(byte[] bytes, int index, int count, HashAlgName hashAlgorithm)
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


            switch (this.AlgorithmName)
            {
                case AsymmetricAlgName.RSA:
                    return (this.provider as System.Security.Cryptography.RSA).SignData(bytes, index, count, ToSysHashAlgorithmName(hashAlgorithm), System.Security.Cryptography.RSASignaturePadding.Pkcs1);

                case AsymmetricAlgName.DSA:
                    return (this.provider as System.Security.Cryptography.DSA).SignData(bytes, index, count, ToSysHashAlgorithmName(hashAlgorithm));

                case AsymmetricAlgName.ECDsa:
                    return (this.provider as System.Security.Cryptography.ECDsa).SignData(bytes, index, count, ToSysHashAlgorithmName(hashAlgorithm));

                default:
                    throw new NotSupportedException($"Unsupported asymmetric algorithm '{this.AlgorithmName}'.");
            }
        }

        public byte[] SignData(Stream stream, HashAlgName hashAlgorithm)
        {
            ThrowIfObjectDisposed();

            AssertUtil.ArgumentNotNull(stream, nameof(stream));

            if (!stream.CanRead)
            { throw new ArgumentException($"Argument '{nameof(stream)}' cannot be read."); }


            switch (this.AlgorithmName)
            {
                case AsymmetricAlgName.RSA:
                    return (this.provider as System.Security.Cryptography.RSA).SignData(stream, ToSysHashAlgorithmName(hashAlgorithm), System.Security.Cryptography.RSASignaturePadding.Pkcs1);

                case AsymmetricAlgName.DSA:
                    return (this.provider as System.Security.Cryptography.DSA).SignData(stream, ToSysHashAlgorithmName(hashAlgorithm));

                case AsymmetricAlgName.ECDsa:
                    return (this.provider as System.Security.Cryptography.ECDsa).SignData(stream, ToSysHashAlgorithmName(hashAlgorithm));

                default:
                    throw new NotSupportedException($"Unsupported asymmetric algorithm '{this.AlgorithmName}'.");
            }
        }


        public bool VerifyData(byte[] bytes, byte[] signature, HashAlgName hashAlgorithm)
        {
            AssertUtil.ArgumentNotNull(bytes, nameof(bytes));
            AssertUtil.ArgumentNotNull(signature, nameof(signature));

            return VerifyData(bytes, 0, bytes.Length, signature, hashAlgorithm);
        }

        public bool VerifyData(byte[] bytes, int index, int count, byte[] signature, HashAlgName hashAlgorithm)
        {
            ThrowIfObjectDisposed();

            if (bytes == null)
            { throw new ArgumentNullException(nameof(bytes)); }

            if (signature == null)
            { throw new ArgumentNullException(nameof(signature)); }

            if (index < 0)
            { throw new ArgumentOutOfRangeException(nameof(index), $"Argument '{nameof(index)}' value must be >= 0."); }

            if (index >= bytes.Length)
            { throw new ArgumentOutOfRangeException(nameof(index), $"Argument '{nameof(index)}' value exceeds the maximum length of argument '{nameof(bytes)}'."); }

            if (count < 0)
            { throw new ArgumentOutOfRangeException(nameof(count), $"Argument '{nameof(count)}' value must be >= 0."); }

            if (index + count > bytes.Length)
            { throw new ArgumentOutOfRangeException(nameof(count), $"Argument '{nameof(index)} + {nameof(count)}' value exceeds the maximum length of argument '{nameof(bytes)}'."); }


            switch (this.AlgorithmName)
            {
                case AsymmetricAlgName.RSA:
                    return (this.provider as System.Security.Cryptography.RSA).VerifyData(bytes, signature, ToSysHashAlgorithmName(hashAlgorithm), System.Security.Cryptography.RSASignaturePadding.Pkcs1);

                case AsymmetricAlgName.DSA:
                    return (this.provider as System.Security.Cryptography.DSA).VerifyData(bytes, signature, ToSysHashAlgorithmName(hashAlgorithm));

                case AsymmetricAlgName.ECDsa:
                    return (this.provider as System.Security.Cryptography.ECDsa).VerifyData(bytes, signature, ToSysHashAlgorithmName(hashAlgorithm));

                default:
                    throw new NotSupportedException($"Unsupported asymmetric algorithm '{this.AlgorithmName}'.");
            }
        }

        public bool VerifyData(Stream stream, byte[] signature, HashAlgName hashAlgorithm)
        {
            ThrowIfObjectDisposed();

            AssertUtil.ArgumentNotNull(stream, nameof(stream));
            AssertUtil.ArgumentNotNull(signature, nameof(signature));

            if (!stream.CanRead)
            { throw new ArgumentException($"Argument '{nameof(stream)}' cannot be read."); }


            switch (this.AlgorithmName)
            {
                case AsymmetricAlgName.RSA:
                    return (this.provider as System.Security.Cryptography.RSA).VerifyData(stream, signature, ToSysHashAlgorithmName(hashAlgorithm), System.Security.Cryptography.RSASignaturePadding.Pkcs1);

                case AsymmetricAlgName.DSA:
                    return (this.provider as System.Security.Cryptography.DSA).VerifyData(stream, signature, ToSysHashAlgorithmName(hashAlgorithm));

                case AsymmetricAlgName.ECDsa:
                    return (this.provider as System.Security.Cryptography.ECDsa).VerifyData(stream, signature, ToSysHashAlgorithmName(hashAlgorithm));

                default:
                    throw new NotSupportedException($"Unsupported asymmetric algorithm '{this.AlgorithmName}'.");
            }
        }


        #region Only supported by RSA.

        public byte[] Encrypt(byte[] bytes, EncryptionPaddingMode padding)
        {
            AssertUtil.ArgumentNotNull(bytes, nameof(bytes));

            return Encrypt(bytes, 0, bytes.Length, padding);
        }

        public byte[] Encrypt(byte[] bytes, int index, int count, EncryptionPaddingMode padding)
        {
            ThrowIfObjectDisposed();

            if (this.AlgorithmName != AsymmetricAlgName.RSA)
            { throw new InvalidOperationException($"Only supported by RSA."); }

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


            if (count == bytes.Length)
            { return (this.provider as System.Security.Cryptography.RSA).Encrypt(bytes, ToSysEncryptionPaddingMode(padding)); }
            else
            {
                var parts = new byte[count];
                Array.Copy(bytes, index, parts, 0, count);

                return (this.provider as System.Security.Cryptography.RSA).Encrypt(parts, ToSysEncryptionPaddingMode(padding));
            }
        }

        public byte[] Decrypt(byte[] bytes, EncryptionPaddingMode padding)
        {
            AssertUtil.ArgumentNotNull(bytes, nameof(bytes));

            return Decrypt(bytes, 0, bytes.Length, padding);
        }

        public byte[] Decrypt(byte[] bytes, int index, int count, EncryptionPaddingMode padding)
        {
            ThrowIfObjectDisposed();

            if (this.AlgorithmName != AsymmetricAlgName.RSA)
            { throw new InvalidOperationException($"Only supported by RSA."); }

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


            if (count == bytes.Length)
            { return (this.provider as System.Security.Cryptography.RSA).Decrypt(bytes, ToSysEncryptionPaddingMode(padding)); }
            else
            {
                var parts = new byte[count];
                Array.Copy(bytes, index, parts, 0, count);

                return (this.provider as System.Security.Cryptography.RSA).Decrypt(parts, ToSysEncryptionPaddingMode(padding));
            }
        }


        public byte[] SignData(byte[] bytes, HashAlgName hashAlgorithm, SignaturePaddingMode padding)
        {
            AssertUtil.ArgumentNotNull(bytes, nameof(bytes));

            return SignData(bytes, 0, bytes.Length, hashAlgorithm, padding);
        }

        public byte[] SignData(byte[] bytes, int index, int count, HashAlgName hashAlgorithm, SignaturePaddingMode padding)
        {
            ThrowIfObjectDisposed();

            if (this.AlgorithmName != AsymmetricAlgName.RSA)
            { throw new InvalidOperationException($"Only supported by RSA."); }

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


            return (this.provider as System.Security.Cryptography.RSA).SignData(bytes, index, count, ToSysHashAlgorithmName(hashAlgorithm), ToSysSignaturePaddingMode(padding));
        }

        public byte[] SignData(Stream stream, HashAlgName hashAlgorithm, SignaturePaddingMode padding)
        {
            ThrowIfObjectDisposed();

            if (this.AlgorithmName != AsymmetricAlgName.RSA)
            { throw new InvalidOperationException($"Only supported by RSA."); }

            AssertUtil.ArgumentNotNull(stream, nameof(stream));

            if (!stream.CanRead)
            { throw new ArgumentException($"Argument '{nameof(stream)}' cannot be read."); }


            return (this.provider as System.Security.Cryptography.RSA).SignData(stream, ToSysHashAlgorithmName(hashAlgorithm), ToSysSignaturePaddingMode(padding));
        }


        public bool VerifyData(byte[] bytes, byte[] signature, HashAlgName hashAlgorithm, SignaturePaddingMode padding)
        {
            AssertUtil.ArgumentNotNull(bytes, nameof(bytes));

            return VerifyData(bytes, 0, bytes.Length, signature, hashAlgorithm, padding);
        }

        public bool VerifyData(byte[] bytes, int index, int count, byte[] signature, HashAlgName hashAlgorithm, SignaturePaddingMode padding)
        {
            ThrowIfObjectDisposed();

            if (this.AlgorithmName != AsymmetricAlgName.RSA)
            { throw new InvalidOperationException($"Only supported by RSA."); }

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


            return (this.provider as System.Security.Cryptography.RSA).VerifyData(bytes, index, count, signature, ToSysHashAlgorithmName(hashAlgorithm), ToSysSignaturePaddingMode(padding));
        }

        public bool VerifyData(Stream stream, byte[] signature, HashAlgName hashAlgorithm, SignaturePaddingMode padding)
        {
            ThrowIfObjectDisposed();

            if (this.AlgorithmName != AsymmetricAlgName.RSA)
            { throw new InvalidOperationException($"Only supported by RSA."); }

            AssertUtil.ArgumentNotNull(stream, nameof(stream));

            if (!stream.CanRead)
            { throw new ArgumentException($"Argument '{nameof(stream)}' cannot be read."); }

            return (this.provider as System.Security.Cryptography.RSA).VerifyData(stream, signature, ToSysHashAlgorithmName(hashAlgorithm), ToSysSignaturePaddingMode(padding));
        }

        #endregion
    }
}