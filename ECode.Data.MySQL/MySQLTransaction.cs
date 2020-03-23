using MySql.Data.MySqlClient;

namespace ECode.Data.MySQL
{
    public class MySQLTransaction : DbTransaction
    {
        internal MySQLTransaction(MySQLSession session, MySqlTransaction transaction)
            : base(session, transaction)
        {

        }
    }
}
