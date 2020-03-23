using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using MySql.Data.MySqlClient;

namespace ECode.Data.MySQL
{
    public class MySQLTable<TEntity> : DbTable<TEntity>
    {
        protected override string LeftKeyWordEscapeChar
        { get { return "`"; } }

        protected override string RightKeyWordEscapeChar
        { get { return "`"; } }


        internal MySQLTable(MySQLSession session, ISchemaManager schemaManager,
                            object shardObject, object partitionObject, IShardStrategy shardStrategy)
            : base(session, schemaManager, shardObject, partitionObject, shardStrategy)
        {

        }


        protected override ExpressionParser GetExpressionParser()
        {
            return new MySQLExpressionParser();
        }


        protected override string GetLastInsertIdSql()
        {
            return "SELECT LAST_INSERT_ID()";
        }

        protected override IDataParameter CreateParameter(string name, ColumnSchema schema)
        {
            var parameter = new MySqlParameter();
            parameter.ParameterName = name;

            if (schema != null && schema.DataType != DataType.Unknow)
            {
                switch (schema.DataType)
                {
                    case DataType.Bool:
                        parameter.MySqlDbType = MySqlDbType.Bit;
                        break;

                    case DataType.Byte:
                        parameter.MySqlDbType = MySqlDbType.Byte;
                        break;

                    case DataType.Int16:
                        parameter.MySqlDbType = MySqlDbType.Int16;
                        break;

                    case DataType.Int32:
                        parameter.MySqlDbType = MySqlDbType.Int32;
                        break;

                    case DataType.Int64:
                        parameter.MySqlDbType = MySqlDbType.Int64;
                        break;

                    case DataType.UInt16:
                        parameter.MySqlDbType = MySqlDbType.UInt16;
                        break;

                    case DataType.UInt32:
                        parameter.MySqlDbType = MySqlDbType.UInt32;
                        break;

                    case DataType.UInt64:
                        parameter.MySqlDbType = MySqlDbType.UInt64;
                        break;

                    case DataType.Float:
                        parameter.MySqlDbType = MySqlDbType.Float;
                        break;

                    case DataType.Double:
                        parameter.MySqlDbType = MySqlDbType.Double;
                        break;

                    case DataType.Decimal:
                        parameter.MySqlDbType = MySqlDbType.Decimal;
                        break;

                    case DataType.Char:
                        if (schema.MaxLength > 0)
                        {
                            parameter.MySqlDbType = MySqlDbType.String;
                            parameter.Size = (int)schema.MaxLength;
                        }
                        break;

                    case DataType.VarChar:
                        if (schema.MaxLength > 0)
                        {
                            parameter.MySqlDbType = MySqlDbType.VarChar;
                            parameter.Size = (int)schema.MaxLength;
                        }
                        break;

                    case DataType.TinyText:
                        parameter.MySqlDbType = MySqlDbType.TinyText;
                        break;

                    case DataType.MediumText:
                        parameter.MySqlDbType = MySqlDbType.MediumText;
                        break;

                    case DataType.Text:
                        parameter.MySqlDbType = MySqlDbType.Text;
                        break;

                    case DataType.LongText:
                        parameter.MySqlDbType = MySqlDbType.LongText;
                        break;

                    case DataType.Binary:
                        if (schema.MaxLength > 0)
                        {
                            parameter.MySqlDbType = MySqlDbType.Binary;
                            parameter.Size = (int)schema.MaxLength;
                        }
                        break;

                    case DataType.VarBinary:
                        if (schema.MaxLength > 0)
                        {
                            parameter.MySqlDbType = MySqlDbType.VarBinary;
                            parameter.Size = (int)schema.MaxLength;
                        }
                        break;

                    case DataType.TinyBlob:
                        parameter.MySqlDbType = MySqlDbType.TinyBlob;
                        break;

                    case DataType.MediumBlob:
                        parameter.MySqlDbType = MySqlDbType.MediumBlob;
                        break;

                    case DataType.Blob:
                        parameter.MySqlDbType = MySqlDbType.Blob;
                        break;

                    case DataType.LongBlob:
                        parameter.MySqlDbType = MySqlDbType.LongBlob;
                        break;

                    case DataType.DateTime:
                        parameter.MySqlDbType = MySqlDbType.DateTime;
                        break;

                    case DataType.Timestamp:
                        parameter.MySqlDbType = MySqlDbType.Timestamp;
                        break;

                    case DataType.Guid:
                        parameter.MySqlDbType = MySqlDbType.Guid;
                        break;

                    case DataType.Set:
                        parameter.MySqlDbType = MySqlDbType.Set;
                        break;

                    case DataType.Enum:
                        parameter.MySqlDbType = MySqlDbType.Enum;
                        break;

                    case DataType.Json:
                        parameter.MySqlDbType = MySqlDbType.JSON;
                        break;

                    default:
                        throw new NotSupportedException($"DataType '{schema.DataType}' cannot be supported.");
                }
            }

            return parameter;
        }

        protected override string ParseInsertSql(TEntity entity, Expression<Func<TEntity, bool>> existsCondition, IList<IDataParameter> parameters)
        {
            var sbFields = new StringBuilder();
            var sbValues = new StringBuilder();

            foreach (var columnSchema in this.Schema.Columns)
            {
                if (columnSchema.DataType == DataType.Timestamp)
                {
                    // Timestamp is automatically set by database.
                    continue;
                }

                if (columnSchema.IsIdentity)
                {
                    IdentityProperty = EntityType.GetProperty(columnSchema.PropertyName);
                    continue;
                }

                var property = EntityType.GetProperty(columnSchema.PropertyName);
                var propertyValue = property.GetValue(entity, null);
                if (propertyValue == null && columnSchema.IsRequired)
                { throw new ArgumentException($"Value '{property.Name}' cannot be null."); }

                if (propertyValue == null)
                { continue; }

                var parameter = this.CreateParameter($"@{columnSchema.ColumnName}", columnSchema);
                parameter.Value = propertyValue;
                parameters.Add(parameter);

                if (sbFields.Length > 0)
                {
                    sbFields.Append(", ");
                    sbValues.Append(", ");
                }

                sbFields.Append($"{LeftKeyWordEscapeChar}{columnSchema.ColumnName}{RightKeyWordEscapeChar}");
                sbValues.Append($"@{columnSchema.ColumnName}");
            }

            if (sbFields.Length == 0)
            { throw new ArgumentException("Not value to be inserted."); }

            var fromTables = new List<TableInfo>();
            fromTables.Add(new TableInfo(this.TableName, $"{LeftKeyWordEscapeChar}{this.TableName}{RightKeyWordEscapeChar}", typeof(TEntity), this.Schema));
            var whereSql = GetExpressionParser().ParseBooleanConditionExpression(fromTables, parameters, existsCondition);

            if (string.IsNullOrWhiteSpace(whereSql))
            { throw new ArgumentException("Where condition cannot be empty."); }

            return $"INSERT INTO {LeftKeyWordEscapeChar}{this.TableName}{RightKeyWordEscapeChar} ({sbFields.ToString()}) SELECT {sbValues.ToString()} FROM DUAL WHERE NOT EXISTS (SELECT {LeftKeyWordEscapeChar}{this.Schema.PrimaryKeys[0].ColumnName}{RightKeyWordEscapeChar} FROM {LeftKeyWordEscapeChar}{this.TableName}{RightKeyWordEscapeChar} WHERE {whereSql})";
        }
    }
}
