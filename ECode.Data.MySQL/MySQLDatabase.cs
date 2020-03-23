
namespace ECode.Data.MySQL
{
    public class MySQLDatabase : AbstractDatabase
    {
        public MySQLDatabase(IConnectionManager connectionManager)
            : base(connectionManager)
        {

        }


        protected override DbSession CreateSession()
        {
            return new MySQLSession(this);
        }
    }
}
