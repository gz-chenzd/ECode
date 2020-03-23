using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ECode.Utility;

namespace ECode.Net.Udp
{
    /// <summary>
    /// This class implements high performance UDP data receiver.
    /// </summary>
    /// <remarks>NOTE: High performance server applications should create multiple instances of this class per one socket.</remarks>
    public class UDP_DataReceiver : IDisposable
    {
        private bool                        m_IsRunning     = false;
        private bool                        m_IsDisposed    = false;
        private Socket                      m_pSocket       = null;
        private byte[]                      m_pBuffer       = null;
        private int                         m_BufferSize    = 1400;
        private SocketAsyncEventArgs        m_pSocketArgs   = null;


        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="socket">UDP socket.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>socket</b> is null reference.</exception>
        public UDP_DataReceiver(Socket socket, int mtu)
        {
            AssertUtil.ArgumentNotNull(socket, nameof(socket));

            if (mtu <= 0 || mtu > 1400)
            { throw new ArgumentOutOfRangeException($"Argumeng '{nameof(mtu)}' value must be > 0 and <= 1400."); }

            m_pSocket = socket;
            m_BufferSize = mtu;
        }


        public void Dispose()
        {
            if (m_IsDisposed)
            { return; }

            m_IsDisposed = true;

            m_pSocket = null;
            m_pBuffer = null;
            if (m_pSocketArgs != null)
            {
                m_pSocketArgs.Dispose();
                m_pSocketArgs = null;
            }

            this.PacketReceived = null;
            this.Error = null;
        }

        protected void ThrowIfObjectDisposed()
        {
            if (m_IsDisposed)
            { throw new ObjectDisposedException(this.GetType().Name); }
        }


        /// <summary>
        /// Starts receiving data.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this calss is disposed and this method is accessed.</exception>
        public void Start()
        {
            ThrowIfObjectDisposed();

            if (m_IsRunning)
            { return; }

            m_IsRunning = true;

            // Move processing to thread pool.
            ThreadPool.QueueUserWorkItem((o) =>
            {
                if (m_IsDisposed)
                { return; }

                m_pBuffer = new byte[m_BufferSize];

                m_pSocketArgs = new SocketAsyncEventArgs();
                m_pSocketArgs.SetBuffer(m_pBuffer, 0, m_BufferSize);
                m_pSocketArgs.RemoteEndPoint = new IPEndPoint(m_pSocket.AddressFamily == AddressFamily.InterNetwork ? IPAddress.Any : IPAddress.IPv6Any, 0);
                m_pSocketArgs.Completed += (s1, e1) =>
                {
                    try
                    {
                        if (m_pSocketArgs.SocketError == SocketError.Success)
                        { OnPacketReceived((IPEndPoint)m_pSocketArgs.RemoteEndPoint, m_pBuffer, m_pSocketArgs.BytesTransferred); }
                        else
                        { OnError(new Exception($"Socket error '{m_pSocketArgs.SocketError}'.")); }

                        IOCompletionReceive();
                    }
                    catch (Exception ex)
                    { OnError(ex); }
                };

                IOCompletionReceive();
            });
        }

        /// <summary>
        /// Receives synchornously(if packet(s) available now) or starts waiting packet asynchronously if no packets at moment.
        /// </summary>
        private void IOCompletionReceive()
        {
            try
            {
                // Use active worker thread as long as ReceiveFromAsync completes synchronously.
                // (With this approach we don't have thread context switches while ReceiveFromAsync completes synchronously)
                while (!m_IsDisposed && !m_pSocket.ReceiveFromAsync(m_pSocketArgs))
                {
                    if (m_pSocketArgs.SocketError == SocketError.Success)
                    {
                        try
                        { OnPacketReceived((IPEndPoint)m_pSocketArgs.RemoteEndPoint, m_pBuffer, m_pSocketArgs.BytesTransferred); }
                        catch (Exception ex)
                        { OnError(ex); }
                    }
                    else
                    { OnError(new Exception($"Socket error '{m_pSocketArgs.SocketError}'.")); }

                    // Reset remote end point.
                    m_pSocketArgs.RemoteEndPoint = new IPEndPoint(m_pSocket.AddressFamily == AddressFamily.InterNetwork ? IPAddress.Any : IPAddress.IPv6Any, 0);
                }
            }
            catch (Exception ex)
            { OnError(ex); }
        }


        #region Events Implementation

        /// <summary>
        /// Is raised when when new UDP packet is available.
        /// </summary>
        public event EventHandler<UDP_e_PacketReceived> PacketReceived = null;

        /// <summary>
        /// Raises <b>PacketReceived</b> event.
        /// </summary>
        /// <param name="remoteEP">Remote IP end point from where data was received.</param>
        /// <param name="buffer">Data buffer.</param>
        /// <param name="count">Number of bytes stored in <b>buffer</b></param>
        private void OnPacketReceived(IPEndPoint remoteEP, byte[] buffer, int count)
        {
            if (this.PacketReceived != null)
            {
                this.PacketReceived(this, new UDP_e_PacketReceived(m_pSocket, remoteEP, buffer, count));
            }
        }


        /// <summary>
        /// Is raised when unhandled error happens.
        /// </summary>
        public event ErrorEventHandler Error = null;

        /// <summary>
        /// Raises <b>Error</b> event.
        /// </summary>
        /// <param name="ex">Exception happened.</param>
        private void OnError(Exception ex)
        {
            if (this.Error != null)
            {
                this.Error(this, new ErrorEventArgs(ex, new StackTrace(ex, true)));
            }
        }

        #endregion
    }
}