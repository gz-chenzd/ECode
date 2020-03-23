using System;
using System.Collections.Generic;
using ECode.Utility;

namespace ECode.Logging
{
    public sealed class Logger
    {
        private static bool     WriteEnabled    = true;

        internal static void DisableAllWrites()
        {
            WriteEnabled = false;
        }


        private List<IAppender>     appenders   = new List<IAppender>();


        public string Name
        { get; private set; }

        public Level Level
        { get; internal set; }


        internal Logger(string name, Level level)
        {
            AssertUtil.ArgumentNotEmpty(name, nameof(name));

            this.Name = name.Trim().ToLower();
            this.Level = level;
        }


        internal void SetAppenders(IList<IAppender> appenders)
        {
            this.appenders = new List<IAppender>(appenders);
        }


        private bool IsLevelEnabled(Level level)
        {
            return this.Level <= level;
        }

        private void WriteLog(Level level, string message, object extra, Exception exception)
        {
            if (WriteEnabled && IsLevelEnabled(level))
            {
                var entry = new LogEntry(level, Name, message, extra, exception);
                foreach (var appender in appenders)
                {
                    appender.DoAppend(entry);
                }
            }
        }


        public void Debug(string message)
        {
            Debug(message, null, null);
        }

        public void Debug(string message, object extra)
        {
            Debug(message, extra, null);
        }

        public void Debug(string message, Exception exception)
        {
            Debug(message, null, exception);
        }

        public void Debug(string message, object extra, Exception exception)
        {
            WriteLog(Level.DEBUG, message, extra, exception);
        }


        public void Info(string message)
        {
            Info(message, null, null);
        }

        public void Info(string message, object extra)
        {
            Info(message, extra, null);
        }

        public void Info(string message, Exception exception)
        {
            Info(message, null, exception);
        }

        public void Info(string message, object extra, Exception exception)
        {
            WriteLog(Level.INFO, message, extra, exception);
        }


        public void Warn(string message)
        {
            Warn(message, null, null);
        }

        public void Warn(string message, object extra)
        {
            Warn(message, extra, null);
        }

        public void Warn(string message, Exception exception)
        {
            Warn(message, null, exception);
        }

        public void Warn(string message, object extra, Exception exception)
        {
            WriteLog(Level.WARN, message, extra, exception);
        }


        public void Error(string message)
        {
            Error(message, null, null);
        }

        public void Error(string message, object extra)
        {
            Error(message, extra, null);
        }

        public void Error(string message, Exception exception)
        {
            Error(message, null, exception);
        }

        public void Error(string message, object extra, Exception exception)
        {
            WriteLog(Level.ERROR, message, extra, exception);
        }


        public void Fatal(string message)
        {
            Fatal(message, null, null);
        }

        public void Fatal(string message, object extra)
        {
            Fatal(message, extra, null);
        }

        public void Fatal(string message, Exception exception)
        {
            Fatal(message, null, exception);
        }

        public void Fatal(string message, object extra, Exception exception)
        {
            WriteLog(Level.FATAL, message, extra, exception);
        }
    }
}