using System;
using System.IO;
using System.Threading.Tasks;
using ECode.Utility;

namespace ECode.IO
{
    public static class FixedCountReader
    {
        public static void Read(Stream fromStream, Stream toStream, long count)
        {
            Read(fromStream, toStream, 1024, count);
        }

        public static void Read(Stream fromStream, Stream toStream, int bufferSize, long count)
        {
            if (bufferSize <= 0)
            { throw new ArgumentOutOfRangeException(nameof(bufferSize), $"Argument '{nameof(bufferSize)}' value must be > 0."); }

            if (count < 0)
            { throw new ArgumentOutOfRangeException(nameof(count), $"Argument '{nameof(count)}' value must be >= 0."); }

            Read(fromStream, toStream, new byte[bufferSize], count);
        }

        public static void Read(Stream fromStream, Stream toStream, byte[] buffer, long count)
        {
            AssertUtil.ArgumentNotNull(fromStream, nameof(fromStream));
            AssertUtil.ArgumentNotNull(toStream, nameof(toStream));
            AssertUtil.ArgumentNotEmpty(buffer, nameof(buffer));

            if (count < 0)
            { throw new ArgumentOutOfRangeException(nameof(count), $"Argument '{nameof(count)}' value must be >= 0."); }

            int bytesReaded = 0;
            while (true)
            {
                if (bytesReaded >= count)
                { break; }

                int countToRead = (int)Math.Min(buffer.Length, count - bytesReaded);
                int countReaded = fromStream.Read(buffer, 0, countToRead);
                if (countReaded <= 0)
                { break; }

                toStream.Write(buffer, 0, countReaded);
                bytesReaded += countReaded;
            }
        }


        public static Task ReadAsync(Stream fromStream, Stream toStream, long count)
        {
            return ReadAsync(fromStream, toStream, 1024, count);
        }

        public static Task ReadAsync(Stream fromStream, Stream toStream, int bufferSize, long count)
        {
            if (bufferSize <= 0)
            { throw new ArgumentOutOfRangeException(nameof(bufferSize), $"Argument '{nameof(bufferSize)}' value must be > 0."); }

            if (count < 0)
            { throw new ArgumentOutOfRangeException(nameof(count), $"Argument '{nameof(count)}' value must be >= 0."); }

            return ReadAsync(fromStream, toStream, new byte[bufferSize], count);
        }

        public static Task ReadAsync(Stream fromStream, Stream toStream, byte[] buffer, long count)
        {
            AssertUtil.ArgumentNotNull(fromStream, nameof(fromStream));
            AssertUtil.ArgumentNotNull(toStream, nameof(toStream));
            AssertUtil.ArgumentNotEmpty(buffer, nameof(buffer));

            if (count < 0)
            { throw new ArgumentOutOfRangeException(nameof(count), $"Argument '{nameof(count)}' value must be >= 0."); }

            return Task.Run(() =>
            {
                Read(fromStream, toStream, buffer, count);
            });
        }
    }
}
