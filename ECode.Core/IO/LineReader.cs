using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ECode.Core;

namespace ECode.IO
{
    public sealed class LineReader
    {
        private Stream      m_pStream   = null;


        /// <summary>
        /// Gets bytes buffer.
        /// </summary>
        public byte[] Buffer
        { get; private set; }

        /// <summary>
        /// Gets bytes buffer length.
        /// </summary>
        public int BufferSize
        {
            get { return this.Buffer.Length; }
        }

        /// <summary>
        /// Gets number of bytes stored in the buffer. Ending line-feed characters included.
        /// </summary>
        public int BytesInBuffer
        { get; private set; }

        /// <summary>
        /// Gets number of line data bytes stored in the buffer. Ending line-feed characters not included.
        /// </summary>
        public int LineBytesInBuffer
        {
            get
            {
                int retVal = this.BytesInBuffer;

                if (this.BytesInBuffer > 1)
                {
                    if (this.Buffer[this.BytesInBuffer - 1] == '\n')
                    {
                        retVal--;

                        if (this.CRLFLines)
                        {
                            if (this.Buffer[this.BytesInBuffer - 2] == '\r')
                            { retVal--; }
                            else
                            { retVal++; }
                        }
                    }
                }
                else if (this.BytesInBuffer > 0)
                {
                    if (!this.CRLFLines)
                    {
                        if (this.Buffer[this.BytesInBuffer - 1] == '\n')
                        { retVal--; }
                    }
                }

                return retVal;
            }
        }


        /// <summary>
        /// Gets or sets if CRLF used.
        /// </summary>
        public bool CRLFLines
        { get; set; } = true;

        /// <summary>
        /// Gets how many lines are readed.
        /// </summary>
        public long LinesReaded
        { get; private set; }

        /// <summary>
        /// Gets how many bytes are readed.
        /// </summary>
        public long BytesReaded
        { get; private set; }


        /// <summary>
        /// Gets ASCII encoded string. Returns null if EOS(end of stream) reached. Ending line-feed characters not included.
        /// </summary>
        public string LineAscii
        {
            get
            {
                if (this.BytesInBuffer == 0)
                { return null; }

                return Encoding.ASCII.GetString(this.Buffer, 0, this.LineBytesInBuffer);
            }
        }

        /// <summary>
        /// Gets UTF8 encoded string. Returns null if EOS(end of stream) reached. Ending line-feed characters not included.
        /// </summary>
        public string LineUtf8
        {
            get
            {
                if (this.BytesInBuffer == 0)
                { return null; }

                return Encoding.UTF8.GetString(this.Buffer, 0, this.LineBytesInBuffer);
            }
        }

        /// <summary>
        /// Converts to specified encoding string. Returns null if EOS(end of stream) reached. Ending line-feed characters not included.
        /// </summary>
        public string ToLineString(Encoding encoding)
        {
            if (encoding == null)
            { throw new ArgumentNullException(nameof(encoding)); }

            if (this.BytesInBuffer == 0)
            { return null; }

            return encoding.GetString(this.Buffer, 0, this.LineBytesInBuffer);
        }


        public LineReader(Stream stream, int bufferSize = 1024)
        {
            if (stream == null)
            { throw new ArgumentNullException(nameof(stream)); }

            if (!stream.CanRead)
            { throw new ArgumentException($"Argument '{nameof(stream)}' cannot be read.", nameof(stream)); }

            if (bufferSize <= 0)
            { throw new ArgumentOutOfRangeException(nameof(bufferSize), $"Argument '{nameof(bufferSize)}' value must be > 0."); }

            m_pStream = stream;
            this.Buffer = new byte[bufferSize];
        }

        public LineReader(Stream stream, byte[] buffer)
        {
            if (stream == null)
            { throw new ArgumentNullException(nameof(stream)); }

            if (!stream.CanRead)
            { throw new ArgumentException($"Argument '{nameof(stream)}' cannot be read.", nameof(stream)); }

            if (buffer == null || buffer.Length <= 0)
            { throw new ArgumentNullException(nameof(buffer)); }

            m_pStream = stream;
            this.Buffer = buffer;
        }


        /// <summary>
        /// Reads a single line string.
        /// </summary>
        /// <returns>
        /// The total number of bytes read into the buffer. 
        /// This can be less than the number of bytes requested if that many bytes are not currently available, 
        /// or zero (0) if the end of the stream has been reached.
        /// </returns>
        public int Read()
        {
            return Read(SizeExceededAction.ThrowException);
        }

        /// <summary>
        /// Reads a single line string.
        /// </summary>
        /// <param name="exceededAction">Specifies how line-reader behaves when maximum line size exceeded.</param>
        /// <returns>
        /// The total number of bytes read into the buffer. 
        /// This can be less than the number of bytes requested if that many bytes are not currently available, 
        /// or zero (0) if the end of the stream has been reached.
        /// </returns>
        public int Read(SizeExceededAction exceededAction)
        {
            int lastByte          = -1;
            int bytesReaded       = 0;

            lock (this)
            {
                while (bytesReaded < this.Buffer.Length)
                {
                    int b = m_pStream.ReadByte();
                    if (b == -1)
                    { break; }

                    // Line buffer full.
                    if (bytesReaded >= this.Buffer.Length)
                    {
                        if (exceededAction == SizeExceededAction.ThrowException)
                        { throw new LineSizeExceededException(); }
                    }
                    // Store byte.
                    else
                    { this.Buffer[bytesReaded++] = (byte)b; }

                    // We have LF line.
                    if (b == '\n')
                    {
                        if (!this.CRLFLines || (this.CRLFLines && lastByte == '\r'))
                        { break; }
                    }

                    lastByte = b;
                }
            }

            if (bytesReaded > 0)
            {
                LinesReaded++;
                BytesReaded += bytesReaded;
            }

            BytesInBuffer = bytesReaded;
            return bytesReaded;
        }

        /// <summary>
        /// Reads a single line string.
        /// </summary>
        /// <returns>
        /// The total number of bytes read into the buffer. 
        /// This can be less than the number of bytes requested if that many bytes are not currently available, 
        /// or zero (0) if the end of the stream has been reached.
        /// </returns>
        public Task<int> ReadAsync()
        {
            return ReadAsync(SizeExceededAction.ThrowException);
        }

        /// <summary>
        /// Reads a single line string.
        /// </summary>
        /// <param name="exceededAction">Specifies how line-reader behaves when maximum line size exceeded.</param>
        /// <returns>
        /// The total number of bytes read into the buffer. 
        /// This can be less than the number of bytes requested if that many bytes are not currently available, 
        /// or zero (0) if the end of the stream has been reached.
        /// </returns>
        public Task<int> ReadAsync(SizeExceededAction exceededAction)
        {
            return Task.Factory.StartNew(() =>
            {
                return Read(exceededAction);
            });
        }
    }
}
