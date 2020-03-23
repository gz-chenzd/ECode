using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using ECode.Utility;

namespace ECode.Cryptography
{
    class PemReader
    {
        public const string             BEGIN_STRING    = "-----BEGIN ";
        public const string             END_STRING      = "-----END ";

        static readonly CompareInfo     COMPARER        = CultureInfo.InvariantCulture.CompareInfo;


        private static int IndexOf(string source, string value)
        {
            return COMPARER.IndexOf(source, value, CompareOptions.Ordinal);
        }

        private static bool StartsWith(string source, string prefix)
        {
            return COMPARER.IsPrefix(source, prefix, CompareOptions.Ordinal);
        }


        public TextReader Reader
        { get; private set; }


        public PemReader(TextReader reader)
        {
            AssertUtil.ArgumentNotNull(reader, nameof(reader));

            this.Reader = reader;
        }


        public PemObject ReadPemObject()
        {
            var line = this.Reader.ReadLine();
            if (line != null && StartsWith(line, BEGIN_STRING))
            {
                line = line.Substring(BEGIN_STRING.Length);

                var index = line.IndexOf('-');
                var type = line.Substring(0, index);

                if (index > 0)
                { return LoadObject(type); }
            }

            return null;
        }

        private PemObject LoadObject(string type)
        {
            var headers = new ArrayList();
            var buffer = new StringBuilder();
            var endMarker = END_STRING + type;

            string line = null;
            while ((line = this.Reader.ReadLine()) != null
                && IndexOf(line, endMarker) == -1)
            {
                int colonPos = line.IndexOf(':');

                if (colonPos == -1)
                { buffer.Append(line.Trim()); }
                else
                {
                    var fieldName = line.Substring(0, colonPos).Trim();
                    if (StartsWith(fieldName, "X-"))
                    {
                        fieldName = fieldName.Substring(2);
                    }

                    var fieldValue = line.Substring(colonPos + 1).Trim();

                    headers.Add(new PemHeader(fieldName, fieldValue));
                }
            }

            if (line == null)
            { throw new FormatException($"Cannot find end marker '{endMarker}'."); }

            if (buffer.Length % 4 != 0)
            { throw new FormatException("base64 data appears to be truncated"); }

            return new PemObject(type, headers, Convert.FromBase64String(buffer.ToString()));
        }
    }
}
