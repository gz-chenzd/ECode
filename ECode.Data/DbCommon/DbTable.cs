using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ECode.TypeConversion;
using ECode.Utility;

namespace ECode.Data
{
    public abstract class DbTable<TEntity> : DbQuerySet<TEntity>, ITable<TEntity>
    {
        protected readonly Type     EntityType              = typeof(TEntity);
        protected PropertyInfo      IdentityProperty        = null;


        public string TableName
        { get; private set; }

        public EntitySchema Schema
        { get; private set; }

        protected ISchemaManager SchemaManager
        { get; private set; }


        protected object ShardObject
        { get; private set; }

        protected object PartitionObject
        { get; private set; }

        protected IShardStrategy ShardStrategy
        { get; private set; }


        protected virtual string LeftKeyWordEscapeChar
        { get { return "["; } }

        protected virtual string RightKeyWordEscapeChar
        { get { return "]"; } }


        protected DbTable(DbSession session, ISchemaManager schemaManager,
                          object shardObject, object partitionObject, IShardStrategy shardStrategy)
            : base(session)
        {
            this.SchemaManager = schemaManager;

            this.ShardObject = shardObject;
            this.PartitionObject = partitionObject;
            this.ShardStrategy = shardStrategy;

            string tableName = this.EntityType.Name;
            if (this.SchemaManager != null)
            {
                this.Schema = this.SchemaManager.GetSchema<TEntity>();
                if (this.Schema != null)
                {
                    tableName = this.Schema.TableName;
                }
            }

            if (this.Schema == null)
            {
                this.Schema = SchemaParser.GetSchema<TEntity>();
                if (this.Schema != null)
                {
                    tableName = this.Schema.TableName;
                }
            }

            if (this.Schema == null)
            { throw new InvalidOperationException($"Cannot parse entity '{this.EntityType}'s schema."); }


            this.TableName = tableName;
            if (this.ShardStrategy != null)
            {
                string tableShardNo = this.ShardStrategy.GetTableShardNo(tableName, this.ShardObject);
                if (!string.IsNullOrWhiteSpace(tableShardNo))
                {
                    this.TableName += ("_" + tableShardNo.Trim());
                }

                string tablePartitionNo = this.ShardStrategy.GetTablePartitionNo(tableName, this.ShardObject, this.PartitionObject);
                if (!string.IsNullOrWhiteSpace(tablePartitionNo))
                {
                    this.TableName += ("_" + tablePartitionNo.Trim());
                }
            }


            this.QueryContext = new DbQueryContext(session);
            this.QueryContext.SetFrom<TEntity>(this);
        }


        protected abstract string GetLastInsertIdSql();

        protected abstract IDataParameter CreateParameter(string name, ColumnSchema schema);


        protected virtual string ParseInsertSql(TEntity entity, IList<IDataParameter> parameters)
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

            return $"INSERT INTO {LeftKeyWordEscapeChar}{this.TableName}{RightKeyWordEscapeChar} ({sbFields.ToString()}) VALUES ({sbValues.ToString()})";
        }

        protected virtual string ParseInsertSql(TEntity entity, Expression<Func<TEntity, bool>> existsCondition, IList<IDataParameter> parameters)
        {
            throw new NotSupportedException();
        }

        protected virtual string ParseUpdateSql(object value, Expression<Func<TEntity, bool>> where, IList<IDataParameter> parameters)
        {
            var fromTables = new List<TableInfo>();
            fromTables.Add(new TableInfo(this.TableName, $"{LeftKeyWordEscapeChar}{this.TableName}{RightKeyWordEscapeChar}", this.EntityType, this.Schema));
            string whereSql = GetExpressionParser().ParseBooleanConditionExpression(fromTables, parameters, where);

            var sbSet = new StringBuilder();
            foreach (var property in value.GetType().GetProperties())
            {
                // Ignore cannot be read
                if (!property.CanRead)
                { continue; }

                var columnSchema = this.Schema.Columns.FirstOrDefault(t => t.PropertyName == property.Name);
                if (columnSchema == null)
                { continue; }

                // Timestamp is automatically set by database.
                if (columnSchema.DataType == DataType.Timestamp)
                { continue; }

                if (columnSchema.IsPrimaryKey || columnSchema.IsIdentity)
                { continue; }

                var parameter = this.CreateParameter($"@p{parameters.Count}", columnSchema);
                parameter.Value = property.GetValue(value, null);

                if (parameter.Value == null)
                {
                    if (columnSchema.IsRequired)
                    { throw new ArgumentException($"Value '{property.Name}' cannot be null."); }
                    else
                    { parameter.Value = DBNull.Value; }
                }

                if (sbSet.Length > 0)
                { sbSet.Append(", "); }

                sbSet.Append($"{LeftKeyWordEscapeChar}{columnSchema.ColumnName}{RightKeyWordEscapeChar}=@p{parameters.Count}");

                parameters.Add(parameter);
            }

            if (sbSet.Length == 0)
            { throw new ArgumentException("Not value to be updated."); }

            if (string.IsNullOrWhiteSpace(whereSql))
            { throw new ArgumentException("Where condition cannot be empty."); }

            return $"UPDATE {LeftKeyWordEscapeChar}{this.TableName}{RightKeyWordEscapeChar} SET {sbSet.ToString()} WHERE {whereSql}";
        }

        protected virtual string ParseUpdateSql(IDictionary value, Expression<Func<TEntity, bool>> where, IList<IDataParameter> parameters)
        {
            var fromTables = new List<TableInfo>();
            fromTables.Add(new TableInfo(this.TableName, $"{LeftKeyWordEscapeChar}{this.TableName}{RightKeyWordEscapeChar}", this.EntityType, this.Schema));
            string whereSql = GetExpressionParser().ParseBooleanConditionExpression(fromTables, parameters, where);

            var sbSet = new StringBuilder();
            foreach (var key in value.Keys)
            {
                if (key == null || !(key is string))
                { throw new ArgumentException("Key is null or not type of string."); }

                var columnSchema = this.Schema.Columns.FirstOrDefault(t => t.PropertyName == (string)key);
                if (columnSchema == null)
                { continue; }

                // Timestamp is automatically set by database.
                if (columnSchema.DataType == DataType.Timestamp)
                { continue; }

                if (columnSchema.IsPrimaryKey || columnSchema.IsIdentity)
                { continue; }

                var parameter = this.CreateParameter($"@p{parameters.Count}", columnSchema);
                parameter.Value = value[key];

                if (parameter.Value == null)
                {
                    if (columnSchema.IsRequired)
                    { throw new ArgumentException($"Value '{key}' cannot be null."); }
                    else
                    { parameter.Value = DBNull.Value; }
                }

                if (sbSet.Length > 0)
                { sbSet.Append(", "); }

                sbSet.Append($"{LeftKeyWordEscapeChar}{columnSchema.ColumnName}{RightKeyWordEscapeChar}=@p{parameters.Count}");

                parameters.Add(parameter);
            }

            if (sbSet.Length == 0)
            { throw new ArgumentException("Not value to be updated."); }

            if (string.IsNullOrWhiteSpace(whereSql))
            { throw new ArgumentException("Where condition cannot be empty."); }

            return $"UPDATE {LeftKeyWordEscapeChar}{this.TableName}{RightKeyWordEscapeChar} SET {sbSet.ToString()} WHERE {whereSql}";
        }

        protected virtual string ParseUpdateSql(Expression<Func<TEntity, object>> value, Expression<Func<TEntity, bool>> where, IList<IDataParameter> parameters)
        {
            var fromTables = new List<TableInfo>();
            fromTables.Add(new TableInfo(this.TableName, $"{LeftKeyWordEscapeChar}{this.TableName}{RightKeyWordEscapeChar}", this.EntityType, this.Schema));
            string whereSql = GetExpressionParser().ParseBooleanConditionExpression(fromTables, parameters, where);

            var sbSet = new StringBuilder();

            if (value.Body is NewExpression)
            {
                var newExpression = value.Body as NewExpression;
                for (int i = 0; i < newExpression.Members.Count; i++)
                {
                    var columnSchema = this.Schema.Columns.FirstOrDefault(t => t.PropertyName == newExpression.Members[i].Name);
                    if (columnSchema == null)
                    { continue; }

                    // Timestamp is automatically set by database.
                    if (columnSchema.DataType == DataType.Timestamp)
                    { continue; }

                    if (columnSchema.IsPrimaryKey || columnSchema.IsIdentity)
                    { continue; }


                    var fieldResult = GetExpressionParser().ParseExpression(fromTables, value.Parameters, parameters, newExpression.Arguments[i]);
                    if (fieldResult.ContainsSql)
                    {
                        if (sbSet.Length > 0)
                        { sbSet.Append(", "); }

                        sbSet.Append($"{LeftKeyWordEscapeChar}{columnSchema.ColumnName}{RightKeyWordEscapeChar}={fieldResult.Value}");
                    }
                    else
                    {
                        var parameter = this.CreateParameter($"@p{parameters.Count}", null);
                        parameter.Value = fieldResult.Value;

                        if (parameter.Value == null)
                        {
                            if (columnSchema.IsRequired)
                            { throw new ArgumentException($"Value '{newExpression.Members[i].Name}' cannot be null."); }
                            else
                            { parameter.Value = DBNull.Value; }
                        }

                        if (sbSet.Length > 0)
                        { sbSet.Append(", "); }

                        sbSet.Append($"{LeftKeyWordEscapeChar}{columnSchema.ColumnName}{RightKeyWordEscapeChar}=@p{parameters.Count}");

                        parameters.Add(parameter);
                    }
                }
            }
            else if (value.Body is MemberInitExpression)
            {
                var memberInitExpression = value.Body as MemberInitExpression;
                for (int i = 0; i < memberInitExpression.Bindings.Count; i++)
                {
                    var memberAssignment = memberInitExpression.Bindings[i] as MemberAssignment;

                    var columnSchema = this.Schema.Columns.FirstOrDefault(t => t.PropertyName == memberAssignment.Member.Name);
                    if (columnSchema == null)
                    { continue; }

                    // Timestamp is automatically set by database.
                    if (columnSchema.DataType == DataType.Timestamp)
                    { continue; }

                    if (columnSchema.IsPrimaryKey || columnSchema.IsIdentity)
                    { continue; }


                    var fieldResult = GetExpressionParser().ParseExpression(fromTables, value.Parameters, parameters, memberAssignment.Expression);
                    if (fieldResult.ContainsSql)
                    {
                        if (sbSet.Length > 0)
                        { sbSet.Append(", "); }

                        sbSet.Append($"{LeftKeyWordEscapeChar}{columnSchema.ColumnName}{RightKeyWordEscapeChar}={fieldResult.Value}");
                    }
                    else
                    {
                        var parameter = this.CreateParameter($"@p{parameters.Count}", null);
                        parameter.Value = fieldResult.Value;

                        if (parameter.Value == null)
                        {
                            if (columnSchema.IsRequired)
                            { throw new ArgumentException($"Value '{memberAssignment.Member.Name}' cannot be null."); }
                            else
                            { parameter.Value = DBNull.Value; }
                        }

                        if (sbSet.Length > 0)
                        { sbSet.Append(", "); }

                        sbSet.Append($"{LeftKeyWordEscapeChar}{columnSchema.ColumnName}{RightKeyWordEscapeChar}=@p{parameters.Count}");

                        parameters.Add(parameter);
                    }
                }
            }
            else
            {
                throw new NotSupportedException($"Not supported update expression '{value.Body.GetType()}'.");
            }

            if (sbSet.Length == 0)
            { throw new ArgumentException("Not value to be updated."); }

            if (string.IsNullOrWhiteSpace(whereSql))
            { throw new ArgumentException("Where condition cannot be empty."); }

            return $"UPDATE {LeftKeyWordEscapeChar}{this.TableName}{RightKeyWordEscapeChar} SET {sbSet.ToString()} WHERE {whereSql}";
        }

        protected virtual string ParseDeleteSql(Expression<Func<TEntity, bool>> where, IList<IDataParameter> parameters)
        {
            var fromTables = new List<TableInfo>();
            fromTables.Add(new TableInfo(this.TableName, $"{LeftKeyWordEscapeChar}{this.TableName}{RightKeyWordEscapeChar}", this.EntityType, this.Schema));
            string whereSql = GetExpressionParser().ParseBooleanConditionExpression(fromTables, parameters, where);

            if (string.IsNullOrWhiteSpace(whereSql))
            { throw new ArgumentException("Where condition cannot be empty."); }

            return $"DELETE FROM {LeftKeyWordEscapeChar}{this.TableName}{RightKeyWordEscapeChar} WHERE {whereSql}";
        }


        public bool Add(TEntity entity)
        {
            return InternalAdd(entity, null);
        }

        public bool AddIfNotExists(TEntity entity, Expression<Func<TEntity, bool>> existsCondition)
        {
            AssertUtil.ArgumentNotNull(entity, nameof(entity));
            AssertUtil.ArgumentNotNull(existsCondition, nameof(existsCondition));

            return InternalAdd(entity, existsCondition);
        }

        private bool InternalAdd(TEntity entity, Expression<Func<TEntity, bool>> existsCondition)
        {
            var entry = new LogEntry();
            entry.SessionID = this.Session.ID;
            entry.CommandType = CommandType.Insert;
            entry.TableName = this.TableName;

            var watch = new Stopwatch();
            watch.Start();

            try
            {
                var conn = this.Session.GetDbConnection(true);
                if (conn is System.Data.Common.DbConnection)
                {
                    var db_conn = (System.Data.Common.DbConnection)conn;
                    entry.Server = db_conn.DataSource;
                    entry.Database = db_conn.Database;
                }

                var connectElapsed = (int)watch.ElapsedMilliseconds;


                var cmd = conn.CreateCommand();
                cmd.Transaction = this.Session.GetDbTransaction();

                entry.TransactionID = this.Session.GetActiveTransaction()?.ID;

                var parameters = new List<IDataParameter>();

                if (existsCondition == null)
                {
                    entry.CommandText = ParseInsertSql(entity, parameters);
                }
                else
                {
                    entry.CommandText = ParseInsertSql(entity, existsCondition, parameters);
                }

                entry.ParseElapsed = (int)watch.ElapsedMilliseconds - connectElapsed;


                cmd.CommandText = entry.CommandText;
                foreach (IDataParameter parameter in parameters)
                { cmd.Parameters.Add(parameter); }

                int affectedRows = cmd.ExecuteNonQuery();
                if (affectedRows > 0 && IdentityProperty != null)
                {
                    cmd = conn.CreateCommand();
                    cmd.CommandText = GetLastInsertIdSql();

                    var lastInsertId = TypeConversionUtil.ConvertValueIfNecessary(IdentityProperty.PropertyType, cmd.ExecuteScalar());
                    IdentityProperty.SetValue(entity, lastInsertId, null);
                }


                watch.Stop();

                entry.TotalElapsed = (int)watch.ElapsedMilliseconds;
                this.Session.Database.OnWriteLog(entry);

                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                watch.Stop();

                entry.Message = ex.Message;
                entry.Exception = ex;
                entry.TotalElapsed = (int)watch.ElapsedMilliseconds;
                this.Session.Database.OnWriteLog(entry);

                throw ex;
            }
        }

        public void AddRange(IEnumerable<TEntity> entities)
        {
            if (entities == null || entities.Count() == 0)
            { return; }


            var entry = new LogEntry();
            entry.SessionID = this.Session.ID;
            entry.CommandType = CommandType.Batch;
            entry.TableName = this.TableName;

            var watch = new Stopwatch();
            watch.Start();


            bool existsTran = this.Session.GetActiveTransaction() != null;

            try
            {
                if (!existsTran)
                { this.Session.BeginTransaction(); }


                entry.TransactionID = this.Session.GetActiveTransaction()?.ID;

                var conn = this.Session.GetDbConnection(true);
                if (conn is System.Data.Common.DbConnection)
                {
                    var db_conn = (System.Data.Common.DbConnection)conn;
                    entry.Server = db_conn.DataSource;
                    entry.Database = db_conn.Database;
                }


                foreach (var entity in entities)
                { Add(entity); }

                if (!existsTran)
                { this.Session.GetActiveTransaction().Commit(); }


                watch.Stop();

                entry.TotalElapsed = (int)watch.ElapsedMilliseconds;
                this.Session.Database.OnWriteLog(entry);
            }
            catch (Exception ex)
            {
                if (!existsTran)
                { this.Session.GetActiveTransaction().Rollback(); }


                watch.Stop();

                entry.Message = ex.Message;
                entry.Exception = ex;
                entry.TotalElapsed = (int)watch.ElapsedMilliseconds;
                this.Session.Database.OnWriteLog(entry);

                throw ex;
            }
        }


        public int Update(object value, Expression<Func<TEntity, bool>> where)
        {
            AssertUtil.ArgumentNotNull(value, nameof(value));
            AssertUtil.ArgumentNotNull(where, nameof(where));

            return InternalUpdate(value, where);
        }

        public int Update(IDictionary value, Expression<Func<TEntity, bool>> where)
        {
            AssertUtil.ArgumentNotNull(value, nameof(value));
            AssertUtil.ArgumentNotNull(where, nameof(where));

            return InternalUpdate(value, where);
        }

        public int Update(Expression<Func<TEntity, object>> value, Expression<Func<TEntity, bool>> where)
        {
            AssertUtil.ArgumentNotNull(value, nameof(value));
            AssertUtil.ArgumentNotNull(where, nameof(where));

            return InternalUpdate(value, where);
        }

        private int InternalUpdate(object value, Expression<Func<TEntity, bool>> where)
        {
            var entry = new LogEntry();
            entry.SessionID = this.Session.ID;
            entry.CommandType = CommandType.Update;
            entry.TableName = this.TableName;

            var watch = new Stopwatch();
            watch.Start();

            try
            {
                var conn = this.Session.GetDbConnection(true);
                if (conn is System.Data.Common.DbConnection)
                {
                    var db_conn = (System.Data.Common.DbConnection)conn;
                    entry.Server = db_conn.DataSource;
                    entry.Database = db_conn.Database;
                }

                var connectElapsed = (int)watch.ElapsedMilliseconds;


                var cmd = conn.CreateCommand();
                cmd.Transaction = this.Session.GetDbTransaction();

                entry.TransactionID = this.Session.GetActiveTransaction()?.ID;

                var parameters = new List<IDataParameter>();

                if (value is IDictionary)
                {
                    entry.CommandText = ParseUpdateSql(value as IDictionary, where, parameters);
                }
                else if (value is Expression<Func<TEntity, object>>)
                {
                    entry.CommandText = ParseUpdateSql(value as Expression<Func<TEntity, object>>, where, parameters);
                }
                else
                {
                    entry.CommandText = ParseUpdateSql(value as object, where, parameters);
                }

                entry.ParseElapsed = (int)watch.ElapsedMilliseconds - connectElapsed;


                cmd.CommandText = entry.CommandText;
                foreach (var parameter in parameters)
                { cmd.Parameters.Add(parameter); }

                var affectedRows = cmd.ExecuteNonQuery();


                watch.Stop();

                entry.TotalElapsed = (int)watch.ElapsedMilliseconds;
                this.Session.Database.OnWriteLog(entry);

                return affectedRows;
            }
            catch (Exception ex)
            {
                watch.Stop();

                entry.Message = ex.Message;
                entry.Exception = ex;
                entry.TotalElapsed = (int)watch.ElapsedMilliseconds;
                this.Session.Database.OnWriteLog(entry);

                throw;
            }
        }


        public int Delete(Expression<Func<TEntity, bool>> where)
        {
            AssertUtil.ArgumentNotNull(where, nameof(where));

            var entry = new LogEntry();
            entry.SessionID = this.Session.ID;
            entry.CommandType = CommandType.Delete;
            entry.TableName = this.TableName;

            var watch = new Stopwatch();
            watch.Start();

            try
            {
                var conn = this.Session.GetDbConnection(true);
                if (conn is System.Data.Common.DbConnection)
                {
                    var db_conn = (System.Data.Common.DbConnection)conn;
                    entry.Server = db_conn.DataSource;
                    entry.Database = db_conn.Database;
                }

                var connectElapsed = (int)watch.ElapsedMilliseconds;


                var cmd = conn.CreateCommand();
                cmd.Transaction = this.Session.GetDbTransaction();

                entry.TransactionID = this.Session.GetActiveTransaction()?.ID;

                var parameters = new List<IDataParameter>();

                entry.CommandText = ParseDeleteSql(where, parameters);
                entry.ParseElapsed = (int)watch.ElapsedMilliseconds - connectElapsed;


                cmd.CommandText = entry.CommandText;
                foreach (var parameter in parameters)
                { cmd.Parameters.Add(parameter); }

                var affectedRows = cmd.ExecuteNonQuery();


                watch.Stop();

                entry.TotalElapsed = (int)watch.ElapsedMilliseconds;
                this.Session.Database.OnWriteLog(entry);

                return affectedRows;
            }
            catch (Exception ex)
            {
                watch.Stop();

                entry.Message = ex.Message;
                entry.Exception = ex;
                entry.TotalElapsed = (int)watch.ElapsedMilliseconds;
                this.Session.Database.OnWriteLog(entry);

                throw ex;
            }
        }
    }
}
