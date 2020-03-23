using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq.Expressions;

namespace ECode.Data.SQLServer
{
    public class SQLServerTable<TEntity> : DbTable<TEntity>
    {
        internal SQLServerTable(SQLServerSession session, ISchemaManager schemaManager,
                                object shardObject, object partitionObject, IShardStrategy shardStrategy)
            : base(session, schemaManager, shardObject, partitionObject, shardStrategy)
        {

        }


        protected override ExpressionParser GetExpressionParser()
        {
            return new SQLServerExpressionParser();
        }


        protected override string GetLastInsertIdSql()
        {
            return "SELECT @@IDENTITY";
        }

        protected override IDataParameter CreateParameter(string name, ColumnSchema schema)
        {
            var parameter = new SqlParameter();
            parameter.ParameterName = name;

            if (schema != null && schema.DataType != DataType.Unknow)
            {
                switch (schema.DataType)
                {
                    case DataType.Bool:
                        parameter.SqlDbType = SqlDbType.Bit;
                        break;

                    case DataType.Byte:
                        parameter.SqlDbType = SqlDbType.TinyInt;
                        break;

                    case DataType.Int16:
                        parameter.SqlDbType = SqlDbType.SmallInt;
                        break;

                    case DataType.Int32:
                        parameter.SqlDbType = SqlDbType.Int;
                        break;

                    case DataType.Int64:
                        parameter.SqlDbType = SqlDbType.BigInt;
                        break;

                    case DataType.UInt16:
                        parameter.DbType = DbType.UInt16;
                        break;

                    case DataType.UInt32:
                        parameter.DbType = DbType.UInt32;
                        break;

                    case DataType.UInt64:
                        parameter.DbType = DbType.UInt64;
                        break;

                    case DataType.Float:
                        parameter.SqlDbType = SqlDbType.Real;
                        break;

                    case DataType.Double:
                        parameter.SqlDbType = SqlDbType.Float;
                        break;

                    case DataType.Decimal:
                        parameter.SqlDbType = SqlDbType.Decimal;
                        break;

                    case DataType.Char:
                        if (schema.MaxLength > 0)
                        {
                            parameter.SqlDbType = SqlDbType.Char;
                            parameter.Size = (int)schema.MaxLength;
                        }
                        break;

                    case DataType.VarChar:
                        if (schema.MaxLength > 0)
                        {
                            parameter.SqlDbType = SqlDbType.VarChar;
                            parameter.Size = (int)schema.MaxLength;
                        }
                        break;

                    case DataType.TinyText:
                    case DataType.MediumText:
                    case DataType.Text:
                    case DataType.LongText:
                        parameter.SqlDbType = SqlDbType.Text;
                        break;

                    case DataType.Binary:
                        if (schema.MaxLength > 0)
                        {
                            parameter.SqlDbType = SqlDbType.Binary;
                            parameter.Size = (int)schema.MaxLength;
                        }
                        break;

                    case DataType.VarBinary:
                        if (schema.MaxLength > 0)
                        {
                            parameter.SqlDbType = SqlDbType.VarBinary;
                            parameter.Size = (int)schema.MaxLength;
                        }
                        break;

                    case DataType.TinyBlob:
                    case DataType.MediumBlob:
                    case DataType.Blob:
                    case DataType.LongBlob:
                        parameter.SqlDbType = SqlDbType.Image;
                        break;

                    case DataType.DateTime:
                        parameter.SqlDbType = SqlDbType.DateTime;
                        break;

                    case DataType.Timestamp:
                        parameter.SqlDbType = SqlDbType.Timestamp;
                        break;

                    case DataType.Guid:
                        parameter.DbType = DbType.Guid;
                        break;

                    case DataType.Set:
                    case DataType.Enum:
                    case DataType.Json:
                        throw new NotSupportedException($"DataType '{schema.DataType}' isnot supported.");

                    default:
                        throw new NotSupportedException($"DataType '{schema.DataType}' isnot supported.");
                }
            }

            return parameter;
        }

        protected override string ParseInsertSql(TEntity entity, Expression<Func<TEntity, bool>> existsCondition, IList<IDataParameter> parameters)
        {
            var insertCommandSql = base.ParseInsertSql(entity, parameters);

            var fromTables = new List<TableInfo>();
            fromTables.Add(new TableInfo(this.TableName, $"{LeftKeyWordEscapeChar}{this.TableName}{RightKeyWordEscapeChar}", typeof(TEntity), this.Schema));
            var whereSql = GetExpressionParser().ParseBooleanConditionExpression(fromTables, parameters, existsCondition);

            if (string.IsNullOrWhiteSpace(whereSql))
            { throw new ArgumentException("Where condition cannot be empty."); }

            return $"IF NOT EXISTS (SELECT {LeftKeyWordEscapeChar}{this.Schema.PrimaryKeys[0].ColumnName}{RightKeyWordEscapeChar} FROM {LeftKeyWordEscapeChar}{this.TableName}{RightKeyWordEscapeChar} WHERE {whereSql}) {insertCommandSql}";
        }


        internal string ParseSelectSqlForUnitTest(ExpressionParser parser, IList<IDataParameter> parameters)
        {
            return base.ParseSqlForUnitTest(parser, parameters);
        }

        internal string ParseInsertSqlForUnitTest(TEntity entity, IList<IDataParameter> parameters)
        {
            return base.ParseInsertSql(entity, parameters);
        }

        internal string ParseInsertSqlForUnitTest(TEntity entity, Expression<Func<TEntity, bool>> notExistsCondition, IList<IDataParameter> parameters)
        {
            return this.ParseInsertSql(entity, notExistsCondition, parameters);
        }

        internal string ParseUpdateSqlForUnitTest(object value, Expression<Func<TEntity, bool>> where, IList<IDataParameter> parameters)
        {
            return base.ParseUpdateSql(value, where, parameters);
        }

        internal string ParseUpdateSqlForUnitTest(Hashtable value, Expression<Func<TEntity, bool>> where, IList<IDataParameter> parameters)
        {
            return base.ParseUpdateSql(value, where, parameters);
        }

        internal string ParseUpdateSqlForUnitTest(Expression<Func<TEntity, object>> value, Expression<Func<TEntity, bool>> where, IList<IDataParameter> parameters)
        {
            return base.ParseUpdateSql(value, where, parameters);
        }

        internal string ParseDeleteSqlForUnitTest(Expression<Func<TEntity, bool>> where, IList<IDataParameter> parameters)
        {
            return base.ParseDeleteSql(where, parameters);
        }
    }
}
