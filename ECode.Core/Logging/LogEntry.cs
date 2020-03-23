using System;
using ECode.Configuration;
using ECode.Core;
using ECode.Json;

namespace ECode.Logging
{
    public sealed class LogEntry
    {
        static ulong                SerialNo            = 0;

        static readonly string      TIME_FORMATTER      = "yyyy-MM-ddTHH:mm:ss.fffzzz";
        static readonly string      APP_NAME            = ConfigurationManager.Get("app_name");
        static readonly string      APP_VERSION         = ConfigurationManager.Get("app_version");


        private string  json    = null;


        public ulong ID
        { get; private set; } = ++SerialNo;

        public DateTime Time
        { get; private set; } = DateTime.Now;

        public string AppName
        { get; private set; } = APP_NAME;

        public string AppVersion
        { get; private set; } = APP_VERSION;

        public Level Level
        { get; private set; }

        public string Logger
        { get; private set; }

        public string Message
        { get; private set; }

        public object Extra
        { get; private set; }

        public Exception Exception
        { get; private set; }


        internal LogEntry(Level level, string logger, string message, object extra, Exception exception)
        {
            this.Level = level;
            this.Logger = logger;
            this.Message = message;
            this.Extra = extra;
            this.Exception = exception;
        }


        public string ToJsonString()
        {
            if (json != null)
            { return json; }

            lock (this)
            {
                if (json != null)
                { return json; }

                try
                {
                    json = JsonUtil.Serialize(new
                    {
                        ID = this.ID,
                        Time = this.Time.ToString(TIME_FORMATTER),
                        TimeStamp = this.Time.ToLongUnixTimeStamp(),
                        AppName = this.AppName,
                        AppVersion = this.AppVersion,
                        Level = this.Level.ToString(),
                        LevelInt = (int)this.Level,
                        Logger = this.Logger,
                        Message = this.Message,
                        Extra = this.Extra,
                        Exception = GetExceptionMessage(this.Exception),
                        StackTrace = GetExceptionStackTrace(this.Exception)
                    });
                }
                catch (Exception ex)
                {
                    try
                    {
                        json = JsonUtil.Serialize(new
                        {
                            ID = this.ID,
                            Time = this.Time.ToString(TIME_FORMATTER),
                            TimeStamp = this.Time.ToLongUnixTimeStamp(),
                            AppName = this.AppName,
                            AppVersion = this.AppVersion,
                            Level = this.Level.ToString(),
                            LevelInt = (int)this.Level,
                            Logger = this.Logger,
                            Message = this.Message,
                            Extra = this.Extra == null ? null : "[IGNORED]",  // Probably this data error.
                            Exception = $"[SOURCE MSG: {GetExceptionMessage(this.Exception)}]\r\n[JSON ERROR: {GetExceptionMessage(ex)}]",
                            StackTrace = $"[SOURCE TRACE: {GetExceptionStackTrace(this.Exception)}]\r\n[JSON TRACE: {GetExceptionStackTrace(ex)}]"
                        });
                    }
                    catch
                    { return null; }
                }

                return json;
            }
        }

        private string GetExceptionMessage(Exception exception)
        {
            if (exception == null)
            { return null; }

            while (exception.InnerException != null)
            {
                exception = exception.InnerException;
            }

            return exception.Message;
        }

        private string GetExceptionStackTrace(Exception exception)
        {
            if (exception == null)
            { return null; }

            return exception.StackTrace;
        }
    }
}