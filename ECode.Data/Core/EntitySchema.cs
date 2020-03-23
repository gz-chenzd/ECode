using System.Collections.Generic;
using System.Linq;

namespace ECode.Data
{
    public class EntitySchema
    {
        private ColumnSchema[]     m_PrimaryKeys     = null;


        public string TableName
        { get; set; }

        public IList<ColumnSchema> Columns
        { get; private set; } = new List<ColumnSchema>();


        public ColumnSchema[] PrimaryKeys
        {
            get
            {
                if (m_PrimaryKeys == null)
                {
                    m_PrimaryKeys = this.Columns.Where(t => t.IsPrimaryKey == true).ToArray();
                }

                return m_PrimaryKeys;
            }
        }
    }
}