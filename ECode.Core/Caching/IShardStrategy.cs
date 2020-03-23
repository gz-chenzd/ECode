namespace ECode.Caching
{
    public interface IShardStrategy
    {
        /// <summary>
        /// Returns shard no.
        /// </summary>
        /// <param name="target">Object for computing shard no.</param>
        /// <returns>Return shard no or null/empty for default.</returns>
        string GetShardNo(object target);
    }
}
