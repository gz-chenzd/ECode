using System;
using System.IO;

namespace ECode.Utility
{
    public static class StreamUtil
    {
        /// <summary>
        /// Copies <b>source</b> stream data to <b>target</b> stream.
        /// </summary>
        /// <param name="source">Source stream. Reading starts from stream current position.</param>
        /// <param name="target">Target stream. Writing starts from stream current position.</param>
        /// <param name="bufferSize">Specifies transfer buffer size in bytes.</param>
        /// <returns>Returns number of bytes copied.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>source</b> or <b>target</b> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Is raised when <b>blockSize</b> is out of valid range.</exception>
        public static long StreamCopy(Stream source, Stream target, int bufferSize = 1024)
        {
            AssertUtil.ArgumentNotNull(source, nameof(source));
            AssertUtil.ArgumentNotNull(target, nameof(target));

            if (bufferSize <= 0)
            { throw new ArgumentOutOfRangeException($"Argument '{nameof(bufferSize)}' value must be > 0."); }


            return StreamCopy(source, target, new byte[bufferSize]);
        }

        /// <summary>
        /// Copies <b>source</b> stream data to <b>target</b> stream.
        /// </summary>
        /// <param name="source">Source stream. Reading starts from stream current position.</param>
        /// <param name="target">Target stream. Writing starts from stream current position.</param>
        /// <param name="buffer">Specifies transfer block buffer.</param>
        /// <returns>Returns number of bytes copied.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>source</b> or <b>target</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when <b>blockBuffer</b> is empty.</exception>
        public static long StreamCopy(Stream source, Stream target, byte[] buffer)
        {
            AssertUtil.ArgumentNotNull(source, nameof(source));
            AssertUtil.ArgumentNotNull(target, nameof(target));
            AssertUtil.ArgumentNotEmpty(buffer, nameof(buffer));


            long totalReaded = 0;
            while (true)
            {
                int readedCount = source.Read(buffer, 0, buffer.Length);
                // We reached end of stream, we readed all data sucessfully.
                if (readedCount == 0)
                { return totalReaded; }
                else
                {
                    target.Write(buffer, 0, readedCount);
                    totalReaded += readedCount;
                }
            }
        }
    }
}
