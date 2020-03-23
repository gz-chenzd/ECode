using System;
using System.Linq.Expressions;
using ECode.Utility;

namespace ECode.Data
{
    public abstract class DbJoinedResult<TEntity, TJoin1> : IJoinedResult<TEntity, TJoin1>
    {
        protected DbSession Session
        { get; private set; }

        protected DbQueryContext QueryContext
        { get; private set; }


        protected DbJoinedResult(DbSession session, DbQueryContext queryContext)
        {
            AssertUtil.ArgumentNotNull(session, nameof(session));
            AssertUtil.ArgumentNotNull(queryContext, nameof(queryContext));

            this.Session = session;
            this.QueryContext = queryContext;
        }


        public IQuerySet<TSelect> Select<TSelect>(Expression<Func<TEntity, TJoin1, TSelect>> selectExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetSelect);
            queryContext.SetSelect(selectExpression);

            return this.Session.CreateQuerySet<TSelect>(queryContext);
        }

        public IJoinFilterResult<TEntity, TJoin1> Where(Expression<Func<TEntity, TJoin1, bool>> whereExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetWhere);
            queryContext.SetWhere(whereExpression);

            return this.Session.CreateJoinFilterResult<TEntity, TJoin1>(queryContext);
        }

        public IJoinGroupedResult<TEntity, TJoin1> GroupBy(Expression<Func<TEntity, TJoin1, object[]>> groupByExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetGroupBy);
            queryContext.SetGroupBy(groupByExpression);

            return this.Session.CreateJoinGroupedResult<TEntity, TJoin1>(queryContext);
        }

        public IJoinSortedResult<TEntity, TJoin1> OrderBy(Expression<Func<TEntity, TJoin1, object[]>> orderByExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetOrderBy);
            queryContext.SetOrderBy(orderByExpression);

            return this.Session.CreateJoinSortedResult<TEntity, TJoin1>(queryContext);
        }


        public IJoinedResult<TEntity, TJoin1, TJoin2> Join<TJoin2>(Expression<Func<TEntity, TJoin1, TJoin2, bool>> onExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetJoin);
            queryContext.AddJoin(this.Session.Table<TJoin2>() as DbTable<TJoin2>, onExpression);

            return this.Session.CreateJoinedResult<TEntity, TJoin1, TJoin2>(queryContext);
        }

        public IJoinedResult<TEntity, TJoin1, TJoin2> Join<TJoin2>(object partitionObject, Expression<Func<TEntity, TJoin1, TJoin2, bool>> onExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetJoin);
            queryContext.AddJoin(this.Session.Table<TJoin2>(partitionObject) as DbTable<TJoin2>, onExpression);

            return this.Session.CreateJoinedResult<TEntity, TJoin1, TJoin2>(queryContext);
        }

        public IJoinedResult<TEntity, TJoin1, TJoin2> Join<TJoin2>(IQuerySet<TJoin2> querySet, Expression<Func<TEntity, TJoin1, TJoin2, bool>> onExpression)
        {
            AssertUtil.ArgumentNotNull(querySet, nameof(querySet));

            if (querySet.Session != this.Session)
            { throw new ArgumentException($"Argument '{querySet}' isnot in the same session."); }

            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetJoin);
            queryContext.AddJoin(querySet as DbQuerySet<TJoin2>, onExpression);

            return this.Session.CreateJoinedResult<TEntity, TJoin1, TJoin2>(queryContext);
        }

        public IJoinedResult<TEntity, TJoin1, TJoin2> LeftJoin<TJoin2>(Expression<Func<TEntity, TJoin1, TJoin2, bool>> onExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetJoin);
            queryContext.AddLeftJoin(this.Session.Table<TJoin2>() as DbTable<TJoin2>, onExpression);

            return this.Session.CreateJoinedResult<TEntity, TJoin1, TJoin2>(queryContext);
        }

        public IJoinedResult<TEntity, TJoin1, TJoin2> LeftJoin<TJoin2>(object partitionObject, Expression<Func<TEntity, TJoin1, TJoin2, bool>> onExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetJoin);
            queryContext.AddLeftJoin(this.Session.Table<TJoin2>(partitionObject) as DbTable<TJoin2>, onExpression);

            return this.Session.CreateJoinedResult<TEntity, TJoin1, TJoin2>(queryContext);
        }

        public IJoinedResult<TEntity, TJoin1, TJoin2> LeftJoin<TJoin2>(IQuerySet<TJoin2> querySet, Expression<Func<TEntity, TJoin1, TJoin2, bool>> onExpression)
        {
            AssertUtil.ArgumentNotNull(querySet, nameof(querySet));

            if (querySet.Session != this.Session)
            { throw new ArgumentException($"Argument '{querySet}' isnot in the same session."); }

            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetJoin);
            queryContext.AddLeftJoin(querySet as DbQuerySet<TJoin2>, onExpression);

            return this.Session.CreateJoinedResult<TEntity, TJoin1, TJoin2>(queryContext);
        }
    }


    public abstract class DbJoinFilterResult<TEntity, TJoin1> : IJoinFilterResult<TEntity, TJoin1>
    {
        protected DbSession Session
        { get; private set; }

        protected DbQueryContext QueryContext
        { get; private set; }


        protected DbJoinFilterResult(DbSession session, DbQueryContext queryContext)
        {
            AssertUtil.ArgumentNotNull(session, nameof(session));
            AssertUtil.ArgumentNotNull(queryContext, nameof(queryContext));

            this.Session = session;
            this.QueryContext = queryContext;
        }


        public IQuerySet<TSelect> Select<TSelect>(Expression<Func<TEntity, TJoin1, TSelect>> selectExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetSelect);
            queryContext.SetSelect(selectExpression);

            return this.Session.CreateQuerySet<TSelect>(queryContext);
        }

        public IJoinFilterResult<TEntity, TJoin1> Where(Expression<Func<TEntity, TJoin1, bool>> whereExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetWhere);
            queryContext.SetWhere(whereExpression);

            return this.Session.CreateJoinFilterResult<TEntity, TJoin1>(queryContext);
        }

        public IJoinGroupedResult<TEntity, TJoin1> GroupBy(Expression<Func<TEntity, TJoin1, object[]>> groupByExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetGroupBy);
            queryContext.SetGroupBy(groupByExpression);

            return this.Session.CreateJoinGroupedResult<TEntity, TJoin1>(queryContext);
        }

        public IJoinSortedResult<TEntity, TJoin1> OrderBy(Expression<Func<TEntity, TJoin1, object[]>> orderByExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetOrderBy);
            queryContext.SetOrderBy(orderByExpression);

            return this.Session.CreateJoinSortedResult<TEntity, TJoin1>(queryContext);
        }
    }


    public abstract class DbJoinSortedResult<TEntity, TJoin1> : IJoinSortedResult<TEntity, TJoin1>
    {
        protected DbSession Session
        { get; private set; }

        protected DbQueryContext QueryContext
        { get; private set; }


        protected DbJoinSortedResult(DbSession session, DbQueryContext queryContext)
        {
            AssertUtil.ArgumentNotNull(session, nameof(session));
            AssertUtil.ArgumentNotNull(queryContext, nameof(queryContext));

            this.Session = session;
            this.QueryContext = queryContext;
        }


        public IJoinPagedResult<TEntity, TJoin1> Paging(uint offset, uint count)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetPaging);
            queryContext.SetPaging(offset, count);

            return this.Session.CreateJoinPagedResult<TEntity, TJoin1>(queryContext);
        }

        public IQuerySet<TSelect> Select<TSelect>(Expression<Func<TEntity, TJoin1, TSelect>> selectExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetSelect);
            queryContext.SetSelect(selectExpression);

            return this.Session.CreateQuerySet<TSelect>(queryContext);
        }
    }


    public abstract class DbJoinPagedResult<TEntity, TJoin1> : IJoinPagedResult<TEntity, TJoin1>
    {
        protected DbSession Session
        { get; private set; }

        protected DbQueryContext QueryContext
        { get; private set; }


        protected DbJoinPagedResult(DbSession session, DbQueryContext queryContext)
        {
            AssertUtil.ArgumentNotNull(session, nameof(session));
            AssertUtil.ArgumentNotNull(queryContext, nameof(queryContext));

            this.Session = session;
            this.QueryContext = queryContext;
        }


        public IQuerySet<TSelect> Select<TSelect>(Expression<Func<TEntity, TJoin1, TSelect>> selectExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetSelect);
            queryContext.SetSelect(selectExpression);

            return this.Session.CreateQuerySet<TSelect>(queryContext);
        }
    }


    public abstract class DbJoinGroupedResult<TEntity, TJoin1> : IJoinGroupedResult<TEntity, TJoin1>
    {
        protected DbSession Session
        { get; private set; }

        protected DbQueryContext QueryContext
        { get; private set; }


        protected DbJoinGroupedResult(DbSession session, DbQueryContext queryContext)
        {
            AssertUtil.ArgumentNotNull(session, nameof(session));
            AssertUtil.ArgumentNotNull(queryContext, nameof(queryContext));

            this.Session = session;
            this.QueryContext = queryContext;
        }


        public IQuerySet<TSelect> Select<TSelect>(Expression<Func<TEntity, TJoin1, TSelect>> selectExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetSelect);
            queryContext.SetSelect(selectExpression);

            return this.Session.CreateQuerySet<TSelect>(queryContext);
        }

        public IJoinGroupHavingResult<TEntity, TJoin1> Having(Expression<Func<TEntity, TJoin1, bool>> havingExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetHaving);
            queryContext.SetHaving(havingExpression);

            return this.Session.CreateJoinGroupHavingResult<TEntity, TJoin1>(queryContext);
        }

        public IJoinGroupSortedResult<TEntity, TJoin1> OrderBy(Expression<Func<TEntity, TJoin1, object[]>> orderByExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetOrderBy);
            queryContext.SetOrderBy(orderByExpression);

            return this.Session.CreateJoinGroupSortedResult<TEntity, TJoin1>(queryContext);
        }
    }


    public abstract class DbJoinGroupHavingResult<TEntity, TJoin1> : IJoinGroupHavingResult<TEntity, TJoin1>
    {
        protected DbSession Session
        { get; private set; }

        protected DbQueryContext QueryContext
        { get; private set; }


        protected DbJoinGroupHavingResult(DbSession session, DbQueryContext queryContext)
        {
            AssertUtil.ArgumentNotNull(session, nameof(session));
            AssertUtil.ArgumentNotNull(queryContext, nameof(queryContext));

            this.Session = session;
            this.QueryContext = queryContext;
        }


        public IQuerySet<TSelect> Select<TSelect>(Expression<Func<TEntity, TJoin1, TSelect>> selectExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetSelect);
            queryContext.SetSelect(selectExpression);

            return this.Session.CreateQuerySet<TSelect>(queryContext);
        }

        public IJoinGroupSortedResult<TEntity, TJoin1> OrderBy(Expression<Func<TEntity, TJoin1, object[]>> orderByExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetOrderBy);
            queryContext.SetOrderBy(orderByExpression);

            return this.Session.CreateJoinGroupSortedResult<TEntity, TJoin1>(queryContext);
        }
    }


    public abstract class DbJoinGroupSortedResult<TEntity, TJoin1> : IJoinGroupSortedResult<TEntity, TJoin1>
    {
        protected DbSession Session
        { get; private set; }

        protected DbQueryContext QueryContext
        { get; private set; }


        protected DbJoinGroupSortedResult(DbSession session, DbQueryContext queryContext)
        {
            AssertUtil.ArgumentNotNull(session, nameof(session));
            AssertUtil.ArgumentNotNull(queryContext, nameof(queryContext));

            this.Session = session;
            this.QueryContext = queryContext;
        }


        public IQuerySet<TSelect> Select<TSelect>(Expression<Func<TEntity, TJoin1, TSelect>> selectExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetSelect);
            queryContext.SetSelect(selectExpression);

            return this.Session.CreateQuerySet<TSelect>(queryContext);
        }
    }
}
