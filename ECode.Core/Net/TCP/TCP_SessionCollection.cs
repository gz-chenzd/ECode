using System.Collections.Generic;
using System.Linq;
using System.Net;
using ECode.Utility;

namespace ECode.Net.Tcp
{
    public class TCP_SessionCollection<T> where T : TCP_Session
    {
        private Dictionary<string, T>           connections         = null;
        private Dictionary<string, long>        connectionsPerIP    = null;


        internal TCP_SessionCollection()
        {
            connections = new Dictionary<string, T>();
            connectionsPerIP = new Dictionary<string, long>();
        }


        /// <summary>
        /// Adds specified session to the colletion.
        /// </summary>
        /// <param name="session">TCP session to add.</param>
        /// <exception cref="System.ArgumentNullException">Is raised when <b>session</b> is null.</exception>
        internal void Add(T session)
        {
            AssertUtil.ArgumentNotNull(session, nameof(session));

            lock (this)
            {
                connections.Add(session.ID, session);

                if (session.IsConnected && session.RemoteEndPoint != null)
                {
                    var remoteIp = session.RemoteEndPoint.Address.ToString();

                    // Increase connections per IP.
                    if (connectionsPerIP.ContainsKey(remoteIp))
                    { connectionsPerIP[remoteIp]++; }
                    // Just add new entry for that IP address.
                    else
                    { connectionsPerIP.Add(remoteIp, 1); }
                }
            }
        }

        /// <summary>
        /// Removes specified session from the collection.
        /// </summary>
        /// <param name="session">TCP session to remove.</param>
        /// <exception cref="System.ArgumentNullException">Is raised when <b>session</b> is null.</exception>
        internal void Remove(T session)
        {
            AssertUtil.ArgumentNotNull(session, nameof(session));

            lock (this)
            {
                connections.Remove(session.ID);

                // Decrease connections per IP.
                if (session.IsConnected)
                {
                    var remoteIp = session.RemoteEndPoint.Address.ToString();

                    if (!connectionsPerIP.ContainsKey(remoteIp))
                    { return; }

                    connectionsPerIP[remoteIp]--;

                    // Last IP, so remove that IP entry.
                    if (connectionsPerIP[remoteIp] == 0)
                    { connectionsPerIP.Remove(remoteIp); }
                }
            }
        }

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        internal void Clear()
        {
            lock (this)
            {
                connections.Clear();
                connectionsPerIP.Clear();
            }
        }


        /// <summary>
        /// Copies all session to new array. This method is thread-safe.
        /// </summary>
        /// <returns>Returns sessions array.</returns>
        public T[] ToArray()
        {
            lock (this)
            {
                return connections.Values.ToArray();
            }
        }

        /// <summary>
        /// Gets number of connections per specified IP.
        /// </summary>
        /// <param name="ip">IP address.</param>
        /// <returns>Returns current number of connections of the specified IP.</returns>
        /// <exception cref="System.ArgumentNullException">Is raised when <b>ip</b> is null reference.</exception>
        public long GetConnectionsPerIP(IPAddress ip)
        {
            AssertUtil.ArgumentNotNull(ip, nameof(ip));

            connectionsPerIP.TryGetValue(ip.ToString(), out long retVal);

            return retVal;
        }


        #region Properties Implementation

        /// <summary>
        /// Gets number of items in the collection.
        /// </summary>
        public int Count
        {
            get { return connections.Count; }
        }

        /// <summary>
        /// Gets session with the specified ID.
        /// </summary>
        /// <param name="id">Session ID.</param>
        /// <returns>Returns session with the specified ID.</returns>
        public T this[string id]
        {
            get { return connections[id]; }
        }

        #endregion
    }
}
