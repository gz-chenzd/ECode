using System;

namespace ECode.Core
{
    /// <summary>
    /// The exception that is thrown when maximum line allowed size has exceeded.
    /// </summary>
    public class LineSizeExceededException : Exception
    {
        public LineSizeExceededException()
            : base()
        {

        }

        public LineSizeExceededException(string message)
            : base(message)
        {

        }
    }
}
