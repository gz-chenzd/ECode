using System;

namespace ECode.Data
{
    public enum CommandType
    {
        Connect,

        Count,

        First,

        List,

        Insert,

        Batch,

        Update,

        Delete,

        Transaction,

        Commit,

        Rollback
    }

    public class LogEntry
    {
        public string SessionID
        { get; set; }

        public string TransactionID
        { get; set; }

        public string Server
        { get; set; }

        public string Database
        { get; set; }

        public string TableName
        { get; set; }

        public CommandType CommandType
        { get; set; }

        public string CommandText
        { get; set; }

        public int AffectedRows
        { get; set; } = -1;

        public string Message
        { get; set; } = "OK";

        public Exception Exception
        { get; set; }

        public int ParseElapsed
        { get; set; } = -1;

        public int TotalElapsed
        { get; set; } = -1;
    }


    public delegate void WriteLogHandler(LogEntry entry);
}