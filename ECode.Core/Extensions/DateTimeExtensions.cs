using System;

namespace ECode.Core
{
    public static class DateTimeExtensions
    {
        internal static readonly DateTime   TIMESTAMP_BASE  = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);


        /// <summary>
        /// Converts to unix timestamp (seconds).
        /// </summary>
        public static long ToUnixTimeStamp(this DateTime dt)
        {
            var utc = dt.ToUniversalTime();
            if (utc < TIMESTAMP_BASE)
            {
                return 0;
            }

            return (long)(utc - TIMESTAMP_BASE).TotalSeconds;
        }

        /// <summary>
        /// Converts to unix timestamp (seconds).
        /// </summary>
        public static long ToUnixTimeStamp(this DateTime? dt)
        {
            if (!dt.HasValue)
            {
                return 0;
            }

            return ToUnixTimeStamp(dt.Value);
        }

        /// <summary>
        /// Converts to unix timestamp (milliseconds).
        /// </summary>
        public static long ToLongUnixTimeStamp(this DateTime dt)
        {
            var utc = dt.ToUniversalTime();
            if (utc < TIMESTAMP_BASE)
            {
                return 0;
            }

            return (long)(utc - TIMESTAMP_BASE).TotalMilliseconds;
        }

        /// <summary>
        /// Converts to unix timestamp (milliseconds).
        /// </summary>
        public static long ToLongUnixTimeStamp(this DateTime? dt)
        {
            if (!dt.HasValue)
            {
                return 0;
            }

            return ToLongUnixTimeStamp(dt.Value);
        }
    }
}
