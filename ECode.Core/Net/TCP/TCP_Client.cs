using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using ECode.Core;
using ECode.IO;
using ECode.Utility;

namespace ECode.Net.Tcp
{
    public class TCP_Client : TCP_Session
    {
        private string                               m_ID                               = "";
        private bool                                 m_IsConnected                      = false;
        private bool                                 m_IsSecureConnection               = false;
        private DateTime                             m_ConnectTime;
        private int                                  m_ReadWriteTimeout                 = 60 * 1000;  // 60s
        private IPEndPoint                           m_pLocalEP                         = null;
        private IPEndPoint                           m_pRemoteEP                        = null;
        private SmartStream                          m_pTcpStream                       = null;
        private LocalCertificateSelectionCallback    m_pSelectCertificateCallback       = null;
        private RemoteCertificateValidationCallback  m_pValidateCertificateCallback     = null;
        private byte[]                               m_pLineBuffer                      = null;
        private int                                  m_LineBufferSize                   = 1 * 1024;  // 1kb


        public TCP_Client(int lineBufferSize = 1024)
        {
            if (lineBufferSize < 1024)
            { lineBufferSize = 1024; }

            m_LineBufferSize = lineBufferSize;
        }

        public TCP_Client(byte[] lineBuffer)
        {
            AssertUtil.ArgumentNotNull(lineBuffer, nameof(lineBuffer));

            if (lineBuffer.Length <= 0)
            { throw new ArgumentException($"Argument '{nameof(lineBuffer)}' value length must be > 0."); }

            m_pLineBuffer = lineBuffer;
            m_LineBufferSize = lineBuffer.Length;
        }


        public override void Dispose()
        {
            if (this.IsDisposed)
            { return; }

            this.IsDisposed = true;

            if (this.IsConnected)
            {
                try
                { Disconnect(); }
                catch (Exception ex)
                {
                    string dummy = ex.Message;
                }
            }

            this.WriteLog = null;
        }

        protected void ThrowIfConnected()
        {
            if (this.IsConnected)
            {
                throw new InvalidOperationException("The client is already connected.");
            }
        }

        protected void ThrowIfNotConnected()
        {
            if (!this.IsConnected)
            {
                throw new InvalidOperationException("The client is not connected.");
            }
        }

        protected void ThrowIfObjectDisposed()
        {
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }


        #region method Connect

        /// <summary>
        /// Connects to the specified host. If the hostname resolves to more than one IP address, 
        /// all IP addresses will be tried for connection, until one of them connects.
        /// </summary>
        /// <param name="host">Host name or IP address.</param>
        /// <param name="port">Port to connect.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when client is already connected.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Is raised when port isnot in valid range.</exception>
        public void Connect(string host, int port)
        {
            Connect(host, port, false);
        }

        /// <summary>
        /// Connects to the specified host. If the hostname resolves to more than one IP address, 
        /// all IP addresses will be tried for connection, until one of them connects.
        /// </summary>
        /// <param name="host">Host name or IP address.</param>
        /// <param name="port">Port to connect.</param>
        /// <param name="ssl">Specifies if connects to SSL end point.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when client is already connected.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Is raised when port isnot in valid range.</exception>
        public void Connect(string host, int port, bool ssl)
        {
            ThrowIfObjectDisposed();
            ThrowIfConnected();

            AssertUtil.ArgumentNotEmpty(host, nameof(host));
            AssertUtil.AssertNetworkPort(port, nameof(port));

            var ips = NameResolver.GetHostAddresses(host);
            for (int i = 0; i < ips.Length; i++)
            {
                try
                {
                    Connect(null, new IPEndPoint(ips[i], port), ssl);
                    break;
                }
                catch (Exception ex)
                {
                    if (this.IsConnected)
                    { throw ex; }
                    // Connect failed for specified IP address, 
                    // if there are some more IPs left, try next, otherwise forward exception.
                    else if (i == (ips.Length - 1))
                    { throw ex; }
                }
            }
        }

        /// <summary>
        /// Connects to the specified remote end point.
        /// </summary>
        /// <param name="remoteEP">Remote IP end point where to connect.</param>
        /// <param name="ssl">Specifies if connects to SSL end point.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when client is already connected.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>remoteEP</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public void Connect(IPEndPoint remoteEP, bool ssl)
        {
            Connect(null, remoteEP, ssl);
        }

        /// <summary>
        /// Connects to the specified remote end point.
        /// </summary>
        /// <param name="localEP">Local IP end point to use. Value null means that system will allocate it.</param>
        /// <param name="remoteEP">Remote IP end point to connect.</param>
        /// <param name="ssl">Specifies if connection switches to SSL affter connect.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when client is already connected.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>remoteEP</b> is null reference.</exception>
        public void Connect(IPEndPoint localEP, IPEndPoint remoteEP, bool ssl)
        {
            ThrowIfObjectDisposed();

            AssertUtil.ArgumentNotNull(remoteEP, nameof(remoteEP));


            lock (this)
            {
                ThrowIfConnected();

                // Create socket.
                if (remoteEP.AddressFamily == AddressFamily.InterNetwork)
                {
                    this.Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    this.Socket.SendTimeout = m_ReadWriteTimeout;
                    this.Socket.ReceiveTimeout = m_ReadWriteTimeout;
                }
                else if (remoteEP.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    this.Socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                    this.Socket.SendTimeout = m_ReadWriteTimeout;
                    this.Socket.ReceiveTimeout = m_ReadWriteTimeout;
                }

                // Bind socket to the specified end point.
                if (localEP != null)
                { this.Socket.Bind(localEP); }


                LogAddText($"Connecting to {remoteEP}.");

                this.Socket.Connect(remoteEP);

                LogAddText($"Connected, localEP='{this.Socket.LocalEndPoint}', remoteEP='{this.Socket.RemoteEndPoint}'.");


                m_ID = ObjectId.NewId();
                m_IsConnected = true;
                m_ConnectTime = DateTime.Now;
                m_pLocalEP = (IPEndPoint)this.Socket.LocalEndPoint;
                m_pRemoteEP = (IPEndPoint)this.Socket.RemoteEndPoint;
                m_pTcpStream = new SmartStream(new NetworkStream(this.Socket, true), true);

                // Start SSL handshake.
                if (ssl)
                { SwitchToSecure(); }

                OnConnected();
            }
        }

        /// <summary>
        /// Switches session to secure connection.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TCP client is not connected or is already secure.</exception>
        protected void SwitchToSecure()
        {
            ThrowIfNotConnected();

            if (this.IsSecureConnection)
            { throw new InvalidOperationException("The client is already secure."); }


            LogAddText("Starting SSL negotiation now.");

            var startTime = DateTime.Now;

            var sslStream = new SslStream(m_pTcpStream.SourceStream, false, this.RemoteCertificateValidationCallback, this.LocalCertificateSelectionCallback);
            sslStream.AuthenticateAsClient("dummy");


            // Close old stream, but leave source stream open.
            m_pTcpStream.IsOwner = false;
            m_pTcpStream.Dispose();

            m_IsSecureConnection = true;
            m_pTcpStream = new SmartStream(sslStream, true);

            LogAddText($"SSL negotiation completed successfully in {(DateTime.Now - startTime).TotalMilliseconds.ToString("f2")} ms.");
        }

        private bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors error)
        {
            // User will handle it.
            if (m_pValidateCertificateCallback != null)
            { return m_pValidateCertificateCallback(sender, certificate, chain, error); }
            else
            {
                if (error == SslPolicyErrors.None)
                { return true; }

                if ((error & SslPolicyErrors.RemoteCertificateNameMismatch) > 0)
                {
                    LogAddText($"Ingore remote certificate error '{error}'.");
                    return true;
                }

                // Do not allow this client to communicate with unauthenticated servers.
                LogAddText("Blocked due to remote certificate mismatch.");

                return false;
            }
        }

        private X509Certificate LocalCertificateSelectionCallback(object sender, string targetHost, X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers)
        {
            // User will handle it.
            if (m_pSelectCertificateCallback == null)
            { return null; }

            return m_pSelectCertificateCallback(sender, targetHost, localCertificates, remoteCertificate, acceptableIssuers);
        }


        /// <summary>
        /// This method is called after client has sucessfully connected.
        /// </summary>
        protected virtual void OnConnected()
        {
            // to do something...
        }


        /// <summary>
        /// Disconnects connection.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when client is not connected.</exception>
        public override void Disconnect()
        {
            ThrowIfObjectDisposed();
            ThrowIfNotConnected();

            m_IsConnected = false;
            m_IsSecureConnection = false;

            m_pLocalEP = null;
            m_pRemoteEP = null;
            m_pTcpStream.Dispose();
            m_pTcpStream = null;

            LogAddText("Disconnected.");
        }

        #endregion

        #region method Read

        /// <summary>
        /// Reads and logs specified line from connected host.
        /// </summary>
        /// <returns>Returns readed line.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when client is not connected.</exception>
        public string ReadLine()
        {
            return ReadLine(SizeExceededAction.ThrowException);
        }

        /// <summary>
        /// Reads and logs specified line from connected host.
        /// </summary>
        /// <param name="exceededAction">Specifies how line-reader behaves when maximum line size exceeded.</param>
        /// <returns>Returns readed line.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when client is not connected.</exception>
        public string ReadLine(SizeExceededAction exceededAction)
        {
            ThrowIfObjectDisposed();
            ThrowIfNotConnected();

            if (m_pLineBuffer == null)
            { m_pLineBuffer = new byte[m_LineBufferSize]; }

            var reader = new LineReader(m_pTcpStream, m_pLineBuffer);
            reader.CRLFLines = m_pTcpStream.CRLFLines;
            reader.Read(exceededAction);

            var line = reader.ToLineString(m_pTcpStream.Encoding);

            if (reader.BytesInBuffer > 0)
            { LogAddRead(reader.BytesInBuffer, line); }
            else
            { LogAddText("Remote host closed connection."); }

            return line;
        }

        #endregion

        #region method Write

        /// <summary>
        /// Sends and logs specified data to connected host.
        /// </summary>
        /// <param name="data">Data to send.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>data</b> is null reference.</exception>
        /// <exception cref="InvalidOperationException">Is raised when client is not connected.</exception>
        public int Write(string data)
        {
            ThrowIfObjectDisposed();
            ThrowIfNotConnected();

            int countWritten = m_pTcpStream.Write(data);
            LogAddWrite(countWritten, data);

            m_pTcpStream.Flush();
            return countWritten;
        }

        /// <summary>
        /// Sends and logs specified line to connected host.
        /// </summary>
        /// <param name="line">Line to send.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>line</b> is null reference.</exception>
        /// <exception cref="InvalidOperationException">Is raised when client is not connected.</exception>
        public int WriteLine(string line)
        {
            ThrowIfObjectDisposed();
            ThrowIfNotConnected();

            int countWritten = m_pTcpStream.WriteLine(line);
            LogAddWrite(countWritten, line);

            m_pTcpStream.Flush();
            return countWritten;
        }

        /// <summary>
        /// Sends all <b>stream</b> data to connected host and logs bytes written.
        /// </summary>
        /// <param name="stream">Stream to send.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>stream</b> is null reference.</exception>
        /// <exception cref="InvalidOperationException">Is raised when client is not connected.</exception>
        public long WriteStream(Stream stream)
        {
            ThrowIfObjectDisposed();
            ThrowIfNotConnected();

            long countWritten = m_pTcpStream.WriteStream(stream);
            LogAddWrite(countWritten, $"Sent {countWritten} message bytes.");

            m_pTcpStream.Flush();
            return countWritten;
        }

        /// <summary>
        /// Sends specified number of bytes from source <b>stream</b> to connected host and logs bytes written.
        /// </summary>
        /// <param name="stream">Stream to send.</param>
        /// <param name="count">Number of bytes to send.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>stream</b> is null reference.</exception>
        /// <exception cref="InvalidOperationException">Is raised when client is not connected.</exception>
        public long WriteStream(Stream stream, long count)
        {
            ThrowIfObjectDisposed();
            ThrowIfNotConnected();

            long countWritten = m_pTcpStream.WriteStream(stream, count);
            LogAddWrite(countWritten, $"Sent {countWritten} message bytes.");

            m_pTcpStream.Flush();
            return countWritten;
        }

        /// <summary>
        /// Sends bytes to connected host and logs bytes written.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>bytes</b> is null reference.</exception>
        /// <exception cref="InvalidOperationException">Is raised when client is not connected.</exception>
        public void Write(byte[] bytes, int index, int count)
        {
            ThrowIfObjectDisposed();
            ThrowIfNotConnected();

            m_pTcpStream.Write(bytes, index, count);
            LogAddWrite(count, $"Sent {count} message bytes.");

            m_pTcpStream.Flush();
        }

        #endregion

        #region method Log

        /// <summary>
        /// Logs read operation.
        /// </summary>
        /// <param name="size">Number of bytes readed.</param>
        /// <param name="text">Log text.</param>
        public void LogAddRead(long size, string text)
        {
            LogAddRead(size, text, null);
        }

        /// <summary>
        /// Logs read operation.
        /// </summary>
        /// <param name="size">Number of bytes readed.</param>
        /// <param name="text">Log text.</param>
        /// <param name="extra">Extra messages.</param>
        public void LogAddRead(long size, string text, dynamic extra)
        {
            OnWriteLog(new LogEntry(LogEntryType.Read,
                                    m_ID,
                                    this.AuthenticatedUser,
                                    size,
                                    text,
                                    extra,
                                    null,
                                    m_pLocalEP,
                                    m_pRemoteEP));
        }

        /// <summary>
        /// Logs write operation.
        /// </summary>
        /// <param name="size">Number of bytes written.</param>
        /// <param name="text">Log text.</param>
        public void LogAddWrite(long size, string text)
        {
            LogAddWrite(size, text, null);
        }

        /// <summary>
        /// Logs write operation.
        /// </summary>
        /// <param name="size">Number of bytes written.</param>
        /// <param name="text">Log text.</param>
        /// <param name="extra">Extra messages.</param>
        public void LogAddWrite(long size, string text, dynamic extra)
        {
            OnWriteLog(new LogEntry(LogEntryType.Write,
                                    m_ID,
                                    this.AuthenticatedUser,
                                    size,
                                    text,
                                    extra,
                                    null,
                                    m_pLocalEP,
                                    m_pRemoteEP));
        }

        /// <summary>
        /// Logs free text entry.
        /// </summary>
        /// <param name="text">Log text.</param>
        public void LogAddText(string text)
        {
            LogAddText(text, null);
        }

        /// <summary>
        /// Logs free text entry.
        /// </summary>
        /// <param name="text">Log text.</param>
        /// <param name="extra">Extra messages.</param>
        public void LogAddText(string text, dynamic extra)
        {
            OnWriteLog(new LogEntry(LogEntryType.Text,
                                    m_ID,
                                    this.AuthenticatedUser,
                                    0,
                                    text,
                                    extra,
                                    null,
                                    m_pLocalEP,
                                    m_pRemoteEP));
        }

        /// <summary>
        /// Logs exception.
        /// </summary>
        /// <param name="exception">Exception happened.</param>
        public void LogAddException(Exception exception)
        {
            AssertUtil.ArgumentNotNull(exception, nameof(exception));

            LogAddException(exception.Message, exception);
        }

        /// <summary>
        /// Logs exception.
        /// </summary>
        /// <param name="text">Log text.</param>
        /// <param name="exception">Exception happened.</param>
        public void LogAddException(string text, Exception exception)
        {
            LogAddException(text, exception, null);
        }

        /// <summary>
        /// Logs exception.
        /// </summary>
        /// <param name="text">Log text.</param>
        /// <param name="extra">Extra messages.</param>
        /// <param name="exception">Exception happened.</param>
        public void LogAddException(string text, dynamic extra, Exception exception)
        {
            OnWriteLog(new LogEntry(LogEntryType.Exception,
                                    m_ID,
                                    this.AuthenticatedUser,
                                    0,
                                    text,
                                    extra,
                                    exception,
                                    m_pLocalEP,
                                    m_pRemoteEP));
        }


        /// <summary>
        /// Is raised when new log entry is available.
        /// </summary>
        public event WriteLogEventHandler WriteLog = null;

        /// <summary>
        /// Raises WriteLog event.
        /// </summary>
        /// <param name="entry">Log entry.</param>
        private void OnWriteLog(LogEntry entry)
        {
            if (this.WriteLog != null)
            {
                this.WriteLog(entry);
            }
        }

        #endregion


        #region Properties Implementation

        /// <summary>
        /// Gets socket.
        /// </summary>
        private Socket Socket
        { get; set; }

        /// <summary>
        /// Gets if this object is disposed.
        /// </summary>
        public bool IsDisposed
        { get; private set; }


        /// <summary>
        /// Gets session ID.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when client is not connected.</exception>
        public override string ID
        {
            get
            {
                ThrowIfObjectDisposed();
                ThrowIfNotConnected();

                return m_ID;
            }
        }

        /// <summary>
        /// Gets if session is connected.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public override bool IsConnected
        {
            get
            {
                ThrowIfObjectDisposed();

                return m_IsConnected;
            }
        }

        /// <summary>
        /// Gets if session is secure connection.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when client is not connected.</exception>
        public override bool IsSecureConnection
        {
            get
            {
                ThrowIfObjectDisposed();
                ThrowIfNotConnected();

                return m_IsSecureConnection;
            }
        }

        /// <summary>
        /// Gets the time when session was connected.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when client is not connected.</exception>
        public override DateTime ConnectTime
        {
            get
            {
                ThrowIfObjectDisposed();
                ThrowIfNotConnected();

                return m_ConnectTime;
            }
        }

        /// <summary>
        /// Gets the last time when data was sent or received.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when client is not connected.</exception>
        public override DateTime LastActivity
        {
            get
            {
                ThrowIfObjectDisposed();
                ThrowIfNotConnected();

                return m_pTcpStream.LastActivity;
            }
        }

        /// <summary>
        /// Gets or sets default TCP read/write timeout.
        /// </summary>
        /// <remarks>This timeout applies only synchronous TCP read/write operations.</remarks>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public int ReadWriteTimeout
        {
            get
            {
                ThrowIfObjectDisposed();

                return m_ReadWriteTimeout;
            }

            set
            {
                ThrowIfObjectDisposed();

                m_ReadWriteTimeout = value;

                if (m_IsConnected)
                {
                    this.Socket.SendTimeout = m_ReadWriteTimeout;
                    this.Socket.ReceiveTimeout = m_ReadWriteTimeout;
                }
            }
        }

        /// <summary>
        /// Gets TCP stream which must be used to send/receive data through this session.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when client is not connected.</exception>
        public override SmartStream TcpStream
        {
            get
            {
                ThrowIfObjectDisposed();
                ThrowIfNotConnected();

                return m_pTcpStream;
            }
        }

        /// <summary>
        /// Gets session local IP end point.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when client is not connected.</exception>
        public override IPEndPoint LocalEndPoint
        {
            get
            {
                ThrowIfObjectDisposed();
                ThrowIfNotConnected();

                return m_pLocalEP;
            }
        }

        /// <summary>
        /// Gets session remote IP end point.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when client is not connected.</exception>
        public override IPEndPoint RemoteEndPoint
        {
            get
            {
                ThrowIfObjectDisposed();
                ThrowIfNotConnected();

                return m_pRemoteEP;
            }
        }

        /// <summary>
        /// Gets or sets callback which selects the local ssl certificate used for authentication.
        /// Value null means not sepcified.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public LocalCertificateSelectionCallback SelectCertificateCallback
        {
            get
            {
                ThrowIfObjectDisposed();

                return m_pSelectCertificateCallback;
            }

            set
            {
                ThrowIfObjectDisposed();

                m_pSelectCertificateCallback = value;
            }
        }

        /// <summary>
        /// Gets or sets callback which verifies the remote ssl certificate used for authentication.
        /// Value null means not sepcified.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public RemoteCertificateValidationCallback ValidateCertificateCallback
        {
            get
            {
                ThrowIfObjectDisposed();

                return m_pValidateCertificateCallback;
            }

            set
            {
                ThrowIfObjectDisposed();

                m_pValidateCertificateCallback = value;
            }
        }

        #endregion        
    }
}