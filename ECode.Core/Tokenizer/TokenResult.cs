using System;

namespace ECode.Tokenizer
{
    public class TokenResult
    {
        public int Offset
        { get; private set; }

        public int Length
        { get; private set; }

        public string Token
        { get; private set; }


        public TokenResult(string token, int offset, int length)
        {
            if (string.IsNullOrWhiteSpace(token))
            { throw new ArgumentNullException(nameof(token)); }

            if (offset < 0)
            { throw new ArgumentException($"Argument '{nameof(offset)}' value must be >= 0", nameof(offset)); }

            if (length <= 0)
            { throw new ArgumentException($"Argument '{nameof(length)}' value must be > 0", nameof(length)); }


            this.Token = token;
            this.Offset = offset;
            this.Length = length;
        }
    }
}
