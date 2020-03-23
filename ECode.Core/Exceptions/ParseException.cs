using System;

namespace ECode.Core
{
    public class ParseException : Exception
    {
        public ParseException(string message)
            : base(message)
        {

        }
    }
}
