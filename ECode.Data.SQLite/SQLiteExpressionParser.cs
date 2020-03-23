using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Microsoft.Data.Sqlite;

namespace ECode.Data.SQLite
{
    public class SQLiteExpressionParser : ExpressionParser
    {
        protected override IDataParameter CreateParameter(string name, object value, DataType dataType = DataType.Unknow)
        {
            var parameter = new SqliteParameter();
            parameter.ParameterName = name;
            parameter.Value = value == null ? DBNull.Value : value;

            return parameter;
        }

        protected override string ParseSqlFunc(string sqlFunc)
        {
            switch (sqlFunc.ToLower())
            {
                case "now":
                    return $"DATETIME('now', 'localtime')";

                case "date":
                    return "DATE('now', 'localtime')";

                case "time":
                    return "TIME('now', 'localtime')";

                case "utcnow":
                    return $"DATETIME('now')";

                case "utcdate":
                    return "DATE('now')";

                case "utctime":
                    return "TIME('now')";

                case "timestamp":
                    return "STRFTIME('%s','now')";

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
                    return $"CAST({sqlValue} AS INT)";

                case "tolong":
                    return $"CAST({sqlValue} AS BIGINT)";

                case "toshortdate":
                    return $"DATE({sqlValue})";

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
                sb.Append($"LIMIT {queryContext.PagingInfo.Count} OFFSET {queryContext.PagingInfo.Offset}");
            }


            return sb.ToString();
        }
    }
}
