using System;

namespace ECode.Core
{
    public class IncompleteDataException : Exception
    {
        public IncompleteDataException(string message)
            : base(message)
        {

        }
    }
}
