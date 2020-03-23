using System;
using ECode.Utility;

namespace ECode.Data
{
    public abstract class AbstractDatabase : IDatabase
    {
        public ISchemaManager SchemaManager
        { get; private set; }

        public IShardStrategy ShardStrategy
        { get; private set; }

        public IConnectionManager ConnectionManager
        { get; private set; }


        public AbstractDatabase(IConnectionManager connectionManager)
        {
            AssertUtil.ArgumentNotNull(connectionManager, nameof(connectionManager));

            this.ConnectionManager = connectionManager;
        }


        public AbstractDatabase SetSchemaManager(ISchemaManager shemaManager)
        {
            AssertUtil.ArgumentNotNull(shemaManager, nameof(shemaManager));

            this.SchemaManager = shemaManager;
            return this;
        }

        public AbstractDatabase SetShardStrategy(IShardStrategy shardStrategy)
        {
            AssertUtil.ArgumentNotNull(shardStrategy, nameof(shardStrategy));

            this.ShardStrategy = shardStrategy;
            return this;
        }


        public ISession OpenSession(object shardObject = null)
        {
            return OpenSession(false, shardObject);
        }

        public ISession OpenMasterSession(object shardObject = null)
        {
            return OpenSession(true, shardObject);
        }

        private DbSession OpenSession(bool master, object shardObject)
        {
            var session = CreateSession();
            session.Initialize(master, this.SchemaManager,
                               shardObject, this.ShardStrategy, this.ConnectionManager);

            return session;
        }

        protected abstract DbSession CreateSession();


        public void ExecAction(Action<ISession> action)
        {
            ExecAction(false, null, action);
        }

        public void ExecAction(object shardObject, Action<ISession> action)
        {
            ExecAction(false, shardObject, action);
        }

        public T ExecAction<T>(Func<ISession, T> func)
        {
            return ExecAction(false, null, func);
        }

        public T ExecAction<T>(object shardObject, Func<ISession, T> func)
        {
            return ExecAction(false, shardObject, func);
        }


        public void ExecActionOnMaster(Action<ISession> action)
        {
            ExecAction(true, null, action);
        }

        public void ExecActionOnMaster(object shardObject, Action<ISession> action)
        {
            ExecAction(true, shardObject, action);
        }

        public T ExecActionOnMaster<T>(Func<ISession, T> func)
        {
            return ExecAction(true, null, func);
        }

        public T ExecActionOnMaster<T>(object shardObject, Func<ISession, T> func)
        {
            return ExecAction(true, shardObject, func);
        }


        private void ExecAction(bool master, object shardObject, Action<ISession> action)
        {
            AssertUtil.ArgumentNotNull(action, nameof(action));

            ISession session = null;

            try
            {
                session = OpenSession(master, shardObject);
                action(session);
            }
            finally
            {
                if (session != null)
                { session.Dispose(); }

                session = null;
            }
        }

        private T ExecAction<T>(bool master, object shardObject, Func<ISession, T> func)
        {
            AssertUtil.ArgumentNotNull(func, nameof(func));

            ISession session = null;

            try
            {
                session = OpenSession(master, shardObject);
                return func(session);
            }
            finally
            {
                if (session != null)
                { session.Dispose(); }

                session = null;
            }
        }


        public event WriteLogHandler WriteLog;

        protected internal void OnWriteLog(LogEntry entry)
        {
            if (WriteLog != null && entry != null)
            {
                WriteLog(entry);
            }
        }
    }
}
