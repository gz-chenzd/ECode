using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using ECode.Utility;

namespace ECode.Encoder
{
    public sealed class QuotedPrintableEncoder : IEncoder
    {
        private char[]  HexTable    = HexEncoder.LOWERCASE_ENCODE_TABLE;


        public QuotedPrintableEncoder(bool upperCase = false)
        {
            if (upperCase)
            {
                HexTable = HexEncoder.UPPERCASE_ENCODE_TABLE;
            }
        }


        static bool IsPrintableChar(byte b)
        {
            if ((b >= 33 && b <= 60) || (b >= 62 && b <= 126))
            {
                return true;
            }

            return false;
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


            int printableBytes = 0;
            for (int p = index, end = index + count; p < end; p++)
            {
                if (IsPrintableChar(bytes[p]))
                { printableBytes++; }
            }

            int capacity = printableBytes + (count - printableBytes) * 3;
            if (insertLineBreaks)
            { capacity += 3 * (int)Math.Ceiling(capacity / 73D); }

            byte b              = 0;
            int  bytesInLine    = 0;
            var  retVal         = new List<char>(capacity);
            for (int i = 0; i < count; i++)
            {
                if (insertLineBreaks && bytesInLine >= 75)
                {
                    retVal.Add('=');
                    retVal.Add('\r');
                    retVal.Add('\n');

                    bytesInLine = 0;
                }

                b = bytes[index + i];

                // We don't need to encode byte.
                if (IsPrintableChar(b))
                {
                    retVal.Add((char)b);

                    bytesInLine++;
                }
                // We need to encode byte.
                else
                {
                    retVal.Add('=');
                    retVal.Add(HexTable[b >> 4]);
                    retVal.Add(HexTable[b & 0xF]);

                    bytesInLine += 3;
                }
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
            var bytesInLine     = 0;

            while ((bytesReaded = inStream.Read(readBuffer, 0, readBuffer.Length)) > 0)
            {
                for (encodePos = 0; encodePos < bytesReaded; encodePos++)
                {
                    if (bytesInOut >= outBuffer.Length - 10)
                    {
                        outStream.Write(outBuffer, 0, bytesInOut);

                        bytesInOut = 0;
                    }

                    if (insertLineBreaks && bytesInLine >= 75)
                    {
                        outBuffer[bytesInOut++] = (byte)'=';
                        outBuffer[bytesInOut++] = (byte)'\r';
                        outBuffer[bytesInOut++] = (byte)'\n';

                        bytesInLine = 0;
                    }

                    if (IsPrintableChar(readBuffer[encodePos]))
                    {
                        outBuffer[bytesInOut++] = readBuffer[encodePos];

                        bytesInLine++;
                    }
                    else
                    {
                        outBuffer[bytesInOut++] = (byte)'=';
                        outBuffer[bytesInOut++] = (byte)HexTable[readBuffer[encodePos] >> 4];
                        outBuffer[bytesInOut++] = (byte)HexTable[readBuffer[encodePos] & 0xF];

                        bytesInLine += 3;
                    }
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

            byte b  = 0;
            byte b1 = 0;
            byte b2 = 0;
            byte b3 = 0;
            var retVal = new MemoryStream(count);
            for (int i = 0; i < count; i++)
            {
                b = bytes[index + i];

                if (b == '\r' || b == '\n' || b == '\t' || b == ' ')
                { continue; }
                // We should have soft line-break or =XX hex-byte.
                else if (b == '=')
                {
                    if (i == count - 1)  // last one
                    { continue; }

                    if (bytes[index + i + 1] == '\r' || bytes[index + i + 1] == '\n'
                       || bytes[index + i + 1] == '\t' || bytes[index + i + 1] == ' ')
                    { continue; }

                    if (i == count - 2)
                    {
                        if (!ignoreInvalidChars)
                        { throw new FormatException($"Invalid quoted-printable chars block '={(char)bytes[index + i + 1]}'."); }

                        retVal.WriteByte(b);
                        retVal.WriteByte(bytes[index + (++i)]);

                        continue;
                    }

                    b1 = bytes[index + (++i)];
                    b2 = bytes[index + (++i)];

                    if (byte.TryParse(new string(new[] { (char)b1, (char)b2 }), NumberStyles.HexNumber, null, out b3))
                    { retVal.WriteByte(b3); }
                    // Not hex number, invalid chars.
                    else if (!ignoreInvalidChars)
                    {
                        throw new FormatException($"Invalid quoted-printable chars block '={(char)b1}{(char)b2}'.");
                    }
                    else
                    {
                        retVal.WriteByte(b);
                        retVal.WriteByte(b1);
                        retVal.WriteByte(b2);
                    }
                }
                // Normal char.
                else if (IsPrintableChar(b))
                { retVal.WriteByte(b); }
                else if (!ignoreInvalidChars)
                {
                    throw new FormatException($"Invalid quoted-printable chars '{(char)b}'.");
                }
                else
                { retVal.WriteByte(b); }
            }

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
            var decodeBlock     = new byte[3];

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

                    for (; bytesInBlock < 3 && decodePos < bytesReaded; decodePos++)
                    {
                        switch (readBuffer[decodePos])
                        {
                            case (byte)' ':
                            case (byte)'\r':
                            case (byte)'\n':
                            case (byte)'\t':
                                bytesInBlock = 0;
                                continue;

                            default:
                                if (bytesInBlock > 0)
                                {
                                    decodeBlock[bytesInBlock++] = readBuffer[decodePos];
                                    continue;
                                }

                                if (readBuffer[decodePos] == '=')
                                {
                                    bytesInBlock++;
                                    continue;
                                }

                                if (IsPrintableChar(readBuffer[decodePos]))
                                {
                                    outBuffer[bytesInOut++] = readBuffer[decodePos++];

                                    goto CONTINUE;
                                }

                                if (!ignoreInvalidChars)
                                { throw new FormatException($"Invalid quoted-printable chars '{(char)readBuffer[decodePos]}'."); }

                                outBuffer[bytesInOut++] = readBuffer[decodePos++];

                                goto CONTINUE;
                        }
                    }

                    if (bytesInBlock == 3)
                    {
                        if (byte.TryParse(new string(new[] { (char)decodeBlock[1], (char)decodeBlock[2] }), NumberStyles.HexNumber, null, out byte b3))
                        { outBuffer[bytesInOut++] = b3; }
                        else if (!ignoreInvalidChars)
                        {
                            throw new FormatException($"Invalid quoted-printable chars block '={(char)decodeBlock[1]}{(char)decodeBlock[2]}'.");
                        }
                        else
                        {
                            outBuffer[bytesInOut++] = (byte)'=';
                            outBuffer[bytesInOut++] = decodeBlock[1];
                            outBuffer[bytesInOut++] = decodeBlock[2];
                        }

                        bytesInBlock = 0;
                    }

                CONTINUE:
                    continue;
                }
            }

            if (bytesInBlock > 1)
            {
                if (!ignoreInvalidChars)
                {
                    throw new FormatException($"Invalid quoted-printable chars block '={(char)decodeBlock[1]}'.");
                }
                else
                {
                    outBuffer[bytesInOut++] = (byte)'=';
                    outBuffer[bytesInOut++] = decodeBlock[1];
                }
            }

            outStream.Write(outBuffer, 0, bytesInOut);
        }
    }
}
