
using System.IO;

namespace ECode.Cryptography
{
    public enum CipherMode
    {
        CBC = 1,

        ECB = 2,

        OFB = 3,

        CFB = 4,

        CTS = 5
    }

    public enum PaddingMode
    {
        None = 1,

        PKCS7 = 2,

        Zeros = 3,

        ANSIX923 = 4,

        ISO10126 = 5
    }

    public interface ISymmetricCrypto
    {
        /// <summary>
        /// The algorithm name of crypto.
        /// </summary>
        SymmetricAlgName AlgorithmName { get; }


        /// <summary>
        /// Gets or sets the secret key for crypto.
        /// </summary>
        byte[] Key { get; set; }

        /// <summary>
        /// Gets or sets the initialization vector for crypto.
        /// </summary>
        byte[] IV { get; set; }

        /// <summary>
        /// Gets or sets the size, in bits, of the secret key used by crypto.
        /// </summary>
        int KeySize { get; set; }

        /// <summary>
        /// Gets or sets the block size, in bits, for operation.
        /// </summary>
        int BlockSize { get; set; }

        /// <summary>
        /// Gets or sets the mode for operation.
        /// </summary>
        CipherMode Mode { get; set; }

        /// <summary>
        /// Gets or sets the padding mode for operation.
        /// </summary>
        PaddingMode Padding { get; set; }


        /// <summary>
        /// Encrypt the specified data.
        /// </summary>
        /// <param name="bytes">The data to encrypt.</param>
        /// <returns>The encrypted data.</returns>
        byte[] Encrypt(byte[] bytes);

        /// <summary>
        /// Encrypt the specified region of the specified byte array.
        /// </summary>
        /// <param name="bytes">The data to encrypt.</param>
        /// <param name="index">The index of the first byte to encrypt.</param>
        /// <param name="count">The number of bytes to encrypt.</param>
        /// <returns>The encrypted data.</returns>
        byte[] Encrypt(byte[] bytes, int index, int count);

        /// <summary>
        /// Encrypt the specified stream.
        /// </summary>
        /// <param name="inStream">The stream to encrypt.</param>
        /// <param name="outStream">The stream for output.</param>
        void Encrypt(Stream inStream, Stream outStream);


        /// <summary>
        /// Decrypt the specified data.
        /// </summary>
        /// <param name="bytes">The data to decrypt.</param>
        /// <returns>The decrypted data.</returns>
        byte[] Decrypt(byte[] bytes);

        /// <summary>
        /// Decrypt the specified region of the specified byte array.
        /// </summary>
        /// <param name="bytes">The data to decrypt.</param>
        /// <param name="index">The index of the first byte to decrypt.</param>
        /// <param name="count">The number of bytes to decrypt.</param>
        /// <returns>The decrypted data.</returns>
        byte[] Decrypt(byte[] bytes, int index, int count);

        /// <summary>
        /// Decrypt the specified stream.
        /// </summary>
        /// <param name="inStream">The stream to decrypt.</param>
        /// <param name="outStream">The stream for output.</param>
        void Decrypt(Stream inStream, Stream outStream);
    }
}