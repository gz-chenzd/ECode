using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ECode.Data
{
    public class ParseResult
    {
        public ParseResult(object value)
            : this(value, DataType.Unknow, false)
        {

        }

        public ParseResult(object value, DataType dataType, bool containsSql)
        {
            this.Value = value;
            this.DataType = dataType;
            this.ContainsSql = containsSql;
        }

        public object Value
        { get; private set; }

        public DataType DataType
        { get; private set; }

        public bool ContainsSql
        { get; private set; }
    }

    public abstract class ExpressionParser
    {
        protected virtual string LeftKeyWordEscapeChar
        { get { return "["; } }

        protected virtual string RightKeyWordEscapeChar
        { get { return "]"; } }


        protected virtual string Escape(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            { return value; }

            return value.Replace(@"\", @"\\")
                        .Replace(@"_", @"\_")
                        .Replace(@"%", @"\%")
                        .Replace(@"[", @"\[")
                        .Replace(@"]", @"\]");
        }

        protected virtual string LikeEscapeSuffix()
        {
            return @"ESCAPE '\' ";
        }

        protected virtual string ToSqlOperator(ExpressionType expressionType)
        {
            switch (expressionType)
            {
                case ExpressionType.Equal:
                    return "=";

                case ExpressionType.NotEqual:
                    return "<>";

                case ExpressionType.LessThan:
                    return "<";

                case ExpressionType.LessThanOrEqual:
                    return "<=";

                case ExpressionType.GreaterThan:
                    return ">";

                case ExpressionType.GreaterThanOrEqual:
                    return ">=";

                case ExpressionType.AndAlso:
                    return "AND";

                case ExpressionType.OrElse:
                    return "OR";


                case ExpressionType.Add:
                    return "+";

                case ExpressionType.Subtract:
                    return "-";

                case ExpressionType.Multiply:
                    return "*";

                case ExpressionType.Divide:
                    return "/";

                case ExpressionType.Modulo:
                    return "%";


                default:
                    throw new ArgumentException($"无法转换为SQL运算符【{expressionType}】");
            }
        }


        protected abstract IDataParameter CreateParameter(string name, object value, DataType dataType = DataType.Unknow);

        protected abstract string ParseSqlFunc(string sqlFunc);

        protected abstract string NullConvert(string nullValue, object newValue);

        protected abstract string ParseConvert(string convertFunc, string sqlValue);

        public abstract string Parse(DbQueryContext queryContext, IList<IDataParameter> commandParameters, out IList<string> selectFields, IList<TableInfo> outQueryTables = null, IList<ParameterExpression> outParameterExpressions = null);


        public Dictionary<string, string> ParseTableInfo(TableInfo fromTable)
        {
            var selects = new Dictionary<string, string>();
            foreach (var column in fromTable.EntitySchema.Columns)
            {
                selects[$"{LeftKeyWordEscapeChar}{column.PropertyName}{RightKeyWordEscapeChar}"] = $"{fromTable.ShortName}.{LeftKeyWordEscapeChar}{column.ColumnName}{RightKeyWordEscapeChar}";
            }

            return selects;
        }

        public Dictionary<string, string> ParseSelectExpression(IList<TableInfo> fromTables, IList<IDataParameter> commandParameters, LambdaExpression selectExpression)
        {
            var selects = new Dictionary<string, string>();
            if (selectExpression.Body.NodeType == ExpressionType.New)
            {
                var newExpression = selectExpression.Body as NewExpression;
                for (int i = 0; i < newExpression.Members.Count; i++)
                {
                    var propertyName = $"{LeftKeyWordEscapeChar}{newExpression.Members[i].Name}{RightKeyWordEscapeChar}";
                    selects[propertyName] = ParseSelectField(fromTables, selectExpression.Parameters, commandParameters, newExpression.Arguments[i]);
                }

                return selects;
            }
            else if (selectExpression.Body.NodeType == ExpressionType.MemberInit)
            {
                var memberInitExpression = selectExpression.Body as MemberInitExpression;
                for (int i = 0; i < memberInitExpression.Bindings.Count; i++)
                {
                    var memberAssignment = memberInitExpression.Bindings[i] as MemberAssignment;

                    var propertyName = $"{LeftKeyWordEscapeChar}{memberAssignment.Member.Name}{RightKeyWordEscapeChar}";
                    selects[propertyName] = ParseSelectField(fromTables, selectExpression.Parameters, commandParameters, memberAssignment.Expression);
                }

                return selects;
            }
            else if (selectExpression.Body.NodeType == ExpressionType.MemberAccess)
            {
                var memberExpression = selectExpression.Body as MemberExpression;

                var propertyName = $"{LeftKeyWordEscapeChar}{memberExpression.Member.Name}{RightKeyWordEscapeChar}";
                selects[propertyName] = ParseSelectField(fromTables, selectExpression.Parameters, commandParameters, memberExpression);

                return selects;
            }
            else if (selectExpression.Body.NodeType == ExpressionType.Conditional)
            {
                var conditionalExpression = selectExpression.Body as ConditionalExpression;

                var propertyName = $"{LeftKeyWordEscapeChar}TmpCondField{RightKeyWordEscapeChar}";
                selects[propertyName] = ParseSelectField(fromTables, selectExpression.Parameters, commandParameters, conditionalExpression);

                return selects;
            }
            else if (selectExpression.Body.NodeType == ExpressionType.Call)
            {
                var methodCallExpression = selectExpression.Body as MethodCallExpression;

                var propertyName = $"{LeftKeyWordEscapeChar}{methodCallExpression.Method.Name}{RightKeyWordEscapeChar}";
                selects[propertyName] = ParseSelectField(fromTables, selectExpression.Parameters, commandParameters, methodCallExpression);

                return selects;
            }

            throw new NotSupportedException();
        }

        private string ParseSelectField(IList<TableInfo> fromTables, IList<ParameterExpression> expressionParameters, IList<IDataParameter> commandParameters, Expression fieldExpression)
        {
            var fieldResult = ParseExpression(fromTables, expressionParameters, commandParameters, fieldExpression);
            if (fieldResult.ContainsSql)
            {
                return (string)fieldResult.Value;
            }
            else
            {
                var parameter = CreateParameter($"@p{commandParameters.Count()}", fieldResult.Value);
                commandParameters.Add(parameter);

                return parameter.ParameterName;
            }
        }

        public string ParseGroupByExpression(IList<TableInfo> fromTables, IList<IDataParameter> commandParameters, LambdaExpression groupByExpression)
        {
            if (groupByExpression.Body.NodeType != ExpressionType.NewArrayInit)
            {
                throw new ArgumentException(string.Format("GroupBy聚合参数错误【{0}】", groupByExpression.Body));
            }

            var sb = new StringBuilder();
            var arrayExpression = groupByExpression.Body as NewArrayExpression;
            for (int i = 0; i < arrayExpression.Expressions.Count; i++)
            {
                var itemResult = ParseExpression(fromTables, groupByExpression.Parameters, commandParameters, arrayExpression.Expressions[i]);
                if (!itemResult.ContainsSql)
                {
                    throw new NotSupportedException();
                }

                if (i > 0)
                {
                    sb.Append(", ");
                }

                sb.Append($"{itemResult.Value}");
            }

            return sb.ToString();
        }

        public string ParseOrderByExpression(IList<TableInfo> fromTables, IList<IDataParameter> commandParameters, LambdaExpression orderByExpression)
        {
            if (orderByExpression.Body.NodeType != ExpressionType.NewArrayInit)
            {
                throw new InvalidOperationException(string.Format("OrderBy排序参数错误【{0}】", orderByExpression.Body));
            }

            var sb = new StringBuilder();
            var arrayExpression = orderByExpression.Body as NewArrayExpression;
            for (int i = 0; i < arrayExpression.Expressions.Count; i++)
            {
                var expression = arrayExpression.Expressions[i];
                if (expression.NodeType == ExpressionType.Convert)
                {
                    expression = (expression as UnaryExpression).Operand;
                }

                if (expression.NodeType != ExpressionType.Call)
                {
                    throw new InvalidOperationException(string.Format("OrderBy排序参数错误【{0}】", expression));
                }

                var methodCallExpression = expression as MethodCallExpression;
                if (methodCallExpression.Method.DeclaringType != typeof(SqlOrderFunc))
                {
                    throw new InvalidOperationException(string.Format("OrderBy函数不是SqlOrder函数【{0}】", methodCallExpression));
                }

                var operandExpression = ((methodCallExpression.Arguments[0] as UnaryExpression).Operand as LambdaExpression).Body;
                var itemResult = ParseExpression(fromTables, orderByExpression.Parameters, commandParameters, operandExpression);
                if (!itemResult.ContainsSql)
                {
                    throw new NotSupportedException();
                }

                if (i > 0)
                {
                    sb.Append(", ");
                }

                sb.Append($"{itemResult.Value} {methodCallExpression.Method.Name.ToUpper()}");
            }

            return sb.ToString();
        }

        public string ParseBooleanConditionExpression(IList<TableInfo> fromTables, IList<IDataParameter> commandParameters, LambdaExpression conditionExpression, IList<TableInfo> outQueryTables = null, IList<ParameterExpression> outParameterExpressions = null)
        {
            var allFromTables = new List<TableInfo>(fromTables);
            if (outQueryTables != null)
            {
                allFromTables.AddRange(outQueryTables);
            }

            var allExpressionParameters = new List<ParameterExpression>();
            allExpressionParameters.AddRange(conditionExpression.Parameters);
            if (outParameterExpressions != null)
            {
                allExpressionParameters.AddRange(outParameterExpressions);
            }

            var conditionResult = ParseExpression(allFromTables, allExpressionParameters, commandParameters, conditionExpression.Body);
            if (!conditionResult.ContainsSql)
            {
                if ((bool)conditionResult.Value)
                {
                    return "1=1";
                }
                else
                {
                    return "1<>1";
                }
            }

            return (string)conditionResult.Value;
        }


        public virtual ParseResult ParseExpression(IList<TableInfo> fromTables, IList<ParameterExpression> expressionParameters, IList<IDataParameter> commandParameters, Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Constant:
                    return ParseConstantExpression(expression as ConstantExpression);

                case ExpressionType.MemberAccess:
                    return ParseMemberExpression(fromTables, expressionParameters, commandParameters, expression as MemberExpression);

                case ExpressionType.Call:
                    return ParseMethodCallExpression(fromTables, expressionParameters, commandParameters, expression as MethodCallExpression);

                case ExpressionType.Convert:
                    return ParseExpression(fromTables, expressionParameters, commandParameters, (expression as UnaryExpression).Operand);

                case ExpressionType.Conditional:
                    return ParseConditionalExpression(fromTables, expressionParameters, commandParameters, expression as ConditionalExpression);

                case ExpressionType.ArrayIndex:
                    {
                        var binaryExpression = expression as BinaryExpression;

                        var arrayResult = ParseExpression(fromTables, expressionParameters, commandParameters, binaryExpression.Left);
                        if (arrayResult.ContainsSql)
                        {
                            throw new NotSupportedException();
                        }

                        var indexResult = ParseExpression(fromTables, expressionParameters, commandParameters, binaryExpression.Right);
                        if (indexResult.ContainsSql)
                        {
                            throw new NotSupportedException();
                        }

                        if (indexResult.Value is int)
                        {
                            return new ParseResult((arrayResult.Value as Array).GetValue((int)indexResult.Value));
                        }

                        return new ParseResult((arrayResult.Value as Array).GetValue((long)indexResult.Value));
                    }

                case ExpressionType.NewArrayInit:
                    {
                        var arrayExpression = expression as NewArrayExpression;
                        var arrayList = Array.CreateInstance(arrayExpression.Type.GetElementType(), arrayExpression.Expressions.Count);

                        for (int i = 0; i < arrayExpression.Expressions.Count; i++)
                        {
                            var itemResult = ParseExpression(fromTables, expressionParameters, commandParameters, arrayExpression.Expressions[i]);
                            if (itemResult.ContainsSql)
                            {
                                throw new NotSupportedException();
                            }

                            (arrayList as Array).SetValue(itemResult.Value, i);
                        }

                        return new ParseResult(arrayList);
                    }

                case ExpressionType.Not:
                    {
                        var operandExpression = (expression as UnaryExpression).Operand;
                        if (operandExpression != null && operandExpression.NodeType == ExpressionType.Call)
                        {
                            var methodCallExpression = operandExpression as MethodCallExpression;
                            if (methodCallExpression.Method.Name == "Contains")
                            {
                                return ParseContainsMethodCallExpression(fromTables, expressionParameters, commandParameters, methodCallExpression, true);
                            }
                            else if (methodCallExpression.Method.Name == "Exists")
                            {
                                return ParseExistsMethodCallExpression(fromTables, expressionParameters, commandParameters, methodCallExpression, true);
                            }
                        }

                        throw new NotSupportedException();
                    }

                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                    {
                        var binaryExpression = expression as BinaryExpression;
                        if (binaryExpression == null)
                        {
                            throw new NotSupportedException();
                        }

                        if (binaryExpression.Left.NodeType == ExpressionType.Call
                            && ((binaryExpression.Left as MethodCallExpression).Method.Name == "Compare"
                                || (binaryExpression.Left as MethodCallExpression).Method.Name == "CompareTo"))
                        {
                            return ParseCompareMethodCallExpression(fromTables, expressionParameters, commandParameters, binaryExpression);
                        }

                        return ParseBooleanExpression(fromTables, expressionParameters, commandParameters, binaryExpression);
                    }

                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                    return ParseCompoundExpression(fromTables, expressionParameters, commandParameters, expression as BinaryExpression);

                case ExpressionType.Add:
                case ExpressionType.Subtract:
                case ExpressionType.Multiply:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                    {
                        var binaryExpression = expression as BinaryExpression;

                        var leftOperandResult = ParseExpression(fromTables, expressionParameters, commandParameters, binaryExpression.Left);
                        var rightOperandResult = ParseExpression(fromTables, expressionParameters, commandParameters, binaryExpression.Right);

                        var leftOperandSql = leftOperandResult.Value as string;
                        if (!leftOperandResult.ContainsSql)
                        {
                            if (leftOperandResult.Value == null)
                            {
                                throw new NullReferenceException();
                            }

                            leftOperandSql = $"@p{commandParameters.Count()}";

                            var parameter = CreateParameter($"@p{commandParameters.Count()}", leftOperandResult.Value, rightOperandResult.DataType);
                            commandParameters.Add(parameter);
                        }

                        var rightOperandSql = rightOperandResult.Value as string;
                        if (!rightOperandResult.ContainsSql)
                        {
                            if (rightOperandResult.Value == null)
                            {
                                throw new NullReferenceException();
                            }

                            rightOperandSql = $"@p{commandParameters.Count()}";

                            var parameter = CreateParameter($"@p{commandParameters.Count()}", rightOperandResult.Value, leftOperandResult.DataType);
                            commandParameters.Add(parameter);
                        }

                        return new ParseResult($"{leftOperandSql}{ToSqlOperator(expression.NodeType)}{rightOperandSql}", DataType.Unknow, true);
                    }

                default:
                    throw new NotSupportedException();
            }
        }

        protected virtual ParseResult ParseConstantExpression(ConstantExpression expression)
        {
            return new ParseResult(expression.Value);
        }

        protected virtual ParseResult ParseParameterMember(IList<TableInfo> fromTables, IList<ParameterExpression> expressionParameters, MemberExpression expression)
        {
            int tableIndex = expressionParameters.IndexOf(expression.Expression as ParameterExpression);
            if (tableIndex >= 0)
            {
                var fromTable = fromTables[tableIndex];
                if (fromTable.EntitySchema != null)
                {
                    var column = fromTable.EntitySchema.Columns.FirstOrDefault(t => t.PropertyName == expression.Member.Name);
                    if (column == null)
                    {
                        throw new NotSupportedException();
                    }

                    return new ParseResult($"{fromTable.ShortName}.{LeftKeyWordEscapeChar}{column.ColumnName}{RightKeyWordEscapeChar}", column.DataType, true);
                }

                return new ParseResult($"{fromTable.ShortName}.{LeftKeyWordEscapeChar}{expression.Member.Name}{RightKeyWordEscapeChar}", DataType.Unknow, true);
            }

            throw new NotSupportedException();
        }

        protected virtual ParseResult ParseMemberExpression(IList<TableInfo> fromTables, IList<ParameterExpression> expressionParameters, IList<IDataParameter> commandParameters, MemberExpression expression)
        {
            var objectResult = new ParseResult(null);
            if (expression.Expression != null)
            {
                if (expression.Expression.NodeType == ExpressionType.Parameter)
                {
                    return ParseParameterMember(fromTables, expressionParameters, expression);
                }

                objectResult = ParseExpression(fromTables, expressionParameters, commandParameters, expression.Expression);
                if (objectResult.ContainsSql)
                {
                    throw new NotSupportedException();
                }
            }

            if (expression.Member is FieldInfo)
            {
                return new ParseResult((expression.Member as FieldInfo).GetValue(objectResult.Value));
            }
            else if (expression.Member is PropertyInfo)
            {
                return new ParseResult((expression.Member as PropertyInfo).GetValue(objectResult.Value, null));
            }

            throw new NotSupportedException();
        }

        protected virtual ParseResult ParseMethodCallExpression(IList<TableInfo> fromTables, IList<ParameterExpression> expressionParameters, IList<IDataParameter> commandParameters, MethodCallExpression expression)
        {
            if (expression.Method.Name == "Equals")
            {
                return ParseEqualsMethodCallExpression(fromTables, expressionParameters, commandParameters, expression);
            }
            else if (expression.Method.DeclaringType == typeof(SqlConvertFunc))
            {
                if (string.Compare("IfNull", expression.Method.Name, true) == 0)
                {
                    var leftOperandResult = ParseExpression(fromTables, expressionParameters, commandParameters, expression.Arguments[0]);
                    var rightOperandResult = ParseExpression(fromTables, expressionParameters, commandParameters, expression.Arguments[1]);

                    var leftOperandSql = leftOperandResult.Value as string;
                    if (!leftOperandResult.ContainsSql)
                    {
                        leftOperandSql = $"@p{commandParameters.Count()}";

                        var parameter = CreateParameter($"@p{commandParameters.Count()}", leftOperandResult.Value, rightOperandResult.DataType);
                        commandParameters.Add(parameter);
                    }

                    var rightOperandSql = rightOperandResult.Value as string;
                    if (!rightOperandResult.ContainsSql)
                    {
                        rightOperandSql = $"@p{commandParameters.Count()}";

                        var parameter = CreateParameter($"@p{commandParameters.Count()}", rightOperandResult.Value, leftOperandResult.DataType);
                        commandParameters.Add(parameter);
                    }

                    return new ParseResult(NullConvert(leftOperandSql, rightOperandSql), DataType.Unknow, true);
                }
                else
                {
                    var operandExpression = ((expression.Arguments[0] as UnaryExpression).Operand as LambdaExpression).Body;
                    if (operandExpression.NodeType == ExpressionType.MemberAccess)
                    {
                        var memberExpression = operandExpression as MemberExpression;
                        if (memberExpression.Expression.NodeType == ExpressionType.Parameter)
                        {
                            var fieldResult = ParseParameterMember(fromTables, expressionParameters, memberExpression);
                            return new ParseResult(ParseConvert(expression.Method.Name, (string)fieldResult.Value), DataType.Unknow, true);
                        }
                    }

                    throw new NotSupportedException();
                }
            }
            else if (expression.Method.DeclaringType == typeof(SqlAggrFunc))
            {
                if (string.Compare("Count", expression.Method.Name, true) == 0)
                {
                    if (expression.Arguments.Count == 0)
                    { return new ParseResult("COUNT(*)", DataType.Unknow, true); }
                    else
                    {
                        var operandExpression = ((expression.Arguments[0] as UnaryExpression).Operand as LambdaExpression).Body;
                        if (operandExpression.NodeType == ExpressionType.MemberAccess)
                        {
                            var memberExpression = operandExpression as MemberExpression;
                            if (memberExpression.Expression.NodeType == ExpressionType.Parameter)
                            {
                                var fieldResult = ParseParameterMember(fromTables, expressionParameters, memberExpression);
                                return new ParseResult($"COUNT({fieldResult.Value})", DataType.Unknow, true);
                            }
                        }
                    }
                }
                else if (string.Compare("Sum", expression.Method.Name, true) == 0
                         || string.Compare("Max", expression.Method.Name, true) == 0
                         || string.Compare("Min", expression.Method.Name, true) == 0
                         || string.Compare("Avg", expression.Method.Name, true) == 0)
                {
                    var operandExpression = ((expression.Arguments[0] as UnaryExpression).Operand as LambdaExpression).Body;
                    if (operandExpression.NodeType == ExpressionType.MemberAccess)
                    {
                        var memberExpression = operandExpression as MemberExpression;
                        if (memberExpression.Expression.NodeType == ExpressionType.Parameter)
                        {
                            var fieldResult = ParseParameterMember(fromTables, expressionParameters, memberExpression);
                            return new ParseResult($"{expression.Method.Name.ToUpper()}({fieldResult.Value})", fieldResult.DataType, true);
                        }
                    }
                }

                throw new NotSupportedException();
            }
            else if (expression.Method.DeclaringType == typeof(SqlFunc))
            {
                if (string.Compare("Now", expression.Method.Name, true) == 0
                    || string.Compare("UtcNow", expression.Method.Name, true) == 0)
                {
                    return new ParseResult(ParseSqlFunc(expression.Method.Name), DataType.DateTime, true);
                }
                else if (string.Compare("TimeStamp", expression.Method.Name, true) == 0)
                {
                    return new ParseResult(ParseSqlFunc(expression.Method.Name), DataType.Int64, true);
                }
            }
            else if (expression.Method.DeclaringType == typeof(string))
            {
                if (expression.Method.Name == "Contains" || expression.Method.Name == "StartsWith" || expression.Method.Name == "EndsWith")
                {
                    return ParseLikesMethodCallExpression(fromTables, expressionParameters, commandParameters, expression);
                }
            }
            else if (expression.Method.DeclaringType.GetInterface("System.Collections.IEnumerable") != null
                || expression.Method.DeclaringType == typeof(System.Linq.Enumerable))
            {
                if (expression.Method.Name == "Contains")
                {
                    return ParseContainsMethodCallExpression(fromTables, expressionParameters, commandParameters, expression, false);
                }
            }
            else if (expression.Method.DeclaringType.IsGenericType
                && (expression.Method.DeclaringType.GetGenericTypeDefinition() == typeof(IQuerySet<>)
                    || Array.Exists(expression.Method.DeclaringType.GetInterfaces(), t => t.GetGenericTypeDefinition() == typeof(IQuerySet<>))))
            {
                if (expression.Method.Name == "Contains")
                {
                    return ParseContainsMethodCallExpression(fromTables, expressionParameters, commandParameters, expression, false);
                }
                else if (expression.Method.Name == "Exists")
                {
                    return ParseExistsMethodCallExpression(fromTables, expressionParameters, commandParameters, expression, false);
                }
            }

            object targetObject = null;
            if (expression.Object != null)
            {
                var objectResult = ParseExpression(fromTables, expressionParameters, commandParameters, expression.Object);
                if (objectResult.ContainsSql)
                {
                    if (expression.Method.Name == "ToString")
                    {
                        return objectResult;
                    }

                    throw new NotSupportedException();
                }

                targetObject = objectResult.Value;
            }

            var arguments = new object[expression.Arguments.Count];
            for (int i = 0; i < expression.Arguments.Count; i++)
            {
                var argumentResult = ParseExpression(fromTables, expressionParameters, commandParameters, expression.Arguments[i]);
                if (argumentResult.ContainsSql)
                {
                    throw new NotSupportedException();
                }

                arguments[i] = argumentResult.Value;
            }

            return new ParseResult(expression.Method.Invoke(targetObject, arguments));
        }

        protected virtual ParseResult ParseLikesMethodCallExpression(IList<TableInfo> fromTables, IList<ParameterExpression> expressionParameters, IList<IDataParameter> commandParameters, MethodCallExpression expression)
        {
            if (expression.Method.DeclaringType == typeof(string)
                && (expression.Method.Name == "Contains" || expression.Method.Name == "StartsWith" || expression.Method.Name == "EndsWith"))
            {
                var objectResult = ParseExpression(fromTables, expressionParameters, commandParameters, expression.Object);
                var argumentResult = ParseExpression(fromTables, expressionParameters, commandParameters, expression.Arguments[0]);

                if (!objectResult.ContainsSql && !argumentResult.ContainsSql)
                {
                    return new ParseResult(expression.Method.Invoke(objectResult.Value, new[] { argumentResult.Value }));
                }

                if (argumentResult.ContainsSql)
                {
                    throw new InvalidOperationException(string.Format("数据库字段必须作为左参数【{0}】", expression));
                }

                var parameterName = $"@p{commandParameters.Count()}";
                var parameterValue = $"%{Escape((string)argumentResult.Value)}%";
                if (expression.Method.Name == "StartsWith")
                {
                    parameterValue = $"{Escape((string)argumentResult.Value)}%";
                }
                else if (expression.Method.Name == "EndsWith")
                {
                    parameterValue = $"%{Escape((string)argumentResult.Value)}";
                }

                var parameter = CreateParameter(parameterName, parameterValue, objectResult.DataType);
                commandParameters.Add(parameter);

                return new ParseResult($"{objectResult.Value} LIKE {parameterName} {LikeEscapeSuffix()}", DataType.Unknow, true);
            }

            throw new NotSupportedException();
        }

        protected virtual ParseResult ParseEqualsMethodCallExpression(IList<TableInfo> fromTables, IList<ParameterExpression> expressionParameters, IList<IDataParameter> commandParameters, MethodCallExpression expression)
        {
            if (expression.Method.Name == "Equals")
            {
                ParseResult leftOperandResult = null;
                ParseResult rightOperandResult = null;

                if (expression.Object != null)
                {
                    leftOperandResult = ParseExpression(fromTables, expressionParameters, commandParameters, expression.Object);
                    rightOperandResult = ParseExpression(fromTables, expressionParameters, commandParameters, expression.Arguments[0]);

                    if (!leftOperandResult.ContainsSql && !rightOperandResult.ContainsSql)
                    {
                        return new ParseResult(expression.Method.Invoke(leftOperandResult.Value, new[] { rightOperandResult.Value }));
                    }
                }
                else
                {
                    leftOperandResult = ParseExpression(fromTables, expressionParameters, commandParameters, expression.Arguments[0]);
                    rightOperandResult = ParseExpression(fromTables, expressionParameters, commandParameters, expression.Arguments[1]);

                    if (!leftOperandResult.ContainsSql && !rightOperandResult.ContainsSql)
                    {
                        return new ParseResult(expression.Method.Invoke(null, new[] { leftOperandResult.Value, rightOperandResult.Value }));
                    }
                }

                var leftOperandSql = leftOperandResult.Value as string;
                if (!leftOperandResult.ContainsSql)
                {
                    if (leftOperandResult.Value == null)
                    {
                        return new ParseResult($"{rightOperandResult.Value} IS NULL", DataType.Unknow, true);
                    }

                    leftOperandSql = $"@p{commandParameters.Count()}";

                    var parameter = CreateParameter($"@p{commandParameters.Count()}", leftOperandResult.Value, rightOperandResult.DataType);
                    commandParameters.Add(parameter);
                }

                var rightOperandSql = rightOperandResult.Value as string;
                if (!rightOperandResult.ContainsSql)
                {
                    if (rightOperandResult.Value == null)
                    {
                        return new ParseResult($"{leftOperandResult.Value} IS NULL", DataType.Unknow, true);
                    }

                    rightOperandSql = $"@p{commandParameters.Count()}";

                    var parameter = CreateParameter($"@p{commandParameters.Count()}", rightOperandResult.Value, leftOperandResult.DataType);
                    commandParameters.Add(parameter);
                }

                return new ParseResult($"{leftOperandSql}={rightOperandSql}", DataType.Unknow, true);
            }

            throw new NotSupportedException();
        }

        protected virtual ParseResult ParseCompareMethodCallExpression(IList<TableInfo> fromTables, IList<ParameterExpression> expressionParameters, IList<IDataParameter> commandParameters, BinaryExpression expression)
        {
            if (expression.Left.NodeType == ExpressionType.Call
                && ((expression.Left as MethodCallExpression).Method.Name == "Compare"
                    || (expression.Left as MethodCallExpression).Method.Name == "CompareTo"))
            {
                var compareResult = ParseExpression(fromTables, expressionParameters, commandParameters, expression.Right);
                if (compareResult.ContainsSql)
                {
                    throw new NotSupportedException();
                }

                ParseResult leftOperandResult = null;
                ParseResult rightOperandResult = null;
                var methodCallExpression = expression.Left as MethodCallExpression;
                if (methodCallExpression.Method.Name == "CompareTo")
                {
                    leftOperandResult = ParseExpression(fromTables, expressionParameters, commandParameters, methodCallExpression.Object);
                    rightOperandResult = ParseExpression(fromTables, expressionParameters, commandParameters, methodCallExpression.Arguments[0]);

                    if (!leftOperandResult.ContainsSql && !rightOperandResult.ContainsSql)
                    {
                        var methodResult = methodCallExpression.Method.Invoke(leftOperandResult.Value, new[] { rightOperandResult.Value });
                        return new ParseResult(expression.Method.Invoke(null, new[] { methodResult, compareResult }));
                    }

                    if (!leftOperandResult.ContainsSql && leftOperandResult.Value == null)
                    {
                        throw new NotSupportedException();
                    }
                }
                else
                {
                    leftOperandResult = ParseExpression(fromTables, expressionParameters, commandParameters, methodCallExpression.Arguments[0]);
                    rightOperandResult = ParseExpression(fromTables, expressionParameters, commandParameters, methodCallExpression.Arguments[1]);

                    if (!leftOperandResult.ContainsSql && !rightOperandResult.ContainsSql)
                    {
                        var methodResult = methodCallExpression.Method.Invoke(null, new[] { leftOperandResult.Value, rightOperandResult.Value });
                        return new ParseResult(expression.Method.Invoke(null, new[] { methodResult, compareResult }));
                    }
                }

                if (((int)compareResult.Value) != 0)
                {
                    throw new NotSupportedException();
                }

                if ((!leftOperandResult.ContainsSql && leftOperandResult.Value == null)
                    || (!rightOperandResult.ContainsSql && rightOperandResult.Value == null))
                {
                    if (expression.NodeType != ExpressionType.Equal
                        && expression.NodeType != ExpressionType.NotEqual)
                    {
                        throw new NotSupportedException();
                    }

                    var operandSqlString = leftOperandResult.Value as string;
                    if (!leftOperandResult.ContainsSql && leftOperandResult.Value == null)
                    {
                        operandSqlString = rightOperandResult.Value as string;
                    }

                    if (expression.NodeType == ExpressionType.Equal)
                    {
                        return new ParseResult($"{operandSqlString} IS NULL", DataType.Unknow, true);
                    }
                    else
                    {
                        return new ParseResult($"{operandSqlString} IS NOT NULL", DataType.Unknow, true);
                    }
                }

                var leftOperandSql = leftOperandResult.Value as string;
                if (!leftOperandResult.ContainsSql)
                {
                    leftOperandSql = $"@p{commandParameters.Count()}";

                    var parameter = CreateParameter($"@p{commandParameters.Count()}", leftOperandResult.Value, rightOperandResult.DataType);
                    commandParameters.Add(parameter);
                }

                var rightOperandSql = rightOperandResult.Value as string;
                if (!rightOperandResult.ContainsSql && rightOperandResult.Value != null)
                {
                    rightOperandSql = $"@p{commandParameters.Count()}";

                    var parameter = CreateParameter($"@p{commandParameters.Count()}", rightOperandResult.Value, leftOperandResult.DataType);
                    commandParameters.Add(parameter);
                }

                return new ParseResult($"{leftOperandSql}{ToSqlOperator(expression.NodeType)}{rightOperandSql}", DataType.Unknow, true);
            }

            throw new NotSupportedException();
        }

        protected virtual ParseResult ParseContainsMethodCallExpression(IList<TableInfo> fromTables, IList<ParameterExpression> expressionParameters, IList<IDataParameter> commandParameters, MethodCallExpression expression, bool useReverse = false)
        {
            if (expression.Method.Name != "Contains")
            {
                throw new NotSupportedException();
            }

            if (expression.Method.DeclaringType.GetInterface("System.Collections.IEnumerable") != null
                || expression.Method.DeclaringType == typeof(System.Linq.Enumerable))
            {
                var objectResult = ParseExpression(fromTables, expressionParameters, commandParameters, expression.Object == null ? expression.Arguments[0] : expression.Object);
                if (objectResult.ContainsSql)
                { throw new NotSupportedException(); }

                var argumentResult = ParseExpression(fromTables, expressionParameters, commandParameters, expression.Object == null ? expression.Arguments[1] : expression.Arguments[0]);
                if (!argumentResult.ContainsSql)
                { return new ParseResult(expression.Method.Invoke(objectResult.Value, new[] { argumentResult.Value })); }

                var arrayList = objectResult.Value as IEnumerable;
                if (arrayList == null)
                { throw new NotSupportedException(); }

                var isFirst = true;
                var sb = new StringBuilder();
                sb.Append($"{argumentResult.Value}");
                sb.Append((useReverse ? " NOT" : "") + " IN (");
                foreach (var item in arrayList)
                {
                    // cannot be null.
                    if (item == null)
                    { throw new NotSupportedException(); }

                    if (!isFirst)
                    { sb.Append(", "); }

                    isFirst = false;

                    sb.Append($"@p{commandParameters.Count()}");

                    var parameter = CreateParameter($"@p{commandParameters.Count()}", item, argumentResult.DataType);
                    commandParameters.Add(parameter);
                }
                sb.Append(")");

                return new ParseResult(sb.ToString(), DataType.Unknow, true);
            }
            else if (expression.Method.DeclaringType.GetGenericTypeDefinition() == typeof(IQuerySet<>)
                || Array.Exists(expression.Method.DeclaringType.GetInterfaces(), t => t.GetGenericTypeDefinition() == typeof(IQuerySet<>)))
            {
                var objectResult = ParseExpression(fromTables, expressionParameters, commandParameters, expression.Object);

                var querySetType = typeof(DbQuerySet<>);
                querySetType = querySetType.MakeGenericType(expression.Method.DeclaringType.GenericTypeArguments[0]);

                PropertyInfo queryContextProperty = null;
                foreach (var property in querySetType.GetRuntimeProperties())
                {
                    if (property.Name == "QueryContext")
                    {
                        queryContextProperty = property;
                        break;
                    }
                }

                var subQueryContext = queryContextProperty.GetValue(objectResult.Value) as DbQueryContext;

                IList<string> selectFields = null;
                var subQueryParsedSql = Parse(subQueryContext, commandParameters, out selectFields);

                var argumentResult = ParseExpression(fromTables, expressionParameters, commandParameters, expression.Arguments[0]);

                var leftOperandSql = argumentResult.Value as string;
                if (!argumentResult.ContainsSql)
                {
                    if (argumentResult.Value == null)
                    {
                        throw new ArgumentNullException();
                    }

                    leftOperandSql = $"@p{commandParameters.Count()}";

                    var parameter = CreateParameter(leftOperandSql, argumentResult.Value, DataType.Unknow);
                    commandParameters.Add(parameter);
                }

                var sqlOperator = useReverse ? "NOT IN" : "IN";
                return new ParseResult($"{leftOperandSql} {sqlOperator} ({subQueryParsedSql})", DataType.Unknow, true);
            }

            throw new NotSupportedException();
        }

        protected virtual ParseResult ParseExistsMethodCallExpression(IList<TableInfo> fromTables, IList<ParameterExpression> expressionParameters, IList<IDataParameter> commandParameters, MethodCallExpression expression, bool useReverse = false)
        {
            if (expression.Method.Name != "Exists")
            {
                throw new NotSupportedException();
            }

            if (expression.Method.DeclaringType.GetGenericTypeDefinition() == typeof(IQuerySet<>)
                || Array.Exists(expression.Method.DeclaringType.GetInterfaces(), t => t.GetGenericTypeDefinition() == typeof(IQuerySet<>)))
            {
                var objectResult = ParseExpression(fromTables, expressionParameters, commandParameters, expression.Object);

                var querySetType = typeof(DbQuerySet<>);
                querySetType = querySetType.MakeGenericType(expression.Method.DeclaringType.GenericTypeArguments[0]);

                PropertyInfo queryContextProperty = null;
                foreach (var property in querySetType.GetRuntimeProperties())
                {
                    if (property.Name == "QueryContext")
                    {
                        queryContextProperty = property;
                        break;
                    }
                }

                var subQueryContext = queryContextProperty.GetValue(objectResult.Value) as DbQueryContext;
                var newQueryContext = subQueryContext.SnapshotForAction(DbQueryAction.SetWhere);
                newQueryContext.SetWhere((expression.Arguments[0] as UnaryExpression).Operand as LambdaExpression);

                IList<string> selectFields = null;
                var subQueryParsedSql = Parse(newQueryContext, commandParameters, out selectFields, fromTables, expressionParameters);

                var sqlOperator = useReverse ? "NOT EXISTS" : "EXISTS";
                return new ParseResult($"{sqlOperator} ({subQueryParsedSql})", DataType.Unknow, true);
            }

            throw new NotSupportedException();
        }

        protected virtual ParseResult ParseBooleanExpression(IList<TableInfo> fromTables, IList<ParameterExpression> expressionParameters, IList<IDataParameter> commandParameters, BinaryExpression expression)
        {
            var leftOperandResult = ParseExpression(fromTables, expressionParameters, commandParameters, expression.Left);
            var rightOperandResult = ParseExpression(fromTables, expressionParameters, commandParameters, expression.Right);

            if (!leftOperandResult.ContainsSql && !rightOperandResult.ContainsSql)
            {
                return new ParseResult(expression.Method.Invoke(null, new[] { leftOperandResult.Value, rightOperandResult.Value }));
            }

            if ((!leftOperandResult.ContainsSql && leftOperandResult.Value == null)
                || (!rightOperandResult.ContainsSql && rightOperandResult.Value == null))
            {
                if (expression.NodeType != ExpressionType.Equal
                    && expression.NodeType != ExpressionType.NotEqual)
                {
                    throw new NotSupportedException();
                }

                var operandSqlString = leftOperandResult.Value as string;
                if (!leftOperandResult.ContainsSql && leftOperandResult.Value == null)
                {
                    operandSqlString = rightOperandResult.Value as string;
                }

                if (expression.NodeType == ExpressionType.Equal)
                {
                    return new ParseResult($"{operandSqlString} IS NULL", DataType.Unknow, true);
                }
                else
                {
                    return new ParseResult($"{operandSqlString} IS NOT NULL", DataType.Unknow, true);
                }
            }

            var leftOperandSql = leftOperandResult.Value as string;
            if (!leftOperandResult.ContainsSql)
            {
                leftOperandSql = $"@p{commandParameters.Count()}";

                var parameter = CreateParameter($"@p{commandParameters.Count()}", leftOperandResult.Value, rightOperandResult.DataType);
                commandParameters.Add(parameter);
            }

            var rightOperandSql = rightOperandResult.Value as string;
            if (!rightOperandResult.ContainsSql && rightOperandResult.Value != null)
            {
                rightOperandSql = $"@p{commandParameters.Count()}";

                var parameter = CreateParameter($"@p{commandParameters.Count()}", rightOperandResult.Value, leftOperandResult.DataType);
                commandParameters.Add(parameter);
            }

            return new ParseResult($"{leftOperandSql}{ToSqlOperator(expression.NodeType)}{rightOperandSql}", DataType.Unknow, true);
        }

        protected virtual ParseResult ParseCompoundExpression(IList<TableInfo> fromTables, IList<ParameterExpression> expressionParameters, IList<IDataParameter> commandParameters, BinaryExpression expression)
        {
            var leftOperandResult = ParseExpression(fromTables, expressionParameters, commandParameters, expression.Left);
            var rightOperandResult = ParseExpression(fromTables, expressionParameters, commandParameters, expression.Right);

            if (!leftOperandResult.ContainsSql && !rightOperandResult.ContainsSql)
            {
                return new ParseResult(expression.Method.Invoke(null, new[] { leftOperandResult.Value, rightOperandResult.Value }));
            }

            var leftOperandSql = leftOperandResult.Value as string;
            if (!leftOperandResult.ContainsSql)
            {
                if ((bool)leftOperandResult.Value)
                {
                    leftOperandSql = "1=1";
                }
                else
                {
                    leftOperandSql = "1<>1";
                }
            }

            var rightOperandSql = rightOperandResult.Value as string;
            if (!rightOperandResult.ContainsSql)
            {
                if ((bool)rightOperandResult.Value)
                {
                    rightOperandSql = "1=1";
                }
                else
                {
                    rightOperandSql = "1<>1";
                }
            }

            if (expression.Left.NodeType != ExpressionType.AndAlso && expression.Left.NodeType != ExpressionType.OrElse)
            {
                if ((expression.Right.NodeType == ExpressionType.AndAlso || expression.Right.NodeType == ExpressionType.OrElse)
                    && expression.Right.NodeType != expression.NodeType)
                {
                    return new ParseResult($"{leftOperandSql} {ToSqlOperator(expression.NodeType)} ({rightOperandSql})", DataType.Unknow, true);
                }

                return new ParseResult($"{leftOperandSql} {ToSqlOperator(expression.NodeType)} {rightOperandSql}", DataType.Unknow, true);
            }

            if (expression.Right.NodeType == ExpressionType.AndAlso || expression.Right.NodeType == ExpressionType.OrElse)
            {
                if (expression.Left.NodeType == expression.Right.NodeType && expression.Left.NodeType == expression.NodeType)
                {
                    return new ParseResult($"{leftOperandSql} {ToSqlOperator(expression.NodeType)} {rightOperandSql}", DataType.Unknow, true);
                }

                return new ParseResult($"({leftOperandSql}) {ToSqlOperator(expression.NodeType)} ({rightOperandSql})", DataType.Unknow, true);
            }

            if (expression.Left.NodeType == expression.NodeType)
            {
                return new ParseResult($"{leftOperandSql} {ToSqlOperator(expression.NodeType)} {rightOperandSql}", DataType.Unknow, true);
            }

            return new ParseResult($"({leftOperandSql}) {ToSqlOperator(expression.NodeType)} {rightOperandSql}", DataType.Unknow, true);
        }

        protected virtual ParseResult ParseConditionalExpression(IList<TableInfo> fromTables, IList<ParameterExpression> expressionParameters, IList<IDataParameter> commandParameters, ConditionalExpression expression)
        {
            var testResult = ParseExpression(fromTables, expressionParameters, commandParameters, expression.Test);
            var trueValueResult = ParseExpression(fromTables, expressionParameters, commandParameters, expression.IfTrue);
            var falseValueResult = ParseExpression(fromTables, expressionParameters, commandParameters, expression.IfFalse);

            if (!testResult.ContainsSql)
            {
                if ((bool)testResult.Value)
                {
                    return trueValueResult;
                }

                return falseValueResult;
            }

            var testSqlString = testResult.Value as string;

            var trueValueSql = trueValueResult.Value as string;
            if (!trueValueResult.ContainsSql)
            {
                trueValueSql = $"@p{commandParameters.Count()}";

                var parameter = CreateParameter($"@p{commandParameters.Count()}", trueValueResult.Value, DataType.Unknow);
                commandParameters.Add(parameter);
            }

            var falseValueSql = falseValueResult.Value as string;
            if (!falseValueResult.ContainsSql)
            {
                falseValueSql = $"@p{commandParameters.Count()}";

                var parameter = CreateParameter($"@p{commandParameters.Count()}", falseValueResult.Value, DataType.Unknow);
                commandParameters.Add(parameter);
            }

            return new ParseResult($"CASE WHEN {testSqlString} THEN {trueValueSql} ELSE {falseValueSql} END", DataType.Unknow, true);
        }
    }
}