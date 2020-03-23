
namespace ECode.Data.MySQL
{
    public class MySQLQuerySet<TEntity> : DbQuerySet<TEntity>
    {
        internal MySQLQuerySet(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }


        protected override ExpressionParser GetExpressionParser()
        {
            return new MySQLExpressionParser();
        }
    }


    public class MySQLSortedQuerySet<TEntity> : DbSortedQuerySet<TEntity>
    {
        internal MySQLSortedQuerySet(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }


        protected override ExpressionParser GetExpressionParser()
        {
            return new MySQLExpressionParser();
        }
    }


    public class MySQLGroupedResult<TEntity> : DbGroupedResult<TEntity>
    {
        internal MySQLGroupedResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }


    public class MySQLGroupSortedResult<TEntity> : DbGroupSortedResult<TEntity>
    {
        internal MySQLGroupSortedResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }


    public class MySQLGroupHavingResult<TEntity> : DbGroupHavingResult<TEntity>
    {
        internal MySQLGroupHavingResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }
}
