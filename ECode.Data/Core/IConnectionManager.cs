
namespace ECode.Data
{
    public interface IConnectionManager
    {
        string GetConnectionString(string shardNo = null, bool writable = true);
    }
}
