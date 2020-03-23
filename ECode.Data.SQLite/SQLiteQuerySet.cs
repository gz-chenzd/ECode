
namespace ECode.Data.SQLite
{
    public class SQLiteQuerySet<TEntity> : DbQuerySet<TEntity>
    {
        internal SQLiteQuerySet(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }


        protected override ExpressionParser GetExpressionParser()
        {
            return new SQLiteExpressionParser();
        }
    }


    public class SQLiteSortedQuerySet<TEntity> : DbSortedQuerySet<TEntity>
    {
        internal SQLiteSortedQuerySet(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }


        protected override ExpressionParser GetExpressionParser()
        {
            return new SQLiteExpressionParser();
        }
    }


    public class SQLiteGroupedResult<TEntity> : DbGroupedResult<TEntity>
    {
        internal SQLiteGroupedResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }


    public class SQLiteGroupSortedResult<TEntity> : DbGroupSortedResult<TEntity>
    {
        internal SQLiteGroupSortedResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }


    public class SQLiteGroupHavingResult<TEntity> : DbGroupHavingResult<TEntity>
    {
        internal SQLiteGroupHavingResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }
}
