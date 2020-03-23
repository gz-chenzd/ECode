using System;
using System.Net;
using System.Security.Principal;

namespace ECode.Net
{
    public class LogEntry
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="type">Log entry type.</param>
        /// <param name="id">Log entry ID.</param>
        /// <param name="user">Log entry related user or null if none.</param>
        /// <param name="size">Log entry read/write size in bytes.</param>
        /// <param name="text">Log text.</param>
        /// <param name="extra">Extra messages. Can be null.</param>
        /// <param name="exception">Exception happened. Can be null.</param>
        /// <param name="localEP">Local IP end point.</param>
        /// <param name="remoteEP">Remote IP end point.</param>
        public LogEntry(LogEntryType type, string id, GenericIdentity user, long size, string text, dynamic extra, Exception exception, IPEndPoint localEP, IPEndPoint remoteEP)
        {
            this.EntryType = type;
            this.ID = id;
            this.User = user;
            this.Size = size;
            this.Text = text;
            this.Extra = extra;
            this.Exception = exception;
            this.LocalEndPoint = localEP;
            this.RemoteEndPoint = remoteEP;
        }


        #region Properties Implementation

        /// <summary>
        /// Gets log entry type.
        /// </summary>
        public LogEntryType EntryType
        { get; set; }

        /// <summary>
        /// Gets log entry ID.
        /// </summary>
        public string ID
        { get; set; }

        /// <summary>
        /// Gets time when log entry was created.
        /// </summary>
        public DateTime Time
        { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets log entry related user identity.
        /// </summary>
        public GenericIdentity User
        { get; set; }

        /// <summary>
        /// Gets how much data was readed or written, depends on <b>EntryType</b>.
        /// </summary>
        public long Size
        { get; set; }

        /// <summary>
        /// Gets describing text.
        /// </summary>
        public string Text
        { get; set; }

        /// <summary>
        /// Gest extra data.
        /// </summary>
        public dynamic Extra
        { get; set; }

        /// <summary>
        /// Gets exception happened. This property is available only if LogEntryType.Exception.
        /// </summary>
        public Exception Exception
        { get; set; }

        /// <summary>
        /// Gets local IP end point. Value null means no local end point.
        /// </summary>
        public IPEndPoint LocalEndPoint
        { get; set; }

        /// <summary>
        /// Gets remote IP end point. Value null means no remote end point.
        /// </summary>
        public IPEndPoint RemoteEndPoint
        { get; set; }

        #endregion
    }
}