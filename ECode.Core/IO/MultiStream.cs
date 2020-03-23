using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ECode.IO
{
    /// <summary>
    /// This class combines multiple stream into one stream for reading.
    /// The most common usage for that stream is when you need to insert some data to the beginning of some stream.
    /// </summary>
    public sealed class MultiStream : Stream
    {
        private Queue<Stream>       m_pStreams          = null;
        private bool                m_IsOwner           = false;


        #region Properties Implementation

        /// <summary>
        /// Gets if this object is disposed.
        /// </summary>
        public bool IsDisposed
        { get; private set; }


        public override bool CanRead
        {
            get
            {
                ThrowIfObjectDisposed();

                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
                ThrowIfObjectDisposed();

                return false;
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

                long totalLength = 0;
                foreach (var stream in m_pStreams.ToArray())
                {
                    totalLength += stream.Length - stream.Position;
                }

                return totalLength;
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


        public MultiStream(bool owner = true)
        {
            this.m_IsOwner = owner;
            this.m_pStreams = new Queue<Stream>();
        }


        protected override void Dispose(bool disposing)
        {
            if (this.IsDisposed)
            { return; }

            this.IsDisposed = true;

            while (this.m_IsOwner && this.m_pStreams.Count > 0)
            {
                this.m_pStreams.Dequeue().Dispose();
            }

            this.m_pStreams.Clear();
            this.m_pStreams = null;

            base.Dispose(disposing);
        }

        private void ThrowIfObjectDisposed()
        {
            if (IsDisposed)
            { throw new ObjectDisposedException(this.GetType().Name); }
        }


        /// <summary>
        /// Appends stream to read queue.
        /// </summary>
        /// <param name="stream">Stream to add.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and and this method is accessed.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>stream</b> is null.</exception>
        public void AppendStream(Stream stream)
        {
            ThrowIfObjectDisposed();

            if (stream == null)
            { throw new ArgumentNullException(nameof(stream)); }

            if (!stream.CanRead)
            { throw new ArgumentException($"Argument '{nameof(stream)}' cannot be read.", nameof(stream)); }


            m_pStreams.Enqueue(stream);
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

            throw new NotSupportedException();
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            ThrowIfObjectDisposed();

            throw new NotSupportedException();
        }

        public override int ReadByte()
        {
            ThrowIfObjectDisposed();

            while (true)
            {
                // We have readed all streams data, no data left.
                if (m_pStreams.Count == 0)
                { return -1; }
                else
                {
                    int b = m_pStreams.Peek().ReadByte();
                    // We have readed all current stream data.
                    if (b == -1)
                    {
                        // Move to next stream .
                        var stream = m_pStreams.Dequeue();
                        if (m_IsOwner)
                        { stream.Dispose(); }

                        // Next while loop will process "read".
                    }
                    else
                    { return b; }
                }
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


            while (true)
            {
                // We have readed all streams data, no data left.
                if (m_pStreams.Count == 0)
                { return 0; }
                else
                {
                    int readedCount = m_pStreams.Peek().Read(buffer, offset, count);
                    // We have readed all current stream data.
                    if (readedCount == 0)
                    {
                        // Move to next stream .
                        var stream = m_pStreams.Dequeue();
                        if (m_IsOwner)
                        { stream.Dispose(); }

                        // Next while loop will process "read".
                    }
                    else
                    { return readedCount; }
                }
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

            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            ThrowIfObjectDisposed();

            throw new NotSupportedException();
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            ThrowIfObjectDisposed();

            throw new NotSupportedException();
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