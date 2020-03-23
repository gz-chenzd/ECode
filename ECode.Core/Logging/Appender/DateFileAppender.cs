using System;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Text;
using ECode.Core;
using ECode.Utility;

namespace ECode.Logging
{
    public sealed class DateFileAppender : AbstractAppender
    {
        public string BaseDir
        { get; private set; }

        public string Pattern
        { get; private set; }

        public Encoding Encoding
        { get; private set; }

        public int WriteInterval
        { get; private set; } = 200;  // ms

        public int MaxQueueSize
        { get; private set; } = 10000;


        private TimerEx                     timer           = null;
        private bool                        timerRunning    = false;

        private ConcurrentQueue<LogEntry>   queue           = new ConcurrentQueue<LogEntry>();
        private readonly string             outputDir       = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);


        public DateFileAppender()
        {
            BaseDir = "Logs";
            Encoding = Encoding.UTF8;
            Pattern = $"yyyy{Path.DirectorySeparatorChar}yyyy-MM-dd.'txt'";

            timer = new TimerEx(this.WriteInterval);
            timer.Elapsed += WriteTimer_Elapsed;
        }


        private void WriteTimer_Elapsed(object sender, EventArgs e)
        {
            if (timerRunning)
            { return; }

            if (queue.Count <= 0)
            { return; }

            timerRunning = true;
            while (queue.Count > 0)
            {
                try
                {
                    string filename = Path.Combine(outputDir,
                                                   this.BaseDir,
                                                   UtilFunctions.PathFix(DateTime.Now.ToString(Pattern)));

                    string dirname = Path.GetDirectoryName(filename);
                    if (!Directory.Exists(dirname))
                    { Directory.CreateDirectory(dirname); }

                    using (var writer = new StreamWriter(filename, true, this.Encoding, 1048576))  // 1MB
                    {
                        while (queue.Count > 0)
                        {
                            if (!queue.TryDequeue(out LogEntry entry))
                            { break; }

                            if (entry == null)
                            { continue; }

                            if (entry.ToJsonString() != null)
                            { writer.WriteLine(entry.ToJsonString()); }
                        }
                    }
                }
                catch (IOException)
                { return; }
                catch (Exception ex)
                {
                    LogLog.Error("Throw an exception while writing log entries.", ex);
                    return;
                }
                finally
                { timerRunning = false; }
            }
        }


        public override void Initialize(NameValueCollection options)
        {
            if (options == null)
            { return; }

            if (!string.IsNullOrWhiteSpace(options["basedir"]))
            {
                this.BaseDir = UtilFunctions.PathFix(options["basedir"].Trim());
            }

            if (!string.IsNullOrWhiteSpace(options["pattern"]))
            {
                this.Pattern = options["pattern"].Trim();
            }

            if (!string.IsNullOrWhiteSpace(options["encoding"]))
            {
                this.Encoding = Encoding.GetEncoding(options["encoding"].Trim());
            }

            if (!string.IsNullOrWhiteSpace(options["interval"]))
            {
                if (!int.TryParse(options["interval"].Trim(), out int interval))
                { interval = 200; }

                if (interval <= 0)
                { interval = 200; }

                this.WriteInterval = interval;
                timer.Interval = interval;
            }

            if (!string.IsNullOrWhiteSpace(options["max_queue_size"]))
            {
                if (!int.TryParse(options["max_queue_size"].Trim(), out int maxQueueSize))
                { maxQueueSize = 10000; }

                if (maxQueueSize <= 0)
                { maxQueueSize = 10000; }

                this.MaxQueueSize = maxQueueSize;
            }
        }

        public override void DoAppend(LogEntry entry)
        {
            if (this.IsClosed)
            { return; }

            if (queue.Count > this.MaxQueueSize)
            { return; }

            queue.Enqueue(entry);

            if (!timer.Enabled)
            { timer.Enabled = true; }
        }

        public override void Close()
        {
            base.Close();

            WriteTimer_Elapsed(this, EventArgs.Empty);
        }
    }
}