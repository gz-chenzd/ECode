using System.IO;

namespace ECode.Checksums
{
    public interface IChecksum
    {
        /// <summary>
        /// Returns the data checksum computed so far.
        /// </summary>
        ulong Value { get; }


        /// <summary>
        /// Resets the data checksum as if no update was ever called.
        /// </summary>
        void Reset();


        /// <summary>
        /// Adds one byte to the data checksum.
        /// </summary>
        void Update(byte b);

        /// <summary>
        /// Adds all the bytes to the data checksum.
        /// </summary>
        void Update(byte[] bytes);

        /// <summary>
        /// Adds a sequence of bytes to the data checksum.
        /// </summary>
        /// <param name = "bytes">
        /// The byte array containing the sequence of bytes to compute.
        /// </param>
        /// <param name = "index">
        /// The index of the first byte to compute.
        /// </param>
        /// <param name = "count">
        /// The number of bytes to compute.
        /// </param>
        void Update(byte[] bytes, int index, int count);

        /// <summary>
        /// Adds stream to the data checksum.
        /// </summary>
        void Update(Stream stream);
    }
}
