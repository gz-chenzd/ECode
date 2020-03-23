using System.Collections.Generic;

namespace ECode.Logging
{
    class LoggerInfo
    {
        public string Name
        { get; set; }

        public Level Level
        { get; set; }

        public string LevelStr
        { get; set; }

        public List<string> Appenders
        { get; set; }
    }
}
