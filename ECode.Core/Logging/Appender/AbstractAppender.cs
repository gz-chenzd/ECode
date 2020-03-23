using System.Collections.Specialized;

namespace ECode.Logging
{
    public abstract class AbstractAppender : IAppender
    {
        protected bool IsClosed { get; set; }


        public virtual void Initialize(NameValueCollection options)
        {

        }

        public abstract void DoAppend(LogEntry entry);

        public virtual void Close()
        {
            this.IsClosed = true;
        }
    }
}