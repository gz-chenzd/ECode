using System;
using System.Net;
using System.Security.Principal;
using ECode.IO;

namespace ECode.Net.Tcp
{
    public abstract class TCP_Session : IDisposable
    {
        /// <summary>
        /// Disconnects session.
        /// </summary>
        public abstract void Disconnect();

        /// <summary>
        /// Cleans up any resources being used.
        /// </summary>
        public abstract void Dispose();


        #region Properties Implementation

        /// <summary>
        /// Gets session ID.
        /// </summary>
        public abstract string ID
        {
            get;
        }

        /// <summary>
        /// Gets if session is connected.
        /// </summary>
        public abstract bool IsConnected
        {
            get;
        }

        /// <summary>
        /// Gets if this session is secure connection.
        /// </summary>
        public virtual bool IsSecureConnection
        {
            get;
        }

        /// <summary>
        /// Gets if this session is authenticated.
        /// </summary>
        public bool IsAuthenticated
        {
            get { return this.AuthenticatedUser != null; }
        }

        /// <summary>
        /// Gets session authenticated user identity , returns null if not authenticated.
        /// </summary>
        public virtual GenericIdentity AuthenticatedUser
        {
            get { return null; }
        }

        /// <summary>
        /// Gets the time when session was connected.
        /// </summary>
        public abstract DateTime ConnectTime
        {
            get;
        }

        /// <summary>
        /// Gets the last time when data was sent or received.
        /// </summary>
        public abstract DateTime LastActivity
        {
            get;
        }

        /// <summary>
        /// Gets TCP stream which must be used to send/receive data through this session.
        /// </summary>
        public abstract SmartStream TcpStream
        {
            get;
        }

        /// <summary>
        /// Gets session local IP end point.
        /// </summary>
        public abstract IPEndPoint LocalEndPoint
        {
            get;
        }

        /// <summary>
        /// Gets session remote IP end point.
        /// </summary>
        public abstract IPEndPoint RemoteEndPoint
        {
            get;
        }

        #endregion
    }
}
