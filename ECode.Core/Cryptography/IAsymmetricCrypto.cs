using System.IO;

namespace ECode.Cryptography
{
    public enum EncryptionPaddingMode
    {
        Pkcs1,

        OaepSHA1,

        OaepSHA256,

        OaepSHA384,

        OaepSHA512,
    }

    public enum SignaturePaddingMode
    {
        Pss,

        Pkcs1,
    }

    public interface IAsymmetricCrypto
    {
        /// <summary>
        /// The algorithm name of crypto.
        /// </summary>
        AsymmetricAlgName AlgorithmName { get; }


        /// <summary>
        /// Initializes the crypto from xml type key.
        /// </summary>
        void ImportParameters(string xmlString);

        /// <summary>
        /// Initializes the crypto from RSAParameters type key.
        /// </summary>
        void ImportParameters(System.Security.Cryptography.RSAParameters parameters);

        /// <summary>
        /// Initializes the crypto from DSAParameters type key.
        /// </summary>
        void ImportParameters(System.Security.Cryptography.DSAParameters parameters);

        /// <summary>
        /// Initializes the crypto from ECParameters type key.
        /// </summary>
        void ImportParameters(System.Security.Cryptography.ECParameters parameters);


        /// <summary>
        /// Computes the hash value of the specified byte array using the specified hash
        /// algorithm and signs the resulting hash value.
        /// </summary>
        /// <param name="bytes">The input data for which to compute the hash.</param>
        /// <param name="hashAlgorithm">The hash algorithm to use to create the hash value.</param>
        /// <returns>The signature for the specified data.</returns>
        byte[] SignData(byte[] bytes, HashAlgName hashAlgorithm);

        /// <summary>
        /// Computes the hash value of a portion of the specified byte array using the specified
        /// hash algorithm and signs the resulting hash value.
        /// </summary>
        /// <param name="bytes">The input data for which to compute the hash.</param>
        /// <param name="index">The index of the first byte to sign.</param>
        /// <param name="count">The number of bytes to sign.</param>
        /// <param name="hashAlgorithm">The hash algorithm to use to create the hash value.</param>
        /// <returns>The signature for the specified data.</returns>
        byte[] SignData(byte[] bytes, int index, int count, HashAlgName hashAlgorithm);

        /// <summary>
        /// Computes the hash value of the specified stream using the specified hash algorithm
        /// and signs the resulting hash value.
        /// </summary>
        /// <param name="stream">The input stream for which to compute the hash.</param>
        /// <param name="hashAlgorithm">The hash algorithm to use to create the hash value.</param>
        /// <returns>The signature for the specified data.</returns>
        byte[] SignData(Stream stream, HashAlgName hashAlgorithm);


        /// <summary>
        /// Verifies that a digital signature is valid by calculating the hash value of the
        /// specified data using the specified hash algorithm and comparing it to the provided signature.
        /// </summary>
        /// <param name="bytes">The signed data.</param>
        /// <param name="signature">The signature data to be verified.</param>
        /// <param name="hashAlgorithm">The hash algorithm used to create the hash value of the data.</param>
        /// <returns>true if the digital signature is valid; otherwise, false.</returns>
        bool VerifyData(byte[] bytes, byte[] signature, HashAlgName hashAlgorithm);

        /// <summary>
        ///  Verifies that a digital signature is valid by calculating the hash value of the
        ///  data in a portion of a byte array using the specified hash algorithm and comparing
        ///  it to the provided signature.
        /// </summary>
        /// <param name="bytes">The signed data.</param>
        /// <param name="index">The index of the first byte signed.</param>
        /// <param name="count">The number of bytes signed.</param>
        /// <param name="signature">The signature data to be verified.</param>
        /// <param name="hashAlgorithm">The hash algorithm used to create the hash value of the data.</param>
        /// <returns>true if the digital signature is valid; otherwise, false.</returns>
        bool VerifyData(byte[] bytes, int index, int count, byte[] signature, HashAlgName hashAlgorithm);

        /// <summary>
        /// Verifies that a digital signature is valid by calculating the hash value of the
        /// specified stream using the specified hash algorithm and comparing it to the provided signature.
        /// </summary>
        /// <param name="stream">The signed data.</param>
        /// <param name="signature">The signature data to be verified.</param>
        /// <param name="hashAlgorithm">The hash algorithm used to create the hash value of the data.</param>
        /// <returns>true if the digital signature is valid; otherwise, false.</returns>
        bool VerifyData(Stream stream, byte[] signature, HashAlgName hashAlgorithm);


        #region Only supported by RSA.

        /// <summary>
        /// Encrypts the input data using the specified padding mode.
        /// </summary>
        /// <param name="bytes">The data to encrypt.</param>
        /// <param name="padding">The padding mode.</param>
        /// <returns>The encrypted data.</returns>
        byte[] Encrypt(byte[] bytes, EncryptionPaddingMode padding);

        /// <summary>
        /// Encrypts the specified region of the bytes using the specified padding mode.
        /// </summary>
        /// <param name="bytes">The data to encrypt.</param>
        /// <param name="index">The index of the first byte to encrypt.</param>
        /// <param name="count">The number of bytes to encrypt.</param>
        /// <param name="padding">The padding mode.</param>
        /// <returns>The encrypted data.</returns>
        byte[] Encrypt(byte[] bytes, int index, int count, EncryptionPaddingMode padding);

        /// <summary>
        /// Decrypts the input data using the specified padding mode.
        /// </summary>
        /// <param name="bytes">The data to decrypt.</param>
        /// <param name="padding">The padding mode.</param>
        /// <returns>The decrypted data.</returns>
        byte[] Decrypt(byte[] bytes, EncryptionPaddingMode padding);

        /// <summary>
        /// Decrypt the specified region of the bytes using the specified padding mode.
        /// </summary>
        /// <param name="bytes">The data to decrypt.</param>
        /// <param name="index">The index of the first byte to decrypt.</param>
        /// <param name="count">The number of bytes to decrypt.</param>
        /// <param name="padding">The padding mode.</param>
        /// <returns>The decrypted data.</returns>
        byte[] Decrypt(byte[] bytes, int index, int count, EncryptionPaddingMode padding);


        /// <summary>
        /// Computes the hash value of the specified byte array using the specified hash
        /// algorithm and padding mode, and signs the resulting hash value.
        /// </summary>
        /// <param name="bytes">The input data for which to compute the hash.</param>
        /// <param name="hashAlgorithm">The hash algorithm to use to create the hash value.</param>
        /// <param name="padding">The padding mode.</param>
        /// <returns>The RSA signature for the specified data.</returns>
        byte[] SignData(byte[] bytes, HashAlgName hashAlgorithm, SignaturePaddingMode padding);

        /// <summary>
        /// Computes the hash value of a portion of the specified byte array using the specified
        /// hash algorithm and padding mode, and signs the resulting hash value.
        /// </summary>
        /// <param name="bytes">The input data for which to compute the hash.</param>
        /// <param name="index">The index of the first byte to sign.</param>
        /// <param name="count">The number of bytes to sign.</param>
        /// <param name="hashAlgorithm">The hash algorithm to use to create the hash value.</param>
        /// <param name="padding">The padding mode.</param>
        /// <returns>The RSA signature for the specified data.</returns>
        byte[] SignData(byte[] bytes, int index, int count, HashAlgName hashAlgorithm, SignaturePaddingMode padding);

        /// <summary>
        /// Computes the hash value of the specified stream using the specified hash algorithm
        /// and padding mode, and signs the resulting hash value.
        /// </summary>
        /// <param name="stream">The input stream for which to compute the hash.</param>
        /// <param name="hashAlgorithm">The hash algorithm to use to create the hash value.</param>
        /// <param name="padding">The padding mode.</param>
        /// <returns>The RSA signature for the specified data.</returns>
        byte[] SignData(Stream stream, HashAlgName hashAlgorithm, SignaturePaddingMode padding);


        /// <summary>
        /// Verifies that a digital signature is valid by calculating the hash value of the
        /// specified data using the specified hash algorithm and padding, and comparing it 
        /// to the provided signature.
        /// </summary>
        /// <param name="bytes">The signed data.</param>
        /// <param name="signature">The signature data to be verified.</param>
        /// <param name="hashAlgorithm">The hash algorithm used to create the hash value of the data.</param>
        /// <param name="padding">The padding mode.</param>
        /// <returns>true if the digital signature is valid; otherwise, false.</returns>
        bool VerifyData(byte[] bytes, byte[] signature, HashAlgName hashAlgorithm, SignaturePaddingMode padding);

        /// <summary>
        /// Verifies that a digital signature is valid by calculating the hash value of the
        /// data in a portion of a byte array using the specified hash algorithm and padding, 
        /// and comparing it to the provided signature.
        /// </summary>
        /// <param name="bytes">The signed data.</param>
        /// <param name="index">The index of the first byte signed.</param>
        /// <param name="count">The number of bytes signed.</param>
        /// <param name="signature">The signature data to be verified.</param>
        /// <param name="hashAlgorithm">The hash algorithm used to create the hash value of the data.</param>
        /// <param name="padding">The padding mode.</param>
        /// <returns>true if the digital signature is valid; otherwise, false.</returns>
        bool VerifyData(byte[] bytes, int index, int count, byte[] signature, HashAlgName hashAlgorithm, SignaturePaddingMode padding);

        /// <summary>
        /// Verifies that a digital signature is valid by calculating the hash value of the
        /// specified stream using the specified hash algorithm and padding, and comparing it 
        /// to the provided signature.
        /// </summary>
        /// <param name="stream">The signed data.</param>
        /// <param name="signature">The signature data to be verified.</param>
        /// <param name="hashAlgorithm">The hash algorithm used to create the hash value of the data.</param>
        /// <param name="padding">The padding mode.</param>
        /// <returns>true if the digital signature is valid; otherwise, false.</returns>
        bool VerifyData(Stream stream, byte[] signature, HashAlgName hashAlgorithm, SignaturePaddingMode padding);

        #endregion
    }
}
