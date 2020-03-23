using System.Net;

namespace ECode.Net
{
    public class SystemDnsResolver : INameResolver
    {
        public IPAddress[] GetHostAddresses(string hostNameOrAddress)
        {
            return Dns.GetHostAddresses(hostNameOrAddress);
        }
    }
}
