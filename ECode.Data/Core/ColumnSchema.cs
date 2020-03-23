
namespace ECode.Data
{
    public class ColumnSchema
    {
        public string ColumnName
        { get; set; }

        public string PropertyName
        { get; set; }

        public DataType DataType
        { get; set; } = DataType.Unknow;

        public bool IsPrimaryKey
        { get; set; }

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
