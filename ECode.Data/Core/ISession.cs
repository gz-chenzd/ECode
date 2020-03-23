using System;

namespace ECode.Data
{
    public interface ISession : IDisposable
    {
        string ID { get; }


        /// <summary>
        /// 开始事务（事务只在master上操作）
        /// </summary>
        ITransaction BeginTransaction();

        /// <summary>
        /// 获取当前事务
        /// </summary>
        ITransaction GetActiveTransaction();


        /// <summary>
        /// 数据库表
        /// </summary>
        /// <typeparam name="TEntity">Model类型</typeparam>
        /// <param name="partitionObject">分片条件</param>
        ITable<TEntity> Table<TEntity>(object partitionObject = null);
    }
}
