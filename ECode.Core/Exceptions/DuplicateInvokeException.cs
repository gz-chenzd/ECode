using System;

namespace ECode.Core
{
    public class DuplicateInvokeException : Exception
    {
        public DuplicateInvokeException()
            : base()
        {

        }

        public DuplicateInvokeException(string message)
            : base(message)
        {

        }
    }
}
