using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ECode.Collections;
using ECode.Utility;

namespace ECode.Net.Udp
{
    public class UDP_Server : IDisposable
    {
        private int                             m_MTU                   = 1400;
        private bool                            m_IsRunning             = false;
        private IPEndPoint[]                    m_pBindings             = new IPEndPoint[0];
        private int                             m_ReceiversPerSocket    = 10;
        private DateTime                        m_StartTime;
        private List<Socket>                    m_pSockets              = null;
        private CycleCollection<Socket>         m_pSocketsIPv4          = null;
        private CycleCollection<Socket>         m_pSocketsIPv6          = null;
        private Dictionary<string, Socket>      m_pBindSockets          = null;
        private List<UDP_DataReceiver>          m_pDataReceivers        = null;
        private IBindSelector                   m_pBindSelector         = null;
        private long                            m_BytesSent             = 0;
        private long                            m_PacketsSent           = 0;
        private long                            m_BytesReceived         = 0;
        private long                            m_PacketsReceived       = 0;


        public UDP_Server(int socketReceivers = 10)
        {
            if (socketReceivers <= 0)
            { socketReceivers = 10; }
            else if (socketReceivers > 50)
            { socketReceivers = 50; }

            m_ReceiversPerSocket = socketReceivers;

            m_pSockets = new List<Socket>();
            m_pSocketsIPv4 = new CycleCollection<Socket>();
            m_pSocketsIPv6 = new CycleCollection<Socket>();
            m_pBindSockets = new Dictionary<string, Socket>();
            m_pDataReceivers = new List<UDP_DataReceiver>();
        }


        public void Dispose()
        {
            if (this.IsDisposed)
            { return; }

            this.IsDisposed = true;


            Stop();

            m_pSockets = null;
            m_pSocketsIPv4 = null;
            m_pSocketsIPv6 = null;
            m_pBindSockets = null;
            m_pDataReceivers = null;

            // Release all events.
            this.PacketReceived = null;
            this.Error = null;
        }

        protected void ThrowIfNotRunning()
        {
            if (!m_IsRunning)
            { throw new InvalidOperationException("The server is not running."); }
        }

        protected void ThrowIfObjectDisposed()
        {
            if (this.IsDisposed)
            { throw new ObjectDisposedException(this.GetType().Name); }
        }


        /// <summary>
        /// Starts this server.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        public void Start()
        {
            ThrowIfObjectDisposed();

            if (m_IsRunning)
            { return; }

            m_IsRunning = true;
            m_StartTime = DateTime.Now;

            m_BytesSent = 0;
            m_PacketsSent = 0;
            m_BytesReceived = 0;
            m_PacketsReceived = 0;

            ThreadPool.QueueUserWorkItem((o) =>
            {
                StartListen();
            });
        }

        /// <summary>
        /// Stops this server.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        public void Stop()
        {
            ThrowIfObjectDisposed();

            if (!m_IsRunning)
            { return; }

            m_IsRunning = false;

            foreach (var receiver in m_pDataReceivers)
            { receiver.Dispose(); }
            m_pDataReceivers.Clear();

            foreach (var socket in m_pSockets)
            { socket.Dispose(); }
            m_pSockets.Clear();
            m_pSocketsIPv4.Clear();
            m_pSocketsIPv6.Clear();
            m_pBindSockets.Clear();
        }

        /// <summary>
        /// Restarts running server. If server is not running, this methods has no efffect.
        /// </summary>
        public void Restart()
        {
            ThrowIfObjectDisposed();

            if (m_IsRunning)
            {
                Stop();
                Start();
            }
        }

        private void StartListen()
        {
            try
            {
                // Dispose all old receivers.
                foreach (var receiver in m_pDataReceivers.ToArray())
                {
                    try
                    { receiver.Dispose(); }
                    catch (Exception ex)
                    { OnError(ex); }
                }
                m_pDataReceivers.Clear();

                // Dispose all old sockets.
                foreach (var socket in m_pSockets.ToArray())
                {
                    try
                    { socket.Dispose(); }
                    catch (Exception ex)
                    { OnError(ex); }
                }
                m_pSockets.Clear();
                m_pSocketsIPv4.Clear();
                m_pSocketsIPv6.Clear();
                m_pBindSockets.Clear();

                // We must replace IPAddress.Any to all available IPs, otherwise it's impossible to send 
                // reply back to UDP packet sender on same local EP where packet received. 
                // This is very important when clients are behind NAT.
                var listeningEPs = new List<IPEndPoint>();
                foreach (var ep in m_pBindings)
                {
                    if (ep.Address.Equals(IPAddress.Any))
                    {
                        // Add localhost.
                        var localEP = new IPEndPoint(IPAddress.Loopback, ep.Port);
                        if (!listeningEPs.Contains(localEP))
                        { listeningEPs.Add(localEP); }

                        foreach (var ip in NetworkUtil.GetIPAddresses())
                        {
                            if (ip.AddressFamily == AddressFamily.InterNetwork)
                            {
                                localEP = new IPEndPoint(ip, ep.Port);
                                if (!listeningEPs.Contains(localEP))
                                { listeningEPs.Add(localEP); }
                            }
                        }
                    }
                    else if (ep.Address.Equals(IPAddress.IPv6Any))
                    {
                        foreach (var ip in NetworkUtil.GetIPAddresses())
                        {
                            if (ip.AddressFamily == AddressFamily.InterNetworkV6)
                            {
                                IPEndPoint localEP = new IPEndPoint(ip, ep.Port);
                                if (!listeningEPs.Contains(localEP))
                                { listeningEPs.Add(localEP); }
                            }
                        }
                    }
                    else
                    {
                        if (!listeningEPs.Contains(ep))
                        { listeningEPs.Add(ep); }
                    }
                }

                // Create sockets.
                m_pSockets = new List<Socket>();
                m_pBindSockets = new Dictionary<string, Socket>();
                foreach (var ep in listeningEPs)
                {
                    try
                    {
                        var socket = NetworkUtil.CreateSocket(ep, ProtocolType.Udp);
                        m_pSockets.Add(socket);

                        if (!ep.Address.Equals(IPAddress.Loopback))
                        { m_pBindSockets[ep.ToString()] = socket; }

                        // Create UDP data receivers.
                        for (int i = 0; i < m_ReceiversPerSocket; i++)
                        {
                            var receiver = new UDP_DataReceiver(socket, m_MTU);
                            receiver.PacketReceived += (sender, e) =>
                            {
                                try
                                { ProcessUdpPacket(e); }
                                catch (Exception ex)
                                { OnError(ex); }
                            };

                            receiver.Error += (sender, e) =>
                            {
                                OnError(e.Exception);
                            };

                            m_pDataReceivers.Add(receiver);
                            receiver.Start();
                        }
                    }
                    catch (Exception ex)
                    { OnError(ex); }
                }

                // Create round-robin send sockets. 
                // NOTE: We must skip localhost, it can't be used for sending out of server.
                m_pSocketsIPv4 = new CycleCollection<Socket>();
                m_pSocketsIPv6 = new CycleCollection<Socket>();
                foreach (Socket socket in m_pSockets)
                {
                    if (((IPEndPoint)socket.LocalEndPoint).AddressFamily == AddressFamily.InterNetwork)
                    {
                        if (!((IPEndPoint)socket.LocalEndPoint).Address.Equals(IPAddress.Loopback))
                        { m_pSocketsIPv4.Add(socket); }
                    }
                    else if (((IPEndPoint)socket.LocalEndPoint).AddressFamily == AddressFamily.InterNetworkV6)
                    { m_pSocketsIPv6.Add(socket); }
                }

                if (m_pBindSelector != null)
                { m_pBindSelector.Load(m_pBindSockets.Keys.ToArray()); }
            }
            catch (Exception ex)
            { OnError(ex); }
        }


        #region method SendPacket

        /// <summary>
        /// Sends specified packet to the specified remote end point.
        /// </summary>
        /// <param name="remoteEP">Remote end point.</param>
        /// <param name="packet">UDP packet to send.</param>
        /// <param name="offset">Offset in the buffer.</param>
        /// <param name="count">Number of bytes to send.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised whan server is not running and this method is accessed.</exception>
        /// <exception cref="ArgumentNullException">Is raised when any of the arumnets is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public void SendPacket(IPEndPoint remoteEP, byte[] packet, int offset, int count)
        {
            IPEndPoint localEP = null;
            SendPacket(remoteEP, packet, offset, count, out localEP);
        }

        /// <summary>
        /// Sends specified packet to the specified remote end point.
        /// </summary>
        /// <param name="remoteEP">Remote end point.</param>
        /// <param name="packet">UDP packet to send.</param>
        /// <param name="offset">Offset in the buffer.</param>
        /// <param name="count">Number of bytes to send.</param>
        /// <param name="localEP">Returns local IP end point which was used to send UDP packet.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised whan server is not running and this method is accessed.</exception>
        /// <exception cref="ArgumentNullException">Is raised when any of the arumnets is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public void SendPacket(IPEndPoint remoteEP, byte[] packet, int offset, int count, out IPEndPoint localEP)
        {
            ThrowIfObjectDisposed();
            ThrowIfNotRunning();

            AssertUtil.ArgumentNotNull(remoteEP, nameof(remoteEP));

            var socket = SelectBindSocket(remoteEP);
            localEP = (IPEndPoint)socket.LocalEndPoint;
            SendPacket(socket, remoteEP, packet, offset, count);
        }

        /// <summary>
        /// Sends specified packet to the specified remote end point.
        /// </summary>
        /// <param name="localEP">Local end point to use for sending.</param>
        /// <param name="remoteEP">Remote end point.</param>
        /// <param name="packet">UDP packet to send.</param>
        /// <param name="offset">Offset in the buffer.</param>
        /// <param name="count">Number of bytes to send.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised whan server is not running and this method is accessed.</exception>
        /// <exception cref="ArgumentNullException">Is raised when any of the arumnets is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public void SendPacket(IPEndPoint localEP, IPEndPoint remoteEP, byte[] packet, int offset, int count)
        {
            ThrowIfObjectDisposed();
            ThrowIfNotRunning();

            AssertUtil.ArgumentNotNull(localEP, nameof(localEP));
            AssertUtil.ArgumentNotNull(remoteEP, nameof(remoteEP));

            if (localEP.AddressFamily != remoteEP.AddressFamily)
            { throw new ArgumentException($"Argumnet {nameof(localEP)} and {nameof(remoteEP)} AddressFamily isnot matched."); }

            // Search specified local end point socket.
            Socket socket = null;
            if (localEP.AddressFamily == AddressFamily.InterNetwork)
            {
                foreach (var s in m_pSocketsIPv4.ToArray())
                {
                    if (localEP.Equals((IPEndPoint)s.LocalEndPoint))
                    {
                        socket = s;
                        break;
                    }
                }
            }
            else if (localEP.AddressFamily == AddressFamily.InterNetworkV6)
            {
                foreach (var s in m_pSocketsIPv6.ToArray())
                {
                    if (localEP.Equals((IPEndPoint)s.LocalEndPoint))
                    {
                        socket = s;
                        break;
                    }
                }
            }
            else
            { throw new ArgumentException($"Argument '{nameof(localEP)}' has unknown AddressFamily."); }

            // We don't have specified local end point.
            if (socket == null)
            { throw new ArgumentException($"Specified local end point '{localEP}' doesn't exist."); }

            SendPacket(socket, remoteEP, packet, offset, count);
        }

        /// <summary>
        /// Sends specified UDP packet to the specified remote end point.
        /// </summary>
        /// <param name="socket">UDP socket to use for data sending.</param>
        /// <param name="remoteEP">Remote end point.</param>
        /// <param name="packet">UDP packet to send.</param>
        /// <param name="offset">Offset in the buffer.</param>
        /// <param name="count">Number of bytes to send.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised whan UDP server is not running and this method is accessed.</exception>
        /// <exception cref="ArgumentNullException">Is raised when any of the arumnets is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public void SendPacket(Socket socket, IPEndPoint remoteEP, byte[] packet, int offset, int count)
        {
            ThrowIfObjectDisposed();
            ThrowIfNotRunning();

            AssertUtil.ArgumentNotNull(remoteEP, nameof(remoteEP));

            if (packet == null)
            { throw new ArgumentNullException(nameof(packet)); }

            if (offset < 0)
            { throw new ArgumentOutOfRangeException(nameof(offset), $"Argument '{nameof(offset)}' value must be >= 0."); }

            if (offset >= packet.Length)
            { throw new ArgumentOutOfRangeException(nameof(offset), $"Argument '{nameof(offset)}' value exceeds the maximum length of argument '{nameof(packet)}'."); }

            if (count < 0)
            { throw new ArgumentOutOfRangeException(nameof(count), $"Argument '{nameof(count)}' value must be >= 0."); }

            if (offset + count > packet.Length)
            { throw new ArgumentOutOfRangeException(nameof(count), $"Argument '{nameof(offset)} + {nameof(count)}' value exceeds the maximum length of argument '{nameof(packet)}'."); }


            if (socket == null)
            { socket = SelectBindSocket(remoteEP); }

            socket.SendTo(packet, offset, count, SocketFlags.None, remoteEP);

            m_PacketsSent++;
            m_BytesSent += count;
        }

        #endregion

        #region method ProcessUdpPacket

        /// <summary>
        /// Processes specified incoming packet.
        /// </summary>
        /// <param name="e">Packet event data.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>e</b> is null reference.</exception>
        private void ProcessUdpPacket(UDP_e_PacketReceived e)
        {
            if (e == null)
            { throw new ArgumentNullException(nameof(e)); }

            m_PacketsReceived++;
            m_BytesReceived += e.Count;

            OnUdpPacketReceived(e);
        }

        #endregion

        #region method SelectBindSocket

        /// <summary>
        /// Gets suitable socket for the specified remote endpoint.
        /// If there are multiple sockets, they will be load-balanched with round-robin.
        /// </summary>
        /// <param name="remoteEP">Remote end point.</param>
        /// <returns>Returns local socket.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="ArgumentNullException">Is raised when argument <b>remoteEP</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when argument <b>remoteEP</b> has invalid value.</exception>
        /// <exception cref="InvalidOperationException">Is raised when no suitable IPv4 or IPv6 socket for <b>remoteEP</b>.</exception>
        public Socket SelectBindSocket(IPEndPoint remoteEP)
        {
            ThrowIfObjectDisposed();

            if (remoteEP == null)
            { throw new ArgumentNullException(nameof(remoteEP)); }

            if (m_pBindSelector != null)
            {
                var bind = m_pBindSelector.Select(remoteEP);
                if (!string.IsNullOrWhiteSpace(bind))
                { return m_pBindSockets[bind]; }
                else
                {
                    return SelectSocketByAddrFamily(remoteEP);
                }
            }
            else
            {
                return SelectSocketByAddrFamily(remoteEP);
            }
        }

        private Socket SelectSocketByAddrFamily(IPEndPoint remoteEP)
        {
            if (remoteEP.AddressFamily == AddressFamily.InterNetwork)
            {
                // We don't have any IPv4 local end point.
                if (m_pSocketsIPv4.Count == 0)
                { throw new InvalidOperationException("There is no suitable IPv4 local end point in this.Bindings."); }

                return m_pSocketsIPv4.Next();
            }
            else if (remoteEP.AddressFamily == AddressFamily.InterNetworkV6)
            {
                // We don't have any IPv6 local end point.
                if (m_pSocketsIPv6.Count == 0)
                { throw new InvalidOperationException("There is no suitable IPv6 local end point in this.Bindings."); }

                return m_pSocketsIPv6.Next();
            }
            else
            { throw new ArgumentException($"Argument '{nameof(remoteEP)}' has unknown AddressFamily."); }
        }

        /// <summary>
        /// Gets suitable local IP end point for the specified remote endpoint.
        /// If there are multiple sending local end points, they will be load-balanched with round-robin.
        /// </summary>
        /// <param name="remoteEP">Remote end point.</param>
        /// <returns>Returns local IP end point.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="ArgumentNullException">Is raised when argument <b>remoteEP</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when argument <b>remoteEP</b> has invalid value.</exception>
        /// <exception cref="InvalidOperationException">Is raised when no suitable IPv4 or IPv6 socket for <b>remoteEP</b>.</exception>
        public IPEndPoint SelectLocalEndPoint(IPEndPoint remoteEP)
        {
            ThrowIfObjectDisposed();

            if (remoteEP == null)
            { throw new ArgumentNullException(nameof(remoteEP)); }


            var socket = SelectBindSocket(remoteEP);
            return (IPEndPoint)socket.LocalEndPoint;
        }

        #endregion


        #region Properties Implementation

        /// <summary>
        /// Gets if server is disposed.
        /// </summary>
        public bool IsDisposed
        { get; private set; }

        /// <summary>
        /// Gets if server is running.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public bool IsRunning
        {
            get
            {
                ThrowIfObjectDisposed();

                return m_IsRunning;
            }
        }

        /// <summary>
        /// Gets or sets maximum network transmission unit.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when server is running and this property value is tried to set.</exception>
        public int MTU
        {
            get
            {
                ThrowIfObjectDisposed();

                return m_MTU;
            }

            set
            {
                ThrowIfObjectDisposed();

                if (m_IsRunning)
                { throw new InvalidOperationException("MTU value can be changed only if the server is not running."); }

                if (value <= 0 || value > 1400)
                { throw new ArgumentOutOfRangeException("MTU value must be > 0 and <= 1400."); }

                m_MTU = value;
            }
        }

        /// <summary>
        /// Gets or sets IP end point where server is binded.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="ArgumentNullException">Is raised when null value is passed.</exception>
        public IPEndPoint[] Bindings
        {
            get
            {
                ThrowIfObjectDisposed();

                return m_pBindings;
            }

            set
            {
                ThrowIfObjectDisposed();

                if (value == null)
                { value = new IPEndPoint[0]; }

                // See if changed. Also if server running we must restart it.
                bool changed = false;
                if (m_pBindings.Length != value.Length)
                { changed = true; }
                else
                {
                    for (int i = 0; i < m_pBindings.Length; i++)
                    {
                        if (!m_pBindings[i].Equals(value[i]))
                        {
                            changed = true;
                            break;
                        }
                    }
                }

                if (changed)
                {
                    m_pBindings = value;

                    if (m_IsRunning)
                    { StartListen(); }
                }
            }
        }

        /// <summary>
        /// Gets or sets local socket selector.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public IBindSelector BindSelector
        {
            get
            {
                ThrowIfObjectDisposed();

                return m_pBindSelector;
            }

            set
            {
                ThrowIfObjectDisposed();

                m_pBindSelector = value;

                if (m_pBindSelector != null && m_IsRunning)
                { m_pBindSelector.Load(m_pBindSockets.Keys.ToArray()); }
            }
        }

        /// <summary>
        /// Gets time when server was started.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised whan server is not running and this property is accessed.</exception>
        public DateTime StartTime
        {
            get
            {
                ThrowIfObjectDisposed();
                ThrowIfNotRunning();

                return m_StartTime;
            }
        }

        /// <summary>
        /// Gets how many bytes this server has sent since start.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised whan server is not running and this property is accessed.</exception>
        public long BytesSent
        {
            get
            {
                ThrowIfObjectDisposed();
                ThrowIfNotRunning();

                return m_BytesSent;
            }
        }

        /// <summary>
        /// Gets how many packets this server has sent since start.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised whan server is not running and this property is accessed.</exception>
        public long PacketsSent
        {
            get
            {
                ThrowIfObjectDisposed();
                ThrowIfNotRunning();

                return m_PacketsSent;
            }
        }

        /// <summary>
        /// Gets how many bytes this server has received since start.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised whan server is not running and this property is accessed.</exception>
        public long BytesReceived
        {
            get
            {
                ThrowIfObjectDisposed();
                ThrowIfNotRunning();

                return m_BytesReceived;
            }
        }

        /// <summary>
        /// Gets how many packets this server has received since start.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised whan server is not running and this property is accessed.</exception>
        public long PacketsReceived
        {
            get
            {
                ThrowIfObjectDisposed();
                ThrowIfNotRunning();

                return m_PacketsReceived;
            }
        }

        #endregion

        #region Events Implementation

        /// <summary>
        /// This event is raised when new packet received.
        /// </summary>
        public event EventHandler<UDP_e_PacketReceived> PacketReceived = null;

        /// <summary>
        /// Raises PacketReceived event.
        /// </summary>
        /// <param name="e">Event data.</param>
        private void OnUdpPacketReceived(UDP_e_PacketReceived e)
        {
            if (this.PacketReceived != null)
            { this.PacketReceived(this, e); }
        }


        /// <summary>
        /// This event is raised when unexpected error happens.
        /// </summary>
        public event ErrorEventHandler Error = null;

        /// <summary>
        /// Raises Error event.
        /// </summary>
        /// <param name="ex">Exception occured.</param>
        private void OnError(Exception ex)
        {
            if (this.Error != null)
            { this.Error(this, new ErrorEventArgs(ex, new StackTrace(ex, true))); }
        }

        #endregion
    }
}