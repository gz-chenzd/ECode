
namespace ECode.Data.SQLite
{
    public class SQLiteJoinedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> : DbJoinedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>
    {
        internal SQLiteJoinedResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }


    public class SQLiteJoinFilterResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> : DbJoinFilterResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>
    {
        internal SQLiteJoinFilterResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }


    public class SQLiteJoinSortedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> : DbJoinSortedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>
    {
        internal SQLiteJoinSortedResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }


    public class SQLiteJoinPagedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> : DbJoinPagedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>
    {
        internal SQLiteJoinPagedResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }


    public class SQLiteJoinGroupedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> : DbJoinGroupedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>
    {
        internal SQLiteJoinGroupedResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }


    public class SQLiteJoinGroupHavingResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> : DbJoinGroupHavingResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>
    {
        internal SQLiteJoinGroupHavingResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }


    public class SQLiteJoinGroupSortedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> : DbJoinGroupSortedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>
    {
        internal SQLiteJoinGroupSortedResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }
}
