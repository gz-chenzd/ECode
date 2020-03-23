using System;
using ECode.Utility;

namespace ECode.Data
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class TableAttribute : Attribute
    {
        public string Name
        { get; private set; }


        public TableAttribute()
        {

        }

        public TableAttribute(string name)
        {
            AssertUtil.ArgumentNotEmpty(name, nameof(name));

            this.Name = name.Trim();
        }
    }
}