using System;
using System.Linq.Expressions;
using ECode.Utility;

namespace ECode.Data
{
    public abstract class DbJoinedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> : IJoinedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>
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


        public IQuerySet<TSelect> Select<TSelect>(Expression<Func<TEntity, TJoin1, TJoin2, TJoin3, TJoin4, TSelect>> selectExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetSelect);
            queryContext.SetSelect(selectExpression);

            return this.Session.CreateQuerySet<TSelect>(queryContext);
        }

        public IJoinFilterResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> Where(Expression<Func<TEntity, TJoin1, TJoin2, TJoin3, TJoin4, bool>> whereExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetWhere);
            queryContext.SetWhere(whereExpression);

            return this.Session.CreateJoinFilterResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>(queryContext);
        }

        public IJoinGroupedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> GroupBy(Expression<Func<TEntity, TJoin1, TJoin2, TJoin3, TJoin4, object[]>> groupByExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetGroupBy);
            queryContext.SetGroupBy(groupByExpression);

            return this.Session.CreateJoinGroupedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>(queryContext);
        }

        public IJoinSortedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> OrderBy(Expression<Func<TEntity, TJoin1, TJoin2, TJoin3, TJoin4, object[]>> orderByExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetOrderBy);
            queryContext.SetOrderBy(orderByExpression);

            return this.Session.CreateJoinSortedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>(queryContext);
        }
    }


    public abstract class DbJoinFilterResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> : IJoinFilterResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>
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


        public IQuerySet<TSelect> Select<TSelect>(Expression<Func<TEntity, TJoin1, TJoin2, TJoin3, TJoin4, TSelect>> selectExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetSelect);
            queryContext.SetSelect(selectExpression);

            return this.Session.CreateQuerySet<TSelect>(queryContext);
        }

        public IJoinFilterResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> Where(Expression<Func<TEntity, TJoin1, TJoin2, TJoin3, TJoin4, bool>> whereExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetWhere);
            queryContext.SetWhere(whereExpression);

            return this.Session.CreateJoinFilterResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>(queryContext);
        }

        public IJoinGroupedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> GroupBy(Expression<Func<TEntity, TJoin1, TJoin2, TJoin3, TJoin4, object[]>> groupByExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetGroupBy);
            queryContext.SetGroupBy(groupByExpression);

            return this.Session.CreateJoinGroupedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>(queryContext);
        }

        public IJoinSortedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> OrderBy(Expression<Func<TEntity, TJoin1, TJoin2, TJoin3, TJoin4, object[]>> orderByExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetOrderBy);
            queryContext.SetOrderBy(orderByExpression);

            return this.Session.CreateJoinSortedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>(queryContext);
        }
    }


    public abstract class DbJoinSortedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> : IJoinSortedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>
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


        public IJoinPagedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> Paging(uint offset, uint count)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetPaging);
            queryContext.SetPaging(offset, count);

            return this.Session.CreateJoinPagedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>(queryContext);
        }

        public IQuerySet<TSelect> Select<TSelect>(Expression<Func<TEntity, TJoin1, TJoin2, TJoin3, TJoin4, TSelect>> selectExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetSelect);
            queryContext.SetSelect(selectExpression);

            return this.Session.CreateQuerySet<TSelect>(queryContext);
        }
    }


    public abstract class DbJoinPagedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> : IJoinPagedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>
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


        public IQuerySet<TSelect> Select<TSelect>(Expression<Func<TEntity, TJoin1, TJoin2, TJoin3, TJoin4, TSelect>> selectExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetSelect);
            queryContext.SetSelect(selectExpression);

            return this.Session.CreateQuerySet<TSelect>(queryContext);
        }
    }


    public abstract class DbJoinGroupedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> : IJoinGroupedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>
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


        public IQuerySet<TSelect> Select<TSelect>(Expression<Func<TEntity, TJoin1, TJoin2, TJoin3, TJoin4, TSelect>> selectExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetSelect);
            queryContext.SetSelect(selectExpression);

            return this.Session.CreateQuerySet<TSelect>(queryContext);
        }

        public IJoinGroupHavingResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> Having(Expression<Func<TEntity, TJoin1, TJoin2, TJoin3, TJoin4, bool>> havingExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetHaving);
            queryContext.SetHaving(havingExpression);

            return this.Session.CreateJoinGroupHavingResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>(queryContext);
        }

        public IJoinGroupSortedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> OrderBy(Expression<Func<TEntity, TJoin1, TJoin2, TJoin3, TJoin4, object[]>> orderByExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetOrderBy);
            queryContext.SetOrderBy(orderByExpression);

            return this.Session.CreateJoinGroupSortedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>(queryContext);
        }
    }


    public abstract class DbJoinGroupHavingResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> : IJoinGroupHavingResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>
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


        public IQuerySet<TSelect> Select<TSelect>(Expression<Func<TEntity, TJoin1, TJoin2, TJoin3, TJoin4, TSelect>> selectExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetSelect);
            queryContext.SetSelect(selectExpression);

            return this.Session.CreateQuerySet<TSelect>(queryContext);
        }

        public IJoinGroupSortedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> OrderBy(Expression<Func<TEntity, TJoin1, TJoin2, TJoin3, TJoin4, object[]>> orderByExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetOrderBy);
            queryContext.SetOrderBy(orderByExpression);

            return this.Session.CreateJoinGroupSortedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>(queryContext);
        }
    }


    public abstract class DbJoinGroupSortedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> : IJoinGroupSortedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>
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


        public IQuerySet<TSelect> Select<TSelect>(Expression<Func<TEntity, TJoin1, TJoin2, TJoin3, TJoin4, TSelect>> selectExpression)
        {
            var queryContext = this.QueryContext.SnapshotForAction(DbQueryAction.SetSelect);
            queryContext.SetSelect(selectExpression);

            return this.Session.CreateQuerySet<TSelect>(queryContext);
        }
    }
}
