using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using ECode.Utility;

namespace ECode.Net
{
    public class IPBindInfo
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="hostName">Host name.</param>
        /// <param name="protocol">Bind protocol.</param>
        /// <param name="ip">IP address to listen.</param>
        /// <param name="port">Port to listen.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>ip</b> is null.</exception>
        public IPBindInfo(string hostName, BindProtocol protocol, IPAddress ip, int port)
            : this(hostName, protocol, ip, port, SslMode.None, null)
        {

        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="hostName">Host name.</param>
        /// <param name="protocol">Bind protocol.</param>
        /// <param name="ip">IP address to listen.</param>
        /// <param name="port">Port to listen.</param>
        /// <param name="sslMode">Specifies SSL mode.</param>
        /// <param name="certificate">Certificate to use for SSL connections.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>ip</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public IPBindInfo(string hostName, BindProtocol protocol, IPAddress ip, int port, SslMode sslMode, X509Certificate2 certificate)
        {
            AssertUtil.ArgumentNotNull(ip, nameof(ip));
            AssertUtil.AssertNetworkPort(port, nameof(port));

            this.HostName = hostName;
            this.Protocol = protocol;
            this.EndPoint = new IPEndPoint(ip, port);

            this.SslMode = sslMode;
            this.Certificate = certificate;
            if ((sslMode == SslMode.SSL || sslMode == SslMode.TLS) && certificate == null)
            {
                throw new ArgumentException($"SSL requested, but argument '{nameof(certificate)}' is not provided.");
            }
        }


        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is IPBindInfo))
            { return false; }

            var other = (IPBindInfo)obj;
            if (other.HostName != this.HostName)
            { return false; }

            if (other.Protocol != this.Protocol)
            { return false; }

            if (!other.EndPoint.Equals(this.EndPoint))
            { return false; }

            if (other.SslMode != this.SslMode)
            { return false; }

            if (!ObjectUtil.NullSafeEquals(other.Certificate, this.Certificate))
            { return false; }

            return true;
        }


        #region Properties Implementation

        /// <summary>
        /// Gets host name.
        /// </summary>
        public string HostName
        { get; private set; }

        /// <summary>
        /// Gets protocol.
        /// </summary>
        public BindProtocol Protocol
        { get; private set; }

        /// <summary>
        /// Gets IP address.
        /// </summary>
        public IPAddress IP
        {
            get { return this.EndPoint.Address; }
        }

        /// <summary>
        /// Gets port.
        /// </summary>
        public int Port
        {
            get { return this.EndPoint.Port; }
        }

        /// <summary>
        /// Gets IP end point.
        /// </summary>
        public IPEndPoint EndPoint
        { get; private set; }

        /// <summary>
        /// Gets SSL mode.
        /// </summary>
        public SslMode SslMode
        { get; private set; }

        /// <summary>
        /// Gets SSL certificate.
        /// </summary>
        public X509Certificate2 Certificate
        { get; private set; }

        /// <summary>
        /// Gets or sets user data. This is used internally don't use it !!!.
        /// </summary>
        public object Tag
        { get; set; }

        #endregion
    }
}