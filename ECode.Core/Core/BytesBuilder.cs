using System;
using System.Collections.Generic;
using System.Text;
using ECode.Utility;

namespace ECode.Core
{
    public class BytesBuilder : IDisposable
    {
        private int                 count           = 0;
        private Encoding            encoding        = null;
        private byte[]              blockBuf        = null;
        private int                 blockPos        = 0;
        private int                 blockSize       = 1024;
        private List<byte[]>        blockList       = null;


        public BytesBuilder()
        {
            encoding = Encoding.UTF8;

            blockPos = 0;
            blockBuf = new byte[blockSize];

            blockList = new List<byte[]>();
            blockList.Add(blockBuf);
        }


        public void Dispose()
        {
            if (this.IsDisposed)
            { return; }

            this.IsDisposed = true;
            this.blockList.Clear();
        }

        private void ThrowIfObjectDisposed()
        {
            if (this.IsDisposed)
            { throw new ObjectDisposedException(this.GetType().Name); }
        }


        /// <summary>
        /// Appends specified string to the buffer.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Is aised when <b>str</b> is null.</exception>
        public void Append(string str)
        {
            AssertUtil.ArgumentNotNull(str, nameof(str));

            Append(encoding.GetBytes(str));
        }

        /// <summary>
        /// Appends specified string to the buffer.
        /// </summary>
        /// <param name="encoding">String encoding.</param>
        /// <param name="str">String value.</param>
        /// <exception cref="System.ArgumentNullException">Is aised when <b>encoding</b> or <b>str</b> is null.</exception>
        public void Append(Encoding encoding, string str)
        {
            AssertUtil.ArgumentNotNull(encoding, nameof(encoding));
            AssertUtil.ArgumentNotNull(str, nameof(str));

            Append(encoding.GetBytes(str));
        }

        /// <summary>
        /// Appends specified bytes to the buffer.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Is raised when <b>bytes</b> is null.</exception>
        public void Append(byte[] bytes)
        {
            AssertUtil.ArgumentNotNull(bytes, nameof(bytes));

            Append(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Appends specified bytes value to the buffer.
        /// </summary>
        /// <param name="bytes">Byte value.</param>
        /// <param name="index">The index of the first byte.</param>
        /// <param name="count">Number of bytes to append.</param>
        /// <exception cref="System.ArgumentNullException">Is raised when <b>bytes</b> is null.</exception>
        public void Append(byte[] bytes, int index, int count)
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

            lock (this)
            {
                while (count > 0)
                {
                    if ((this.blockPos + count) <= this.blockSize)
                    {
                        Array.Copy(bytes, index, this.blockBuf, this.blockPos, count);

                        this.count += count;
                        this.blockPos += count;

                        break;
                    }
                    else
                    {
                        int subCount = this.blockSize - this.blockPos;
                        Array.Copy(bytes, index, this.blockBuf, this.blockPos, subCount);

                        index += subCount;
                        count -= subCount;

                        this.count += subCount;

                        this.blockPos = 0;
                        this.blockBuf = new byte[blockSize];
                        this.blockList.Add(this.blockBuf);
                    }
                }

                if (this.blockPos == this.blockSize - 1)
                {
                    this.blockPos = 0;
                    this.blockBuf = new byte[blockSize];
                    this.blockList.Add(this.blockBuf);
                }
            }
        }


        /// <summary>
        /// Returns this as byte[] data.
        /// </summary>
        public byte[] ToBytes()
        {
            ThrowIfObjectDisposed();

            lock (this)
            {
                var retVal = new byte[this.count];

                for (var i = 0; i < this.blockList.Count - 1; i++)
                {
                    Array.Copy(this.blockList[i], 0, retVal, i * this.blockSize, this.blockSize);
                }

                Array.Copy(this.blockBuf, 0, retVal, (this.blockList.Count - 1) * this.blockSize, this.blockPos);

                return retVal;
            }
        }


        #region Properties Implementation

        private bool IsDisposed
        { get; set; }

        /// <summary>
        /// Gets number of bytes in the buffer.
        /// </summary>
        public int Count
        {
            get
            {
                ThrowIfObjectDisposed();

                return count;
            }
        }

        /// <summary>
        /// Gets or sets default charset encoding.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Is raised when null reference value is set.</exception>
        public Encoding Charset
        {
            get { return encoding; }

            set
            {
                ThrowIfObjectDisposed();

                AssertUtil.ArgumentNotNull(value, nameof(Charset));

                encoding = value;
            }
        }

        #endregion
    }
}
