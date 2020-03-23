using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ECode.Core;
using ECode.Utility;

namespace ECode.IO
{
    /// <summary>
    /// This class represents auto switching memory/temp-file stream.
    /// </summary>
    public sealed class HybridStream : Stream
    {
        static int  defaultMemorySize   = 64 * 1024;  // 64k

        /// <summary>
        /// Gets or sets default memory size in bytes, before switching to temp file.
        /// </summary>
        public static int DefaultMemorySize
        {
            get { return defaultMemorySize; }

            set
            {
                if (value < 32 * 1024)
                { throw new ArgumentException($"Property '{nameof(DefaultMemorySize)}' value must be >= 32k."); }

                defaultMemorySize = value;
            }
        }


        private Stream      m_pStream       = null;
        private int         m_MaxMemSize    = 64 * 1024;  // 64k


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


        public HybridStream()
            : this(defaultMemorySize)
        {

        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="memSize">Maximum bytes store to memory, before switching over to temporary file.</param>
        public HybridStream(int memSize)
        {
            m_MaxMemSize = memSize;

            m_pStream = new MemoryStream();
        }


        protected override void Dispose(bool disposing)
        {
            if (this.IsDisposed)
            { return; }

            this.IsDisposed = true;

            if (m_pStream != null)
            {
                m_pStream.Dispose();
                m_pStream = null;
            }

            base.Dispose(disposing);
        }

        private void ThrowIfObjectDisposed()
        {
            if (this.IsDisposed)
            { throw new ObjectDisposedException(this.GetType().Name); }
        }


        #region Override methods

        public override long Seek(long offset, SeekOrigin origin)
        {
            ThrowIfObjectDisposed();

            return m_pStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            ThrowIfObjectDisposed();

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

            return m_pStream.ReadByte();
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


            return m_pStream.Read(buffer, offset, count);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            ThrowIfObjectDisposed();

            return base.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override void WriteByte(byte value)
        {
            ThrowIfObjectDisposed();

            m_pStream.Write(new byte[] { value }, 0, 1);
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


            // We need switch to temporary file.
            if (m_pStream is MemoryStream && (m_pStream.Position + count) > m_MaxMemSize)
            {
                var fs = new FileStream(Path.GetTempPath() + "tf-" + ObjectId.NewId() + ".tmp", FileMode.Create, FileAccess.ReadWrite, FileShare.Read, 64 * 1024, FileOptions.DeleteOnClose);

                m_pStream.Position = 0;
                StreamUtil.StreamCopy(m_pStream, fs, 65536);

                m_pStream.Dispose();
                m_pStream = fs;
            }

            m_pStream.Write(bytes, index, count);
        }

        public override Task WriteAsync(byte[] bytes, int index, int count, CancellationToken cancellationToken)
        {
            ThrowIfObjectDisposed();

            return base.WriteAsync(bytes, index, count, cancellationToken);
        }

        public override void CopyTo(Stream destination, int bufferSize)
        {
            ThrowIfObjectDisposed();

            if (destination == null)
            { throw new ArgumentNullException(nameof(destination)); }

            if (bufferSize <= 0)
            { throw new ArgumentOutOfRangeException(nameof(bufferSize), $"Argument '{nameof(bufferSize)}' value must be > 0."); }


            m_pStream.CopyTo(destination, bufferSize);
        }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            ThrowIfObjectDisposed();

            return base.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        #endregion
    }
}