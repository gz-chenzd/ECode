using System;

namespace ECode.Data
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class PrimaryKeyAttribute : ColumnAttribute
    {
        public PrimaryKeyAttribute()
            : base()
        {

        }

        public PrimaryKeyAttribute(string name)
            : base(name)
        {

        }
    }
}
