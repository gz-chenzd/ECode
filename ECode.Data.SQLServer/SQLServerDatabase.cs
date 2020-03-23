
namespace ECode.Data.SQLServer
{
    public class SQLServerDatabase : AbstractDatabase
    {
        public SQLServerDatabase(IConnectionManager connectionManager)
            : base(connectionManager)
        {

        }


        protected override DbSession CreateSession()
        {
            return new SQLServerSession(this);
        }
    }
}
