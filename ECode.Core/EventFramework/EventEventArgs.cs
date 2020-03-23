using System;
using ECode.Utility;

namespace ECode.EventFramework
{
    public sealed class EventEventArgs : EventArgs
    {
        /// <summary>
        /// Event name.
        /// </summary>
        public string Name
        { get; private set; }

        /// <summary>
        /// Event ref data.
        /// </summary>
        public object Data
        { get; private set; }


        public EventEventArgs(string name, object data)
        {
            AssertUtil.ArgumentNotEmpty(name, nameof(name));

            this.Name = name.Trim();
            this.Data = data;
        }
    }
}