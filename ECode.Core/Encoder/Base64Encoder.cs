using System;
using System.Collections.Generic;
using System.IO;
using ECode.Utility;

namespace ECode.Encoder
{
    public sealed class Base64Encoder : IEncoder
    {
        #region RFC 4648

        /* RFC 4648.
            
            Base64 is processed from left to right by 4 6-bit byte block, 4 6-bit byte block 
            are converted to 3 8-bit bytes.
            If base64 4 byte block doesn't have 3 8-bit bytes, missing bytes are marked with =. 
                            
            Value Encoding  Value Encoding  Value Encoding  Value Encoding
                0 A            17 R            34 i            51 z
                1 B            18 S            35 j            52 0
                2 C            19 T            36 k            53 1
                3 D            20 U            37 l            54 2
                4 E            21 V            38 m            55 3
                5 F            22 W            39 n            56 4
                6 G            23 X            40 o            57 5
                7 H            24 Y            41 p            58 6
                8 I            25 Z            42 q            59 7
                9 J            26 a            43 r            60 8
                10 K           27 b            44 s            61 9
                11 L           28 c            45 t            62 +
                12 M           29 d            46 u            63 /
                13 N           30 e            47 v
                14 O           31 f            48 w         (pad) =
                15 P           32 g            49 x
                16 Q           33 h            50 y
                    
            NOTE: 4 base64 6-bit bytes = 3 8-bit bytes              
                // |    6-bit    |    6-bit    |    6-bit    |    6-bit    |
                // | 1 2 3 4 5 6 | 1 2 3 4 5 6 | 1 2 3 4 5 6 | 1 2 3 4 5 6 |
                // |    8-bit         |    8-bit        |    8-bit         |
        */

        #endregion


        public static readonly char[] ENCODE_TABLE = {
            (char)'A', (char)'B', (char)'C', (char)'D', (char)'E', (char)'F', (char)'G',
            (char)'H', (char)'I', (char)'J', (char)'K', (char)'L', (char)'M', (char)'N',
            (char)'O', (char)'P', (char)'Q', (char)'R', (char)'S', (char)'T',
            (char)'U', (char)'V', (char)'W', (char)'X', (char)'Y', (char)'Z',
            (char)'a', (char)'b', (char)'c', (char)'d', (char)'e', (char)'f', (char)'g',
            (char)'h', (char)'i', (char)'j', (char)'k', (char)'l', (char)'m', (char)'n',
            (char)'o', (char)'p', (char)'q', (char)'r', (char)'s', (char)'t',
            (char)'u', (char)'v', (char)'w', (char)'x', (char)'y', (char)'z',
            (char)'0', (char)'1', (char)'2', (char)'3', (char)'4',
            (char)'5', (char)'6', (char)'7', (char)'8', (char)'9',
            (char)'+', (char)'/', (char)'='
        };

        public static readonly short[] DECODE_TABLE = {
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,  // 0 -    9
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,  //10 -   19
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,  //20 -   29
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,  //30 -   39
            -1, -1, -1, 62, -1, -1, -1, 63, 52, 53,  //40 -   49
            54, 55, 56, 57, 58, 59, 60, 61, -1, -1,  //50 -   59
            -1, -1, -1, -1, -1,  0,  1,  2,  3,  4,  //60 -   69
             5,  6,  7,  8,  9, 10, 11, 12, 13, 14,  //70 -   79
            15, 16, 17, 18, 19, 20, 21 ,22, 23, 24,  //80 -   89
            25, -1, -1, -1, -1, -1, -1, 26, 27, 28,  //90 -   99
            29, 30, 31, 32, 33, 34, 35, 36, 37, 38,  //100 - 109
            39, 40, 41, 42, 43, 44, 45, 46, 47, 48,  //110 - 119
            49, 50, 51, -1, -1, -1, -1, -1           //120 - 127
        };


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


            int     encodePos       = index;
            int     lineBlocks      = 0;
            var     blockBuffer     = new byte[3];
            int     bytesInBlock    = 0;

            int capacity = 4 * (int)Math.Ceiling(count / 3D);
            if (insertLineBreaks)
            { capacity += 2 * (int)Math.Ceiling(capacity / 76D); }

            var retVal = new List<char>(capacity);
            while ((encodePos - index) < count)
            {
                if (insertLineBreaks && lineBlocks == 19)
                {
                    retVal.Add('\r');
                    retVal.Add('\n');

                    lineBlocks = 0;
                }

                // Read 3-byte source block.
                bytesInBlock = 0;
                while (bytesInBlock < 3)
                {
                    // Check that we won't exceed buffer data.
                    if ((encodePos - index) >= count)
                    { break; }

                    // Read byte.
                    blockBuffer[bytesInBlock++] = bytes[encodePos++];
                }

                // Encode source block.
                if (bytesInBlock == 1)
                {
                    retVal.Add(ENCODE_TABLE[(blockBuffer[0] & 0xfc) >> 2]);
                    retVal.Add(ENCODE_TABLE[(blockBuffer[0] & 3) << 4]);
                    retVal.Add(ENCODE_TABLE[0x40]);
                    retVal.Add(ENCODE_TABLE[0x40]);
                }
                else if (bytesInBlock == 2)
                {
                    retVal.Add(ENCODE_TABLE[(blockBuffer[0] & 0xfc) >> 2]);
                    retVal.Add(ENCODE_TABLE[((blockBuffer[0] & 3) << 4) | ((blockBuffer[1] & 240) >> 4)]);
                    retVal.Add(ENCODE_TABLE[(blockBuffer[1] & 15) << 2]);
                    retVal.Add(ENCODE_TABLE[0x40]);
                }
                else
                {
                    retVal.Add(ENCODE_TABLE[(blockBuffer[0] & 0xfc) >> 2]);
                    retVal.Add(ENCODE_TABLE[((blockBuffer[0] & 3) << 4) | ((blockBuffer[1] & 240) >> 4)]);
                    retVal.Add(ENCODE_TABLE[((blockBuffer[1] & 15) << 2) | ((blockBuffer[2] & 0xc0) >> 6)]);
                    retVal.Add(ENCODE_TABLE[blockBuffer[2] & 0x3f]);
                }

                lineBlocks++;
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
            var encodeLeft      = 0;
            var bytesReaded     = 0;
            var readBuffer      = new byte[1024];
            var bytesInOut      = 0;
            var outBuffer       = new byte[1024];
            var blocksInLine    = 0;

            while ((bytesReaded = inStream.Read(readBuffer, encodeLeft, readBuffer.Length - encodeLeft)) > 0)
            {
                bytesReaded += encodeLeft;

                encodePos = 0;
                encodeLeft = 0;

                while (encodePos < bytesReaded)
                {
                    if (bytesInOut >= outBuffer.Length - 10)
                    {
                        outStream.Write(outBuffer, 0, bytesInOut);

                        bytesInOut = 0;
                    }

                    if (insertLineBreaks && blocksInLine == 19)
                    {
                        outBuffer[bytesInOut++] = (byte)'\r';
                        outBuffer[bytesInOut++] = (byte)'\n';

                        blocksInLine = 0;
                    }

                    if (encodePos + 3 <= bytesReaded)
                    {
                        outBuffer[bytesInOut++] = (byte)ENCODE_TABLE[(readBuffer[encodePos] & 0xfc) >> 2];
                        outBuffer[bytesInOut++] = (byte)ENCODE_TABLE[((readBuffer[encodePos] & 3) << 4) | ((readBuffer[encodePos + 1] & 240) >> 4)];
                        outBuffer[bytesInOut++] = (byte)ENCODE_TABLE[((readBuffer[encodePos + 1] & 15) << 2) | ((readBuffer[encodePos + 2] & 0xc0) >> 6)];
                        outBuffer[bytesInOut++] = (byte)ENCODE_TABLE[readBuffer[encodePos + 2] & 0x3f];

                        encodePos += 3;
                        blocksInLine++;
                    }
                    else
                    {
                        encodeLeft = bytesReaded - encodePos;
                        for (var i = 0; i < encodeLeft; i++)
                        { readBuffer[i] = readBuffer[encodePos + i]; }

                        break;
                    }
                }
            }

            if (encodeLeft == 1)
            {
                outBuffer[bytesInOut++] = (byte)ENCODE_TABLE[(readBuffer[0] & 0xfc) >> 2];
                outBuffer[bytesInOut++] = (byte)ENCODE_TABLE[(readBuffer[0] & 3) << 4];
                outBuffer[bytesInOut++] = (byte)ENCODE_TABLE[0x40];
                outBuffer[bytesInOut++] = (byte)ENCODE_TABLE[0x40];
            }
            else if (encodeLeft == 2)
            {
                outBuffer[bytesInOut++] = (byte)ENCODE_TABLE[(readBuffer[0] & 0xfc) >> 2];
                outBuffer[bytesInOut++] = (byte)ENCODE_TABLE[((readBuffer[0] & 3) << 4) | ((readBuffer[1] & 240) >> 4)];
                outBuffer[bytesInOut++] = (byte)ENCODE_TABLE[(readBuffer[1] & 15) << 2];
                outBuffer[bytesInOut++] = (byte)ENCODE_TABLE[0x40];
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


            int     decodePos       = index;
            var     blockBuffer     = new byte[4];
            int     bytesInBlock    = 0;

            byte b = 0;
            var retVal = new MemoryStream(3 * (int)Math.Ceiling(count / 4D));
            while ((decodePos - index) < count)
            {
                // Read 4-byte base64 block.
                bytesInBlock = 0;
                while (bytesInBlock < 4 && (decodePos - index) < count)
                {
                    // Read byte.
                    b = bytes[decodePos++];

                    // Line break chars.
                    if (b == '\r' || b == '\n' || b == '\t' || b == ' ')
                    { continue; }
                    // Pad char.
                    if (b == '=')
                    {
                        // Padding may appear only in last two chars of 4-char block.
                        // ab==
                        // abc=
                        if (bytesInBlock < 2)
                        {
                            if (ignoreInvalidChars && (decodePos - index) < count)
                            {
                                bytesInBlock = 0;
                                continue;
                            }
                            else if (ignoreInvalidChars)
                            {
                                bytesInBlock = 0;
                                break;
                            }

                            if (bytesInBlock == 1)
                            { throw new FormatException($"Invalid base64 padding '{(char)blockBuffer[0]}='."); }

                            throw new FormatException("Invalid base64 padding '='.");
                        }

                        // Skip next padding char.
                        if (bytesInBlock == 2)
                        {
                            while ((decodePos - index) < count)
                            {
                                if (bytes[decodePos] == '\r' || bytes[decodePos] == '\n'
                                   || bytes[decodePos] == '\t' || bytes[decodePos] == ' ')
                                {
                                    decodePos++;
                                    continue;
                                }

                                if (bytes[decodePos] != '=')
                                {
                                    if (bytes[decodePos] > 127 || DECODE_TABLE[bytes[decodePos]] == -1)
                                    {
                                        if (ignoreInvalidChars)
                                        {
                                            decodePos++;
                                            continue;
                                        }

                                        throw new FormatException($"Invalid base64 padding '{(char)blockBuffer[0]}{(char)blockBuffer[1]}={(char)bytes[decodePos]}'.");
                                    }

                                    if (ignoreInvalidChars)
                                    {
                                        bytesInBlock = 0;
                                        break;
                                    }

                                    throw new FormatException($"Invalid base64 padding '{(char)blockBuffer[0]}{(char)blockBuffer[1]}={(char)bytes[decodePos]}'.");
                                }

                                decodePos++;
                                break;
                            }
                        }

                        break;
                    }
                    // Non-base64 char.
                    else if (b > 127 || DECODE_TABLE[b] == -1)
                    {
                        if (!ignoreInvalidChars)
                        {
                            throw new FormatException($"Invalid base64 char '{(char)b}'.");
                        }
                        // Igonre that char.
                        //else{
                    }
                    // Base64 char.
                    else
                    { blockBuffer[bytesInBlock++] = (byte)DECODE_TABLE[b]; }
                }

                if (bytesInBlock > 0)
                {
                    // Incomplete 4-byte base64 data block.
                    if (bytesInBlock == 1 && !ignoreInvalidChars)
                    { throw new FormatException($"Invalid incomplete base64 4-char block '{(char)blockBuffer[0]}'."); }

                    // Decode base64 block.
                    if (bytesInBlock > 1)
                    { retVal.WriteByte((byte)((blockBuffer[0] << 2) | (blockBuffer[1] >> 4))); }

                    if (bytesInBlock > 2)
                    { retVal.WriteByte((byte)(((blockBuffer[1] & 0xF) << 4) | (blockBuffer[2] >> 2))); }

                    if (bytesInBlock > 3)
                    { retVal.WriteByte((byte)(((blockBuffer[2] & 0x3) << 6) | blockBuffer[3])); }
                }
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
            var decodeBlock     = new byte[4];
            var waitForEnd      = false;

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

                    for (; bytesInBlock < 4 && decodePos < bytesReaded; decodePos++)
                    {
                        switch (readBuffer[decodePos])
                        {
                            case (byte)' ':
                            case (byte)'\r':
                            case (byte)'\n':
                            case (byte)'\t':
                                continue;

                            case (byte)'=':
                                // Padding may appear only in last two chars of 4-char block.
                                // ab==
                                // abc=
                                if (bytesInBlock < 2)
                                {
                                    if (ignoreInvalidChars)
                                    {
                                        bytesInBlock = 0;
                                        continue;
                                    }

                                    if (bytesInBlock == 1)
                                    { throw new FormatException($"Invalid base64 padding '{(char)decodeBlock[0]}='."); }
                                    else
                                    { throw new FormatException("Invalid base64 padding '='."); }
                                }
                                else if (bytesInBlock == 2)
                                {
                                    if (waitForEnd)
                                    { goto CONTINUE; }

                                    waitForEnd = true;
                                    continue;
                                }
                                else
                                { goto CONTINUE; }

                            default:
                                if (readBuffer[decodePos] > 127 || DECODE_TABLE[readBuffer[decodePos]] == -1)
                                {
                                    if (ignoreInvalidChars)
                                    { continue; }

                                    if (waitForEnd)
                                    { throw new FormatException($"Invalid base64 padding '{(char)decodeBlock[0]}{(char)decodeBlock[1]}={(char)readBuffer[decodePos]}'."); }

                                    throw new FormatException($"Invalid base64 char '{(char)readBuffer[decodePos]}'.");
                                }

                                if (waitForEnd && !ignoreInvalidChars)
                                {
                                    throw new FormatException($"Invalid base64 padding '{(char)decodeBlock[0]}{(char)decodeBlock[1]}={(char)readBuffer[decodePos]}'.");
                                }
                                else if (waitForEnd)
                                {
                                    waitForEnd = false;

                                    bytesInBlock = 0;
                                    decodeBlock[bytesInBlock++] = (byte)DECODE_TABLE[readBuffer[decodePos]];
                                }
                                else
                                { decodeBlock[bytesInBlock++] = (byte)DECODE_TABLE[readBuffer[decodePos]]; }

                                continue;
                        }
                    }

                    if (bytesInBlock == 4)
                    {
                        outBuffer[bytesInOut++] = (byte)((decodeBlock[0] << 2) | (decodeBlock[1] >> 4));
                        outBuffer[bytesInOut++] = (byte)(((decodeBlock[1] & 0xF) << 4) | (decodeBlock[2] >> 2));
                        outBuffer[bytesInOut++] = (byte)(((decodeBlock[2] & 0x3) << 6) | decodeBlock[3]);

                        bytesInBlock = 0;
                    }
                }
            }

        CONTINUE:
            if (bytesInBlock > 0)
            {
                // Incomplete 4-byte base64 data block.
                if (bytesInBlock == 1 && !ignoreInvalidChars)
                { throw new FormatException($"Invalid incomplete base64 4-char block '{(char)decodeBlock[0]}'."); }

                if (bytesInBlock > 1)
                { outBuffer[bytesInOut++] = (byte)((decodeBlock[0] << 2) | (decodeBlock[1] >> 4)); }

                if (bytesInBlock > 2)
                { outBuffer[bytesInOut++] = (byte)(((decodeBlock[1] & 0xF) << 4) | (decodeBlock[2] >> 2)); }

                if (bytesInBlock > 3)
                { outBuffer[bytesInOut++] = (byte)(((decodeBlock[2] & 0x3) << 6) | decodeBlock[3]); }
            }

            outStream.Write(outBuffer, 0, bytesInOut);
        }
    }
}
