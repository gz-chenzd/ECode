using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ECode.Utility;

namespace ECode.IO
{
    /// <summary>
    /// This class is wrapper to normal stream, provides most needed stream methods which are missing from normal stream.
    /// </summary>
    public sealed class SmartStream : Stream
    {
        private Stream          m_pStream               = null;
        private bool            m_IsOwner               = false;
        private bool            m_CRLFLines             = true;
        private Encoding        m_pEncoding             = Encoding.UTF8;
        private DateTime        m_LastActivity          = DateTime.Now;
        private long            m_BytesReaded           = 0;
        private long            m_BytesWritten          = 0;
        private byte[]          m_pReadBuffer           = null;
        private int             m_BufferSize            = 1024;  // 1k
        private int             m_BytesInBuffer         = 0;
        private int             m_OffsetInBuffer        = 0;
        private byte[]          m_pLineBuffer           = null;
        private int             m_LineBufferSize        = 1024;  // 1k


        #region Properties Implementation

        /// <summary>
        /// Gets if this object is disposed.
        /// </summary>
        public bool IsDisposed
        { get; private set; }

        /// <summary>
        /// Gets or set SmartStream is owner of source stream. This property affects like closing this stream will close SourceStream if IsOwner true.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public bool IsOwner
        {
            get
            {
                ThrowIfObjectDisposed();

                return m_IsOwner;
            }

            set
            {
                ThrowIfObjectDisposed();

                m_IsOwner = value;
            }
        }

        /// <summary>
        /// Gets this stream underlying stream.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public Stream SourceStream
        {
            get
            {
                ThrowIfObjectDisposed();

                return m_pStream;
            }
        }

        /// <summary>
        /// Gets or sets if only CRLF lines accepted. If false LF lines accepted. 
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public bool CRLFLines
        {
            get
            {
                ThrowIfObjectDisposed();

                return m_CRLFLines;
            }

            set
            {
                ThrowIfObjectDisposed();

                m_CRLFLines = value;
            }
        }

        /// <summary>
        /// Gets or sets string related methods default encoding.
        /// </summary>
        /// <exception cref="ArgumentNullException">Is raised when null value is passed.</exception>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public Encoding Encoding
        {
            get
            {
                ThrowIfObjectDisposed();

                return m_pEncoding;
            }

            set
            {
                ThrowIfObjectDisposed();

                if (value == null)
                { throw new ArgumentNullException(nameof(Encoding)); }

                m_pEncoding = value;
            }
        }

        /// <summary>
        /// Gets the last time when data was read or written.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public DateTime LastActivity
        {
            get
            {
                ThrowIfObjectDisposed();

                return m_LastActivity;
            }
        }

        /// <summary>
        /// Gets how many bytes are readed through this stream.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public long BytesReaded
        {
            get
            {
                ThrowIfObjectDisposed();

                return m_BytesReaded;
            }
        }

        /// <summary>
        /// Gets how many bytes are written through this stream.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public long BytesWritten
        {
            get
            {
                ThrowIfObjectDisposed();

                return m_BytesWritten;
            }
        }

        /// <summary>
        /// Gets number of bytes in read buffer.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public int BytesInReadBuffer
        {
            get
            {
                ThrowIfObjectDisposed();

                return m_BytesInBuffer - m_OffsetInBuffer;
            }
        }


        public override bool CanRead
        {
            get
            {
                ThrowIfObjectDisposed();

                return m_pStream.CanRead;
            }
        }

        public override bool CanWrite
        {
            get
            {
                ThrowIfObjectDisposed();

                return m_pStream.CanWrite;
            }
        }

        public override bool CanSeek
        {
            get
            {
                ThrowIfObjectDisposed();

                return m_pStream.CanSeek;
            }
        }

        public override long Length
        {
            get
            {
                ThrowIfObjectDisposed();

                return m_pStream.Length;
            }
        }

        public override long Position
        {
            get
            {
                ThrowIfObjectDisposed();

                return m_pStream.Position;
            }

            set
            {
                ThrowIfObjectDisposed();

                ResetReadBufferState();
                m_pStream.Position = value;
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
        /// <param name="stream">Stream to wrap.</param>
        /// <param name="owner">Specifies if SmartStream is owner of <b>stream</b>.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>stream</b> is null.</exception>
        public SmartStream(Stream stream, bool owner = true)
        {
            AssertUtil.ArgumentNotNull(stream, nameof(stream));

            m_pStream = stream;
            m_IsOwner = owner;

            m_pReadBuffer = new byte[m_BufferSize];
        }


        protected override void Dispose(bool disposing)
        {
            if (this.IsDisposed)
            { return; }

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


        private void BufferRead()
        {
            m_BytesInBuffer = 0;
            m_OffsetInBuffer = 0;

            m_BytesInBuffer = m_pStream.Read(m_pReadBuffer, 0, m_pReadBuffer.Length);
            m_BytesReaded += m_BytesInBuffer;

            m_LastActivity = DateTime.Now;
        }

        private void ResetReadBufferState()
        {
            m_BytesInBuffer = 0;
            m_OffsetInBuffer = 0;
        }


        /// <summary>
        /// Returns the next available character but does not consume it.
        /// </summary>
        /// <returns>An integer representing the next character to be read, or -1 if no more characters are available.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        public int Peek()
        {
            ThrowIfObjectDisposed();

            if (this.BytesInReadBuffer == 0)
            { BufferRead(); }

            // We are end of stream.
            if (this.BytesInReadBuffer == 0)
            { return -1; }
            else
            { return m_pReadBuffer[m_OffsetInBuffer]; }
        }


        /// <summary>
        /// Reads a single line string.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        public string ReadLine()
        {
            return ReadLine(SizeExceededAction.ThrowException);
        }

        /// <summary>
        /// Reads a single line string.
        /// </summary>
        /// <param name="exceededAction">Specifies how line-reader behaves when maximum line size exceeded.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        public string ReadLine(SizeExceededAction exceededAction)
        {
            ThrowIfObjectDisposed();

            if (m_pLineBuffer == null)
            { m_pLineBuffer = new byte[m_LineBufferSize]; }

            var lineReader = new LineReader(this, m_pLineBuffer);
            lineReader.CRLFLines = m_CRLFLines;

            lineReader.Read(exceededAction);

            return lineReader.ToLineString(m_pEncoding);
        }


        /// <summary>
        /// Writes specified string data to stream.
        /// </summary>
        /// <param name="text">Data to write.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>data</b> is null.</exception>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <returns>Returns number of raw bytes written.</returns>
        public int Write(string text)
        {
            ThrowIfObjectDisposed();

            AssertUtil.ArgumentNotNull(text, nameof(text));

            var bytes = m_pEncoding.GetBytes(text);
            Write(bytes, 0, bytes.Length);

            return bytes.Length;
        }

        /// <summary>
        /// Writes specified line to stream. If CRLF is missing, it will be added automatically to line data.
        /// </summary>
        /// <param name="line">Line to send.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>line</b> is null.</exception>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <returns>Returns number of raw bytes written.</returns>
        public int WriteLine(string line)
        {
            ThrowIfObjectDisposed();

            AssertUtil.ArgumentNotNull(line, nameof(line));

            if (m_CRLFLines && !line.EndsWith("\r\n", StringComparison.InvariantCultureIgnoreCase))
            { line += "\r\n"; }
            else if (!m_CRLFLines && !line.EndsWith("\n", StringComparison.InvariantCultureIgnoreCase))
            { line += "\n"; }

            var bytes = m_pEncoding.GetBytes(line);
            Write(bytes, 0, bytes.Length);

            return bytes.Length;
        }

        /// <summary>
        /// Writes all source <b>stream</b> data to stream.
        /// </summary>
        /// <param name="stream">Stream which data to write.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>stream</b> is null.</exception>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <returns>Returns number of raw bytes written.</returns>
        public long WriteStream(Stream stream)
        {
            ThrowIfObjectDisposed();

            if (stream == null)
            { throw new ArgumentNullException(nameof(stream)); }

            if (!stream.CanRead)
            { throw new ArgumentException($"Argument '{nameof(stream)}' cannot be read.", nameof(stream)); }

            long writtenCount = 0;
            var buffer = new byte[m_BufferSize];
            while (true)
            {
                int readed = stream.Read(buffer, 0, buffer.Length);

                if (readed == 0)
                { break; }

                writtenCount += readed;
                Write(buffer, 0, readed);
            }

            return writtenCount;
        }

        /// <summary>
        /// Writes specified number of bytes from source <b>stream</b> to stream.
        /// </summary>
        /// <param name="stream">Stream which data to write.</param>
        /// <param name="count">Number of bytes to write.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>stream</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when <b>count</b> argument has invalid value.</exception>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <returns>Returns number of raw bytes written.</returns>
        public long WriteStream(Stream stream, long count)
        {
            ThrowIfObjectDisposed();

            if (stream == null)
            { throw new ArgumentNullException(nameof(stream)); }

            if (!stream.CanRead)
            { throw new ArgumentException($"Argument '{nameof(stream)}' cannot be read.", nameof(stream)); }

            if (count <= 0)
            { throw new ArgumentException($"Argument '{nameof(count)}' value must be > 0.", nameof(count)); }

            long readedCount = 0;
            var buffer = new byte[m_BufferSize];
            while (readedCount < count)
            {
                int readed = stream.Read(buffer, 0, (int)Math.Min(buffer.Length, count - readedCount));

                if (readed == 0)
                { break; }

                readedCount += readed;
                Write(buffer, 0, readed);
            }

            return readedCount;
        }


        #region Override Methods

        public override long Seek(long offset, SeekOrigin origin)
        {
            ThrowIfObjectDisposed();

            ResetReadBufferState();
            return m_pStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            ThrowIfObjectDisposed();

            ResetReadBufferState();
            m_pStream.SetLength(value);
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

            if (this.BytesInReadBuffer == 0)
            { BufferRead(); }

            if (this.BytesInReadBuffer == 0)
            { return -1; }
            else
            {
                return m_pReadBuffer[m_OffsetInBuffer++];
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            ThrowIfObjectDisposed();

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


            if (this.BytesInReadBuffer == 0)
            { BufferRead(); }

            if (this.BytesInReadBuffer == 0)
            { return 0; }
            else
            {
                int countToCopy = Math.Min(count, this.BytesInReadBuffer);
                Array.Copy(m_pReadBuffer, m_OffsetInBuffer, buffer, offset, countToCopy);
                m_OffsetInBuffer += countToCopy;

                return countToCopy;
            }
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            ThrowIfObjectDisposed();

            return base.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override void WriteByte(byte value)
        {
            ThrowIfObjectDisposed();

            m_pStream.WriteByte(value);
            m_BytesWritten++;

            m_LastActivity = DateTime.Now;
        }

        public override void Write(byte[] bytes, int index, int count)
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


            m_pStream.Write(bytes, index, count);
            m_BytesWritten += count;

            m_LastActivity = DateTime.Now;
        }

        public override Task WriteAsync(byte[] bytes, int index, int count, CancellationToken cancellationToken)
        {
            ThrowIfObjectDisposed();

            return base.WriteAsync(bytes, index, count, cancellationToken);
        }

        public override void CopyTo(Stream destination, int bufferSize)
        {
            throw new NotSupportedException();
        }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}