using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using ECode.Utility;

namespace ECode.Core
{
    /// <summary>
    /// This class represent 2-point <b>ipv4</b> value range.
    /// </summary>
    public class Range_ipv4
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="value">Start/End value.</param>
        public Range_ipv4(IPAddress value)
        {
            AssertUtil.ArgumentNotNull(value, nameof(value));

            if (value.AddressFamily != AddressFamily.InterNetwork)
            { throw new ArgumentException($"Argument '{value}' is not valid ipv4 address."); }

            this.Start = value;
            this.End = value;
            this.Mask = 32;

            this.StartInteger = ToInteger(value);
            this.EndInteger = this.StartInteger;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="start">Range start value.</param>
        /// <param name="mask">Range mask value.</param>
        public Range_ipv4(IPAddress start, int mask)
        {
            AssertUtil.ArgumentNotNull(start, nameof(start));

            if (start.AddressFamily != AddressFamily.InterNetwork)
            { throw new ArgumentException($"Argument '{start}' is not valid ipv4 address."); }

            if (mask < 8 || mask > 32)
            { throw new ArgumentException($"Argument '{nameof(mask)}' must be >= 8 and <= 32."); }

            this.Start = start;
            this.Mask = mask;

            this.StartInteger = ToInteger(start);
            this.EndInteger = this.StartInteger | 0xffffffff >> mask;

            this.End = new IPAddress(BitConverter.GetBytes(this.EndInteger).Reverse().ToArray());
        }


        /// <summary>
        /// Gets if the specified value is within range.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <returns>Returns true if specified value is within range, otherwise false.</returns>
        public bool Contains(IPAddress value)
        {
            AssertUtil.ArgumentNotNull(value, nameof(value));

            if (value.AddressFamily != AddressFamily.InterNetwork)
            { throw new ArgumentException($"Argument '{value}' is not valid ipv4 address."); }

            var val = ToInteger(value);
            if (val >= this.StartInteger && val <= this.EndInteger)
            {
                return true;
            }

            return false;
        }


        private uint ToInteger(IPAddress ip)
        {
            var bytes = ip.GetAddressBytes();
            return (uint)(bytes[0] << 24 | bytes[1] << 16 | bytes[2] << 8 | bytes[3]);
        }


        #region Properties Implementation

        /// <summary>
        /// Gets range start.
        /// </summary>
        public IPAddress Start
        { get; private set; }

        /// <summary>
        /// Gets range end.
        /// </summary>
        public IPAddress End
        { get; private set; }

        /// <summary>
        /// Gets mask value.
        /// </summary>
        public int Mask
        { get; private set; }


        private uint StartInteger
        { get; set; }

        private uint EndInteger
        { get; set; }

        #endregion
    }
}
