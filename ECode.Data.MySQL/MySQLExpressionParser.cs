using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using MySql.Data.MySqlClient;

namespace ECode.Data.MySQL
{
    public class MySQLExpressionParser : ExpressionParser
    {
        protected override string LeftKeyWordEscapeChar
        { get { return "`"; } }

        protected override string RightKeyWordEscapeChar
        { get { return "`"; } }


        protected override string LikeEscapeSuffix()
        {
            return @"ESCAPE '\\' ";
        }


        protected override IDataParameter CreateParameter(string name, object value, DataType dataType = DataType.Unknow)
        {
            var parameter = new MySqlParameter();
            parameter.ParameterName = name;
            parameter.Value = value == null ? DBNull.Value : value;

            switch (dataType)
            {
                case DataType.Unknow:
                    break;

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
                    parameter.MySqlDbType = MySqlDbType.String;
                    break;

                case DataType.VarChar:
                    parameter.MySqlDbType = MySqlDbType.VarChar;
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
                    parameter.MySqlDbType = MySqlDbType.Binary;
                    break;

                case DataType.VarBinary:
                    parameter.MySqlDbType = MySqlDbType.VarBinary;
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
                    throw new NotSupportedException($"DataType '{dataType}' cannot be supported.");
            }

            return parameter;
        }

        protected override string ParseSqlFunc(string sqlFunc)
        {
            switch (sqlFunc.ToLower())
            {
                case "now":
                    return $"CURRENT_TIMESTAMP()";

                case "date":
                    return "CURRENT_DATE()";

                case "time":
                    return "CURRENT_TIME()";

                case "utcnow":
                    return $"UTC_TIMESTAMP()";

                case "utcdate":
                    return "UTC_DATE()";

                case "utctime":
                    return "UTC_TIME()";

                case "timestamp":
                    return "UNIX_TIMESTAMP()";

                default:
                    throw new NotSupportedException();
            }
        }

        protected override string NullConvert(string nullValue, object newValue)
        {
            return $"IFNULL({nullValue}, {newValue})";
        }

        protected override string ParseConvert(string convertFunc, string sqlValue)
        {
            switch (convertFunc.ToLower())
            {
                case "toint":
                    return $"CAST({sqlValue} AS SIGNED)";

                case "tolong":
                    return $"CAST({sqlValue} AS SIGNED)";

                case "toshortdate":
                    return $"DATE_FORMAT({sqlValue}, '%Y-%m-%d')";

                default:
                    throw new NotSupportedException();
            }
        }

        public override string Parse(DbQueryContext queryContext, IList<IDataParameter> commandParameters, out IList<string> selectFields, IList<TableInfo> outQueryTables = null, IList<ParameterExpression> outParameterExpressions = null)
        {
            var sb = new StringBuilder();

            sb.Append("SELECT ");
            if (queryContext.Distinct)
            { sb.Append("DISTINCT "); }

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
            for (int i = 0; i < selectKeys.Length; i++)
            {
                if (i > 0)
                { sb.Append(", "); }

                sb.Append($"{selects[selectKeys[i]]} AS {selectKeys[i]}");
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


            if (queryContext.OrderByExpressions.Count > 0)
            {
                sb.Append("ORDER BY ");
                for (int i = 0; i < queryContext.OrderByExpressions.Count; i++)
                {
                    if (i > 0)
                    { sb.Append(", "); }

                    sb.Append(ParseOrderByExpression(queryContext.FromTables, commandParameters, queryContext.OrderByExpressions[i]));
                }

                sb.Append(" ");
            }


            if (queryContext.SelectFirst)
            {
                sb.Append("LIMIT 1");
            }
            else if (queryContext.PagingInfo != null)
            {
                sb.Append($"LIMIT {queryContext.PagingInfo.Offset}, {queryContext.PagingInfo.Count}");
            }


            return sb.ToString();
        }
    }
}
