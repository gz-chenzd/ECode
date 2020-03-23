using System.Collections;

namespace ECode.Cryptography
{
    class PemObject
    {
        public string Type
        { get; private set; }

        public IList Headers
        { get; private set; }

        public byte[] Content
        { get; private set; }


        public PemObject(string type, byte[] content)
            : this(type, new ArrayList(), content)
        { }

        public PemObject(string type, IList headers, byte[] content)
        {
            this.Type = type;
            this.Headers = new ArrayList(headers);
            this.Content = content;
        }
    }
}