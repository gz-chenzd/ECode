using System;
using System.IO;
using System.Text;

namespace ECode.Tokenizer
{
    public class PhraseReader
    {
        private Stream      inStream        = null;
        private int         runeOffset      = -1;
        private byte[]      buffer          = new byte[128];
        private int         bytesInBuf      = 0;


        public PhraseReader(string text)
        {
            if (text == null)
            { throw new ArgumentNullException(nameof(text)); }

            inStream = new MemoryStream(Encoding.UTF8.GetBytes(text));
        }

        public PhraseReader(Stream stream)
        {
            if (stream == null)
            { throw new ArgumentNullException(nameof(stream)); }

            if (!stream.CanRead)
            { throw new ArgumentException($"Argument '{nameof(stream)}' cannot be read.", nameof(stream)); }

            inStream = stream;
        }


        public PortionToken Read()
        {
        RE_READ:
            int b = -1;
            int startOffset = -1;

            int runeLength = -1;
            bool isNumbers = false;
            bool dotAppeared = false;

            bytesInBuf = 0;
            while ((b = inStream.ReadByte()) != -1)
            {
                if (startOffset == -1)
                {
                    if (IsWhiteSpace(b))
                    {
                        ++runeOffset;
                        continue;
                    }
                    else
                    { startOffset = runeOffset + 1; }
                }

                if (runeLength > 0)
                {
                    if ((b & 0xC0) != 0x80)  // 多字节符的非首字节,应为 10xxxxxx
                    {
                        inStream.Position--;
                        goto RE_READ;
                    }

                    buffer[bytesInBuf++] = (byte)b;
                    if (bytesInBuf < runeLength)
                    { continue; }

                    break;
                }

                if (IsAscii((byte)b))
                {
                    if (!IsNormalChar(b))
                    {
                        if (isNumbers && b == '.' && !dotAppeared)
                        {
                            dotAppeared = true;
                            buffer[bytesInBuf++] = (byte)b;
                            continue;
                        }
                        else if (bytesInBuf > 0)
                        {
                            inStream.Position--;
                            break;
                        }

                        buffer[bytesInBuf++] = (byte)b;
                        break;
                    }

                    buffer[bytesInBuf++] = (byte)b;
                    if (bytesInBuf == 1 || isNumbers)
                    { isNumbers = IsNumber(b); }
                }
                else
                {
                    if (bytesInBuf > 0)
                    {
                        inStream.Position--;
                        break;
                    }

                    runeLength = GetRuneLength(b);
                    if (runeLength > 0)
                    {
                        buffer[bytesInBuf++] = (byte)b;
                        continue;
                    }

                    goto RE_READ;
                }
            }

            runeOffset += runeLength > 1 ? 1 : bytesInBuf;
            return startOffset < 0 ? null : new PortionToken(Encoding.UTF8.GetString(buffer, 0, bytesInBuf), startOffset, runeLength > 1 ? 1 : bytesInBuf);
        }


        static bool IsAscii(int b)
        {
            if (b > 127)
            { return false; }

            return true;
        }

        static bool IsNumber(int b)
        {
            if (b >= '0' && b <= '9')
            { return true; }

            return false;
        }

        static bool IsNormalChar(int b)
        {
            if (b >= '0' && b <= '9')
            { return true; }

            if (b >= 'a' && b <= 'z')
            { return true; }

            if (b >= 'A' && b <= 'Z')
            { return true; }

            if (b == '_')
            { return true; }

            return false;
        }

        static bool IsWhiteSpace(int b)
        {
            switch (b)
            {
                case ' ':
                case '\t':
                case '\r':
                case '\n':
                    return true;

                default:
                    return false;
            }
        }

        static int GetRuneLength(int b)
        {
            if (IsAscii(b))
            { return 1; }

            if (b >= 0x80)
            {
                if (b >= 0xFC && b <= 0xFD)
                { return 6; }
                else if (b >= 0xF8)
                { return 5; }
                else if (b >= 0xF0)
                { return 4; }
                else if (b >= 0xE0)
                { return 3; }
                else if (b >= 0xC0)
                { return 2; }
            }

            return -1;
        }
    }
}
