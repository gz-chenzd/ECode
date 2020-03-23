using System;
using ECode.Utility;

namespace ECode.Configuration
{
    public enum ChangedStatus
    {
        /// <summary>
        /// Add or Update
        /// </summary>
        Set,

        /// <summary>
        /// Delete
        /// </summary>
        Delete
    }

    public class ChangedEventArgs : EventArgs
    {
        public string Key { get; }

        public string Value { get; }

        public ChangedStatus Status { get; }


        public ChangedEventArgs(string key, string value, ChangedStatus status)
        {
            AssertUtil.ArgumentNotEmpty(key, nameof(key));

            this.Key = key;
            this.Value = value;
            this.Status = status;
        }
    }

    public delegate void ChangedHandler(ChangedEventArgs e);
}
