using System;
using System.Collections.Generic;
using System.Linq;
using ECode.Utility;

namespace ECode.EventFramework
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class EventNameAttribute : Attribute
    {
        public string[] EventNames
        { get; private set; }


        public EventNameAttribute(string eventName)
        {
            AssertUtil.ArgumentNotEmpty(eventName, nameof(eventName));

            this.EventNames = new string[] { eventName.Trim() };
        }

        public EventNameAttribute(string[] eventNames)
        {
            AssertUtil.ArgumentNotEmpty(eventNames, nameof(eventNames));

            var eventList = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            foreach (string eventName in eventNames)
            {
                if (string.IsNullOrWhiteSpace(eventName))
                { throw new ArgumentException($"Argument '{nameof(eventNames)}' contains null or empty item."); }

                if (eventList.Contains(eventName.Trim()))
                { throw new ArgumentException($"Argument '{nameof(eventNames)}' contains duplicate items (ignore case)."); }

                eventList.Add(eventName.Trim());
            }

            this.EventNames = eventList.ToArray();
        }
    }
}