using System;
using ECode.Utility;

namespace ECode.Data
{
    public class PropertyBuilder
    {
        private ColumnSchema        m_pSchema   = null;


        internal PropertyBuilder(ColumnSchema schema)
        {
            m_pSchema = schema;
        }


        public PropertyBuilder HasColumnName(string columnName)
        {
            AssertUtil.ArgumentNotEmpty(columnName, nameof(columnName));

            m_pSchema.ColumnName = columnName.Trim();
            return this;
        }

        public PropertyBuilder HasDataType(DataType dataType)
        {
            m_pSchema.DataType = dataType;
            return this;
        }

        public PropertyBuilder IsRequired()
        {
            m_pSchema.IsRequired = true;
            return this;
        }

        public PropertyBuilder IsIdentity()
        {
            m_pSchema.IsIdentity = true;
            return this;
        }

        public PropertyBuilder HasMaxLength(uint maxLength)
        {
            m_pSchema.MaxLength = maxLength;
            return this;
        }

        public PropertyBuilder HasDefaultValue(object value)
        {
            m_pSchema.DefaultValue = value == null ? DBNull.Value : value;
            return this;
        }
    }
}
