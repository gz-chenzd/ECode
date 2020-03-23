
namespace ECode.Data.MySQL
{
    public class MySQLDatabase : AbstractDatabase
    {
        public MySQLDatabase(IConnectionManager connectionManager)
            : base(connectionManager, null, null)
        {

        }

        public MySQLDatabase(IConnectionManager connectionManager, IShardStrategy shardStrategy)
            : base(connectionManager, shardStrategy, null)
        {

        }

        public MySQLDatabase(IConnectionManager connectionManager, IShardStrategy shardStrategy, ISchemaManager schemaManager)
            : base(connectionManager, shardStrategy, schemaManager)
        {

        }


        protected override DbSession CreateSession()
        {
            return new MySQLSession(this);
        }
    }
}
