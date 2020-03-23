
namespace ECode.Data
{
    public interface IShardStrategy
    {
        /// <summary>
        /// 获取分库编号
        /// </summary>
        /// <param name="shardObject">分库分表对象</param>
        /// <returns>null或空：不分库，非空：分库编号</returns>
        string GetDbShardNo(object shardObject);

        /// <summary>
        /// 获取分表编号
        /// </summary>
        /// <param name="tableName">数据库表</param>
        /// <param name="shardObject">分库分表对象</param>
        /// <returns>null或空：不分表，非空：分表编号</returns>
        string GetTableShardNo(string tableName, object shardObject);

        /// <summary>
        /// 获取分片编号
        /// </summary>
        /// <param name="tableName">数据库表</param>
        /// <param name="shardObject">分库分表对象</param>
        /// <param name="partitionObject">分片条件对象</param>
        /// <returns>null或空：不分片，非空：分片编号</returns>
        string GetTablePartitionNo(string tableName, object shardObject, object partitionObject);
    }
}
