using System;
using System.Data;
using ECode.Core;
using ECode.Utility;

namespace ECode.Data
{
    public abstract class DbTransaction : ITransaction
    {
        private readonly DateTime   StartTime   = DateTime.Now;


        public string ID
        { get; private set; }

        public bool IsActive
        { get; private set; } = true;


        protected DbSession Session
        { get; private set; }

        protected IDbTransaction Transaction
        { get; private set; }


        protected DbTransaction(DbSession session, IDbTransaction transaction)
        {
            AssertUtil.ArgumentNotNull(session, nameof(session));
            AssertUtil.ArgumentNotNull(transaction, nameof(transaction));

            this.ID = ObjectId.NewId();
            this.Session = session;
            this.Transaction = transaction;
        }

        internal IDbTransaction GetDbTransaction()
        {
            if (this.IsActive)
            { return this.Transaction; }

            return null;
        }


        public void Commit()
        {
            if (!this.IsActive)
            { throw new InvalidOperationException("The transaction is not active."); }

            var entry = new LogEntry();
            entry.SessionID = this.Session.ID;
            entry.TransactionID = this.ID;
            entry.CommandType = CommandType.Commit;

            try
            {
                if (this.Transaction.Connection is System.Data.Common.DbConnection)
                {
                    var db_conn = (System.Data.Common.DbConnection)this.Transaction.Connection;
                    entry.Server = db_conn.DataSource;
                    entry.Database = db_conn.Database;
                }

                this.Transaction.Commit();

                entry.TotalElapsed = (int)(DateTime.Now - StartTime).TotalMilliseconds;
                this.Session.Database.OnWriteLog(entry);
            }
            catch (Exception ex)
            {
                entry.Message = ex.Message;
                entry.Exception = ex;
                entry.TotalElapsed = (int)(DateTime.Now - StartTime).TotalMilliseconds;
                this.Session.Database.OnWriteLog(entry);

                throw ex;
            }
            finally
            { this.IsActive = false; }
        }

        public void Rollback()
        {
            if (!this.IsActive)
            { throw new InvalidOperationException("The transaction is not active."); }

            var entry = new LogEntry();
            entry.SessionID = this.Session.ID;
            entry.TransactionID = this.ID;
            entry.CommandType = CommandType.Rollback;

            try
            {
                if (this.Transaction.Connection is System.Data.Common.DbConnection)
                {
                    var db_conn = (System.Data.Common.DbConnection)this.Transaction.Connection;
                    entry.Server = db_conn.DataSource;
                    entry.Database = db_conn.Database;
                }

                this.Transaction.Rollback();

                entry.TotalElapsed = (int)(DateTime.Now - StartTime).TotalMilliseconds;
                this.Session.Database.OnWriteLog(entry);
            }
            catch (Exception ex)
            {
                entry.Message = ex.Message;
                entry.Exception = ex;
                entry.TotalElapsed = (int)(DateTime.Now - StartTime).TotalMilliseconds;
                this.Session.Database.OnWriteLog(entry);

                throw ex;
            }
            finally
            { this.IsActive = false; }
        }
    }
}
