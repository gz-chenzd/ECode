using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ECode.IO
{
    /// <summary>
    /// This stream just junks all written data.
    /// </summary>
    public class JunkingStream : Stream
    {
        #region Properties Implementation

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        public override long Position
        {
            get { throw new NotSupportedException(); }

            set { throw new NotSupportedException(); }
        }

        public override bool CanTimeout
        {
            get { throw new NotSupportedException(); }
        }

        public override int ReadTimeout
        {
            get { throw new NotSupportedException(); }

            set { throw new NotSupportedException(); }
        }

        public override int WriteTimeout
        {
            get { throw new NotSupportedException(); }

            set { throw new NotSupportedException(); }
        }

        #endregion


        #region Override Methods

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Flush()
        {

        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }

        public override int ReadByte()
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public override void WriteByte(byte value)
        {

        }

        public override void Write(byte[] bytes, int index, int count)
        {

        }

        public override Task WriteAsync(byte[] bytes, int index, int count, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
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