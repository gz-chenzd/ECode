using System.Net;

namespace ECode.Net
{
    public interface INameResolver
    {
        IPAddress[] GetHostAddresses(string hostNameOrAddress);
    }
}
