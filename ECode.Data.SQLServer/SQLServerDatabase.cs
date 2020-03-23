
namespace ECode.Data.SQLServer
{
    public class SQLServerDatabase : AbstractDatabase
    {
        public SQLServerDatabase(IConnectionManager connectionManager)
           : base(connectionManager, null, null)
        {

        }

        public SQLServerDatabase(IConnectionManager connectionManager, IShardStrategy shardStrategy)
            : base(connectionManager, shardStrategy, null)
        {

        }

        public SQLServerDatabase(IConnectionManager connectionManager, IShardStrategy shardStrategy, ISchemaManager schemaManager)
            : base(connectionManager, shardStrategy, schemaManager)
        {

        }


        protected override DbSession CreateSession()
        {
            return new SQLServerSession(this);
        }
    }
}
