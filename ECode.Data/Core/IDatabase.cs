using System;

namespace ECode.Data
{
    public interface IDatabase
    {
        /// <summary>
        /// Opens a connection, but need to manually close it
        /// </summary>
        ISession OpenSession(object shardObject = null);

        /// <summary>
        /// Opens a connection on master, but need to manually close it
        /// </summary>
        ISession OpenMasterSession(object shardObject = null);


        void ExecAction(Action<ISession> action);

        void ExecAction(object shardObject, Action<ISession> action);

        T ExecAction<T>(Func<ISession, T> func);

        T ExecAction<T>(object shardObject, Func<ISession, T> func);


        void ExecActionOnMaster(Action<ISession> action);

        void ExecActionOnMaster(object shardObject, Action<ISession> action);

        T ExecActionOnMaster<T>(Func<ISession, T> func);

        T ExecActionOnMaster<T>(object shardObject, Func<ISession, T> func);
    }
}
