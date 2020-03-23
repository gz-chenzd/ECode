using System.Net;

namespace ECode.Net.Udp
{
    public interface IBindSelector
    {
        void Load(string[] bindings);

        string Select(IPEndPoint remoteEP);
    }
}
