using System;

namespace ECode.Logging
{
    internal static class LogLog
    {
        public static void Debug(string message, Exception exception = null)
        {
            OnWriteLog(Level.DEBUG, message, exception);
        }

        public static void Info(string message, Exception exception = null)
        {
            OnWriteLog(Level.INFO, message, exception);
        }

        public static void Warn(string message, Exception exception = null)
        {
            OnWriteLog(Level.WARN, message, exception);
        }

        public static void Error(string message, Exception exception = null)
        {
            OnWriteLog(Level.ERROR, message, exception);
        }

        public static void Fatal(string message, Exception exception = null)
        {
            OnWriteLog(Level.FATAL, message, exception);
        }


        private static void OnWriteLog(Level level, string message, Exception exception)
        {
            Console.WriteLine("{0}  {1,-8}  {2,-10} - {3}",
                              DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                              level,
                              "LogLog",
                              message);

            if (exception != null)
            { Console.WriteLine(exception); }
        }
    }
}