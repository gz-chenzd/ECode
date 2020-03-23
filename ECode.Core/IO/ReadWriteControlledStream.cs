using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ECode.Utility;

namespace ECode.IO
{
    /// <summary>
    /// This class implements read, write or read-write access stream.
    /// </summary>
    public sealed class ReadWriteControlledStream : Stream
    {
        private Stream          m_pStream       = null;
        private bool            m_IsOwner       = false;
        private FileAccess      m_AccessMode    = FileAccess.ReadWrite;


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


        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="stream">Stream which to encode/decode.</param>
        /// <param name="owner">Specifies if Base64Stream is owner of <b>stream</b>.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>stream</b> is null reference.</exception>
        public ReadWriteControlledStream(Stream stream, bool owner = true)
            : this(stream, owner, FileAccess.ReadWrite)
        {

        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="stream">Stream which to encode/decode.</param>
        /// <param name="owner">Specifies if Base64Stream is owner of <b>stream</b>.</param>
        /// <param name="access">This stream access mode.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>stream</b> is null reference.</exception>
        public ReadWriteControlledStream(Stream stream, bool owner, FileAccess access)
        {
            AssertUtil.ArgumentNotNull(stream, nameof(stream));

            m_pStream = stream;
            m_IsOwner = owner;
            m_AccessMode = access;
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


        #region Override Methods

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

            if ((m_AccessMode & FileAccess.Read) == 0)
            { throw new NotSupportedException(); }

            return m_pStream.ReadByte();
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


            return m_pStream.Read(buffer, offset, count);
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

            if ((m_AccessMode & FileAccess.Write) == 0)
            { throw new NotSupportedException(); }

            m_pStream.WriteByte(value);
        }

        public override void Write(byte[] bytes, int index, int count)
        {
            ThrowIfObjectDisposed();

            if ((m_AccessMode & FileAccess.Write) == 0)
            { throw new NotSupportedException(); }

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
        }

        public override Task WriteAsync(byte[] bytes, int index, int count, CancellationToken cancellationToken)
        {
            ThrowIfObjectDisposed();

            if ((m_AccessMode & FileAccess.Write) == 0)
            { throw new NotSupportedException(); }

            return base.WriteAsync(bytes, index, count, cancellationToken);
        }

        public override void CopyTo(Stream destination, int bufferSize)
        {
            ThrowIfObjectDisposed();

            if ((m_AccessMode & FileAccess.Read) == 0)
            { throw new NotSupportedException(); }

            if (destination == null)
            { throw new ArgumentNullException(nameof(destination)); }

            if (bufferSize <= 0)
            { throw new ArgumentOutOfRangeException(nameof(bufferSize), $"Argument '{nameof(bufferSize)}' value must be > 0."); }


            m_pStream.CopyTo(destination, bufferSize);
        }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            ThrowIfObjectDisposed();

            if ((m_AccessMode & FileAccess.Read) == 0)
            { throw new NotSupportedException(); }

            return base.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        #endregion
    }
}
