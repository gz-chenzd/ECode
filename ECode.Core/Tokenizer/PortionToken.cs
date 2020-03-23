using System;

namespace ECode.Tokenizer
{
    public class PortionToken
    {
        public int Offset
        { get; private set; }

        public int Length
        { get; private set; }

        public string Portion
        { get; private set; }


        public PortionToken(string portion, int offset, int length)
        {
            if (string.IsNullOrWhiteSpace(portion))
            { throw new ArgumentNullException(nameof(portion)); }

            if (offset < 0)
            { throw new ArgumentException($"Argument '{nameof(offset)}' value must be >= 0", nameof(offset)); }

            if (length <= 0)
            { throw new ArgumentException($"Argument '{nameof(length)}' value must be > 0", nameof(length)); }


            this.Portion = portion;
            this.Offset = offset;
            this.Length = length;
        }
    }
}
