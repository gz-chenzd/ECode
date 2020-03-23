using System.Collections.Specialized;

namespace ECode.Logging
{
    class AppenderInfo
    {
        public string Name
        { get; set; }

        public string TypeClass
        { get; set; }

        public NameValueCollection Options
        { get; set; }
    }
}
