using System;

namespace ECode.Core
{
    /// <summary>
    /// This class represent 2-point <b>int</b> value range.
    /// </summary>
    public class Range_int
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="value">Start/End value.</param>
        public Range_int(int value)
        {
            this.Start = value;
            this.End = value;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="start">Range start value.</param>
        /// <param name="end">Range end value.</param>
        public Range_int(int start, int end)
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
        public bool Contains(int value)
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
        public int Start
        { get; private set; }

        /// <summary>
        /// Gets range end.
        /// </summary>
        public int End
        { get; private set; }

        #endregion
    }
}
