using System;

namespace ECode.Net.Tcp
{
    public class TCP_ServerSessionEventArgs<T> : EventArgs where T : TCP_ServerSession, new()
    {
        internal TCP_ServerSessionEventArgs(TCP_Server<T> server, T session)
        {
            this.Server = server;
            this.Session = session;
        }


        #region Properties Implementation

        /// <summary>
        /// Gets server.
        /// </summary>
        public TCP_Server<T> Server
        { get; private set; }

        /// <summary>
        /// Gets session.
        /// </summary>
        public T Session
        { get; private set; }

        #endregion
    }
}