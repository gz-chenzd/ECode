
namespace ECode.Data.SQLServer
{
    public class SQLServerJoinedResult<TEntity, TJoin1, TJoin2> : DbJoinedResult<TEntity, TJoin1, TJoin2>
    {
        internal SQLServerJoinedResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }


    public class SQLServerJoinFilterResult<TEntity, TJoin1, TJoin2> : DbJoinFilterResult<TEntity, TJoin1, TJoin2>
    {
        internal SQLServerJoinFilterResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }


    public class SQLServerJoinSortedResult<TEntity, TJoin1, TJoin2> : DbJoinSortedResult<TEntity, TJoin1, TJoin2>
    {
        internal SQLServerJoinSortedResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }


    public class SQLServerJoinPagedResult<TEntity, TJoin1, TJoin2> : DbJoinPagedResult<TEntity, TJoin1, TJoin2>
    {
        internal SQLServerJoinPagedResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }


    public class SQLServerJoinGroupedResult<TEntity, TJoin1, TJoin2> : DbJoinGroupedResult<TEntity, TJoin1, TJoin2>
    {
        internal SQLServerJoinGroupedResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }


    public class SQLServerJoinGroupHavingResult<TEntity, TJoin1, TJoin2> : DbJoinGroupHavingResult<TEntity, TJoin1, TJoin2>
    {
        internal SQLServerJoinGroupHavingResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }


    public class SQLServerJoinGroupSortedResult<TEntity, TJoin1, TJoin2> : DbJoinGroupSortedResult<TEntity, TJoin1, TJoin2>
    {
        internal SQLServerJoinGroupSortedResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }
}
