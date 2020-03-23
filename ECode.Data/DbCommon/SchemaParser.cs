using System;
using System.Reflection;

namespace ECode.Data
{
    public static class SchemaParser
    {
        public static EntitySchema GetSchema<TEntity>()
        {
            var entityType = typeof(TEntity);
            var attrs = entityType.GetCustomAttributes(typeof(TableAttribute), false);
            if (attrs == null || attrs.Length == 0)
            { return null; }


            var entitySchema = new EntitySchema();
            entitySchema.TableName = (attrs[0] as TableAttribute).Name ?? entityType.Name;

            foreach (var property in entityType.GetProperties())
            {
                attrs = property.GetCustomAttributes(typeof(ColumnAttribute), false);
                if (attrs == null || attrs.Length == 0)
                { continue; }

                var attr = attrs[0] as ColumnAttribute;
                var columnSchema = new ColumnSchema();
                columnSchema.PropertyName = property.Name;
                columnSchema.ColumnName = attr.Name ?? property.Name;
                columnSchema.DataType = attr.DataType;
                columnSchema.MaxLength = attr.MaxLength;
                columnSchema.IsRequired = attr.IsRequired;
                columnSchema.IsIdentity = attr.IsIdentity;
                columnSchema.DefaultValue = attr.DefaultValue;

                if (attr is PrimaryKeyAttribute)
                { columnSchema.IsPrimaryKey = true; }

                entitySchema.Columns.Add(columnSchema);
            }

            return entitySchema;
        }
    }
}