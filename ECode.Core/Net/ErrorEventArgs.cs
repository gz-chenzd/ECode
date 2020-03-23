using System;
using System.Diagnostics;

namespace ECode.Net
{
    public class ErrorEventArgs
    {
        public ErrorEventArgs(Exception exception, StackTrace stackTrace)
        {
            this.Exception = exception;
            this.StackTrace = stackTrace;
        }


        #region Properties Implementaion

        /// <summary>
        /// Occured error's exception.
        /// </summary>
        public Exception Exception
        { get; private set; }

        /// <summary>
        /// Occured error's stacktrace.
        /// </summary>
        public StackTrace StackTrace
        { get; private set; }

        #endregion
    }
}
