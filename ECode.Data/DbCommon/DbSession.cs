using System;
using System.Data;
using System.Diagnostics;
using ECode.Core;
using ECode.Utility;

namespace ECode.Data
{
    public abstract class DbSession : ISession
    {
        private string              m_DbShardNo             = null;
        private bool                m_WritableConn          = false;
        private IDbConnection       m_pConnection           = null;
        private DbTransaction       m_pTransaction          = null;
        private IDbTransaction      m_pSlaveReadTran        = null;


        public string ID
        { get; private set; }

        public bool IsDisposed
        { get; private set; }

        public bool UsingMaster
        { get; private set; }

        public AbstractDatabase Database
        { get; private set; }

        protected ISchemaManager SchemaManager
        { get; private set; }


        protected object ShardObject
        { get; private set; }

        protected IShardStrategy ShardStrategy
        { get; private set; }

        protected IConnectionManager ConnectionManager
        { get; private set; }


        protected DbSession(AbstractDatabase database)
        {
            AssertUtil.ArgumentNotNull(database, nameof(database));

            this.Database = database;
            this.ID = ObjectId.NewId();
        }

        protected void ThrowIfObjectDisposed()
        {
            if (this.IsDisposed)
            { throw new ObjectDisposedException(this.GetType().Name); }
        }


        internal void Initialize(bool master, ISchemaManager schemaManager,
                                 object shardObject, IShardStrategy shardStrategy, IConnectionManager connectionManager)
        {
            this.UsingMaster = master;

            this.SchemaManager = schemaManager;

            this.ShardObject = shardObject;
            this.ShardStrategy = shardStrategy;
            this.ConnectionManager = connectionManager;
        }


        internal IDbConnection GetDbConnection(bool writable = true)
        {
            ThrowIfObjectDisposed();

            bool switchingMaster = false;
            if (m_pConnection != null)
            {
                if (m_pConnection.State == ConnectionState.Open
                    && (m_WritableConn || !writable))
                { return m_pConnection; }

                m_pSlaveReadTran.Rollback();
                m_pSlaveReadTran = null;

                m_pConnection.Dispose();
                m_pConnection = null;

                switchingMaster = true;
            }


            var entry = new LogEntry();
            entry.SessionID = this.ID;
            entry.CommandType = CommandType.Connect;

            if (switchingMaster)
            { entry.Message = "Switch to master"; }

            var watch = new Stopwatch();
            watch.Start();

            try
            {
                m_WritableConn = writable;
                if (this.UsingMaster)
                { m_WritableConn = true; }

                if (m_DbShardNo == null && this.ShardStrategy != null)
                { m_DbShardNo = this.ShardStrategy.GetDbShardNo(this.ShardObject); }

                var connectionString = this.ConnectionManager.GetConnectionString(m_DbShardNo, m_WritableConn);
                m_pConnection = CreateDbConnection(connectionString);
                m_pConnection.Open();

                if (!m_WritableConn)
                { m_pSlaveReadTran = m_pConnection.BeginTransaction(IsolationLevel.ReadUncommitted); }


                watch.Stop();

                if (m_pConnection is System.Data.Common.DbConnection)
                {
                    var db_conn = (System.Data.Common.DbConnection)m_pConnection;
                    entry.Server = db_conn.DataSource;
                    entry.Database = db_conn.Database;
                }

                entry.TotalElapsed = (int)watch.ElapsedMilliseconds;
                this.Database.OnWriteLog(entry);


                return m_pConnection;
            }
            catch (Exception ex)
            {
                watch.Stop();

                entry.Message = switchingMaster ? $"Switch to master error: {ex.Message}" : ex.Message;
                entry.Exception = ex;
                entry.TotalElapsed = (int)watch.ElapsedMilliseconds;
                this.Database.OnWriteLog(entry);

                throw ex;
            }
        }

        protected abstract IDbConnection CreateDbConnection(string connectionString);


        internal IDbTransaction GetDbTransaction()
        {
            ThrowIfObjectDisposed();

            if (!m_WritableConn)
            { return m_pSlaveReadTran; }

            return m_pTransaction?.GetDbTransaction();
        }


        public ITransaction BeginTransaction()
        {
            ThrowIfObjectDisposed();

            if (m_pTransaction != null && m_pTransaction.IsActive)
            { return m_pTransaction; }


            var entry = new LogEntry();
            entry.SessionID = this.ID;
            entry.CommandType = CommandType.Transaction;

            var watch = new Stopwatch();
            watch.Start();

            try
            {
                var conn = GetDbConnection(true);
                m_pTransaction = BeginDbTransaction(conn);


                watch.Stop();

                entry.TransactionID = m_pTransaction.ID;
                if (conn is System.Data.Common.DbConnection)
                {
                    var db_conn = (System.Data.Common.DbConnection)conn;
                    entry.Server = db_conn.DataSource;
                    entry.Database = db_conn.Database;
                }

                entry.TotalElapsed = (int)watch.ElapsedMilliseconds;
                this.Database.OnWriteLog(entry);


                return m_pTransaction;
            }
            catch (Exception ex)
            {
                watch.Stop();

                entry.Message = ex.Message;
                entry.Exception = ex;
                entry.TotalElapsed = (int)watch.ElapsedMilliseconds;
                this.Database.OnWriteLog(entry);

                throw ex;
            }
        }

        public ITransaction GetActiveTransaction()
        {
            ThrowIfObjectDisposed();

            if (m_pTransaction == null)
            { return null; }

            if (m_pTransaction.IsActive)
            { return m_pTransaction; }

            return null;
        }

        protected abstract DbTransaction BeginDbTransaction(IDbConnection connection);


        private ushort tempTableNo = 0;

        internal string CreateTempTableName()
        {
            return "t" + (tempTableNo++);
        }

        public abstract ITable<TEntity> Table<TEntity>(object partitionObject = null);


        public void Dispose()
        {
            if (this.IsDisposed)
            { return; }

            this.IsDisposed = true;

            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!m_WritableConn && m_pSlaveReadTran != null)
            {
                m_pSlaveReadTran.Rollback();
                m_pSlaveReadTran = null;
            }
            else if (m_pTransaction != null && m_pTransaction.IsActive)
            {
                m_pTransaction.Rollback();
                m_pTransaction = null;
            }

            if (m_pConnection != null)
            {
                try
                { m_pConnection.Dispose(); }
                catch (Exception ex)
                { string dummy = ex.Message; }

                m_pConnection = null;
            }
        }


        protected internal abstract DbQuerySet<TEntity> CreateQuerySet<TEntity>(DbQueryContext queryContext);

        protected internal abstract DbSortedQuerySet<TEntity> CreateSortedQuerySet<TEntity>(DbQueryContext queryContext);

        protected internal abstract DbGroupedResult<TEntity> CreateGroupedResult<TEntity>(DbQueryContext queryContext);

        protected internal abstract DbGroupHavingResult<TEntity> CreateGroupHavingResult<TEntity>(DbQueryContext queryContext);

        protected internal abstract DbGroupSortedResult<TEntity> CreateGroupSortedResult<TEntity>(DbQueryContext queryContext);


        protected internal abstract DbJoinedResult<TEntity, TJoin1> CreateJoinedResult<TEntity, TJoin1>(DbQueryContext queryContext);

        protected internal abstract DbJoinFilterResult<TEntity, TJoin1> CreateJoinFilterResult<TEntity, TJoin1>(DbQueryContext queryContext);

        protected internal abstract DbJoinSortedResult<TEntity, TJoin1> CreateJoinSortedResult<TEntity, TJoin1>(DbQueryContext queryContext);

        protected internal abstract DbJoinPagedResult<TEntity, TJoin1> CreateJoinPagedResult<TEntity, TJoin1>(DbQueryContext queryContext);

        protected internal abstract DbJoinGroupedResult<TEntity, TJoin1> CreateJoinGroupedResult<TEntity, TJoin1>(DbQueryContext queryContext);

        protected internal abstract DbJoinGroupHavingResult<TEntity, TJoin1> CreateJoinGroupHavingResult<TEntity, TJoin1>(DbQueryContext queryContext);

        protected internal abstract DbJoinGroupSortedResult<TEntity, TJoin1> CreateJoinGroupSortedResult<TEntity, TJoin1>(DbQueryContext queryContext);


        protected internal abstract DbJoinedResult<TEntity, TJoin1, TJoin2> CreateJoinedResult<TEntity, TJoin1, TJoin2>(DbQueryContext queryContext);

        protected internal abstract DbJoinFilterResult<TEntity, TJoin1, TJoin2> CreateJoinFilterResult<TEntity, TJoin1, TJoin2>(DbQueryContext queryContext);

        protected internal abstract DbJoinSortedResult<TEntity, TJoin1, TJoin2> CreateJoinSortedResult<TEntity, TJoin1, TJoin2>(DbQueryContext queryContext);

        protected internal abstract DbJoinPagedResult<TEntity, TJoin1, TJoin2> CreateJoinPagedResult<TEntity, TJoin1, TJoin2>(DbQueryContext queryContext);

        protected internal abstract DbJoinGroupedResult<TEntity, TJoin1, TJoin2> CreateJoinGroupedResult<TEntity, TJoin1, TJoin2>(DbQueryContext queryContext);

        protected internal abstract DbJoinGroupHavingResult<TEntity, TJoin1, TJoin2> CreateJoinGroupHavingResult<TEntity, TJoin1, TJoin2>(DbQueryContext queryContext);

        protected internal abstract DbJoinGroupSortedResult<TEntity, TJoin1, TJoin2> CreateJoinGroupSortedResult<TEntity, TJoin1, TJoin2>(DbQueryContext queryContext);


        protected internal abstract DbJoinedResult<TEntity, TJoin1, TJoin2, TJoin3> CreateJoinedResult<TEntity, TJoin1, TJoin2, TJoin3>(DbQueryContext queryContext);

        protected internal abstract DbJoinFilterResult<TEntity, TJoin1, TJoin2, TJoin3> CreateJoinFilterResult<TEntity, TJoin1, TJoin2, TJoin3>(DbQueryContext queryContext);

        protected internal abstract DbJoinSortedResult<TEntity, TJoin1, TJoin2, TJoin3> CreateJoinSortedResult<TEntity, TJoin1, TJoin2, TJoin3>(DbQueryContext queryContext);

        protected internal abstract DbJoinPagedResult<TEntity, TJoin1, TJoin2, TJoin3> CreateJoinPagedResult<TEntity, TJoin1, TJoin2, TJoin3>(DbQueryContext queryContext);

        protected internal abstract DbJoinGroupedResult<TEntity, TJoin1, TJoin2, TJoin3> CreateJoinGroupedResult<TEntity, TJoin1, TJoin2, TJoin3>(DbQueryContext queryContext);

        protected internal abstract DbJoinGroupHavingResult<TEntity, TJoin1, TJoin2, TJoin3> CreateJoinGroupHavingResult<TEntity, TJoin1, TJoin2, TJoin3>(DbQueryContext queryContext);

        protected internal abstract DbJoinGroupSortedResult<TEntity, TJoin1, TJoin2, TJoin3> CreateJoinGroupSortedResult<TEntity, TJoin1, TJoin2, TJoin3>(DbQueryContext queryContext);


        protected internal abstract DbJoinedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> CreateJoinedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>(DbQueryContext queryContext);

        protected internal abstract DbJoinFilterResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> CreateJoinFilterResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>(DbQueryContext queryContext);

        protected internal abstract DbJoinSortedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> CreateJoinSortedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>(DbQueryContext queryContext);

        protected internal abstract DbJoinPagedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> CreateJoinPagedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>(DbQueryContext queryContext);

        protected internal abstract DbJoinGroupedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> CreateJoinGroupedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>(DbQueryContext queryContext);

        protected internal abstract DbJoinGroupHavingResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> CreateJoinGroupHavingResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>(DbQueryContext queryContext);

        protected internal abstract DbJoinGroupSortedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4> CreateJoinGroupSortedResult<TEntity, TJoin1, TJoin2, TJoin3, TJoin4>(DbQueryContext queryContext);
    }
}