using System.Collections.Specialized;

namespace ECode.Logging
{
    public interface IAppender
    {
        void Initialize(NameValueCollection options);

        void DoAppend(LogEntry entry);

        void Close();
    }
}