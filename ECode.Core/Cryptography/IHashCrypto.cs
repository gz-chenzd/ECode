using System.IO;

namespace ECode.Cryptography
{
    public interface IHashCrypto
    {
        /// <summary>
        /// The algorithm name of hasher.
        /// </summary>
        HashAlgName AlgorithmName { get; }


        /// <summary>
        /// Computes the hash value for the bytes.
        /// </summary>
        /// <param name="bytes">The input to compute the hash code for.</param>
        /// <returns>The computed hash code.</returns>
        byte[] ComputeHash(byte[] bytes);

        /// <summary>
        /// Computes the hash value for the specified region of the bytes.
        /// </summary>
        /// <param name="bytes">The input to compute the hash code for.</param>
        /// <param name="index">The index of the first byte to compute.</param>
        /// <param name="count">The number of bytes to compute.</param>
        /// <returns>The computed hash code.</returns>
        byte[] ComputeHash(byte[] bytes, int index, int count);

        /// <summary>
        /// Computes the hash value for the specified stream.
        /// </summary>
        /// <param name="stream">The input to compute the hash code for.</param>
        /// <returns>The computed hash code.</returns>
        byte[] ComputeHash(Stream stream);
    }
}
