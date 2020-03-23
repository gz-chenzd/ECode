using System;
using System.Data;
using Microsoft.Data.Sqlite;

namespace ECode.Data.SQLite
{
    public class SQLiteSession : DbSession
    {
        internal SQLiteSession(SQLiteDatabase database)
            : base(database)
        {

        }


        protected override IDbConnection CreateDbConnection(string connectionString)
        {
            return new SqliteConnection(connectionString);
        }

        protected override DbTransaction BeginDbTransaction(IDbConnection connection)
        {
            var tran = (connection as SqliteConnection).BeginTransaction(IsolationLevel.ReadCommitted);
            return new SQLiteTransaction(this, tran);
        }


        public override ITable<TEntity> Table<TEntity>(object partitionObject = null)
        {
            ThrowIfObjectDisposed();

            return new SQLiteTable<TEntity>(this, this.SchemaManager, this.ShardObject, partitionObject, this.ShardStrategy);
        }


        protected override DbQuerySet<TEntity> CreateQuerySet<TEntity>(DbQueryContext queryContext)
        {
            return new SQLiteQuerySet<TEntity>(this, queryContext);
        }

        protected override DbSortedQuerySet<TEntity> CreateSortedQuerySet<TEntity>(DbQueryContext queryContext)
        {
            return new SQLiteSortedQuerySet<TEntity>(this, queryContext);
        }

        protected override DbGroupedResult<TEntity> CreateGroupedResult<TEntity>(DbQueryContext queryContext)
        {
            return new SQLiteGroupedResult<TEntity>(this, queryContext);
        }

        protected override DbGroupHavingResult<TEntity> CreateGroupHavingResult<TEntity>(DbQueryContext queryContext)
        {
            return new SQLiteGroupHavingResult<TEntity>(this, queryContext);
        }

        protected override DbGroupSortedResult<TEntity> CreateGroupSortedResult<TEntity>(DbQueryContext queryContext)
        {
            return new SQLiteGroupSortedResult<TEntity>(this, queryContext);
        }


        protected override DbJoinedResult<TEntity, TJoin1> CreateJoinedResult<TEntity, TJoin1>(DbQueryContext queryContext)
        {
            return new SQLiteJoinedResult<TEntity, TJoin1>(this, queryContext);
        }

        protected override DbJoinFilterResult<TEntity, TJoin1> CreateJoinFilterResult<TEntity, TJoin1>(DbQueryContext queryContext)
        {
            return new SQLiteJoinFilterResult<TEntity, TJoin1>(this, queryContext);
        }

        protected override DbJoinSortedResult<TEntity, TJoin1> CreateJoinSortedResult<TEntity, TJoin1>(DbQueryContext queryContext)
        {
            return new SQLiteJoinSortedResult<TEntity, TJoin1>(this, queryContext);
        }

        protected override DbJoinPagedResult<TEntity, TJoin1> CreateJoinPagedResult<TEntity, TJoin1>(DbQueryContext queryContext)
        {
            return new SQLiteJoinPagedResult<TEntity, TJoin1>(this, queryContext);
        }

        protected override DbJoinGroupedResult<TEntity, TJoin1> CreateJoinGroupedResult<TEntity, TJoin1>(DbQueryContext queryContext)
        {
            return new SQLiteJoinGroupedResult<TEntity, TJoin1>(this, queryContext);
        }

        protected override DbJoinGroupHavingResult<TEntity, TJoin1> CreateJoinGroupHavingResult<TEntity, TJoin1>(DbQueryContext queryContext)
        {
            return new SQLiteJoinGroupHavingResult<TEntity, TJoin1>(this, queryContext);
        }

        protected override DbJoinGroupSortedResult<TEntity, TJoin1> CreateJoinGroupSortedResult<TEntity, TJoin1>(DbQueryContext queryContext)
        {
            return new SQLiteJoinGroupSortedResult<TEntity, TJoin1>(this, queryContext);
        }


        protected override DbJoinedResult<TEntity, TJoin1, TJoin2> CreateJoinedResult<TEntity, TJoin1, TJoin2>(DbQueryContext queryContext)
        {
            return new SQLiteJoinedResult<TEntity, TJoin1, TJoin2>(this, queryContext);
        }

        protected override DbJoinFilterResult<TEntity, TJoin1, TJoin2> CreateJoinFilterResult<TEntity, TJoin1, TJoin2>(DbQueryContext queryContext)
        {
            return new SQLiteJoinFilterResult<TEntity, TJoin1, TJoin2>(this, queryContext);
        }

        protected override DbJoinSortedResult<TEntity, TJoin1, TJoin2> CreateJoinSortedResult<TEntity, TJoin1, TJoin2>(DbQueryContext queryContext)
        {
            return new SQLiteJoinSortedResult<TEntity, TJoin1, TJoin2>(this, queryContext);
        }

        protected override DbJoinPagedResult<TEntity, TJoin1, TJoin2> CreateJoinPagedResult<TEntity, TJoin1, TJoin2>(DbQueryContext queryContext)
        {
            return new SQLiteJoinPagedResult<TEntity, TJoin1, TJoin2>(this, queryContext);
        }

        protected override DbJoinGroupedResult<TEntity, TJoin1, TJoin2> CreateJoinGroupedResult<TEntity, TJoin1, TJoin2>(DbQueryContext queryContext)
        {
            return new SQLiteJoinGroupedResult<TEntity, TJoin1, TJoin2>(this, queryContext);
        }

        protected override DbJoinGroupHavingResult<TEntity, TJoin1, TJoin2> CreateJoinGroupHavingResult<TEntity, TJoin1, TJoin2>(DbQueryContext queryContext)
        {
            return new SQLiteJoinGroupHavingResult<TEntity, TJoin1, TJoin2>(this, queryContext);
        }

        protected override DbJoinGroupSortedResult<TEntity, TJoin1, TJoin2> CreateJoinGroupSortedResult<TEntity, TJoin1, TJoin2>(DbQueryContext queryContext)
        {
            return new SQLiteJoinGroupSortedResult<TEntity, TJoin1, TJoin2>(this, queryContext);
        }


        protected override DbJoinedResult<TEntity, TJoin1, TJoin2, TJoin3> CreateJoinedResult<TEntity, TJoin1, TJoin2, TJoin3>(DbQueryContext queryContext)
        {
            return new SQLiteJoinedResult<TEntity, TJoin1, TJoin2, TJoin3>(this, queryContext);
        }

        protected override DbJoinFilterResult<TEntity, TJoin1, TJoin2, TJoin3> CreateJoinFilterResult<TEntity, TJoin1, TJoin2, TJoin3>(DbQueryContext queryContext)
        {
            return new SQLiteJoinFilterResult<TEntity, TJoin1, TJoin2, TJoin3>(this, queryContext);
        }

        protected override DbJoinSortedResult<TEntity, TJoin1, TJoin2, TJoin3> CreateJoinSortedResult<TEntity, TJoin1, TJoin2, TJoin3>(DbQueryContext queryContext)
        {
            return new SQLiteJoinSortedResult<TEntity, TJoin1, TJoin2, TJoin3>(this, queryContext);
        }

        protected override DbJoinPagedResult<TEntity, TJoin1, TJoin2, TJoin3> CreateJoinPagedResult<TEntity, TJoin1, TJoin2, TJoin3>(DbQueryContext queryContext)
        {
            return new SQLiteJoinPagedResult<TEntity, TJoin1, TJoin2, TJoin3>(this, queryContext);
        }

        protected override DbJoinGroupedResult<TEntity, TJoin1, TJoin2, TJoin3> CreateJoinGroupedResult<TEntity, TJoin1, TJoin2, TJoin3>(DbQueryContext queryContext)
        {
            return new SQLiteJoinGroupedResult<TEntity, TJoin1, TJoin2, TJoin3>(this, queryContext);
        }

        protected override DbJoinGroupHavingResult<TEntity, TJoin1, TJoin2, TJoin3> CreateJoinGroupHavingResult<TEntity, TJoin1, TJoin2, TJoin3>(DbQueryContext queryContext)
        {
            return new SQLiteJoinGroupHavingResult<TEntity, TJoin1, TJoin2, TJoin3>(this, queryContext);
        }

        protected override DbJoinGroupSortedResult<TEntity, TJoin1, TJoin2, TJoin3> CreateJoinGroupSortedResult<TEntity, TJoin1, TJoin2, TJoin3>(DbQueryContext queryContext)
        {
            return new SQLiteJoinGroupSortedResult<TEntity, TJoin1, TJoin2, TJoin3>(this, queryContext);
        }


        protected override DbJoinedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> CreateJoinedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>(DbQueryContext queryContext)
        {
            return new SQLiteJoinedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>(this, queryContext);
        }

        protected override DbJoinFilterResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> CreateJoinFilterResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>(DbQueryContext queryContext)
        {
            return new SQLiteJoinFilterResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>(this, queryContext);
        }

        protected override DbJoinSortedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> CreateJoinSortedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>(DbQueryContext queryContext)
        {
            return new SQLiteJoinSortedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>(this, queryContext);
        }

        protected override DbJoinPagedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> CreateJoinPagedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>(DbQueryContext queryContext)
        {
            return new SQLiteJoinPagedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>(this, queryContext);
        }

        protected override DbJoinGroupedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> CreateJoinGroupedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>(DbQueryContext queryContext)
        {
            return new SQLiteJoinGroupedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>(this, queryContext);
        }

        protected override DbJoinGroupHavingResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> CreateJoinGroupHavingResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>(DbQueryContext queryContext)
        {
            return new SQLiteJoinGroupHavingResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>(this, queryContext);
        }

        protected override DbJoinGroupSortedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> CreateJoinGroupSortedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>(DbQueryContext queryContext)
        {
            return new SQLiteJoinGroupSortedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>(this, queryContext);
        }
    }
}