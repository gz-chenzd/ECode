using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ECode.Data.SQLServer
{
    public class SQLServerExpressionParser : ExpressionParser
    {
        protected override IDataParameter CreateParameter(string name, object value, DataType dataType = DataType.Unknow)
        {
            var parameter = new SqlParameter();
            parameter.ParameterName = name;
            parameter.Value = value == null ? DBNull.Value : value;

            switch (dataType)
            {
                case DataType.Unknow:
                    break;

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
                    parameter.SqlDbType = SqlDbType.Char;
                    break;

                case DataType.VarChar:
                    parameter.SqlDbType = SqlDbType.VarChar;
                    break;

                case DataType.TinyText:
                case DataType.MediumText:
                case DataType.Text:
                case DataType.LongText:
                    parameter.SqlDbType = SqlDbType.Text;
                    break;

                case DataType.Binary:
                    parameter.SqlDbType = SqlDbType.Binary;
                    break;

                case DataType.VarBinary:
                    parameter.SqlDbType = SqlDbType.VarBinary;
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
                    throw new NotSupportedException($"DataType '{dataType}' isnot supported.");

                default:
                    throw new NotSupportedException($"DataType '{dataType}' isnot supported.");
            }

            return parameter;
        }

        protected override string ParseSqlFunc(string sqlFunc)
        {
            switch (sqlFunc.ToLower())
            {
                case "now":
                    return $"GETDATE()";

                case "date":
                    return "CONVERT(VARCHAR(10), GETDATE(), 120)";

                case "time":
                    return "CONVERT(VARCHAR(8), GETDATE(), 108)";

                case "utcnow":
                    return $"GETUTCDATE()";

                case "utcdate":
                    return "CONVERT(VARCHAR(10), GETUTCDATE(), 120)";

                case "utctime":
                    return "CONVERT(VARCHAR(8), GETUTCDATE(), 108)";

                case "timestamp":
                    return "DATEDIFF(s, '1970-01-01', GETUTCDATE())";

                default:
                    throw new NotSupportedException();
            }
        }

        protected override string NullConvert(string nullValue, object newValue)
        {
            return $"ISNULL({nullValue}, {newValue})";
        }

        protected override string ParseConvert(string convertFunc, string sqlValue)
        {
            switch (convertFunc.ToLower())
            {
                case "toint":
                    return $"CAST({sqlValue} AS INT)";

                case "tolong":
                    return $"CAST({sqlValue} AS BIGINT)";

                case "toshortdate":
                    return $"CONVERT(VARCHAR(10), {sqlValue}, 23)";

                default:
                    throw new NotSupportedException();
            }
        }

        public override string Parse(DbQueryContext queryContext, IList<IDataParameter> commandParameters, out IList<string> selectFields, IList<TableInfo> outQueryTables = null, IList<ParameterExpression> outParameterExpressions = null)
        {
            var sb = new StringBuilder();
            var rowIdColumnName = $"[RowId_{queryContext.FromTables[0].ShortName}]";

            string fromSubQueryParsedSql = null;
            IList<string> innerSelectFields = null;
            Dictionary<string, string> selects = null;
            if (queryContext.SelectExpression == null)
            {
                if (queryContext.FromTables[0].EntitySchema != null)
                { selects = ParseTableInfo(queryContext.FromTables[0]); }
                else
                {
                    var fromTable = queryContext.FromTables[0];
                    fromSubQueryParsedSql = Parse(fromTable.SubQuery, commandParameters, out innerSelectFields);

                    selects = new Dictionary<string, string>();
                    foreach (var selectField in innerSelectFields)
                    {
                        selects[selectField] = $"{fromTable.ShortName}.{selectField}";
                    }
                }

                selectFields = selects.Keys.ToList();
            }
            else
            {
                selects = ParseSelectExpression(queryContext.FromTables, commandParameters, queryContext.SelectExpression);
                selectFields = selects.Keys.ToList();
            }

            var selectKeys = selects.Keys.ToArray();


            var orderByString = string.Empty;
            if (queryContext.OrderByExpressions.Count > 0)
            {
                for (int i = 0; i < queryContext.OrderByExpressions.Count; i++)
                {
                    if (i > 0)
                    { orderByString += ", "; }

                    orderByString += ParseOrderByExpression(queryContext.FromTables, commandParameters, queryContext.OrderByExpressions[i]);
                }
            }


            sb.Append("SELECT ");
            if (queryContext.Distinct)
            { sb.Append("DISTINCT "); }

            if (queryContext.SelectFirst)
            { sb.Append("TOP 1 "); }

            if (queryContext.PagingInfo == null)
            {
                for (int i = 0; i < selectKeys.Length; i++)
                {
                    if (i > 0)
                    { sb.Append(", "); }

                    sb.Append($"{selects[selectKeys[i]]} AS {selectKeys[i]}");
                }
            }
            else
            {
                for (int i = 0; i < selectKeys.Length; i++)
                {
                    if (i > 0)
                    { sb.Append(", "); }

                    sb.Append($"{selectKeys[i]}");
                }

                sb.Append(" FROM (SELECT ");
                for (int i = 0; i < selectKeys.Length; i++)
                {
                    if (i > 0)
                    { sb.Append(", "); }

                    sb.Append($"{selects[selectKeys[i]]} AS {selectKeys[i]}");
                }

                sb.Append($", ROW_NUMBER() OVER(ORDER BY {orderByString}) AS {rowIdColumnName}");
            }

            sb.Append(" FROM ");
            if (queryContext.FromTables[0].SubQuery == null)
            {
                sb.Append($"{LeftKeyWordEscapeChar}{queryContext.FromTables[0].TableName}{RightKeyWordEscapeChar} AS {queryContext.FromTables[0].ShortName} ");
            }
            else
            {
                if (fromSubQueryParsedSql == null)
                { fromSubQueryParsedSql = Parse(queryContext.FromTables[0].SubQuery, commandParameters, out innerSelectFields); }

                sb.Append($"({fromSubQueryParsedSql}) AS {queryContext.FromTables[0].ShortName} ");
            }


            for (int i = 0; i < queryContext.JoinTargets.Count; i++)
            {
                var joinTarget = queryContext.JoinTargets[i];
                sb.Append(joinTarget.Mode == JoinMode.Join ? "JOIN " : "LEFT JOIN ");

                if (joinTarget.TableInfo.SubQuery == null)
                {
                    sb.Append($"{LeftKeyWordEscapeChar}{joinTarget.TableInfo.TableName}{RightKeyWordEscapeChar} AS {joinTarget.TableInfo.ShortName} ");
                }
                else
                {
                    var subQueryParsedSql = Parse(joinTarget.TableInfo.SubQuery, commandParameters, out innerSelectFields);
                    sb.Append($"({subQueryParsedSql}) AS {joinTarget.TableInfo.ShortName} ");
                }

                var onString = ParseBooleanConditionExpression(queryContext.FromTables, commandParameters, joinTarget.OnExpression);
                sb.Append($"ON {onString} ");
            }


            if (queryContext.WhereExpressions.Count > 0)
            {
                sb.Append("WHERE ");
                for (int i = 0; i < queryContext.WhereExpressions.Count; i++)
                {
                    if (i > 0)
                    { sb.Append("AND "); }

                    var whereString = ParseBooleanConditionExpression(queryContext.FromTables, commandParameters, queryContext.WhereExpressions[i], outQueryTables, outParameterExpressions);
                    if (queryContext.WhereExpressions.Count > 1)
                    {
                        sb.Append($"({whereString}) ");
                    }
                    else
                    {
                        sb.Append($"{whereString} ");
                    }
                }
            }


            if (queryContext.GroupByExpression != null)
            {
                var groupByString = ParseGroupByExpression(queryContext.FromTables, commandParameters, queryContext.GroupByExpression);

                sb.Append($"GROUP BY {groupByString} ");
            }


            if (queryContext.HavingExpression != null)
            {
                var havingString = ParseBooleanConditionExpression(queryContext.FromTables, commandParameters, queryContext.HavingExpression);

                sb.Append($"HAVING {havingString} ");
            }


            for (int i = 0; i < queryContext.UnionTargets.Count; i++)
            {
                var unionTarget = queryContext.UnionTargets[i];
                sb.Append(unionTarget.Mode == UnionMode.Union ? "UNION " : "UNION ALL ");

                var subQueryParsedSql = Parse(unionTarget.Query, commandParameters, out innerSelectFields);
                sb.Append($"SELECT {string.Join(", ", innerSelectFields)} FROM ({subQueryParsedSql}) AS {unionTarget.ShortName} ");
            }


            if (queryContext.OrderByExpressions != null && queryContext.PagingInfo == null)
            {
                sb.Append($"ORDER BY {orderByString} ");
            }


            if (queryContext.PagingInfo != null)
            {
                sb.Append($") AS {queryContext.FromTables[0].ShortName}_p ");
                sb.Append($"WHERE {rowIdColumnName} BETWEEN {queryContext.PagingInfo.Offset + 1} AND {queryContext.PagingInfo.Offset + queryContext.PagingInfo.Count} ");
            }

            return sb.ToString();
        }
    }
}
