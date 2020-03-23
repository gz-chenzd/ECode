using System;
using System.Collections.Generic;
using System.IO;
using ECode.Utility;

namespace ECode.Encoder
{
    public sealed class HexEncoder : IEncoder
    {
        public static readonly char[]   UPPERCASE_ENCODE_TABLE = {
            (char)'0', (char)'1', (char)'2', (char)'3', (char)'4',
            (char)'5', (char)'6', (char)'7', (char)'8', (char)'9',
            (char)'A', (char)'B', (char)'C', (char)'D', (char)'E', (char)'F'
        };

        public static readonly char[]   LOWERCASE_ENCODE_TABLE = {
            (char)'0', (char)'1', (char)'2', (char)'3', (char)'4',
            (char)'5', (char)'6', (char)'7', (char)'8', (char)'9',
            (char)'a', (char)'b', (char)'c', (char)'d', (char)'e', (char)'f'
        };


        private char[]  ENCODE_TABLE    = LOWERCASE_ENCODE_TABLE;


        public HexEncoder(bool upperCase = false)
        {
            if (upperCase)
            {
                ENCODE_TABLE = UPPERCASE_ENCODE_TABLE;
            }
        }


        public string Encode(byte[] bytes)
        {
            AssertUtil.ArgumentNotNull(bytes, nameof(bytes));

            return Encode(bytes, 0, bytes.Length, false);
        }

        public string Encode(byte[] bytes, int index, int count)
        {
            return Encode(bytes, index, count, false);
        }

        public string Encode(byte[] bytes, int index, int count, bool insertLineBreaks)
        {
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


            int capacity = count * 2;
            if (insertLineBreaks)
            { capacity += 2 * (int)Math.Ceiling(capacity / 76D); }

            var retVal = new List<char>(capacity);
            for (int i = 0; i < count; i++)
            {
                if (insertLineBreaks && i > 0 && i % 38 == 0)
                {
                    retVal.Add('\r');
                    retVal.Add('\n');
                }

                retVal.Add(ENCODE_TABLE[bytes[index + i] >> 4]);
                retVal.Add(ENCODE_TABLE[bytes[index + i] & 0xF]);
            }

            return new string(retVal.ToArray());
        }


        public void Encode(Stream inStream, Stream outStream)
        {
            Encode(inStream, outStream, false);
        }

        public void Encode(Stream inStream, Stream outStream, bool insertLineBreaks)
        {
            AssertUtil.ArgumentNotNull(inStream, nameof(inStream));
            AssertUtil.ArgumentNotNull(outStream, nameof(outStream));

            if (!inStream.CanRead)
            { throw new ArgumentException($"Argument '{nameof(inStream)}' cannot be read."); }

            if (!outStream.CanWrite)
            { throw new ArgumentException($"Argument '{nameof(outStream)}' not supports writing."); }


            var encodePos       = 0;
            var bytesReaded     = 0;
            var readBuffer      = new byte[1024];
            var bytesInOut      = 0;
            var outBuffer       = new byte[1024];
            var blocksInLine    = 0;

            while ((bytesReaded = inStream.Read(readBuffer, 0, readBuffer.Length)) > 0)
            {
                for (encodePos = 0; encodePos < bytesReaded; encodePos++)
                {
                    if (bytesInOut >= outBuffer.Length - 10)
                    {
                        outStream.Write(outBuffer, 0, bytesInOut);

                        bytesInOut = 0;
                    }

                    if (insertLineBreaks && blocksInLine == 38)
                    {
                        outBuffer[bytesInOut++] = (byte)'\r';
                        outBuffer[bytesInOut++] = (byte)'\n';

                        blocksInLine = 0;
                    }

                    outBuffer[bytesInOut++] = (byte)ENCODE_TABLE[readBuffer[encodePos] >> 4];
                    outBuffer[bytesInOut++] = (byte)ENCODE_TABLE[readBuffer[encodePos] & 0xF];

                    blocksInLine++;
                }
            }

            outStream.Write(outBuffer, 0, bytesInOut);
        }


        public byte[] Decode(byte[] bytes)
        {
            AssertUtil.ArgumentNotNull(bytes, nameof(bytes));

            return Decode(bytes, 0, bytes.Length, false);
        }

        public byte[] Decode(byte[] bytes, int index, int count)
        {
            return Decode(bytes, index, count, false);
        }

        public byte[] Decode(byte[] bytes, int index, int count, bool ignoreInvalidChars)
        {
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


            int bytesInBuf  = 0;
            var decodedBuf  = new byte[2];

            var retVal = new MemoryStream((int)Math.Ceiling(count / 2D));
            for (int i = 0; i < count; i++)
            {
                switch ((char)bytes[index + i])
                {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        decodedBuf[bytesInBuf++] = (byte)(bytes[index + i] - '0');
                        break;

                    case 'a':
                    case 'A':
                        decodedBuf[bytesInBuf++] = 10;
                        break;

                    case 'b':
                    case 'B':
                        decodedBuf[bytesInBuf++] = 11;
                        break;

                    case 'c':
                    case 'C':
                        decodedBuf[bytesInBuf++] = 12;
                        break;

                    case 'd':
                    case 'D':
                        decodedBuf[bytesInBuf++] = 13;
                        break;

                    case 'e':
                    case 'E':
                        decodedBuf[bytesInBuf++] = 14;
                        break;

                    case 'f':
                    case 'F':
                        decodedBuf[bytesInBuf++] = 15;
                        break;

                    case ' ':
                    case '\r':
                    case '\n':
                    case '\t':
                        continue;

                    default:
                        if (ignoreInvalidChars)
                        { continue; }
                        else
                        { throw new FormatException($"Invalid hex char '{(char)bytes[index + i]}'."); }
                }

                if (bytesInBuf == 2)
                {
                    // Join hex 4 bit(left hex cahr) + 4bit(right hex char) in bytes 8 it
                    retVal.WriteByte((byte)((decodedBuf[0] << 4) | decodedBuf[1]));

                    bytesInBuf = 0;
                }
            }

            if (bytesInBuf == 1 && !ignoreInvalidChars)
            { throw new FormatException($"Invalid incomplete hex 2-char block '{(char)decodedBuf[0]}'"); }

            return retVal.ToArray();
        }


        public void Decode(Stream inStream, Stream outStream)
        {
            Decode(inStream, outStream, false);
        }

        public void Decode(Stream inStream, Stream outStream, bool ignoreInvalidChars)
        {
            AssertUtil.ArgumentNotNull(inStream, nameof(inStream));
            AssertUtil.ArgumentNotNull(outStream, nameof(outStream));

            if (!inStream.CanRead)
            { throw new ArgumentException($"Argument '{nameof(inStream)}' cannot be read."); }

            if (!outStream.CanWrite)
            { throw new ArgumentException($"Argument '{nameof(outStream)}' not supports writing."); }


            var decodePos       = 0;
            var bytesReaded     = 0;
            var readBuffer      = new byte[1024];
            var bytesInOut      = 0;
            var outBuffer       = new byte[1024];
            var bytesInBlock    = 0;
            var decodeBlock     = new byte[2];

            while ((bytesReaded = inStream.Read(readBuffer, 0, readBuffer.Length)) > 0)
            {
                decodePos = 0;

                while (decodePos < bytesReaded)
                {
                    if (bytesInOut >= outBuffer.Length - 10)
                    {
                        outStream.Write(outBuffer, 0, bytesInOut);

                        bytesInOut = 0;
                    }

                    for (; bytesInBlock < 2 && decodePos < bytesReaded; decodePos++)
                    {
                        switch (readBuffer[decodePos])
                        {
                            case (byte)' ':
                            case (byte)'\r':
                            case (byte)'\n':
                            case (byte)'\t':
                                continue;

                            case (byte)'0':
                            case (byte)'1':
                            case (byte)'2':
                            case (byte)'3':
                            case (byte)'4':
                            case (byte)'5':
                            case (byte)'6':
                            case (byte)'7':
                            case (byte)'8':
                            case (byte)'9':
                                decodeBlock[bytesInBlock++] = (byte)(readBuffer[decodePos] - '0');
                                continue;

                            case (byte)'a':
                            case (byte)'A':
                                decodeBlock[bytesInBlock++] = 10;
                                continue;

                            case (byte)'b':
                            case (byte)'B':
                                decodeBlock[bytesInBlock++] = 11;
                                continue;

                            case (byte)'c':
                            case (byte)'C':
                                decodeBlock[bytesInBlock++] = 12;
                                continue;

                            case (byte)'d':
                            case (byte)'D':
                                decodeBlock[bytesInBlock++] = 13;
                                continue;

                            case (byte)'e':
                            case (byte)'E':
                                decodeBlock[bytesInBlock++] = 14;
                                continue;

                            case (byte)'f':
                            case (byte)'F':
                                decodeBlock[bytesInBlock++] = 15;
                                continue;

                            default:
                                if (ignoreInvalidChars)
                                { continue; }
                                else
                                { throw new FormatException($"Invalid hex char '{(char)readBuffer[decodePos]}'."); }
                        }
                    }

                    if (bytesInBlock == 2)
                    {
                        outBuffer[bytesInOut++] = (byte)((decodeBlock[0] << 4) | decodeBlock[1]);

                        bytesInBlock = 0;
                    }
                }
            }

            if (bytesInBlock == 1 && !ignoreInvalidChars)
            { throw new FormatException($"Invalid incomplete hex 2-char block '{(char)decodeBlock[0]}'"); }

            outStream.Write(outBuffer, 0, bytesInOut);
        }
    }
}
