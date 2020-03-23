using System.Data.SqlClient;

namespace ECode.Data.SQLServer
{
    public class SQLServerTransaction : DbTransaction
    {
        internal SQLServerTransaction(SQLServerSession session, SqlTransaction transaction)
            : base(session, transaction)
        {

        }
    }
}
