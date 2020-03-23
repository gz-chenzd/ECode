
namespace ECode.Data.SQLite
{
    public class SQLiteDatabase : AbstractDatabase
    {
        public SQLiteDatabase(IConnectionManager connectionManager)
            : base(connectionManager, null, null)
        {

        }

        public SQLiteDatabase(IConnectionManager connectionManager, IShardStrategy shardStrategy)
            : base(connectionManager, shardStrategy, null)
        {

        }

        public SQLiteDatabase(IConnectionManager connectionManager, IShardStrategy shardStrategy, ISchemaManager schemaManager)
            : base(connectionManager, shardStrategy, schemaManager)
        {

        }


        protected override DbSession CreateSession()
        {
            return new SQLiteSession(this);
        }
    }
}
