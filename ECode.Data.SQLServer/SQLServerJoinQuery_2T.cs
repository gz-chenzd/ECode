
namespace ECode.Data.SQLServer
{
    public class SQLServerJoinedResult<TEntity, TJoin1> : DbJoinedResult<TEntity, TJoin1>
    {
        internal SQLServerJoinedResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }


    public class SQLServerJoinFilterResult<TEntity, TJoin1> : DbJoinFilterResult<TEntity, TJoin1>
    {
        internal SQLServerJoinFilterResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }


    public class SQLServerJoinSortedResult<TEntity, TJoin1> : DbJoinSortedResult<TEntity, TJoin1>
    {
        internal SQLServerJoinSortedResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }


    public class SQLServerJoinPagedResult<TEntity, TJoin1> : DbJoinPagedResult<TEntity, TJoin1>
    {
        internal SQLServerJoinPagedResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }


    public class SQLServerJoinGroupedResult<TEntity, TJoin1> : DbJoinGroupedResult<TEntity, TJoin1>
    {
        internal SQLServerJoinGroupedResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }


    public class SQLServerJoinGroupHavingResult<TEntity, TJoin1> : DbJoinGroupHavingResult<TEntity, TJoin1>
    {
        internal SQLServerJoinGroupHavingResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }


    public class SQLServerJoinGroupSortedResult<TEntity, TJoin1> : DbJoinGroupSortedResult<TEntity, TJoin1>
    {
        internal SQLServerJoinGroupSortedResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }
}
