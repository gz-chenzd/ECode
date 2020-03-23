
namespace ECode.Data.SQLite
{
    public class SQLiteJoinedResult<TEntity, TJoin1> : DbJoinedResult<TEntity, TJoin1>
    {
        internal SQLiteJoinedResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }


    public class SQLiteJoinFilterResult<TEntity, TJoin1> : DbJoinFilterResult<TEntity, TJoin1>
    {
        internal SQLiteJoinFilterResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }


    public class SQLiteJoinSortedResult<TEntity, TJoin1> : DbJoinSortedResult<TEntity, TJoin1>
    {
        internal SQLiteJoinSortedResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }


    public class SQLiteJoinPagedResult<TEntity, TJoin1> : DbJoinPagedResult<TEntity, TJoin1>
    {
        internal SQLiteJoinPagedResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }


    public class SQLiteJoinGroupedResult<TEntity, TJoin1> : DbJoinGroupedResult<TEntity, TJoin1>
    {
        internal SQLiteJoinGroupedResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }


    public class SQLiteJoinGroupHavingResult<TEntity, TJoin1> : DbJoinGroupHavingResult<TEntity, TJoin1>
    {
        internal SQLiteJoinGroupHavingResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }


    public class SQLiteJoinGroupSortedResult<TEntity, TJoin1> : DbJoinGroupSortedResult<TEntity, TJoin1>
    {
        internal SQLiteJoinGroupSortedResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }
}
