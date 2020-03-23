using System;
using ECode.Utility;

namespace ECode.Data
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ColumnAttribute : Attribute
    {
        public ColumnAttribute()
        {

        }

        public ColumnAttribute(string name)
        {
            AssertUtil.ArgumentNotEmpty(name, nameof(name));

            this.Name = name.Trim();
        }


        public string Name
        { get; private set; }

        public DataType DataType
        { get; set; } = DataType.Unknow;

        public bool IsRequired
        { get; set; }

        public bool IsIdentity
        { get; set; }

        public uint MaxLength
        { get; set; }

        public object DefaultValue
        { get; set; }
    }
}
