
namespace ECode.Data.SQLite
{
    public class SQLiteDatabase : AbstractDatabase
    {
        public SQLiteDatabase(IConnectionManager connectionManager)
            : base(connectionManager)
        {

        }


        protected override DbSession CreateSession()
        {
            return new SQLiteSession(this);
        }
    }
}
