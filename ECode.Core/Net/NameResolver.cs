using System.Net;
using ECode.Utility;

namespace ECode.Net
{
    public static class NameResolver
    {
        static INameResolver    resolver    = new SystemDnsResolver();


        public static INameResolver Resolver
        {
            get { return resolver; }

            set
            {
                AssertUtil.ArgumentNotNull(value, nameof(Resolver));

                resolver = value;
            }
        }


        public static IPAddress[] GetHostAddresses(string hostNameOrAddress)
        {
            return resolver.GetHostAddresses(hostNameOrAddress);
        }
    }
}
