using System;
using System.Linq;
using System.Linq.Expressions;
using ECode.Utility;

namespace ECode.Data
{
    public class SchemaBuilder<TEntity>
    {
        private EntitySchema        m_pSchema   = null;


        internal SchemaBuilder(EntitySchema schema)
        {
            m_pSchema = schema;
        }


        public void ToTable(string tableName)
        {
            AssertUtil.ArgumentNotEmpty(tableName, nameof(tableName));

            m_pSchema.TableName = tableName.Trim();
        }

        public void HasKey(Expression<Func<TEntity, object>> keyExpression)
        {
            foreach (var columnSchema in m_pSchema.Columns.Where(t => t.IsPrimaryKey == true))
            {
                columnSchema.IsPrimaryKey = false;
            }

            switch (keyExpression.Body.NodeType)
            {
                case ExpressionType.MemberAccess:
                    {
                        var propertyName = (keyExpression.Body as MemberExpression).Member.Name;
                        var columnSchema = m_pSchema.Columns.FirstOrDefault(t => t.PropertyName == propertyName);
                        if (columnSchema == null)
                        {
                            columnSchema = new ColumnSchema() { PropertyName = propertyName, ColumnName = propertyName };
                            m_pSchema.Columns.Add(columnSchema);
                        }

                        columnSchema.IsPrimaryKey = true;
                    }
                    return;

                case ExpressionType.New:
                    {
                        var newExpression = keyExpression.Body as NewExpression;
                        foreach (var argument in newExpression.Arguments)
                        {
                            var propertyName = (argument as MemberExpression).Member.Name;
                            var columnSchema = m_pSchema.Columns.FirstOrDefault(t => t.PropertyName == propertyName);
                            if (columnSchema == null)
                            {
                                columnSchema = new ColumnSchema() { PropertyName = propertyName, ColumnName = propertyName };
                                m_pSchema.Columns.Add(columnSchema);
                            }

                            columnSchema.IsPrimaryKey = true;
                        }
                    }
                    return;

                default:
                    throw new NotSupportedException($"不支持的Lambda表达式：{keyExpression}");
            }
        }

        public PropertyBuilder Property<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
        {
            if (propertyExpression.Body.NodeType != ExpressionType.MemberAccess)
            {
                throw new NotSupportedException($"不支持的Lambda表达式：{propertyExpression}");
            }


            var propertyName = (propertyExpression.Body as MemberExpression).Member.Name;
            var columnSchema = m_pSchema.Columns.FirstOrDefault(t => t.PropertyName == propertyName);
            if (columnSchema == null)
            {
                columnSchema = new ColumnSchema() { PropertyName = propertyName };
                m_pSchema.Columns.Add(columnSchema);
            }

            return new PropertyBuilder(columnSchema);
        }
    }
}
