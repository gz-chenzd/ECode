using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using ECode.Core;
using ECode.Utility;

namespace ECode.Net
{
    public abstract class ConnectionPool
    {
        internal abstract void ReturnToPool(Connector connector);
    }


    public class ConnectionPool<TConnector> : ConnectionPool, IDisposable where TConnector : Connector, new()
    {
        private string              m_Host                      = null;
        private int                 m_Port                      = 0;
        private bool                m_UseSsl                    = false;
        private int                 m_MinPoolSize               = 0;
        private int                 m_MaxPoolSize               = 100;
        private int                 m_ConnectTimeout            = 3 * 1000;  // 3s
        private int                 m_ReadWriteTimeout          = 5 * 1000;  // 5s
        private int                 m_ConnectionIdleTimeout     = 60;  // 60s
        private int                 m_ConnectionBusyTimeout     = 600;  // 600s
        private TimerEx             m_pTimer_IdleTimeout        = null;
        private List<TConnector>    m_pIdleConnectors           = new List<TConnector>();
        private List<TConnector>    m_pTotalConnectors          = new List<TConnector>();


        /// <summary>
        /// Gets if pool is disposed.
        /// </summary>
        public bool IsDisposed
        { get; private set; }

        /// <summary>
        /// Gets connection target host.
        /// </summary>
        public string TargetHost
        {
            get
            {
                ThrowIfObjectDisposed();

                return m_Host;
            }
        }

        /// <summary>
        /// Gets connection target port.
        /// </summary>
        public int TargetPort
        {
            get
            {
                ThrowIfObjectDisposed();

                return m_Port;
            }
        }

        /// <summary>
        /// Gets if the connections use ssl.
        /// </summary>
        public bool UseSsl
        {
            get
            {
                ThrowIfObjectDisposed();

                return m_UseSsl;
            }
        }

        /// <summary>
        /// Gets or sets the minimum size of the pool (0~1000).
        /// </summary>
        public int MinPoolSize
        {
            get
            {
                ThrowIfObjectDisposed();

                return m_MinPoolSize;
            }

            set
            {
                ThrowIfObjectDisposed();

                if (value < 0 || value > 1000)
                { throw new ArgumentOutOfRangeException(nameof(MinPoolSize), $"Property '{nameof(MinPoolSize)}' value must be >= 0 and <= 1000."); }

                m_MinPoolSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the size of the max pool (1~1000).
        /// </summary>
        public int MaxPoolSize
        {
            get
            {
                ThrowIfObjectDisposed();

                return m_MaxPoolSize;
            }

            set
            {
                ThrowIfObjectDisposed();

                if (value <= 0 || value > 1000)
                { throw new ArgumentOutOfRangeException(nameof(MaxPoolSize), $"Property '{nameof(MaxPoolSize)}' value must be > 0 and <= 1000."); }

                if (value < MinPoolSize)
                { throw new ArgumentException(nameof(MaxPoolSize), $"Property '{nameof(MaxPoolSize)}' value must be >= '{nameof(MinPoolSize)}'."); }

                m_MaxPoolSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the connect timeout in milliseconds (0~60s).
        /// </summary>
        public int ConnectTimeout
        {
            get
            {
                ThrowIfObjectDisposed();

                return m_ConnectTimeout;
            }

            set
            {
                ThrowIfObjectDisposed();

                if (value <= 0 || value > 60 * 1000)
                { throw new ArgumentOutOfRangeException(nameof(ConnectTimeout), $"Property '{nameof(ConnectTimeout)}' value must be > 0 and <= 60000."); }

                m_ConnectTimeout = value;
            }
        }

        /// <summary>
        /// Gets or sets the read/write timeout in milliseconds (0~60s).
        /// </summary>
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

                if (value <= 0 || value > 60 * 1000)
                { throw new ArgumentOutOfRangeException(nameof(ReadWriteTimeout), $"Property '{nameof(ReadWriteTimeout)}' value must be > 0 and <= 60000."); }

                m_ReadWriteTimeout = value;
            }
        }

        /// <summary>
        /// Gets or sets the connection idle timeout in seconds (30~600s).
        /// </summary>
        public int ConnectionIdleTimeout
        {
            get
            {
                ThrowIfObjectDisposed();

                return m_ConnectionIdleTimeout;
            }

            set
            {
                ThrowIfObjectDisposed();

                if (value < 30 || value > 600)
                { throw new ArgumentOutOfRangeException(nameof(ConnectionIdleTimeout), $"Property '{nameof(ConnectionIdleTimeout)}' value must be >= 30 and <= 600."); }

                m_ConnectionIdleTimeout = value;
            }
        }

        /// <summary>
        /// Gets or sets the connection busy timeout in seconds (30~3600s).
        /// </summary>
        public int ConnectionBusyTimeout
        {
            get
            {
                ThrowIfObjectDisposed();

                return m_ConnectionBusyTimeout;
            }

            set
            {
                ThrowIfObjectDisposed();

                if (value < 30 || value > 3600)
                { throw new ArgumentOutOfRangeException(nameof(ConnectionBusyTimeout), $"Property '{nameof(ConnectionBusyTimeout)}' value must be >= 30 and <= 3600."); }

                m_ConnectionBusyTimeout = value;
            }
        }


        /// <summary>
        /// This event is raised when internal error raised.
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


        public ConnectionPool(string host, int port, bool ssl = false)
        {
            AssertUtil.ArgumentNotEmpty(host, nameof(host));
            AssertUtil.AssertNetworkPort(port, nameof(port));

            m_Host = host;
            m_Port = port;
            m_UseSsl = ssl;

            m_pTimer_IdleTimeout = new TimerEx(30 * 1000); // 30s
            m_pTimer_IdleTimeout.Elapsed += (sender, e) =>
            {
                ClearIdleTimeoutConnectors();
            };
            m_pTimer_IdleTimeout.Start();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void ClearIdleTimeoutConnectors()
        {
            foreach (var connector in m_pTotalConnectors.ToArray())
            {
                try
                {
                    if (connector.IsDisposed)
                    {
                        m_pIdleConnectors.Remove(connector);
                        m_pTotalConnectors.Remove(connector);

                        continue;
                    }

                    if (!connector.IsConnected)
                    {
                        if (connector.IsLoanedOut)  // just connecting.
                        { continue; }

                        connector.Owner = null;
                        connector.Dispose();

                        m_pIdleConnectors.Remove(connector);
                        m_pTotalConnectors.Remove(connector);

                        continue;
                    }

                    if (connector.IsLoanedOut && connector.LoanedOutTime.AddSeconds(m_ConnectionBusyTimeout) <= DateTime.Now)
                    {
                        connector.Owner = null;
                        connector.Dispose();

                        m_pIdleConnectors.Remove(connector);
                        m_pTotalConnectors.Remove(connector);

                        continue;
                    }
                }
                catch (Exception ex)
                { string dummy = ex.Message; }
            }

            if (m_pTotalConnectors.Count > m_MinPoolSize)  // keep min pool size.
            {
                foreach (var connector in m_pTotalConnectors.ToArray())
                {
                    try
                    {
                        if (!connector.IsLoanedOut && connector.ReturnedTime.AddSeconds(m_ConnectionIdleTimeout) <= DateTime.Now)
                        {
                            connector.Owner = null;
                            connector.Dispose();

                            m_pIdleConnectors.Remove(connector);
                            m_pTotalConnectors.Remove(connector);

                            continue;
                        }
                    }
                    catch (Exception ex)
                    { string dummy = ex.Message; }
                }
            }
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Dispose()
        {
            if (this.IsDisposed)
            { return; }

            this.IsDisposed = true;


            m_pTimer_IdleTimeout.Stop();
            m_pTimer_IdleTimeout = null;

            foreach (var connector in m_pTotalConnectors.ToArray())
            {
                if (connector.IsDisposed)
                { continue; }

                connector.Owner = null;
                connector.Dispose();
            }

            m_pIdleConnectors.Clear();
            m_pTotalConnectors.Clear();
        }

        private void ThrowIfObjectDisposed()
        {
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        internal override void ReturnToPool(Connector connector)
        {
            if (this.IsDisposed)  // ignore disposed error.
            { return; }

            AssertUtil.ArgumentNotNull(connector, nameof(connector));

            if (!m_pTotalConnectors.Contains(connector as TConnector))
            { return; }

            if (connector.IsDisposed)
            {
                m_pIdleConnectors.Remove(connector as TConnector);
                m_pTotalConnectors.Remove(connector as TConnector);

                return;
            }

            if (!connector.IsConnected)
            {
                connector.Owner = null;
                connector.Dispose();

                m_pIdleConnectors.Remove(connector as TConnector);
                m_pTotalConnectors.Remove(connector as TConnector);

                return;
            }

            connector.IsLoanedOut = false;
            connector.ReturnedTime = DateTime.Now;
            connector.LoanedOutTime = DateTime.MinValue;

            if (!m_pIdleConnectors.Contains(connector as TConnector))
            { m_pIdleConnectors.Insert(0, connector as TConnector); }
        }


        public TConnector GetConnector()
        {
            ThrowIfObjectDisposed();

            var start = DateTime.Now;

        RETRY:
            var connector = GetIdleConnector();
            if (connector != null)
            {
                return connector;
            }

            connector = CreateNewConnector();
            if (connector != null)
            {
                return connector;
            }

            if (start.AddMilliseconds(m_ConnectTimeout) < DateTime.Now)
            {
                throw new TimeoutException("Gets connector timeout from the pool.");
            }

            Thread.Sleep(5);  // just wait sometime
            goto RETRY;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private TConnector GetIdleConnector()
        {
        RETRY:
            if (m_pIdleConnectors.Count == 0)
            { return null; }

            var connector = m_pIdleConnectors[0];
            m_pIdleConnectors.RemoveAt(0);

            if (connector.IsDisposed)
            {
                m_pTotalConnectors.Remove(connector);

                goto RETRY;
            }

            if (!connector.IsConnected)
            {
                connector.Owner = null;
                connector.Dispose();

                m_pTotalConnectors.Remove(connector);

                goto RETRY;
            }

            connector.IsLoanedOut = true;
            connector.LoanedOutTime = DateTime.Now;
            connector.ReturnedTime = DateTime.MinValue;

            return connector;
        }

        private TConnector CreateNewConnector()
        {
            var connector = CreateConnectorSync();

            if (connector == null)  // pool is full.
            { return null; }

            bool timeoutReturned = false;
            bool connectorReturned = false;
            object threadSyncObj = new object();
            var wait = new ManualResetEvent(false);

            new Thread(() =>
            {
                try
                {
                    connector.Connect(m_Host, m_Port, m_UseSsl);

                    if (!connector.IsConnected)
                    {
                        connector.Owner = null;
                        connector.Dispose();

                        m_pTotalConnectors.Remove(connector);

                        return;
                    }

                    lock (threadSyncObj)
                    {
                        if (!timeoutReturned)
                        { return; }

                        if (!connectorReturned)  // give back to pool.
                        { connector.Dispose(); }
                    }
                }
                catch (Exception ex)
                {
                    connector.Owner = null;
                    connector.Dispose();

                    m_pTotalConnectors.Remove(connector);

                    OnError(ex);
                }
                finally { wait.Set(); }
            }).Start();

            new Thread(() =>
            {
                Thread.Sleep(m_ConnectTimeout);

                wait.Set();
            }).Start();


            wait.WaitOne();

            lock (threadSyncObj)
            {
                timeoutReturned = true;
                if (connector.IsDisposed || !connector.IsConnected)
                { return null; }

                connectorReturned = true;
                return connector;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private TConnector CreateConnectorSync()
        {
            if (m_pTotalConnectors.Count >= m_MaxPoolSize)
            { return null; }

            var connector = new TConnector();
            m_pTotalConnectors.Add(connector);

            connector.Owner = this;
            connector.IsLoanedOut = true;
            connector.LoanedOutTime = DateTime.Now;
            connector.ReturnedTime = DateTime.MinValue;
            connector.ReadWriteTimeout = m_ReadWriteTimeout;

            return connector;
        }
    }
}
