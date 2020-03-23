using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ECode.Encoder;
using ECode.Utility;

namespace ECode.IO
{
    public sealed class Base64Stream : Stream, IDisposable
    {
        private Stream              m_pStream                   = null;
        private bool                m_IsOwner                   = false;
        private bool                m_IsFinished                = false;
        private FileAccess          m_AccessMode                = FileAccess.ReadWrite;

        private bool                m_AddLineBreaks             = true;
        private bool                m_IgnoreInvalidPadding      = false;
        private byte[]              m_pEncodeBlock              = new byte[3];
        private int                 m_OffsetInEncodeBlock       = 0;
        private byte[]              m_pDecodeBlock              = new byte[4];
        private int                 m_OffsetInDecodeBlock       = 0;
        private int                 m_DecodePaddingCount        = 0;
        private byte[]              m_pEncodedBuffer            = new byte[78];
        private int                 m_OffsetInEncodedBuffer     = 0;
        private byte[]              m_pDecodedBuffer            = null;
        private int                 m_DecodedInBuffer           = 0;
        private int                 m_OffsetInDecodedBuffer     = 0;
        private byte[]              m_pReadBuffer               = null;
        private int                 m_ReadBufferSize            = 1024;  // 1k


        #region Properties Implementation

        /// <summary>
        /// Gets if this object is disposed.
        /// </summary>
        public bool IsDisposed
        { get; private set; }

        /// <summary>
        /// Gets or sets if invalid(missing '=' chars) base64 last block padding allowed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public bool IgnoreInvalidPadding
        {
            get
            {
                ThrowIfObjectDisposed();

                return m_IgnoreInvalidPadding;
            }

            set
            {
                ThrowIfObjectDisposed();

                m_IgnoreInvalidPadding = value;
            }
        }


        public override bool CanRead
        {
            get
            {
                ThrowIfObjectDisposed();

                return m_pStream.CanRead && (m_AccessMode & FileAccess.Read) != 0;
            }
        }

        public override bool CanWrite
        {
            get
            {
                ThrowIfObjectDisposed();

                return m_pStream.CanWrite && (m_AccessMode & FileAccess.Write) != 0;
            }
        }

        public override bool CanSeek
        {
            get
            {
                ThrowIfObjectDisposed();

                return false;
            }
        }

        public override long Length
        {
            get
            {
                ThrowIfObjectDisposed();

                throw new NotSupportedException();
            }
        }

        public override long Position
        {
            get
            {
                ThrowIfObjectDisposed();

                throw new NotSupportedException();
            }

            set
            {
                ThrowIfObjectDisposed();

                throw new NotSupportedException();
            }
        }

        public override bool CanTimeout
        {
            get
            {
                ThrowIfObjectDisposed();

                throw new NotSupportedException();
            }
        }

        public override int ReadTimeout
        {
            get
            {
                ThrowIfObjectDisposed();

                throw new NotSupportedException();
            }

            set
            {
                ThrowIfObjectDisposed();

                throw new NotSupportedException();
            }
        }

        public override int WriteTimeout
        {
            get
            {
                ThrowIfObjectDisposed();

                throw new NotSupportedException();
            }

            set
            {
                ThrowIfObjectDisposed();

                throw new NotSupportedException();
            }
        }

        #endregion


        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="stream">Stream which to encode/decode.</param>
        /// <param name="owner">Specifies if Base64Stream is owner of <b>stream</b>.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>stream</b> is null reference.</exception>
        public Base64Stream(Stream stream, bool owner = true)
            : this(stream, owner, true, FileAccess.ReadWrite)
        {

        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="stream">Stream which to encode/decode.</param>
        /// <param name="owner">Specifies if Base64Stream is owner of <b>stream</b>.</param>
        /// <param name="addLineBreaks">Specifies if encoder inserts CRLF after each 76 bytes.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>stream</b> is null reference.</exception>
        public Base64Stream(Stream stream, bool owner, bool addLineBreaks)
            : this(stream, owner, addLineBreaks, FileAccess.ReadWrite)
        {

        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="stream">Stream which to encode/decode.</param>
        /// <param name="owner">Specifies if Base64Stream is owner of <b>stream</b>.</param>
        /// <param name="addLineBreaks">Specifies if encoder inserts CRLF after each 76 bytes.</param>
        /// <param name="access">This stream access mode.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>stream</b> is null reference.</exception>
        public Base64Stream(Stream stream, bool owner, bool addLineBreaks, FileAccess access)
        {
            AssertUtil.ArgumentNotNull(stream, nameof(stream));

            m_pStream = stream;
            m_IsOwner = owner;
            m_AccessMode = access;
            m_AddLineBreaks = addLineBreaks;

            m_pReadBuffer = new byte[m_ReadBufferSize];
            m_pDecodedBuffer = new byte[m_ReadBufferSize];
        }


        protected override void Dispose(bool disposing)
        {
            if (this.IsDisposed)
            { return; }

            try
            { Finish(); }
            catch (Exception ex)
            {
                string dummy = ex.Message;
            }

            this.IsDisposed = true;

            if (m_IsOwner)
            { m_pStream.Dispose(); }

            base.Dispose(disposing);
        }

        private void ThrowIfObjectDisposed()
        {
            if (this.IsDisposed)
            { throw new ObjectDisposedException(this.GetType().Name); }
        }


        /// <summary>
        /// Completes encoding. Call this method if all data has written and no more data. 
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        public void Finish()
        {
            ThrowIfObjectDisposed();

            if (m_IsFinished)
            { return; }

            m_IsFinished = true;

            // PAD left-over, if any. Write encode buffer to underlaying stream.
            if (m_OffsetInEncodeBlock == 1)
            {
                m_pEncodedBuffer[m_OffsetInEncodedBuffer++] = (byte)Base64Encoder.ENCODE_TABLE[m_pEncodeBlock[0] >> 2];
                m_pEncodedBuffer[m_OffsetInEncodedBuffer++] = (byte)Base64Encoder.ENCODE_TABLE[(m_pEncodeBlock[0] & 0x03) << 4];
                m_pEncodedBuffer[m_OffsetInEncodedBuffer++] = (byte)Base64Encoder.ENCODE_TABLE[0x40];
                m_pEncodedBuffer[m_OffsetInEncodedBuffer++] = (byte)Base64Encoder.ENCODE_TABLE[0x40];
            }
            else if (m_OffsetInEncodeBlock == 2)
            {
                m_pEncodedBuffer[m_OffsetInEncodedBuffer++] = (byte)Base64Encoder.ENCODE_TABLE[m_pEncodeBlock[0] >> 2];
                m_pEncodedBuffer[m_OffsetInEncodedBuffer++] = (byte)Base64Encoder.ENCODE_TABLE[(m_pEncodeBlock[0] & 0x03) << 4 | m_pEncodeBlock[1] >> 4];
                m_pEncodedBuffer[m_OffsetInEncodedBuffer++] = (byte)Base64Encoder.ENCODE_TABLE[(m_pEncodeBlock[1] & 0x0F) << 2];
                m_pEncodedBuffer[m_OffsetInEncodedBuffer++] = (byte)Base64Encoder.ENCODE_TABLE[0x40];
            }

            if (m_OffsetInEncodedBuffer > 0)
            {
                m_pStream.Write(m_pEncodedBuffer, 0, m_OffsetInEncodedBuffer);
                m_pStream.Flush();
            }
        }


        private void DecodeBytes()
        {
            // We havn't any decoded data left, decode new data block.
            if ((m_DecodedInBuffer - m_OffsetInDecodedBuffer) == 0)
            {
                m_DecodedInBuffer = 0;
                m_OffsetInDecodedBuffer = 0;

                int bytesReaded = m_pStream.Read(m_pReadBuffer, 0, m_ReadBufferSize - 3);

                int decodedInBuffer = 0;

                // We reached end of stream, no more data.
                if (bytesReaded == 0)
                {
                    if (m_OffsetInDecodeBlock >= 2)
                    {
                        m_pDecodedBuffer[decodedInBuffer++] = (byte)((m_pDecodeBlock[0] << 2) | (m_pDecodeBlock[1] >> 4));

                        if (m_OffsetInDecodeBlock > 2)
                        { m_pDecodedBuffer[decodedInBuffer++] = (byte)(((m_pDecodeBlock[1] & 0xF) << 4) | (m_pDecodeBlock[2] >> 2)); }

                        if (m_OffsetInDecodeBlock > 3)
                        { m_pDecodedBuffer[decodedInBuffer++] = (byte)(((m_pDecodeBlock[2] & 0x3) << 6) | m_pDecodeBlock[3]); }

                        m_DecodePaddingCount = 0;
                        m_OffsetInDecodeBlock = 0;
                    }

                    m_DecodedInBuffer = decodedInBuffer;
                    m_OffsetInDecodedBuffer = 0;

                    return;
                }

                byte b          = 0;
                int decodePos   = 0;
                while (decodePos < bytesReaded)
                {
                    while (m_OffsetInDecodeBlock < 4 && decodePos < bytesReaded)
                    {
                        b = m_pReadBuffer[decodePos++];

                        if (b == ' ' || b == '\r' || b == '\n' || b == '\t')
                        { continue; }

                        if (b == '=')
                        {
                            // Padding may appear only in last two chars of 4-char block.
                            // ab==
                            // abc=
                            if (m_OffsetInDecodeBlock < 2)
                            {
                                m_OffsetInDecodeBlock = 0;
                                continue;
                            }

                            if (m_OffsetInDecodeBlock == 2)
                            {
                                m_DecodePaddingCount++;
                                if (m_DecodePaddingCount == 2)
                                { break; }

                                continue;
                            }
                            else
                            { m_DecodePaddingCount++; }

                            break;
                        }
                        else if (b > 127 || Base64Encoder.DECODE_TABLE[b] == -1)
                        { continue; }
                        else
                        {
                            if (m_DecodePaddingCount > 0)
                            {
                                m_DecodePaddingCount = 0;
                                m_OffsetInDecodeBlock = 0;
                            }

                            m_pDecodeBlock[m_OffsetInDecodeBlock++] = b;
                        }
                    }

                    if (m_OffsetInDecodeBlock + m_DecodePaddingCount == 4)
                    {
                        m_pDecodedBuffer[decodedInBuffer++] = (byte)((m_pDecodeBlock[0] << 2) | (m_pDecodeBlock[1] >> 4));

                        if (m_OffsetInDecodeBlock > 2)
                        { m_pDecodedBuffer[decodedInBuffer++] = (byte)(((m_pDecodeBlock[1] & 0xF) << 4) | (m_pDecodeBlock[2] >> 2)); }

                        if (m_OffsetInDecodeBlock > 3)
                        { m_pDecodedBuffer[decodedInBuffer++] = (byte)(((m_pDecodeBlock[2] & 0x3) << 6) | m_pDecodeBlock[3]); }

                        m_DecodePaddingCount = 0;
                        m_OffsetInDecodeBlock = 0;
                    }
                }

                m_DecodedInBuffer = decodedInBuffer;
                m_OffsetInDecodedBuffer = 0;
            }
        }


        #region Override Methods

        public override long Seek(long offset, SeekOrigin origin)
        {
            ThrowIfObjectDisposed();

            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            ThrowIfObjectDisposed();

            throw new NotSupportedException();
        }

        public override void Flush()
        {
            ThrowIfObjectDisposed();

            m_pStream.Flush();
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            ThrowIfObjectDisposed();

            return m_pStream.FlushAsync(cancellationToken);
        }

        public override int ReadByte()
        {
            ThrowIfObjectDisposed();

            if ((m_AccessMode & FileAccess.Read) == 0)
            { throw new NotSupportedException(); }


            DecodeBytes();

            if (m_DecodedInBuffer <= 0)
            { return -1; }

            return m_pDecodedBuffer[m_OffsetInDecodedBuffer++];
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            ThrowIfObjectDisposed();

            if ((m_AccessMode & FileAccess.Read) == 0)
            { throw new NotSupportedException(); }

            if (buffer == null)
            { throw new ArgumentNullException(nameof(buffer)); }

            if (offset < 0)
            { throw new ArgumentOutOfRangeException(nameof(offset), $"Argument '{nameof(offset)}' value must be >= 0."); }

            if (offset > buffer.Length)
            { throw new ArgumentOutOfRangeException(nameof(offset), $"Argument '{nameof(offset)}' value exceeds the maximum length of argument '{nameof(buffer)}'."); }

            if (count < 0)
            { throw new ArgumentOutOfRangeException(nameof(count), $"Argument '{nameof(count)}' value must be >= 0."); }

            if (offset + count > buffer.Length)
            { throw new ArgumentOutOfRangeException(nameof(count), $"Argument '{nameof(offset)} + {nameof(count)}' value exceeds the maximum length of argument '{nameof(buffer)}'."); }


            DecodeBytes();

            if (m_DecodedInBuffer <= 0)
            { return 0; }

            int available = m_DecodedInBuffer - m_OffsetInDecodedBuffer;
            int countToCopy = Math.Min(count, available);

            Array.Copy(m_pDecodedBuffer, m_OffsetInDecodedBuffer, buffer, offset, countToCopy);
            m_OffsetInDecodedBuffer += countToCopy;

            return countToCopy;
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            ThrowIfObjectDisposed();

            if ((m_AccessMode & FileAccess.Read) == 0)
            { throw new NotSupportedException(); }


            return base.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override void WriteByte(byte value)
        {
            ThrowIfObjectDisposed();

            Write(new byte[] { value }, 0, 1);
        }

        public override void Write(byte[] bytes, int index, int count)
        {
            ThrowIfObjectDisposed();

            if ((m_AccessMode & FileAccess.Write) == 0)
            { throw new NotSupportedException(); }

            if (m_IsFinished)
            { throw new InvalidOperationException("Stream is marked as finished by calling Finish method."); }

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


            int encodeBufSize = m_pEncodedBuffer.Length;

            // Process all bytes.
            for (int i = 0; i < count; i++)
            {
                m_pEncodeBlock[m_OffsetInEncodeBlock++] = bytes[index + i];

                // 3x8-bit encode block is full, encode it.
                if (m_OffsetInEncodeBlock == 3)
                {
                    m_pEncodedBuffer[m_OffsetInEncodedBuffer++] = (byte)Base64Encoder.ENCODE_TABLE[m_pEncodeBlock[0] >> 2];
                    m_pEncodedBuffer[m_OffsetInEncodedBuffer++] = (byte)Base64Encoder.ENCODE_TABLE[(m_pEncodeBlock[0] & 0x03) << 4 | m_pEncodeBlock[1] >> 4];
                    m_pEncodedBuffer[m_OffsetInEncodedBuffer++] = (byte)Base64Encoder.ENCODE_TABLE[(m_pEncodeBlock[1] & 0x0F) << 2 | m_pEncodeBlock[2] >> 6];
                    m_pEncodedBuffer[m_OffsetInEncodedBuffer++] = (byte)Base64Encoder.ENCODE_TABLE[(m_pEncodeBlock[2] & 0x3F)];

                    // Encode buffer is full, write buffer to underlaying stream (we reserved 2 bytes for CRLF).
                    if (m_OffsetInEncodedBuffer >= (encodeBufSize - 2))
                    {
                        if (m_AddLineBreaks)
                        {
                            m_pEncodedBuffer[m_OffsetInEncodedBuffer++] = (byte)'\r';
                            m_pEncodedBuffer[m_OffsetInEncodedBuffer++] = (byte)'\n';
                        }

                        m_pStream.Write(m_pEncodedBuffer, 0, m_OffsetInEncodedBuffer);
                        m_OffsetInEncodedBuffer = 0;
                    }

                    m_OffsetInEncodeBlock = 0;
                }
            }
        }

        public override Task WriteAsync(byte[] bytes, int index, int count, CancellationToken cancellationToken)
        {
            ThrowIfObjectDisposed();

            if ((m_AccessMode & FileAccess.Write) == 0)
            { throw new NotSupportedException(); }

            if (m_IsFinished)
            { throw new InvalidOperationException("Stream is marked as finished by calling Finish method."); }


            return base.WriteAsync(bytes, index, count, cancellationToken);
        }

        public override void CopyTo(Stream destination, int bufferSize)
        {
            ThrowIfObjectDisposed();

            throw new NotSupportedException();
        }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            ThrowIfObjectDisposed();

            throw new NotSupportedException();
        }

        #endregion
    }
}
