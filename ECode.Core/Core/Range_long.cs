using System;

namespace ECode.Core
{
    /// <summary>
    /// This class represent 2-point <b>long</b> value range.
    /// </summary>
    public class Range_long
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="value">Start/End value.</param>
        public Range_long(long value)
        {
            this.Start = value;
            this.End = value;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="start">Range start value.</param>
        /// <param name="end">Range end value.</param>
        public Range_long(long start, long end)
        {
            if (start > end)
            { throw new ArgumentException($"Argument '{nameof(end)}' must be >= '{nameof(start)}'."); }

            this.Start = start;
            this.End = end;
        }


        /// <summary>
        /// Gets if the specified value is within range.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <returns>Returns true if specified value is within range, otherwise false.</returns>
        public bool Contains(long value)
        {
            if (value >= this.Start && value <= this.End)
            {
                return true;
            }

            return false;
        }


        #region Properties Implementation

        /// <summary>
        /// Gets range start.
        /// </summary>
        public long Start
        { get; private set; }

        /// <summary>
        /// Gets range end.
        /// </summary>
        public long End
        { get; private set; }

        #endregion
    }
}
