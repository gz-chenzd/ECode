
namespace ECode.Data.SQLServer
{
    public class SQLServerJoinedResult<TEntity, TJoin1, TJoin2, TJoin3> : DbJoinedResult<TEntity, TJoin1, TJoin2, TJoin3>
    {
        internal SQLServerJoinedResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }


    public class SQLServerJoinFilterResult<TEntity, TJoin1, TJoin2, TJoin3> : DbJoinFilterResult<TEntity, TJoin1, TJoin2, TJoin3>
    {
        internal SQLServerJoinFilterResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }


    public class SQLServerJoinSortedResult<TEntity, TJoin1, TJoin2, TJoin3> : DbJoinSortedResult<TEntity, TJoin1, TJoin2, TJoin3>
    {
        internal SQLServerJoinSortedResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }


    public class SQLServerJoinPagedResult<TEntity, TJoin1, TJoin2, TJoin3> : DbJoinPagedResult<TEntity, TJoin1, TJoin2, TJoin3>
    {
        internal SQLServerJoinPagedResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }


    public class SQLServerJoinGroupedResult<TEntity, TJoin1, TJoin2, TJoin3> : DbJoinGroupedResult<TEntity, TJoin1, TJoin2, TJoin3>
    {
        internal SQLServerJoinGroupedResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }


    public class SQLServerJoinGroupHavingResult<TEntity, TJoin1, TJoin2, TJoin3> : DbJoinGroupHavingResult<TEntity, TJoin1, TJoin2, TJoin3>
    {
        internal SQLServerJoinGroupHavingResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }


    public class SQLServerJoinGroupSortedResult<TEntity, TJoin1, TJoin2, TJoin3> : DbJoinGroupSortedResult<TEntity, TJoin1, TJoin2, TJoin3>
    {
        internal SQLServerJoinGroupSortedResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }
}
