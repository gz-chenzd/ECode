using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ECode.Core;
using ECode.Utility;

namespace ECode.Json
{
    public class JsonParser
    {
        private bool IsWhiteSpace(int b)
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

        private void IgnoreWhiteSpace()
        {
            do
            {
                if (!IsWhiteSpace(readedRune))
                { break; }
            } while (ReadNext());
        }



        const string                NULL            = "null";
        const string                TRUE            = "true";
        const string                FALSE           = "false";
        static readonly int[]       END_CHARS       = new[] { -1, ',', ']', '}' };
        static readonly Regex       NumberRegex     = new Regex("^[-]?((0(\\.\\d+)?)|([1-9]\\d*(\\.\\d+)?))([eE][+-]?\\d+)?$", RegexOptions.Compiled);

        private JValue ReadNull()
        {
            for (var i = 0; i < 4; i++)
            {
                if (readedRune != NULL[i])
                { throw new JsonException("Json invalid: invalid null"); }

                ReadNext();
            }

            IgnoreWhiteSpace();
            if (!END_CHARS.Contains(readedRune))
            { throw new JsonException("Json invalid: invalid null"); }

            return JValue.NULL;
        }

        private JValue ReadTrue()
        {
            for (var i = 0; i < 4; i++)
            {
                if (readedRune != TRUE[i])
                { throw new JsonException("Json invalid: invalid true"); }

                ReadNext();
            }

            IgnoreWhiteSpace();
            if (!END_CHARS.Contains(readedRune))
            { throw new JsonException("Json invalid: invalid true"); }

            return JValue.TRUE;
        }

        private JValue ReadFalse()
        {
            for (var i = 0; i < 5; i++)
            {
                if (readedRune != FALSE[i])
                { throw new JsonException("Json invalid: invalid false"); }

                ReadNext();
            }

            IgnoreWhiteSpace();
            if (!END_CHARS.Contains(readedRune))
            { throw new JsonException("Json invalid: invalid false"); }

            return JValue.FALSE;
        }

        private JValue ReadNumber()
        {
            var buffer = new List<char>();
            buffer.Add((char)readedRune);

            while (ReadNext())
            {
                if (IsWhiteSpace(readedRune) || END_CHARS.Contains(readedRune))
                { break; }

                buffer.Add((char)readedRune);
            }

            IgnoreWhiteSpace();
            if (!END_CHARS.Contains(readedRune))
            { throw new JsonException("Json invalid: invalid number"); }

            var number = new string(buffer.ToArray());
            if (!NumberRegex.IsMatch(number))
            { throw new JsonException("Json invalid: invalid number"); }

            return new JValue(number, JValueKind.Number);
        }

        private JValue ReadString()
        {
            int preRune = -1;
            var buffer = new StringBuilder();
            var rawBuf = new StringBuilder();

            while (ReadNext())
            {
                if (readedRune == '"' && preRune != '\\')
                { break; }

                preRune = readedRune;
                rawBuf.Append((char)readedRune);

                if (readedRune != '\\')
                { buffer.Append((char)readedRune); }
            }

            if (readedRune != '"' || preRune == '\\')
            { throw new JsonException("Json invalid: invalid string"); }

            ReadNext();
            IgnoreWhiteSpace();
            if (!END_CHARS.Contains(readedRune))
            { throw new JsonException("Json invalid: invalid string"); }

            return new JValue(buffer.ToString(), rawBuf.ToString(), JValueKind.String);
        }

        private JArray ReadArray()
        {
            ReadNext();
            IgnoreWhiteSpace();
            if (readedRune == -1)
            { throw new JsonException("Json invalid: invalid array"); }

            var items = new List<JToken>();

            do
            {
                if (readedRune == ']')
                { break; }

                items.Add(ReadAnyKindValue());

                IgnoreWhiteSpace();
                if (readedRune == ']')
                { break; }
                else if (readedRune == ',')
                {
                    ReadNext();
                    IgnoreWhiteSpace();
                    if (readedRune == ']')
                    { throw new JsonException("Json invalid: invalid array"); }
                }
                else
                { throw new JsonException("Json invalid: invalid array"); }
            } while (true);

            ReadNext();
            IgnoreWhiteSpace();
            if (!END_CHARS.Contains(readedRune))
            { throw new JsonException("Json invalid: invalid array"); }

            return new JArray(items);
        }

        private string ReadKey()
        {
            int preRune = -1;
            var buffer = new StringBuilder();

            while (ReadNext())
            {
                if (readedRune == '"' && preRune != '\\')
                { break; }

                preRune = readedRune;
                if (readedRune != '\\')
                { buffer.Append((char)readedRune); }
            }

            if (readedRune != '"' || preRune == '\\')
            { throw new JsonException("Json invalid: invalid object key"); }

            return buffer.ToString();
        }

        private string ResolveKey(string key)
        {
            if (trimKeySpace)
            { return key.Trim(); }

            return key;
        }

        private IDictionary<string, JToken> CreateDictionary()
        {
            if (ignoreCase)
            { return new Dictionary<string, JToken>(StringComparer.InvariantCultureIgnoreCase); }

            return new Dictionary<string, JToken>();
        }

        private JObject ReadObject()
        {
            ReadNext();
            IgnoreWhiteSpace();
            if (readedRune == -1)
            { throw new JsonException("Json invalid: invalid object"); }

            var fields = CreateDictionary();

            do
            {
                if (readedRune == '}')
                { break; }

                if (readedRune != '"')
                { throw new JsonException("Json invalid: invalid object"); }

                var key = ReadKey();
                key = ResolveKey(key);

                ReadNext();
                IgnoreWhiteSpace();
                if (readedRune != ':')
                { throw new JsonException("Json invalid: invalid object"); }

                ReadNext();
                IgnoreWhiteSpace();
                if (readedRune == -1 || readedRune == '}')
                { throw new JsonException("Json invalid: invalid object"); }

                fields[key] = ReadAnyKindValue();

                IgnoreWhiteSpace();
                if (readedRune == '}')
                { break; }
                else if (readedRune == ',')
                {
                    ReadNext();
                    IgnoreWhiteSpace();
                    if (readedRune == '}')
                    { throw new JsonException("Json invalid: invalid object"); }
                }
                else
                { throw new JsonException("Json invalid: invalid object"); }
            } while (true);

            ReadNext();
            IgnoreWhiteSpace();
            if (!END_CHARS.Contains(readedRune))
            { throw new JsonException("Json invalid: invalid object"); }

            return new JObject(fields);
        }

        private JToken ReadAnyKindValue()
        {
            switch (readedRune)
            {
                case '{':
                    return ReadObject();

                case '[':
                    return ReadArray();

                case '"':
                    return ReadString();

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
                case '-':
                    return ReadNumber();

                case 'n':
                    return ReadNull();

                case 't':
                    return ReadTrue();

                case 'f':
                    return ReadFalse();

                case -1:
                    return new JValue("null", JValueKind.Null);

                default:
                    throw new JsonException("Json invalid");
            }
        }



        private TextReader      inReader        = null;
        private int             readedRune      = -1;
        private bool            ignoreCase      = false;
        private bool            trimKeySpace    = false;


        private JsonParser(TextReader reader, bool ignoreCase = false, bool trimKeySpace = false)
        {
            AssertUtil.ArgumentNotNull(reader, nameof(reader));

            this.inReader = reader;
            this.ignoreCase = ignoreCase;
            this.trimKeySpace = trimKeySpace;
        }


        private bool ReadNext()
        {
            return (readedRune = inReader.Read()) != -1;
        }

        private JToken InternalParse()
        {
            ReadNext();
            IgnoreWhiteSpace();
            var root = ReadAnyKindValue();

            IgnoreWhiteSpace();
            if (readedRune != -1)
            { throw new JsonException("Json invalid"); }

            return root;
        }


        public static JToken Parse(string json, bool ignoreCase = false, bool trimKeySpace = false)
        {
            AssertUtil.ArgumentNotEmpty(json, nameof(json));

            using (var reader = new System.IO.StringReader(json))
            {
                return new JsonParser(reader, ignoreCase, trimKeySpace).InternalParse();
            }
        }

        public static JToken Parse(TextReader reader, bool ignoreCase = false, bool trimKeySpace = false)
        {
            AssertUtil.ArgumentNotNull(reader, nameof(reader));

            return new JsonParser(reader, ignoreCase, trimKeySpace).InternalParse();
        }
    }
}
