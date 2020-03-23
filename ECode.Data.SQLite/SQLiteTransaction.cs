using Microsoft.Data.Sqlite;

namespace ECode.Data.SQLite
{
    public class SQLiteTransaction : DbTransaction
    {
        internal SQLiteTransaction(SQLiteSession session, SqliteTransaction transaction)
            : base(session, transaction)
        {

        }
    }
}
