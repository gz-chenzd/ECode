
namespace ECode.Cryptography
{
    class PemHeader
    {
        public string Name
        { get; private set; }

        public string Value
        { get; private set; }


        public PemHeader(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }


        private int GetHashCode(string str)
        {
            if (str == null)
            { return 1; }

            return str.GetHashCode();
        }

        public override int GetHashCode()
        {
            return GetHashCode(this.Name) + 31 * GetHashCode(this.Value);
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            { return true; }

            if (!(obj is PemHeader))
            { return false; }

            PemHeader other = (PemHeader)obj;

            return object.Equals(this.Name, other.Name)
                && object.Equals(this.Value, other.Value);
        }
    }
}