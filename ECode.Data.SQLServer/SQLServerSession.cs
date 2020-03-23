using System;
using System.Data;
using System.Data.SqlClient;

namespace ECode.Data.SQLServer
{
    public class SQLServerSession : DbSession
    {
        internal SQLServerSession(SQLServerDatabase database)
            : base(database)
        {

        }


        protected override IDbConnection CreateDbConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }

        protected override DbTransaction BeginDbTransaction(IDbConnection connection)
        {
            var tran = (connection as SqlConnection).BeginTransaction(IsolationLevel.ReadCommitted);
            return new SQLServerTransaction(this, tran);
        }


        public override ITable<TEntity> Table<TEntity>(object partitionObject = null)
        {
            ThrowIfObjectDisposed();

            return new SQLServerTable<TEntity>(this, this.SchemaManager, this.ShardObject, partitionObject, this.ShardStrategy);
        }


        protected override DbQuerySet<TEntity> CreateQuerySet<TEntity>(DbQueryContext queryContext)
        {
            return new SQLServerQuerySet<TEntity>(this, queryContext);
        }

        protected override DbSortedQuerySet<TEntity> CreateSortedQuerySet<TEntity>(DbQueryContext queryContext)
        {
            return new SQLServerSortedQuerySet<TEntity>(this, queryContext);
        }

        protected override DbGroupedResult<TEntity> CreateGroupedResult<TEntity>(DbQueryContext queryContext)
        {
            return new SQLServerGroupedResult<TEntity>(this, queryContext);
        }

        protected override DbGroupHavingResult<TEntity> CreateGroupHavingResult<TEntity>(DbQueryContext queryContext)
        {
            return new SQLServerGroupHavingResult<TEntity>(this, queryContext);
        }

        protected override DbGroupSortedResult<TEntity> CreateGroupSortedResult<TEntity>(DbQueryContext queryContext)
        {
            return new SQLServerGroupSortedResult<TEntity>(this, queryContext);
        }


        protected override DbJoinedResult<TEntity, TJoin1> CreateJoinedResult<TEntity, TJoin1>(DbQueryContext queryContext)
        {
            return new SQLServerJoinedResult<TEntity, TJoin1>(this, queryContext);
        }

        protected override DbJoinFilterResult<TEntity, TJoin1> CreateJoinFilterResult<TEntity, TJoin1>(DbQueryContext queryContext)
        {
            return new SQLServerJoinFilterResult<TEntity, TJoin1>(this, queryContext);
        }

        protected override DbJoinSortedResult<TEntity, TJoin1> CreateJoinSortedResult<TEntity, TJoin1>(DbQueryContext queryContext)
        {
            return new SQLServerJoinSortedResult<TEntity, TJoin1>(this, queryContext);
        }

        protected override DbJoinPagedResult<TEntity, TJoin1> CreateJoinPagedResult<TEntity, TJoin1>(DbQueryContext queryContext)
        {
            return new SQLServerJoinPagedResult<TEntity, TJoin1>(this, queryContext);
        }

        protected override DbJoinGroupedResult<TEntity, TJoin1> CreateJoinGroupedResult<TEntity, TJoin1>(DbQueryContext queryContext)
        {
            return new SQLServerJoinGroupedResult<TEntity, TJoin1>(this, queryContext);
        }

        protected override DbJoinGroupHavingResult<TEntity, TJoin1> CreateJoinGroupHavingResult<TEntity, TJoin1>(DbQueryContext queryContext)
        {
            return new SQLServerJoinGroupHavingResult<TEntity, TJoin1>(this, queryContext);
        }

        protected override DbJoinGroupSortedResult<TEntity, TJoin1> CreateJoinGroupSortedResult<TEntity, TJoin1>(DbQueryContext queryContext)
        {
            return new SQLServerJoinGroupSortedResult<TEntity, TJoin1>(this, queryContext);
        }


        protected override DbJoinedResult<TEntity, TJoin1, TJoin2> CreateJoinedResult<TEntity, TJoin1, TJoin2>(DbQueryContext queryContext)
        {
            return new SQLServerJoinedResult<TEntity, TJoin1, TJoin2>(this, queryContext);
        }

        protected override DbJoinFilterResult<TEntity, TJoin1, TJoin2> CreateJoinFilterResult<TEntity, TJoin1, TJoin2>(DbQueryContext queryContext)
        {
            return new SQLServerJoinFilterResult<TEntity, TJoin1, TJoin2>(this, queryContext);
        }

        protected override DbJoinSortedResult<TEntity, TJoin1, TJoin2> CreateJoinSortedResult<TEntity, TJoin1, TJoin2>(DbQueryContext queryContext)
        {
            return new SQLServerJoinSortedResult<TEntity, TJoin1, TJoin2>(this, queryContext);
        }

        protected override DbJoinPagedResult<TEntity, TJoin1, TJoin2> CreateJoinPagedResult<TEntity, TJoin1, TJoin2>(DbQueryContext queryContext)
        {
            return new SQLServerJoinPagedResult<TEntity, TJoin1, TJoin2>(this, queryContext);
        }

        protected override DbJoinGroupedResult<TEntity, TJoin1, TJoin2> CreateJoinGroupedResult<TEntity, TJoin1, TJoin2>(DbQueryContext queryContext)
        {
            return new SQLServerJoinGroupedResult<TEntity, TJoin1, TJoin2>(this, queryContext);
        }

        protected override DbJoinGroupHavingResult<TEntity, TJoin1, TJoin2> CreateJoinGroupHavingResult<TEntity, TJoin1, TJoin2>(DbQueryContext queryContext)
        {
            return new SQLServerJoinGroupHavingResult<TEntity, TJoin1, TJoin2>(this, queryContext);
        }

        protected override DbJoinGroupSortedResult<TEntity, TJoin1, TJoin2> CreateJoinGroupSortedResult<TEntity, TJoin1, TJoin2>(DbQueryContext queryContext)
        {
            return new SQLServerJoinGroupSortedResult<TEntity, TJoin1, TJoin2>(this, queryContext);
        }


        protected override DbJoinedResult<TEntity, TJoin1, TJoin2, TJoin3> CreateJoinedResult<TEntity, TJoin1, TJoin2, TJoin3>(DbQueryContext queryContext)
        {
            return new SQLServerJoinedResult<TEntity, TJoin1, TJoin2, TJoin3>(this, queryContext);
        }

        protected override DbJoinFilterResult<TEntity, TJoin1, TJoin2, TJoin3> CreateJoinFilterResult<TEntity, TJoin1, TJoin2, TJoin3>(DbQueryContext queryContext)
        {
            return new SQLServerJoinFilterResult<TEntity, TJoin1, TJoin2, TJoin3>(this, queryContext);
        }

        protected override DbJoinSortedResult<TEntity, TJoin1, TJoin2, TJoin3> CreateJoinSortedResult<TEntity, TJoin1, TJoin2, TJoin3>(DbQueryContext queryContext)
        {
            return new SQLServerJoinSortedResult<TEntity, TJoin1, TJoin2, TJoin3>(this, queryContext);
        }

        protected override DbJoinPagedResult<TEntity, TJoin1, TJoin2, TJoin3> CreateJoinPagedResult<TEntity, TJoin1, TJoin2, TJoin3>(DbQueryContext queryContext)
        {
            return new SQLServerJoinPagedResult<TEntity, TJoin1, TJoin2, TJoin3>(this, queryContext);
        }

        protected override DbJoinGroupedResult<TEntity, TJoin1, TJoin2, TJoin3> CreateJoinGroupedResult<TEntity, TJoin1, TJoin2, TJoin3>(DbQueryContext queryContext)
        {
            return new SQLServerJoinGroupedResult<TEntity, TJoin1, TJoin2, TJoin3>(this, queryContext);
        }

        protected override DbJoinGroupHavingResult<TEntity, TJoin1, TJoin2, TJoin3> CreateJoinGroupHavingResult<TEntity, TJoin1, TJoin2, TJoin3>(DbQueryContext queryContext)
        {
            return new SQLServerJoinGroupHavingResult<TEntity, TJoin1, TJoin2, TJoin3>(this, queryContext);
        }

        protected override DbJoinGroupSortedResult<TEntity, TJoin1, TJoin2, TJoin3> CreateJoinGroupSortedResult<TEntity, TJoin1, TJoin2, TJoin3>(DbQueryContext queryContext)
        {
            return new SQLServerJoinGroupSortedResult<TEntity, TJoin1, TJoin2, TJoin3>(this, queryContext);
        }


        protected override DbJoinedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> CreateJoinedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>(DbQueryContext queryContext)
        {
            return new SQLServerJoinedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>(this, queryContext);
        }

        protected override DbJoinFilterResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> CreateJoinFilterResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>(DbQueryContext queryContext)
        {
            return new SQLServerJoinFilterResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>(this, queryContext);
        }

        protected override DbJoinSortedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> CreateJoinSortedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>(DbQueryContext queryContext)
        {
            return new SQLServerJoinSortedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>(this, queryContext);
        }

        protected override DbJoinPagedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> CreateJoinPagedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>(DbQueryContext queryContext)
        {
            return new SQLServerJoinPagedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>(this, queryContext);
        }

        protected override DbJoinGroupedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> CreateJoinGroupedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>(DbQueryContext queryContext)
        {
            return new SQLServerJoinGroupedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>(this, queryContext);
        }

        protected override DbJoinGroupHavingResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> CreateJoinGroupHavingResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>(DbQueryContext queryContext)
        {
            return new SQLServerJoinGroupHavingResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>(this, queryContext);
        }

        protected override DbJoinGroupSortedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> CreateJoinGroupSortedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>(DbQueryContext queryContext)
        {
            return new SQLServerJoinGroupSortedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>(this, queryContext);
        }
    }
}