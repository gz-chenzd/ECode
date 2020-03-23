
namespace ECode.Data.SQLite
{
    public class SQLiteJoinedResult<TEntity, TJoin1, TJoin2> : DbJoinedResult<TEntity, TJoin1, TJoin2>
    {
        internal SQLiteJoinedResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }


    public class SQLiteJoinFilterResult<TEntity, TJoin1, TJoin2> : DbJoinFilterResult<TEntity, TJoin1, TJoin2>
    {
        internal SQLiteJoinFilterResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }


    public class SQLiteJoinSortedResult<TEntity, TJoin1, TJoin2> : DbJoinSortedResult<TEntity, TJoin1, TJoin2>
    {
        internal SQLiteJoinSortedResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }


    public class SQLiteJoinPagedResult<TEntity, TJoin1, TJoin2> : DbJoinPagedResult<TEntity, TJoin1, TJoin2>
    {
        internal SQLiteJoinPagedResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }


    public class SQLiteJoinGroupedResult<TEntity, TJoin1, TJoin2> : DbJoinGroupedResult<TEntity, TJoin1, TJoin2>
    {
        internal SQLiteJoinGroupedResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }


    public class SQLiteJoinGroupHavingResult<TEntity, TJoin1, TJoin2> : DbJoinGroupHavingResult<TEntity, TJoin1, TJoin2>
    {
        internal SQLiteJoinGroupHavingResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }


    public class SQLiteJoinGroupSortedResult<TEntity, TJoin1, TJoin2> : DbJoinGroupSortedResult<TEntity, TJoin1, TJoin2>
    {
        internal SQLiteJoinGroupSortedResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }
}
