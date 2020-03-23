using System;
using System.Net;
using System.Net.Sockets;

namespace ECode.Net.Udp
{
    public class UDP_e_PacketReceived : EventArgs
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="socket">Socket which received data.</param>
        /// <param name="remoteEP">Remote IP end point from where data was received.</param>
        /// <param name="buffer">Data buffer.</param>
        /// <param name="count">Number of bytes stored in <b>buffer</b></param>
        internal UDP_e_PacketReceived(Socket socket, IPEndPoint remoteEP, byte[] buffer, int count)
        {
            this.Socket = socket;
            this.RemoteEP = remoteEP;
            this.Buffer = buffer;
            this.Count = count;
        }


        #region Properties Implementation

        /// <summary>
        /// Gets socket which received data.
        /// </summary>
        public Socket Socket
        { get; private set; }

        /// <summary>
        /// Gets remote host from where data was received.
        /// </summary>
        public IPEndPoint RemoteEP
        { get; private set; }

        /// <summary>
        /// Gets data buffer.
        /// </summary>
        public byte[] Buffer
        { get; private set; }

        /// <summary>
        /// Gets number of bytes stored in <b>Buffer</b>.
        /// </summary>
        public int Count
        { get; private set; }

        #endregion
    }
}
