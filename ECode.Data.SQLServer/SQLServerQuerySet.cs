using System;
using System.Collections.Generic;
using System.Data;

namespace ECode.Data.SQLServer
{
    public class SQLServerQuerySet<TEntity> : DbQuerySet<TEntity>
    {
        internal SQLServerQuerySet(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }


        protected override ExpressionParser GetExpressionParser()
        {
            return new SQLServerExpressionParser();
        }


        internal string ParseSelectSqlForUnitTest(ExpressionParser parser, IList<IDataParameter> parameters)
        {
            return base.ParseSqlForUnitTest(parser, parameters);
        }
    }


    public class SQLServerSortedQuerySet<TEntity> : DbSortedQuerySet<TEntity>
    {
        internal SQLServerSortedQuerySet(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }


        protected override ExpressionParser GetExpressionParser()
        {
            return new SQLServerExpressionParser();
        }
    }


    public class SQLServerGroupedResult<TEntity> : DbGroupedResult<TEntity>
    {
        internal SQLServerGroupedResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }


    public class SQLServerGroupSortedResult<TEntity> : DbGroupSortedResult<TEntity>
    {
        internal SQLServerGroupSortedResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }


    public class SQLServerGroupHavingResult<TEntity> : DbGroupHavingResult<TEntity>
    {
        internal SQLServerGroupHavingResult(DbSession session, DbQueryContext queryContext)
            : base(session, queryContext)
        {

        }
    }
}
