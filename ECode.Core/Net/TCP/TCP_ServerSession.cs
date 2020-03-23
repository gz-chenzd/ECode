using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using ECode.Core;
using ECode.IO;
using ECode.Utility;

namespace ECode.Net.Tcp
{
    public class TCP_ServerSession : TCP_Session
    {
        private string                              m_ID                    = "";
        private object                              m_pServer               = null;
        private MethodInfo                          m_pLogMethod            = null;
        private string                              m_LocalHostName         = "";
        private DateTime                            m_ConnectTime;
        private bool                                m_IsTerminated          = false;
        private IPEndPoint                          m_pLocalEP              = null;
        private IPEndPoint                          m_pRemoteEP             = null;
        private bool                                m_RequireSsl            = false;
        private bool                                m_IsSecureConnection    = false;
        private X509Certificate                     m_pCertificate          = null;
        private SmartStream                         m_pTcpStream            = null;
        private NetworkStream                       m_pRawTcpStream         = null;
        private Dictionary<string, object>          m_pTags                 = null;


        public TCP_ServerSession()
        {
            m_pTags = new Dictionary<string, object>();
        }


        public override void Dispose()
        {
            if (this.IsDisposed)
            { return; }

            if (!m_IsTerminated)
            {
                try
                { Disconnect(); }
                catch (Exception ex)
                {
                    string dummy = ex.Message;
                }
            }

            // We must call disposed event before we release events.
            try
            { OnDisposed(); }
            catch (Exception ex)
            {
                string dummy = ex.Message;
            }

            this.IsDisposed = true;

            m_pTags.Clear();
            m_pTags = null;
            m_pLocalEP = null;
            m_pRemoteEP = null;
            m_pCertificate = null;

            if (m_pTcpStream != null)
            { m_pTcpStream.Dispose(); }
            m_pTcpStream = null;

            if (m_pRawTcpStream != null)
            { m_pRawTcpStream.Dispose(); }
            m_pRawTcpStream = null;

            // Release events.
            this.IdleTimeout = null;
            this.Disonnected = null;
            this.Error = null;
            this.Disposed = null;
        }

        protected void ThrowIfObjectDisposed()
        {
            if (this.IsDisposed)
            { throw new ObjectDisposedException(this.GetType().Name); }
        }


        /// <summary>
        /// Initializes session. This method is called from TCP_Server when new session created.
        /// </summary>
        /// <param name="server">Owner server.</param>
        /// <param name="socket">Connected socket.</param>
        /// <param name="hostName">Local host name.</param>
        /// <param name="requireSsl">Specifies if session should switch to SSL.</param>
        /// <param name="certificate">SSL certificate.</param>
        internal void Init(object server, Socket socket, string hostName, bool requireSsl, X509Certificate certificate)
        {
            // NOTE: We may not raise any event here !

            m_ID = ObjectId.NewId();
            m_pServer = server;
            m_LocalHostName = hostName;
            m_ConnectTime = DateTime.Now;
            m_RequireSsl = requireSsl;
            m_pCertificate = certificate;
            m_pLocalEP = (IPEndPoint)socket.LocalEndPoint;
            m_pRemoteEP = (IPEndPoint)socket.RemoteEndPoint;

            socket.SendBufferSize = 32 * 1024;  // 32k
            socket.ReceiveBufferSize = 32 * 1024;  // 32k

            m_pRawTcpStream = new NetworkStream(socket, true);
            m_pTcpStream = new SmartStream(m_pRawTcpStream, true);

            m_pLogMethod = server.GetType().GetMethod("OnWriteLog", BindingFlags.Instance | BindingFlags.NonPublic);
        }


        /// <summary>
        /// This method is called from TCP server when session should start processing incoming connection.
        /// </summary>
        internal void Start()
        {
            if (m_RequireSsl)
            { SwitchToSecure(); }

            OnStart();
        }

        /// <summary>
        /// Switches session to secure connection.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when connection is already secure or when SSL certificate is not specified.</exception>
        public void SwitchToSecure()
        {
            ThrowIfObjectDisposed();

            if (m_IsSecureConnection)
            { throw new InvalidOperationException("Session is already SSL/TLS."); }

            if (m_pCertificate == null)
            { throw new InvalidOperationException("There is no certificate specified."); }


            LogAddText("Starting SSL negotiation now.");

            var startTime = DateTime.Now;

            var sslStream = new SslStream(m_pTcpStream.SourceStream, true);
            sslStream.AuthenticateAsServer(m_pCertificate);

            // Close old stream, but leave source stream open.
            m_pTcpStream.IsOwner = false;
            m_pTcpStream.Dispose();

            m_IsSecureConnection = true;
            m_pTcpStream = new SmartStream(sslStream, true);

            LogAddText($"SSL negotiation completed successfully in {(DateTime.Now - startTime).TotalMilliseconds.ToString("f2")} ms.");
        }

        /// <summary>
        /// This method is called from TCP server when session should start processing incoming connection.
        /// </summary>
        protected virtual void OnStart()
        {

        }

        /// <summary>
        /// This method is called when specified session times out.
        /// </summary>
        /// <remarks>
        /// This method allows inhereted classes to report error message to connected client.
        /// Session will be disconnected after this method completes.
        /// </remarks>
        protected internal virtual void OnTimeout()
        {

        }


        /// <summary>
        /// Disconnects session.
        /// </summary>
        public override void Disconnect()
        {
            Disconnect(null);
        }

        /// <summary>
        /// Disconnects session.
        /// </summary>
        /// <param name="text">Text what is sent to connected host before disconnecting.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and and this method is accessed.</exception>
        public void Disconnect(string text)
        {
            ThrowIfObjectDisposed();

            if (m_IsTerminated)
            { return; }

            m_IsTerminated = true;

            if (!string.IsNullOrEmpty(text))
            {
                try
                { m_pTcpStream.Write(text); }
                catch (Exception ex)
                { OnError(ex); }
            }

            try
            { OnDisonnected(); }
            catch (Exception ex)
            {
                // We never should get exception here, user should handle it.
                OnError(ex);
            }
        }


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
            m_pLogMethod.Invoke(m_pServer,
                                new[] { new LogEntry(LogEntryType.Read,
                                                     this.ID,
                                                     this.AuthenticatedUser,
                                                     size,
                                                     text,
                                                     extra,
                                                     null,
                                                     this.LocalEndPoint,
                                                     this.RemoteEndPoint) });
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
            m_pLogMethod.Invoke(m_pServer,
                                new[] { new LogEntry(LogEntryType.Write,
                                                     this.ID,
                                                     this.AuthenticatedUser,
                                                     size,
                                                     text,
                                                     extra,
                                                     null,
                                                     this.LocalEndPoint,
                                                     this.RemoteEndPoint) });
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
            m_pLogMethod.Invoke(m_pServer,
                                new[] { new LogEntry(LogEntryType.Text,
                                                     this.ID,
                                                     this.AuthenticatedUser,
                                                     0,
                                                     text,
                                                     extra,
                                                     null,
                                                     this.LocalEndPoint,
                                                     this.RemoteEndPoint) });
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
            m_pLogMethod.Invoke(m_pServer,
                                new[] { new LogEntry(LogEntryType.Exception,
                                                     this.ID,
                                                     this.AuthenticatedUser,
                                                     0,
                                                     text,
                                                     extra,
                                                     exception,
                                                     this.LocalEndPoint,
                                                     this.RemoteEndPoint) });
        }


        #region Properties Implementation

        /// <summary>
        /// Gets if session is disposed.
        /// </summary>
        public bool IsDisposed
        { get; private set; }

        /// <summary>
        /// Gets owner server.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public object Server
        {
            get
            {
                ThrowIfObjectDisposed();

                return m_pServer;
            }
        }

        /// <summary>
        /// Gets local host name.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public string LocalHostName
        {
            get
            {
                ThrowIfObjectDisposed();

                return m_LocalHostName;
            }
        }

        /// <summary>
        /// Gets session certificate.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public X509Certificate Certificate
        {
            get
            {
                ThrowIfObjectDisposed();

                return m_pCertificate;
            }
        }

        /// <summary>
        /// Gets user data items collection.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public Dictionary<string, object> Tags
        {
            get
            {
                ThrowIfObjectDisposed();

                return m_pTags;
            }
        }


        /// <summary>
        /// Gets session ID.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public override string ID
        {
            get
            {
                ThrowIfObjectDisposed();

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

                return true;
            }
        }

        /// <summary>
        /// Gets if session is secure connection.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public override bool IsSecureConnection
        {
            get
            {
                ThrowIfObjectDisposed();

                return m_IsSecureConnection;
            }
        }

        /// <summary>
        /// Gets the time when session was connected.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public override DateTime ConnectTime
        {
            get
            {
                ThrowIfObjectDisposed();

                return m_ConnectTime;
            }
        }

        /// <summary>
        /// Gets the last time when data was sent or received.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public override DateTime LastActivity
        {
            get
            {
                ThrowIfObjectDisposed();

                return m_pTcpStream.LastActivity;
            }
        }

        /// <summary>
        /// Gets TCP stream which must be used to send/receive data through this session.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public override SmartStream TcpStream
        {
            get
            {
                ThrowIfObjectDisposed();

                return m_pTcpStream;
            }
        }

        /// <summary>
        /// Gets session local IP end point.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public override IPEndPoint LocalEndPoint
        {
            get
            {
                ThrowIfObjectDisposed();

                return m_pLocalEP;
            }
        }

        /// <summary>
        /// Gets session remote IP end point.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public override IPEndPoint RemoteEndPoint
        {
            get
            {
                ThrowIfObjectDisposed();

                return m_pRemoteEP;
            }
        }

        #endregion

        #region Events Implementation

        /// <summary>
        /// This event is raised when session idle(no activity) timeout reached.
        /// </summary>
        public event EventHandler IdleTimeout = null;

        /// <summary>
        /// Raises <b>IdleTimeout</b> event.
        /// </summary>
        private void OnIdleTimeout()
        {
            if (this.IdleTimeout != null)
            { this.IdleTimeout(this, new EventArgs()); }
        }


        /// <summary>
        /// This event is raised when session has disconnected and will be disposed soon.
        /// </summary>
        public event EventHandler Disonnected = null;

        /// <summary>
        /// Raises <b>Disonnected</b> event.
        /// </summary>
        private void OnDisonnected()
        {
            if (this.Disonnected != null)
            { this.Disonnected(this, new EventArgs()); }
        }


        /// <summary>
        /// This event is raised when TCP server session has unknown unhandled error.
        /// </summary>
        public event ErrorEventHandler Error = null;

        /// <summary>
        /// Raises <b>Error</b> event.
        /// </summary>
        /// <param name="ex">Exception happened.</param>
        protected virtual void OnError(Exception ex)
        {
            if (this.Error != null)
            { this.Error(this, new ErrorEventArgs(ex, new StackTrace(ex, true))); }
        }


        /// <summary>
        /// This event is raised when session has disposed.
        /// </summary>
        public event EventHandler Disposed = null;

        /// <summary>
        /// Raises <b>Disposed</b> event.
        /// </summary>
        private void OnDisposed()
        {
            if (this.Disposed != null)
            { this.Disposed(this, new EventArgs()); }
        }

        #endregion
    }
}