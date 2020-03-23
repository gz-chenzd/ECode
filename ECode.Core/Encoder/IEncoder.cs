using System.IO;

namespace ECode.Encoder
{
    public interface IEncoder
    {
        /// <summary>
        /// Encodes bytes.
        /// </summary>
        /// <param name="bytes">Data to be encoded.</param>
        /// <returns>Returns encoded string.</returns>
        string Encode(byte[] bytes);

        /// <summary>
        /// Encodes bytes.
        /// </summary>
        /// <param name="bytes">Data to be encoded.</param>
        /// <param name="index">The index of the first byte to encode.</param>
        /// <param name="count">The number of bytes to encode.</param>
        /// <returns>Returns encoded string.</returns>
        string Encode(byte[] bytes, int index, int count);

        /// <summary>
        /// Encodes bytes.
        /// </summary>
        /// <param name="bytes">Data to be encoded.</param>
        /// <param name="index">The index of the first byte to encode.</param>
        /// <param name="count">The number of bytes to encode.</param>
        /// <param name="insertLineBreaks">If true, inserts line break after every 76 characters.</param>
        /// <returns>Returns encoded string.</returns>
        string Encode(byte[] bytes, int index, int count, bool insertLineBreaks);


        /// <summary>
        /// Encodes stream.
        /// </summary>
        /// <param name="inStream">The stream to encode.</param>
        /// <param name="outStream">The stream for output.</param>
        void Encode(Stream inStream, Stream outStream);

        /// <summary>
        /// Encodes stream.
        /// </summary>
        /// <param name="inStream">The stream to encode.</param>
        /// <param name="outStream">The stream for output.</param>
        /// <param name="insertLineBreaks">If true, inserts line break after every 76 characters.</param>
        void Encode(Stream inStream, Stream outStream, bool insertLineBreaks);


        /// <summary>
        /// Decodes bytes.
        /// </summary>
        /// <param name="bytes">Data to be decoded.</param>
        /// <returns>Returns decoded data.</returns>
        byte[] Decode(byte[] bytes);

        /// <summary>
        /// Decodes bytes.
        /// </summary>
        /// <param name="bytes">Data to be decoded.</param>
        /// <param name="index">The index of the first byte to decode.</param>
        /// <param name="count">The number of bytes to decode.</param>
        /// <returns>The decode.</returns>
        byte[] Decode(byte[] bytes, int index, int count);

        /// <summary>
        /// Decodes bytes.
        /// </summary>
        /// <param name="bytes">Data to be decoded.</param>
        /// <param name="index">The index of the first byte to decode.</param>
        /// <param name="count">The number of bytes to decode.</param>
        /// <param name="ignoreInvalidChars">If true all invalid chars ignored. If false, <see cref="System.FormatException"/> is raised.</param>
        /// <returns>The decode.</returns>
        byte[] Decode(byte[] bytes, int index, int count, bool ignoreInvalidChars);


        /// <summary>
        /// Decodes stream.
        /// </summary>
        /// <param name="inStream">The stream to decode.</param>
        /// <param name="outStream">The stream for output.</param>
        void Decode(Stream inStream, Stream outStream);

        /// <summary>
        /// Decodes stream.
        /// </summary>
        /// <param name="inStream">The stream to decode.</param>
        /// <param name="outStream">The stream for output.</param>
        /// <param name="ignoreInvalidChars">If true all invalid chars ignored. If false, <see cref="System.FormatException"/> is raised.</param>
        void Decode(Stream inStream, Stream outStream, bool ignoreInvalidChars);
    }
}
