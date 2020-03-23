using System;

namespace ECode.Net
{
    public class EventArgs<T> : EventArgs
    {
        public EventArgs(T value)
        {
            this.Value = value;
        }


        #region Properties Implementation

        /// <summary>
        /// Gets event data.
        /// </summary>
        public T Value
        { get; private set; }

        #endregion
    }
}