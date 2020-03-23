using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.Data.Sqlite;

namespace ECode.Data.SQLite
{
    public class SQLiteTable<TEntity> : DbTable<TEntity>
    {
        internal SQLiteTable(SQLiteSession session, ISchemaManager schemaManager,
                             object shardObject, object partitionObject, IShardStrategy shardStrategy)
            : base(session, schemaManager, shardObject, partitionObject, shardStrategy)
        {

        }


        protected override ExpressionParser GetExpressionParser()
        {
            return new SQLiteExpressionParser();
        }


        protected override string GetLastInsertIdSql()
        {
            return "SELECT LAST_INSERT_ROWID()";
        }

        protected override IDataParameter CreateParameter(string name, ColumnSchema schema)
        {
            var parameter = new SqliteParameter();
            parameter.ParameterName = name;

            return parameter;
        }

        protected override string ParseInsertSql(TEntity entity, Expression<Func<TEntity, bool>> existsCondition, IList<IDataParameter> parameters)
        {
            var sbFields = new StringBuilder();
            var sbValues = new StringBuilder();

            foreach (var column in this.Schema.Columns)
            {
                if (column.DataType == DataType.Timestamp)
                {
                    // Timestamp is automatically set by database.
                    continue;
                }

                if (column.IsIdentity)
                {
                    IdentityProperty = EntityType.GetProperty(column.PropertyName);
                    continue;
                }

                var property = EntityType.GetProperty(column.PropertyName);
                var propertyValue = property.GetValue(entity, null);
                if (propertyValue == null && column.IsRequired)
                { throw new ArgumentException($"Value '{property.Name}' cannot be null."); }

                if (propertyValue == null)
                { continue; }

                var parameter = this.CreateParameter($"@{column.ColumnName}", column);
                parameter.Value = propertyValue;
                parameters.Add(parameter);

                if (sbFields.Length > 0)
                {
                    sbFields.Append(", ");
                    sbValues.Append(", ");
                }

                sbFields.Append($"{LeftKeyWordEscapeChar}{column.ColumnName}{RightKeyWordEscapeChar}");
                sbValues.Append($"@{column.ColumnName}");
            }

            if (sbFields.Length == 0)
            { throw new ArgumentException("Not value to be inserted."); }

            var fromTables = new List<TableInfo>();
            fromTables.Add(new TableInfo(this.TableName, $"{LeftKeyWordEscapeChar}{this.TableName}{RightKeyWordEscapeChar}", typeof(TEntity), this.Schema));
            var whereSql = GetExpressionParser().ParseBooleanConditionExpression(fromTables, parameters, existsCondition);

            if (string.IsNullOrWhiteSpace(whereSql))
            { throw new ArgumentException("Where condition cannot be empty."); }

            return $"INSERT INTO {LeftKeyWordEscapeChar}{this.TableName}{RightKeyWordEscapeChar} ({sbFields.ToString()}) SELECT {sbValues.ToString()} WHERE NOT EXISTS (SELECT {LeftKeyWordEscapeChar}{this.Schema.PrimaryKeys[0].ColumnName}{RightKeyWordEscapeChar} FROM {LeftKeyWordEscapeChar}{this.TableName}{RightKeyWordEscapeChar} WHERE {whereSql})";
        }
    }
}
