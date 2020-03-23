using System;
using System.IO;
using System.Threading.Tasks;
using ECode.Core;

namespace ECode.IO
{
    public sealed class StreamReadResult
    {
        /// <summary>
        /// Gets how many lines are readed.
        /// </summary>
        public long LinesReaded
        { get; internal set; }

        /// <summary>
        /// Gets how many bytes are readed.
        /// </summary>
        public long BytesReaded
        { get; internal set; }
    }

    public sealed class StreamWriteResult
    {
        /// <summary>
        /// Gets how many lines are written.
        /// </summary>
        public int LinesWritten
        { get; internal set; }

        /// <summary>
        /// Gets how many bytes are written.
        /// </summary>
        public long BytesWritten
        { get; internal set; }
    }

    public static class PeriodTerminatedHelper
    {
        public static StreamReadResult Read(LineReader fromReader, Stream toStream)
        {
            return Read(fromReader, toStream, 0);
        }

        public static StreamReadResult Read(LineReader fromReader, Stream toStream, long maxCount)
        {
            return Read(fromReader, toStream, maxCount, SizeExceededAction.ThrowException);
        }

        public static StreamReadResult Read(LineReader fromReader, Stream toStream, long maxCount, SizeExceededAction exceededAction)
        {
            if (fromReader == null)
            { throw new ArgumentNullException(nameof(fromReader)); }

            if (toStream == null)
            { throw new ArgumentNullException(nameof(toStream)); }

            if (!toStream.CanWrite)
            { throw new ArgumentException($"Argument '{nameof(toStream)}' cannot be written.", nameof(toStream)); }


            var hasReadExceeded = false;
            var result = new StreamReadResult();

            while (true)
            {
                fromReader.Read();

                if (fromReader.BytesInBuffer == 0)
                { throw new IncompleteDataException("Data is not period-terminated."); }
                // We have period terminator.
                else if (fromReader.LineBytesInBuffer == 1 && fromReader.Buffer[0] == '.')
                { break; }
                // Normal line.
                else
                {
                    if (hasReadExceeded == true)
                    { continue; }

                    // Period handling: If line starts with '.', it must be removed.
                    if (fromReader.Buffer[0] == '.')
                    {
                        if (maxCount > 0 && (result.BytesReaded + fromReader.BytesInBuffer - 1) > maxCount)
                        {
                            hasReadExceeded = true;

                            // Maximum allowed to read bytes exceeded.
                            if (exceededAction == SizeExceededAction.ThrowException)
                            { throw new DataSizeExceededException(); }

                            continue;
                        }

                        result.LinesReaded++;
                        result.BytesReaded += fromReader.BytesInBuffer - 1;

                        toStream.Write(fromReader.Buffer, 1, fromReader.BytesInBuffer - 1);

                        continue;
                    }

                    // Nomrmal line.
                    if (maxCount > 0 && (result.BytesReaded + fromReader.BytesInBuffer) > maxCount)
                    {
                        hasReadExceeded = true;

                        // Maximum allowed to read bytes exceeded.
                        if (exceededAction == SizeExceededAction.ThrowException)
                        { throw new DataSizeExceededException(); }

                        continue;
                    }

                    result.LinesReaded++;
                    result.BytesReaded += fromReader.BytesInBuffer;

                    toStream.Write(fromReader.Buffer, 0, fromReader.BytesInBuffer);
                }
            }

            toStream.Flush();

            return result;
        }

        public static Task<StreamReadResult> ReadAsync(LineReader fromReader, Stream toStream)
        {
            return ReadAsync(fromReader, toStream, 0);
        }

        public static Task<StreamReadResult> ReadAsync(LineReader fromReader, Stream toStream, long maxCount)
        {
            return ReadAsync(fromReader, toStream, maxCount, SizeExceededAction.ThrowException);
        }

        public static Task<StreamReadResult> ReadAsync(LineReader fromReader, Stream toStream, long maxCount, SizeExceededAction exceededAction)
        {
            if (fromReader == null)
            { throw new ArgumentNullException(nameof(fromReader)); }

            if (toStream == null)
            { throw new ArgumentNullException(nameof(toStream)); }

            if (!toStream.CanWrite)
            { throw new ArgumentException($"Argument '{nameof(toStream)}' cannot be written.", nameof(toStream)); }


            return Task.Factory.StartNew(() =>
            {
                return Read(fromReader, toStream, maxCount, exceededAction);
            });
        }


        public static StreamWriteResult Write(LineReader fromReader, Stream toStream)
        {
            return Write(fromReader, toStream, 0);
        }

        public static StreamWriteResult Write(LineReader fromReader, Stream toStream, long maxCount)
        {
            return Write(fromReader, toStream, maxCount, SizeExceededAction.ThrowException);
        }

        public static StreamWriteResult Write(LineReader fromReader, Stream toStream, long maxCount, SizeExceededAction exceededAction)
        {
            if (fromReader == null)
            { throw new ArgumentNullException(nameof(fromReader)); }

            if (toStream == null)
            { throw new ArgumentNullException(nameof(toStream)); }

            if (!toStream.CanWrite)
            { throw new ArgumentException($"Argument '{nameof(toStream)}' cannot be written.", nameof(toStream)); }


            var hasWriteExceeded = false;
            var lastLineEndsWithCRLF = false;
            var result = new StreamWriteResult();

            while (true)
            {
                fromReader.Read();

                // We have readed all source stream data, we are done.
                if (fromReader.BytesInBuffer == 0)
                {
                    if (hasWriteExceeded == true)
                    { break; }

                    var lastEndBytes = new byte[] { (byte)'.', (byte)'\r', (byte)'\n' };

                    // if last line isnot end with CRLF.
                    if (!lastLineEndsWithCRLF)
                    {
                        lastEndBytes = new byte[] { (byte)'\r', (byte)'\n', (byte)'.', (byte)'\r', (byte)'\n' };
                    }

                    if (maxCount > 0 && (result.BytesWritten + lastEndBytes.Length) > maxCount)
                    {
                        hasWriteExceeded = true;

                        // Maximum allowed to write bytes exceeded.
                        if (exceededAction == SizeExceededAction.ThrowException)
                        { throw new DataSizeExceededException(); }

                        break;
                    }

                    result.LinesWritten++;
                    result.BytesWritten += lastEndBytes.Length;

                    toStream.Write(lastEndBytes, 0, lastEndBytes.Length);

                    break;
                }
                // Write readed line.
                else
                {
                    if (hasWriteExceeded == true)
                    { continue; }

                    // Check if line ends CRLF.
                    if (fromReader.BytesInBuffer >= 2
                        && fromReader.Buffer[fromReader.BytesInBuffer - 2] == '\r'
                        && fromReader.Buffer[fromReader.BytesInBuffer - 1] == '\n')
                    {
                        lastLineEndsWithCRLF = true;
                    }
                    else
                    {
                        lastLineEndsWithCRLF = false;
                    }

                    // Period handling. If line starts with period(.), additional period is added.
                    if (fromReader.Buffer[0] == '.')
                    {
                        if (maxCount > 0 && (result.BytesWritten + fromReader.BytesInBuffer + 1) > maxCount)
                        {
                            hasWriteExceeded = true;

                            // Maximum allowed to write bytes exceeded.
                            if (exceededAction == SizeExceededAction.ThrowException)
                            { throw new DataSizeExceededException(); }

                            continue;
                        }

                        result.LinesWritten++;
                        result.BytesWritten += fromReader.BytesInBuffer + 1;

                        toStream.Write(new byte[] { (byte)'.' }, 0, 1);
                        toStream.Write(fromReader.Buffer, 0, fromReader.BytesInBuffer);

                        continue;
                    }

                    // Normal line.
                    if (maxCount > 0 && (result.BytesWritten + fromReader.BytesInBuffer) > maxCount)
                    {
                        hasWriteExceeded = true;

                        // Maximum allowed to write bytes exceeded.
                        if (exceededAction == SizeExceededAction.ThrowException)
                        { throw new DataSizeExceededException(); }

                        continue;
                    }

                    result.LinesWritten++;
                    result.BytesWritten += fromReader.BytesInBuffer;

                    toStream.Write(fromReader.Buffer, 0, fromReader.BytesInBuffer);
                }
            }

            toStream.Flush();

            return result;
        }

        public static Task<StreamWriteResult> WriteAsync(LineReader fromReader, Stream toStream)
        {
            return WriteAsync(fromReader, toStream, 0);
        }

        public static Task<StreamWriteResult> WriteAsync(LineReader fromReader, Stream toStream, long maxCount)
        {
            return WriteAsync(fromReader, toStream, maxCount, SizeExceededAction.ThrowException);
        }

        public static Task<StreamWriteResult> WriteAsync(LineReader fromReader, Stream toStream, long maxCount, SizeExceededAction exceededAction)
        {
            if (fromReader == null)
            { throw new ArgumentNullException(nameof(fromReader)); }

            if (toStream == null)
            { throw new ArgumentNullException(nameof(toStream)); }

            if (!toStream.CanWrite)
            { throw new ArgumentException($"Argument '{nameof(toStream)}' cannot be written.", nameof(toStream)); }


            return Task.Factory.StartNew(() =>
            {
                return Write(fromReader, toStream, maxCount, exceededAction);
            });
        }
    }
}
