using System;
using ECode.Json;

namespace ECode.Logging
{
    public sealed class ConsoleAppender : AbstractAppender
    {
        const string TIME_FORMATTER = "yyyy-MM-dd HH:mm:ss.fff";


        public override void DoAppend(LogEntry entry)
        {
            if (this.IsClosed)
            { return; }

            try
            {
                Console.WriteLine("{0}  {1,-8}  {2,-10} - {3}",
                                  entry.Time.ToString(TIME_FORMATTER),
                                  entry.Level,
                                  entry.Logger,
                                  entry.Message);

                if (entry.Extra != null)
                { Console.WriteLine(JsonUtil.Serialize(entry.Extra)); }

                if (entry.Exception != null)
                { Console.WriteLine(entry.Exception); }
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0}  {1,-8}  {2,-10} - {3}",
                                  entry.Time.ToString(TIME_FORMATTER),
                                  entry.Level,
                                  entry.Logger,
                                  "【日志输出异常】");
                Console.WriteLine(ex);
            }
        }
    }
}