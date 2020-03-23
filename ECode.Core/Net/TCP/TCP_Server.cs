using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ECode.Core;
using ECode.Utility;

namespace ECode.Net.Tcp
{
    public class TCP_Server<T> : IDisposable where T : TCP_ServerSession, new()
    {
        /// <summary>
        /// This class holds listening point info.
        /// </summary>
        class ListeningPoint
        {
            /// <summary>
            /// Default constructor.
            /// </summary>
            /// <param name="socket">Listening socket.</param>
            /// <param name="bindInfo">Bind info what acceped socket.</param>
            /// <exception cref="ArgumentNullException">Is raised when <b>socket</b> or <b>bind</b> is null.</exception>
            public ListeningPoint(Socket socket, IPBindInfo bindInfo)
            {
                AssertUtil.ArgumentNotNull(socket, nameof(socket));
                AssertUtil.ArgumentNotNull(bindInfo, nameof(bindInfo));

                this.Socket = socket;
                this.BindInfo = bindInfo;
            }


            #region Properties Implementation

            /// <summary>
            /// Gets socket.
            /// </summary>
            public Socket Socket { get; private set; }

            /// <summary>
            /// Gets bind info.
            /// </summary>
            public IPBindInfo BindInfo { get; private set; }

            #endregion
        }

        /// <summary>
        /// Implements single TCP connection acceptor.
        /// </summary>
        /// <remarks>For higher performance, mutiple acceptors per socket must be created.</remarks>
        class TCP_Acceptor : IDisposable
        {
            /// <summary>
            /// Default constructor.
            /// </summary>
            /// <param name="socket">Socket.</param>
            /// <exception cref="ArgumentNullException">Is raised when <b>socket</b> is null reference.</exception>
            public TCP_Acceptor(Socket socket)
            {
                AssertUtil.ArgumentNotNull(socket, nameof(socket));

                this.Socket = socket;
            }


            public void Dispose()
            {
                if (this.IsDisposed)
                { return; }

                this.IsDisposed = true;

                this.Tags = null;
                this.Socket = null;
                this.SocketArgs = null;

                this.ConnectionAccepted = null;
                this.Error = null;
            }

            private void ThrowIfObjectDisposed()
            {
                if (this.IsDisposed)
                { throw new ObjectDisposedException(this.GetType().Name); }
            }


            /// <summary>
            /// Starts accpeting connections.
            /// </summary>
            /// <exception cref="ObjectDisposedException">Is raised when this calss is disposed and this method is accessed.</exception>
            public void Start()
            {
                ThrowIfObjectDisposed();

                if (this.IsRunning)
                { return; }

                this.IsRunning = true;


                // Move processing to thread pool.
                ThreadPool.QueueUserWorkItem((o) =>
                {
                    if (this.IsDisposed)
                    { return; }

                    try
                    {
                        #region IO completion ports

                        this.SocketArgs = new SocketAsyncEventArgs();
                        this.SocketArgs.Completed += (s1, e1) =>
                        {
                            try
                            {
                                if (this.SocketArgs.SocketError == SocketError.Success)
                                { OnConnectionAccepted(this.SocketArgs.AcceptSocket); }
                                else
                                { OnError(new Exception($"Socket error '{this.SocketArgs.SocketError}'.")); }

                                IOCompletionAccept();
                            }
                            catch (Exception ex)
                            { OnError(ex); }
                        };

                        IOCompletionAccept();

                        #endregion
                    }
                    catch (Exception ex)
                    { OnError(ex); }
                });
            }

            /// <summary>
            /// Accpets connection synchornously(if connection(s) available now) or starts waiting TCP connection asynchronously if no connections at moment.
            /// </summary>
            private void IOCompletionAccept()
            {
                try
                {
                    // We need to clear it, before reuse.
                    this.SocketArgs.AcceptSocket = null;

                    // Use active worker thread as long as ReceiveFromAsync completes synchronously.
                    // (With this approach we don't have thread context switches while ReceiveFromAsync completes synchronously)
                    while (!this.IsDisposed && !this.Socket.AcceptAsync(this.SocketArgs))
                    {
                        if (this.SocketArgs.SocketError == SocketError.Success)
                        {
                            try
                            {
                                OnConnectionAccepted(this.SocketArgs.AcceptSocket);

                                // We need to clear it, before reuse.
                                this.SocketArgs.AcceptSocket = null;
                            }
                            catch (Exception ex)
                            { OnError(ex); }
                        }
                        else
                        { OnError(new Exception($"Socket error '{this.SocketArgs.SocketError}'.")); }
                    }
                }
                catch (Exception ex)
                { OnError(ex); }
            }


            #region Properties Implementation

            /// <summary>
            /// Gets if this object is disposed.
            /// </summary>
            private bool IsDisposed { get; set; }

            /// <summary>
            /// Gets if this object is running.
            /// </summary>
            private bool IsRunning { get; set; }

            /// <summary>
            /// Gets socket.
            /// </summary>
            private Socket Socket { get; set; }

            /// <summary>
            /// Gets socket args.
            /// </summary>
            private SocketAsyncEventArgs SocketArgs { get; set; }

            /// <summary>
            /// Gets user data items.
            /// </summary>
            public Dictionary<string, object> Tags
            { get; private set; } = new Dictionary<string, object>();

            #endregion

            #region Events Implementation

            /// <summary>
            /// Is raised when new connection was accepted.
            /// </summary>
            public event EventHandler<EventArgs<Socket>> ConnectionAccepted = null;

            /// <summary>
            /// Raises <b>ConnectionAccepted</b> event.
            /// </summary>
            /// <param name="socket">Accepted socket.</param>
            private void OnConnectionAccepted(Socket socket)
            {
                if (this.ConnectionAccepted != null)
                { this.ConnectionAccepted(this, new EventArgs<Socket>(socket)); }
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
                { this.Error(this, new ErrorEventArgs(ex, new StackTrace(ex, true))); }
            }

            #endregion
        }


        private bool                                     m_IsRunning                = false;
        private IPBindInfo[]                             m_pBindings                = new IPBindInfo[0];
        private int                                      m_AcceptorsPerSocket       = 10;
        private int                                      m_MaxConnections           = 0;
        private int                                      m_MaxConnectionsPerIP      = 0;
        private int                                      m_SessionIdleTimeout       = 60;  // 60s
        private DateTime                                 m_StartTime;
        private long                                     m_ConnectionsProcessed     = 0;
        private List<ListeningPoint>                     m_pListeningPoints         = null;
        private List<TCP_Acceptor>                       m_pConnectionAcceptors     = null;
        private TCP_SessionCollection<TCP_ServerSession> m_pSessions                = null;
        private TimerEx                                  m_pTimer_IdleTimeout       = null;


        public TCP_Server(int socketAcceptors = 10)
        {
            if (socketAcceptors <= 0)
            { socketAcceptors = 10; }
            else if (socketAcceptors > 50)
            { socketAcceptors = 50; }

            m_AcceptorsPerSocket = socketAcceptors;
            m_pListeningPoints = new List<ListeningPoint>();
            m_pConnectionAcceptors = new List<TCP_Acceptor>();
            m_pSessions = new TCP_SessionCollection<TCP_ServerSession>();
        }


        public void Dispose()
        {
            if (this.IsDisposed)
            { return; }

            if (m_IsRunning)
            {
                try
                { Stop(); }
                catch (Exception ex)
                {
                    // just skip.
                    string dummy = ex.Message;
                }
            }

            // We must call disposed event before we release events.
            try
            { OnDisposed(); }
            catch (Exception ex)
            {
                // just skip.
                string dummy = ex.Message;
            }

            this.IsDisposed = true;

            m_pSessions.Clear();
            m_pSessions = null;

            // Release all events.
            this.Started = null;
            this.Stopped = null;
            this.Error = null;
            this.Disposed = null;
            this.WriteLog = null;
            this.SessionCreated = null;
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
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public void Start()
        {
            ThrowIfObjectDisposed();

            if (m_IsRunning)
            { return; }

            m_IsRunning = true;
            m_StartTime = DateTime.Now;
            m_ConnectionsProcessed = 0;

            ThreadPool.QueueUserWorkItem((o) =>
            {
                StartListen();
            });


            m_pTimer_IdleTimeout = new TimerEx(30 * 1000); // 30s
            m_pTimer_IdleTimeout.Elapsed += (sender, e) =>
            {
                try
                {
                    foreach (T session in this.Sessions.ToArray())
                    {
                        try
                        {
                            if (m_SessionIdleTimeout > 0
                                && DateTime.Now > session.TcpStream.LastActivity.AddSeconds(m_SessionIdleTimeout))
                            {
                                session.OnTimeout();

                                // Session didn't dispose itself, so dispose it.
                                if (!session.IsDisposed)
                                {
                                    session.Disconnect();
                                    session.Dispose();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // just skip.
                            string dummy = ex.Message;
                        }
                    }
                }
                catch (Exception ex)
                { OnError(ex); }
            };

            OnStarted();
        }

        /// <summary>
        /// Stops this server, all active connections will be terminated.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public void Stop()
        {
            ThrowIfObjectDisposed();

            if (!m_IsRunning)
            { return; }

            m_IsRunning = false;

            // Dispose all old TCP acceptors.
            foreach (var acceptor in m_pConnectionAcceptors.ToArray())
            {
                try
                { acceptor.Dispose(); }
                catch (Exception ex)
                { OnError(ex); }
            }
            m_pConnectionAcceptors.Clear();


            // Dispose all old binds.
            foreach (var listeningPoint in m_pListeningPoints.ToArray())
            {
                try
                { listeningPoint.Socket.Dispose(); }
                catch (Exception ex)
                { OnError(ex); }
            }
            m_pListeningPoints.Clear();


            m_pTimer_IdleTimeout.Stop();
            m_pTimer_IdleTimeout.Dispose();
            m_pTimer_IdleTimeout = null;

            OnStopped();
        }

        /// <summary>
        /// Restarts running server. If server is not running, this methods has no efffect.
        /// </summary>
        public void Restart()
        {
            if (m_IsRunning)
            {
                Stop();
                Start();
            }
        }

        /// <summary>
        /// Starts listening incoming connections. NOTE: All active listening points will be disposed.
        /// </summary>
        private void StartListen()
        {
            try
            {
                // Dispose all old acceptors.
                foreach (var acceptor in m_pConnectionAcceptors.ToArray())
                {
                    try
                    { acceptor.Dispose(); }
                    catch (Exception ex)
                    { OnError(ex); }
                }
                m_pConnectionAcceptors.Clear();

                // Dispose all old binds.
                foreach (var listeningPoint in m_pListeningPoints.ToArray())
                {
                    try
                    { listeningPoint.Socket.Dispose(); }
                    catch (Exception ex)
                    { OnError(ex); }
                }
                m_pListeningPoints.Clear();


                // Create new listening points and start accepting connections.
                foreach (var bindInfo in m_pBindings)
                {
                    try
                    {
                        Socket socket = null;
                        if (bindInfo.IP.AddressFamily == AddressFamily.InterNetwork)
                        { socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); }
                        else if (bindInfo.IP.AddressFamily == AddressFamily.InterNetworkV6)
                        { socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp); }
                        else
                        {
                            // Invalid address family, just skip it.
                            continue;
                        }

                        socket.Bind(bindInfo.EndPoint);
                        socket.Listen(100);

                        var listeningPoint = new ListeningPoint(socket, bindInfo);
                        m_pListeningPoints.Add(listeningPoint);

                        // Create TCP connection acceptors.
                        for (int i = 0; i < m_AcceptorsPerSocket; i++)
                        {
                            var acceptor = new TCP_Acceptor(socket);
                            acceptor.Tags["bind"] = bindInfo;
                            acceptor.ConnectionAccepted += (s1, e1) =>
                            {
                                // NOTE: We may not use 'bind' variable here, foreach changes it's value before we reach here.
                                ProcessConnection(e1.Value, (IPBindInfo)acceptor.Tags["bind"]);
                            };

                            acceptor.Error += (s1, e1) =>
                            {
                                OnError(e1.Exception);
                            };

                            m_pConnectionAcceptors.Add(acceptor);
                            acceptor.Start();
                        }
                    }
                    catch (Exception ex)
                    {
                        // The only exception what we should get there is if socket is in use.
                        OnError(ex);
                    }
                }
            }
            catch (Exception ex)
            { OnError(ex); }
        }

        /// <summary>
        /// Processes specified connection.
        /// </summary>
        /// <param name="socket">Accpeted socket.</param>
        /// <param name="bindInfo">Local bind info what accpeted connection.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>socket</b> or <b>bindInfo</b> is null reference.</exception>
        private void ProcessConnection(Socket socket, IPBindInfo bindInfo)
        {
            AssertUtil.ArgumentNotNull(socket, nameof(socket));
            AssertUtil.ArgumentNotNull(bindInfo, nameof(bindInfo));

            m_ConnectionsProcessed++;

            try
            {
                T session = new T();
                session.Init(this, socket, bindInfo.HostName, bindInfo.SslMode == SslMode.SSL, bindInfo.Certificate);

                // Maximum allowed connections exceeded, reject connection.
                if (m_MaxConnections > 0 && m_pSessions.Count > m_MaxConnections)
                {
                    OnMaxConnectionsExceeded(session);
                    session.Dispose();
                }
                // Maximum allowed connections per IP exceeded, reject connection.
                else if (m_MaxConnectionsPerIP > 0
                         && m_pSessions.GetConnectionsPerIP(session.RemoteEndPoint.Address) > m_MaxConnectionsPerIP)
                {
                    OnMaxConnectionsPerIPExceeded(session);
                    session.Dispose();
                }
                // Start processing new session.
                else
                {
                    session.Disonnected += (sender, e) =>
                    {
                        m_pSessions.Remove((TCP_ServerSession)sender);
                    };

                    m_pSessions.Add(session);
                    OnSessionCreated(session);

                    session.Start();
                }
            }
            catch (Exception ex)
            { OnError(ex); }
        }

        /// <summary>
        /// Is called when new incoming session and server maximum allowed connections exceeded.
        /// </summary>
        /// <param name="session">Incoming session.</param>
        /// <remarks>
        /// This method allows inhereted classes to report error message to connected client.
        /// Session will be disconnected after this method completes.
        /// </remarks>
        protected virtual void OnMaxConnectionsExceeded(T session)
        {

        }

        /// <summary>
        /// Is called when new incoming session and server maximum allowed connections per connected IP exceeded.
        /// </summary>
        /// <param name="session">Incoming session.</param>
        /// <remarks>
        /// This method allows inhereted classes to report error message to connected client.
        /// Session will be disconnected after this method completes.
        /// </remarks>
        protected virtual void OnMaxConnectionsPerIPExceeded(T session)
        {

        }


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
        /// Gets or sets server IP bindings.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public IPBindInfo[] Bindings
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
                { value = new IPBindInfo[0]; }

                //--- See binds has changed --------------
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
        /// Gets local listening IP end points.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public IPEndPoint[] LocalEndPoints
        {
            get
            {
                ThrowIfObjectDisposed();

                var retVal = new List<IPEndPoint>();
                foreach (var bindInfo in this.Bindings)
                {
                    if (bindInfo.IP.Equals(IPAddress.Any))
                    {
                        foreach (var ip in NetworkUtil.GetIPAddresses())
                        {
                            if (ip.AddressFamily == AddressFamily.InterNetwork
                                && !retVal.Contains(new IPEndPoint(ip, bindInfo.Port)))
                            { retVal.Add(new IPEndPoint(ip, bindInfo.Port)); }
                        }
                    }
                    else if (bindInfo.IP.Equals(IPAddress.IPv6Any))
                    {
                        foreach (var ip in NetworkUtil.GetIPAddresses())
                        {
                            if (ip.AddressFamily == AddressFamily.InterNetworkV6
                                && !retVal.Contains(new IPEndPoint(ip, bindInfo.Port)))
                            { retVal.Add(new IPEndPoint(ip, bindInfo.Port)); }
                        }
                    }
                    else
                    {
                        if (!retVal.Contains(bindInfo.EndPoint))
                        { retVal.Add(bindInfo.EndPoint); }
                    }
                }

                return retVal.ToArray();
            }
        }

        /// <summary>
        /// Gets or sets maximum allowed concurent connections. Value lte 0 means unlimited.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="ArgumentException">Is raised when negative value is passed.</exception>
        public int MaxConnections
        {
            get
            {
                ThrowIfObjectDisposed();

                return m_MaxConnections;
            }

            set
            {
                ThrowIfObjectDisposed();

                m_MaxConnections = value;
            }
        }

        /// <summary>
        /// Gets or sets maximum allowed connections for 1 IP address. Value lte 0 means unlimited.
        /// </summary>>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="ArgumentException">Is raised when negative value is passed.</exception>
        public int MaxConnectionsPerIP
        {
            get
            {
                ThrowIfObjectDisposed();

                return m_MaxConnectionsPerIP;
            }

            set
            {
                ThrowIfObjectDisposed();

                m_MaxConnectionsPerIP = value;
            }
        }

        /// <summary>
        /// Gets or sets maximum allowed session idle time in seconds, after what session will be terminated. Value lte 0 means unlimited,
        /// but this is strongly not recommened.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="ArgumentException">Is raised when negative value is passed.</exception>
        public int SessionIdleTimeout
        {
            get
            {
                ThrowIfObjectDisposed();

                return m_SessionIdleTimeout;
            }

            set
            {
                ThrowIfObjectDisposed();

                m_SessionIdleTimeout = value;
            }
        }

        /// <summary>
        /// Gets the time when server was started.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TCP server is not running and this property is accesed.</exception>
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
        /// Gets how many connections this TCP server has processed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TCP server is not running and this property is accesed.</exception>
        public long ConnectionsProcessed
        {
            get
            {
                ThrowIfObjectDisposed();
                ThrowIfNotRunning();

                return m_ConnectionsProcessed;
            }
        }

        /// <summary>
        /// Gets TCP server active sessions.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TCP server is not running and this property is accesed.</exception>
        public TCP_SessionCollection<TCP_ServerSession> Sessions
        {
            get
            {
                ThrowIfObjectDisposed();
                ThrowIfNotRunning();

                return m_pSessions;
            }
        }

        #endregion

        #region Events Implementation

        /// <summary>
        /// This event is raised when TCP server has started.
        /// </summary>
        public event EventHandler Started = null;

        /// <summary>
        /// Raises <b>Started</b> event.
        /// </summary>
        protected void OnStarted()
        {
            if (this.Started != null)
            { this.Started(this, new EventArgs()); }
        }


        /// <summary>
        /// This event is raised when TCP server has stopped.
        /// </summary>
        public event EventHandler Stopped = null;

        /// <summary>
        /// Raises <b>Stopped</b> event.
        /// </summary>
        protected void OnStopped()
        {
            if (this.Stopped != null)
            { this.Stopped(this, new EventArgs()); }
        }


        /// <summary>
        /// This event is raised when TCP server has unknown unhandled error.
        /// </summary>
        public event ErrorEventHandler Error = null;

        /// <summary>
        /// Raises <b>Error</b> event.
        /// </summary>
        /// <param name="ex">Exception happened.</param>
        private void OnError(Exception ex)
        {
            if (this.Error != null)
            { this.Error(this, new ErrorEventArgs(ex, new StackTrace(ex, true))); }
        }


        /// <summary>
        /// This event is raised when TCP server has disposed.
        /// </summary>
        public event EventHandler Disposed = null;

        /// <summary>
        /// Raises <b>Disposed</b> event.
        /// </summary>
        protected void OnDisposed()
        {
            if (this.Disposed != null)
            { this.Disposed(this, new EventArgs()); }
        }


        /// <summary>
        /// Is raised when new log entry is available.
        /// </summary>
        public event WriteLogEventHandler WriteLog = null;

        /// <summary>
        /// Raises <b>WriteLog</b> event.
        /// </summary>
        /// <param name="entry">Log entry.</param>
        internal void OnWriteLog(LogEntry entry)
        {
            if (this.WriteLog != null)
            {
                this.WriteLog(entry);
            }
        }


        /// <summary>
        /// This event is raised when TCP server creates new session.
        /// </summary>
        public event EventHandler<TCP_ServerSessionEventArgs<T>> SessionCreated = null;

        /// <summary>
        /// Raises <b>SessionCreated</b> event.
        /// </summary>
        /// <param name="session">TCP server session that was created.</param>
        private void OnSessionCreated(T session)
        {
            if (this.SessionCreated != null)
            { this.SessionCreated(this, new TCP_ServerSessionEventArgs<T>(this, session)); }
        }

        #endregion
    }
}