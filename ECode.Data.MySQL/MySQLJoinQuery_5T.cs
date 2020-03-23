
namespace ECode.Data.MySQL
{
    public class MySQLJoinedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> : DbJoinedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>
    {
        internal MySQLJoinedResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }


    public class MySQLJoinFilterResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> : DbJoinFilterResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>
    {
        internal MySQLJoinFilterResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }


    public class MySQLJoinSortedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> : DbJoinSortedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>
    {
        internal MySQLJoinSortedResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }


    public class MySQLJoinPagedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> : DbJoinPagedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>
    {
        internal MySQLJoinPagedResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }


    public class MySQLJoinGroupedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> : DbJoinGroupedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>
    {
        internal MySQLJoinGroupedResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }


    public class MySQLJoinGroupHavingResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> : DbJoinGroupHavingResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>
    {
        internal MySQLJoinGroupHavingResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }


    public class MySQLJoinGroupSortedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> : DbJoinGroupSortedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>
    {
        internal MySQLJoinGroupSortedResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }
}
