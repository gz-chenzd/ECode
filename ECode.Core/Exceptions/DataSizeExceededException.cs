using System;

namespace ECode.Core
{
    /// <summary>
    /// The exception that is thrown when maximum allowed size has exceeded.
    /// </summary>
    public class DataSizeExceededException : Exception
    {
        public DataSizeExceededException()
            : base()
        {

        }

        public DataSizeExceededException(string message)
            : base(message)
        {

        }
    }
}
