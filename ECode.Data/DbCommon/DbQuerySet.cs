using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using ECode.Json;
using ECode.TypeConversion;
using ECode.Utility;

namespace ECode.Data
{
    public abstract class DbQuerySet<TEntity> : IQuerySet<TEntity>
    {
        private bool?                   m_IsAtomicType          = null;
        private List<PropertyInfo>      m_pFieldProperties      = null;
        private bool                    m_IsAnonymousType       = typeof(TEntity).Name.StartsWith("<>f__AnonymousType");


        ISession IQuerySet.Session => this.Session;

        protected internal DbSession Session
        { get; private set; }

        protected internal DbQueryContext QueryContext
        { get; protected set; }


        protected DbQuerySet(DbSession session)
        {
            AssertUtil.ArgumentNotNull(session, nameof(session));

            this.Session = session;
        }

        protected DbQuerySet(DbSession session, DbQueryContext queryContext)
            : this(session)
        {
            AssertUtil.ArgumentNotNull(session, nameof(session));

            this.QueryContext = queryContext;
        }


        protected abstract ExpressionParser GetExpressionParser();

        protected string ParseSqlForUnitTest(ExpressionParser parser, IList<IDataParameter> parameters)
        {
            IList<string> selectFields = null;
            return parser.Parse(this.QueryContext, parameters, out selectFields);
        }


        public int Count()
        {
            var resultSet = this.Select(t => SqlAggrFunc.Count());
            var queryContext = (resultSet as DbQuerySet<int>).QueryContext;

            var entry = new LogEntry();
            entry.SessionID = this.Session.ID;
            entry.CommandType = CommandType.Count;

            var watch = new Stopwatch();
            watch.Start();

            try
            {
                var parameters = new List<IDataParameter>();

                IList<string> selectFields = null;
                entry.CommandText = GetExpressionParser().Parse(queryContext, parameters, out selectFields);
                entry.ParseElapsed = (int)watch.ElapsedMilliseconds;

                var conn = this.Session.GetDbConnection(false);
                if (conn is System.Data.Common.DbConnection)
                {
                    var db_conn = (System.Data.Common.DbConnection)conn;
                    entry.Server = db_conn.DataSource;
                    entry.Database = db_conn.Database;
                }

                var cmd = conn.CreateCommand();
                cmd.Transaction = this.Session.GetDbTransaction();

                entry.TransactionID = this.Session.GetActiveTransaction()?.ID;

                cmd.CommandText = entry.CommandText;
                foreach (IDataParameter parameter in parameters)
                { cmd.Parameters.Add(parameter); }

                var result = (int)TypeConversionUtil.ConvertValueIfNecessary(typeof(int), cmd.ExecuteScalar());

                watch.Stop();

                entry.TotalElapsed = (int)watch.ElapsedMilliseconds;
                this.Session.Database.OnWriteLog(entry);

                return result;
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

        public TEntity First()
        {
            var entry = new LogEntry();
            entry.SessionID = this.Session.ID;
            entry.CommandType = CommandType.First;

            var watch = new Stopwatch();
            watch.Start();

            try
            {
                this.QueryContext.SelectFirst = true;

                var parameters = new List<IDataParameter>();

                IList<string> selectFields = null;
                entry.CommandText = GetExpressionParser().Parse(this.QueryContext, parameters, out selectFields);
                entry.ParseElapsed = (int)watch.ElapsedMilliseconds;

                var conn = this.Session.GetDbConnection(false);
                if (conn is System.Data.Common.DbConnection)
                {
                    var db_conn = (System.Data.Common.DbConnection)conn;
                    entry.Server = db_conn.DataSource;
                    entry.Database = db_conn.Database;
                }

                var cmd = conn.CreateCommand();
                cmd.Transaction = this.Session.GetDbTransaction();

                entry.TransactionID = this.Session.GetActiveTransaction()?.ID;

                cmd.CommandText = entry.CommandText;
                foreach (IDataParameter parameter in parameters)
                { cmd.Parameters.Add(parameter); }

                TEntity result = default(TEntity);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        result = ReadObject(reader);
                    }
                }

                watch.Stop();

                entry.AffectedRows = result == null ? 0 : 1;
                entry.TotalElapsed = (int)watch.ElapsedMilliseconds;
                this.Session.Database.OnWriteLog(entry);

                return result;
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

        public IList<TEntity> ToList()
        {
            var entry = new LogEntry();
            entry.SessionID = this.Session.ID;
            entry.CommandType = CommandType.List;

            var watch = new Stopwatch();
            watch.Start();

            try
            {
                this.QueryContext.SelectFirst = false;

                var parameters = new List<IDataParameter>();

                IList<string> selectFields = null;
                entry.CommandText = GetExpressionParser().Parse(this.QueryContext, parameters, out selectFields);
                entry.ParseElapsed = (int)watch.ElapsedMilliseconds;

                var conn = this.Session.GetDbConnection(false);
                if (conn is System.Data.Common.DbConnection)
                {
                    var db_conn = (System.Data.Common.DbConnection)conn;
                    entry.Server = db_conn.DataSource;
                    entry.Database = db_conn.Database;
                }

                var cmd = conn.CreateCommand();
                cmd.Transaction = this.Session.GetDbTransaction();

                entry.TransactionID = this.Session.GetActiveTransaction()?.ID;

                cmd.CommandText = entry.CommandText;
                foreach (IDataParameter parameter in parameters)
                { cmd.Parameters.Add(parameter); }

                var list = new List<TEntity>();
                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(ReadObject(reader));
                    }
                }

                watch.Stop();

                entry.AffectedRows = list.Count;
                entry.TotalElapsed = (int)watch.ElapsedMilliseconds;
                this.Session.Database.OnWriteLog(entry);

                return list;
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

        private bool IsAtomicType(Type t)
        {
            if (t.IsPrimitive || t.IsEnum)
            { return true; }

            if (typeof(Boolean) == t)
            { return true; }

            if (typeof(Char) == t)
            { return true; }

            if (typeof(String) == t)
            { return true; }

            if (typeof(Byte) == t || typeof(SByte) == t || t.IsAssignableFrom(typeof(byte[])))
            { return true; }

            if (typeof(Int16) == t || typeof(UInt16) == t)
            { return true; }

            if (typeof(Int32) == t || typeof(UInt32) == t)
            { return true; }

            if (typeof(Int64) == t || typeof(UInt64) == t)
            { return true; }

            if (typeof(Single) == t || typeof(Double) == t)
            { return true; }

            if (typeof(Decimal) == t)
            { return true; }

            if (typeof(DateTime) == t)
            { return true; }

            if (typeof(Guid) == t)
            { return true; }

            return false;
        }

        private TEntity ReadObject(IDataReader reader)
        {
            if (reader.FieldCount == 1 && !m_IsAnonymousType)
            {
                if (!m_IsAtomicType.HasValue)
                { m_IsAtomicType = IsAtomicType(typeof(TEntity)); }

                if (m_IsAtomicType.Value)
                {
                    var value = reader.GetValue(0);
                    if (value != null && value != DBNull.Value && !typeof(TEntity).IsAssignableFrom(value.GetType()))
                    {
                        value = TypeConversionUtil.ConvertValueIfNecessary(typeof(TEntity), value);
                    }

                    return (TEntity)(value == DBNull.Value ? null : value);
                }
                //else if (reader.GetDataTypeName(0).ToUpper() == "JSON")
                //{
                //    // TODO: ...
                //}
            }

            if (m_pFieldProperties == null)
            {
                var t = typeof(TEntity);
                m_pFieldProperties = new List<PropertyInfo>();

                if (!m_IsAnonymousType)
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var fieldName = reader.GetName(i);
                        var property = t.GetProperty(fieldName);

                        if (property == null || !property.CanWrite)
                        {
                            throw new InvalidOperationException($"Property '{property.Name}' cannot be set on '{t.FullName}'.");
                        }

                        m_pFieldProperties.Add(property);
                    }
                }
                else
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var fieldName = reader.GetName(i);
                        m_pFieldProperties.Add(t.GetProperty(fieldName));
                    }
                }
            }

            if (!m_IsAnonymousType)
            {
                var entity = Activator.CreateInstance<TEntity>();
                for (int i = 0; i < m_pFieldProperties.Count; i++)
                {
                    var value = reader.GetValue(i);
                    if (value != null && value != DBNull.Value && !m_pFieldProperties[i].PropertyType.IsAssignableFrom(value.GetType()))
                    {
                        value = TypeConversionUtil.ConvertValueIfNecessary(m_pFieldProperties[i].PropertyType, value);
                    }

                    m_pFieldProperties[i].SetValue(entity, value == DBNull.Value ? null : value, null);
                }

                return entity;
            }
            else
            {
                var entity = new Dictionary<string, object>();
                for (int i = 0; i < m_pFieldProperties.Count; i++)
                {
                    var value = reader.GetValue(i);
                    entity[m_pFieldProperties[i].Name] = value == DBNull.Value ? null : value;
                }

                return JsonUtil.Deserialize<TEntity>(JsonUtil.Serialize(entity));
            }
        }


        public bool Contains(TEntity value)
        {
            throw new NotImplementedException();
        }

        public bool Exists(Expression<Func<TEntity, bool>> expression)
        {
            throw new NotImplementedException();
        }


        public IQuerySet<TEntity> Distinct()
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetDistinct);
            queryContext.SetDistinct();

            return this.Session.CreateQuerySet<TEntity>(queryContext);
        }

        public IQuerySet<TSelect> Select<TSelect>(Expression<Func<TEntity, TSelect>> selectExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetSelect);
            queryContext.SetSelect(selectExpression);

            return this.Session.CreateQuerySet<TSelect>(queryContext);
        }

        public IQuerySet<TEntity> Where(Expression<Func<TEntity, bool>> whereExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetWhere);
            queryContext.SetWhere(whereExpression);

            return this.Session.CreateQuerySet<TEntity>(queryContext);
        }

        public IGroupedResult<TEntity> GroupBy(Expression<Func<TEntity, object[]>> groupByExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetGroupBy);
            queryContext.SetGroupBy(groupByExpression);

            return this.Session.CreateGroupedResult<TEntity>(queryContext);
        }

        public ISortedQuerySet<TEntity> OrderBy(Expression<Func<TEntity, object[]>> orderByExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetOrderBy);
            queryContext.SetOrderBy(orderByExpression);

            return this.Session.CreateSortedQuerySet<TEntity>(queryContext);
        }


        public IJoinedResult<TEntity, TJoin1> Join<TJoin1>(Expression<Func<TEntity, TJoin1, bool>> onExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetJoin);
            queryContext.AddJoin(this.Session.Table<TJoin1>() as DbTable<TJoin1>, onExpression);

            return this.Session.CreateJoinedResult<TEntity, TJoin1>(queryContext);
        }

        public IJoinedResult<TEntity, TJoin1> Join<TJoin1>(object partitionObject, Expression<Func<TEntity, TJoin1, bool>> onExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetJoin);
            queryContext.AddJoin(this.Session.Table<TJoin1>(partitionObject) as DbTable<TJoin1>, onExpression);

            return this.Session.CreateJoinedResult<TEntity, TJoin1>(queryContext);
        }

        public IJoinedResult<TEntity, TJoin1> Join<TJoin1>(IQuerySet<TJoin1> querySet, Expression<Func<TEntity, TJoin1, bool>> onExpression)
        {
            AssertUtil.ArgumentNotNull(querySet, nameof(querySet));

            if (querySet.Session != this.Session)
            { throw new ArgumentException($"Argument '{querySet}' isnot in the same session."); }

            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetJoin);
            queryContext.AddJoin(querySet as DbQuerySet<TJoin1>, onExpression);

            return this.Session.CreateJoinedResult<TEntity, TJoin1>(queryContext);
        }

        public IJoinedResult<TEntity, TJoin1> LeftJoin<TJoin1>(Expression<Func<TEntity, TJoin1, bool>> onExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetJoin);
            queryContext.AddLeftJoin(this.Session.Table<TJoin1>() as DbTable<TJoin1>, onExpression);

            return this.Session.CreateJoinedResult<TEntity, TJoin1>(queryContext);
        }

        public IJoinedResult<TEntity, TJoin1> LeftJoin<TJoin1>(object partitionObject, Expression<Func<TEntity, TJoin1, bool>> onExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetJoin);
            queryContext.AddLeftJoin(this.Session.Table<TJoin1>(partitionObject) as DbTable<TJoin1>, onExpression);

            return this.Session.CreateJoinedResult<TEntity, TJoin1>(queryContext);
        }

        public IJoinedResult<TEntity, TJoin1> LeftJoin<TJoin1>(IQuerySet<TJoin1> querySet, Expression<Func<TEntity, TJoin1, bool>> onExpression)
        {
            AssertUtil.ArgumentNotNull(querySet, nameof(querySet));

            if (querySet.Session != this.Session)
            { throw new ArgumentException($"Argument '{querySet}' isnot in the same session."); }

            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetJoin);
            queryContext.AddLeftJoin(querySet as DbQuerySet<TJoin1>, onExpression);

            return this.Session.CreateJoinedResult<TEntity, TJoin1>(queryContext);
        }


        public IQuerySet<TEntity> Union(IQuerySet<TEntity> querySet)
        {
            AssertUtil.ArgumentNotNull(querySet, nameof(querySet));

            if (querySet.Session != this.Session)
            { throw new ArgumentException($"Argument '{querySet}' isnot in the same session."); }

            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetUnion);
            queryContext.AddUnion(querySet as DbQuerySet<TEntity>);

            return this.Session.CreateQuerySet<TEntity>(queryContext);
        }

        public IQuerySet<TEntity> UnionAll(IQuerySet<TEntity> querySet)
        {
            AssertUtil.ArgumentNotNull(querySet, nameof(querySet));

            if (querySet.Session != this.Session)
            { throw new ArgumentException($"Argument '{querySet}' isnot in the same session."); }

            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetUnion);
            queryContext.AddUnionAll(querySet as DbQuerySet<TEntity>);

            return this.Session.CreateQuerySet<TEntity>(queryContext);
        }
    }


    public abstract class DbSortedQuerySet<TEntity> : DbQuerySet<TEntity>, ISortedQuerySet<TEntity>
    {
        protected DbSortedQuerySet(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }


        public IQuerySet<TEntity> Paging(uint offset, uint count)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetPaging);
            queryContext.SetPaging(offset, count);

            return this.Session.CreateQuerySet<TEntity>(queryContext);
        }
    }


    public abstract class DbGroupedResult<TEntity> : IGroupedResult<TEntity>
    {
        protected DbSession Session
        { get; private set; }

        protected DbQueryContext QueryContext
        { get; private set; }


        protected DbGroupedResult(DbSession session, DbQueryContext queryContext)
        {
            AssertUtil.ArgumentNotNull(session, nameof(session));
            AssertUtil.ArgumentNotNull(queryContext, nameof(queryContext));

            this.Session = session;
            this.QueryContext = queryContext;
        }


        public IQuerySet<TSelect> Select<TSelect>(Expression<Func<TEntity, TSelect>> selectExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetSelect);
            queryContext.SetSelect(selectExpression);

            return this.Session.CreateQuerySet<TSelect>(queryContext);
        }

        public IGroupHavingResult<TEntity> Having(Expression<Func<TEntity, bool>> havingExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetHaving);
            queryContext.SetHaving(havingExpression);

            return this.Session.CreateGroupHavingResult<TEntity>(queryContext);
        }

        public IGroupSortedResult<TEntity> OrderBy(Expression<Func<TEntity, object[]>> orderByExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetOrderBy);
            queryContext.SetOrderBy(orderByExpression);

            return this.Session.CreateGroupSortedResult<TEntity>(queryContext);
        }
    }


    public abstract class DbGroupSortedResult<TEntity> : IGroupSortedResult<TEntity>
    {
        protected DbSession Session
        { get; private set; }

        protected DbQueryContext QueryContext
        { get; private set; }


        protected DbGroupSortedResult(DbSession session, DbQueryContext queryContext)
        {
            AssertUtil.ArgumentNotNull(session, nameof(session));
            AssertUtil.ArgumentNotNull(queryContext, nameof(queryContext));

            this.Session = session;
            this.QueryContext = queryContext;
        }


        public IQuerySet<TSelect> Select<TSelect>(Expression<Func<TEntity, TSelect>> selectExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetSelect);
            queryContext.SetSelect(selectExpression);

            return this.Session.CreateQuerySet<TSelect>(queryContext);
        }
    }


    public abstract class DbGroupHavingResult<TEntity> : IGroupHavingResult<TEntity>
    {
        protected DbSession Session
        { get; private set; }

        protected DbQueryContext QueryContext
        { get; private set; }


        protected DbGroupHavingResult(DbSession session, DbQueryContext queryContext)
        {
            AssertUtil.ArgumentNotNull(session, nameof(session));
            AssertUtil.ArgumentNotNull(queryContext, nameof(queryContext));

            this.Session = session;
            this.QueryContext = queryContext;
        }


        public IQuerySet<TSelect> Select<TSelect>(Expression<Func<TEntity, TSelect>> selectExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetSelect);
            queryContext.SetSelect(selectExpression);

            return this.Session.CreateQuerySet<TSelect>(queryContext);
        }

        public IGroupSortedResult<TEntity> OrderBy(Expression<Func<TEntity, object[]>> orderByExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetOrderBy);
            queryContext.SetOrderBy(orderByExpression);

            return this.Session.CreateGroupSortedResult<TEntity>(queryContext);
        }
    }
}