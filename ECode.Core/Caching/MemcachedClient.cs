using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using ECode.IO;
using ECode.Net;
using ECode.Net.Tcp;

namespace ECode.Caching
{
    public class MemcachedClient : Connector
    {
        private const string    OK              = "OK";
        private const string    END             = "END";
        private const string    VALUE           = "VALUE";
        private const string    TOUCHED         = "TOUCHED";
        private const string    DELETED         = "DELETED";
        private const string    EXISTS          = "EXISTS";
        private const string    NOT_FOUND       = "NOT_FOUND";
        private const string    STORED          = "STORED";
        private const string    NOT_STORED      = "NOT_STORED";
        private const string    ERROR           = "ERROR";
        private const string    CLIENT_ERROR    = "CLIENT_ERROR";
        private const string    SERVER_ERROR    = "SERVER_ERROR";


        private TCP_Client      m_pClient       = new TCP_Client();
        private Regex           m_pReNumber     = new Regex("^\\d+$", RegexOptions.Compiled);


        [MethodImpl(MethodImplOptions.Synchronized)]
        protected override void OnDispose()
        {
            try
            {
                if (!m_pClient.IsDisposed && m_pClient.IsConnected)
                {
                    m_pClient.WriteLine("quit");
                    m_pClient.Disconnect();
                }
            }
            catch (Exception ex)
            { string dummy = ex.Message; }

            m_pClient.Dispose();
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


        [MethodImpl(MethodImplOptions.Synchronized)]
        public override void Connect(string host, int port, bool ssl)
        {
            try
            {
                m_pClient.Connect(host, port, ssl);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Quit()
        {
            ThrowIfObjectDisposed();
            ThrowIfNotConnected();

            try
            {
                m_pClient.WriteLine("quit");
                m_pClient.Disconnect();
            }
            catch (SocketException ex)
            {
                m_pClient.Dispose();
                throw ex;
            }
            catch (IOException ex)
            {
                m_pClient.Dispose();
                throw ex;
            }
        }


        public MemcachedCacheItem Get(string key)
        {
            return Get(new string[] { key }).FirstOrDefault();
        }

        public MemcachedCacheItem[] Get(string[] keys)
        {
            return ExecGetCommand("get", keys);
        }

        public MemcachedCacheItem Gets(string key)
        {
            return Gets(new string[] { key }).FirstOrDefault();
        }

        public MemcachedCacheItem[] Gets(string[] keys)
        {
            return ExecGetCommand("gets", keys);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private MemcachedCacheItem[] ExecGetCommand(string cmd, string[] keys)
        {
            ThrowIfObjectDisposed();
            ThrowIfNotConnected();

            try
            {
                m_pClient.WriteLine($"{cmd} {string.Join(" ", keys)}");

                var list = new List<MemcachedCacheItem>();

                while (true)
                {
                    string line = m_pClient.ReadLine();
                    if (line == string.Empty)
                    { line = m_pClient.ReadLine(); }

                    if (line.Equals(END, StringComparison.InvariantCultureIgnoreCase))
                    { break; }

                    if (line.Equals(ERROR, StringComparison.InvariantCultureIgnoreCase))  // not happended yet.
                    { throw new Exception($"Error."); }

                    if (line.StartsWith(CLIENT_ERROR, StringComparison.InvariantCultureIgnoreCase))
                    { throw new Exception($"Client error: {line.Split(' ', 2)[1]}"); }

                    if (line.StartsWith(SERVER_ERROR, StringComparison.InvariantCultureIgnoreCase))
                    { throw new Exception($"Server error: {line.Split(' ', 2)[1]}"); }

                    if (!line.StartsWith(VALUE, StringComparison.InvariantCultureIgnoreCase))
                    { throw new NotSupportedException($"Unsupported response: {line}"); }

                    var items   = line.Split(' ', 5, StringSplitOptions.RemoveEmptyEntries);
                    var kvItem  = new MemcachedCacheItem();
                    kvItem.Key = items[1];
                    kvItem.Flags = int.Parse(items[2]);
                    kvItem.Length = int.Parse(items[3]);
                    kvItem.Revision = items.Length > 4 ? int.Parse(items[4]) : 0;

                    using (var toStream = new MemoryStream())
                    {
                        FixedCountReader.Read(m_pClient.TcpStream, toStream, kvItem.Length);
                        kvItem.ValueBytes = toStream.ToArray();
                    }

                    m_pClient.ReadLine();  // remove end crlf.

                    list.Add(kvItem);
                }

                return list.ToArray();
            }
            catch (SocketException ex)
            {
                m_pClient.Dispose();
                throw ex;
            }
            catch (IOException ex)
            {
                m_pClient.Dispose();
                throw ex;
            }
        }


        public bool Add(string key, byte[] value, int flags, int ttl)
        {
            return ExecSetCommand("add", key, flags, ttl, null, value);
        }

        public bool Set(string key, byte[] value, int flags, int ttl)
        {
            return ExecSetCommand("set", key, flags, ttl, null, value);
        }

        public bool Replace(string key, byte[] value, int flags, int ttl)
        {
            return ExecSetCommand("replace", key, flags, ttl, null, value);
        }

        public bool Append(string key, byte[] value, int flags, int ttl)
        {
            return ExecSetCommand("append", key, flags, ttl, null, value);
        }

        public bool Prepend(string key, byte[] value, int flags, int ttl)
        {
            return ExecSetCommand("prepend", key, flags, ttl, null, value);
        }

        public bool Cas(string key, long revision, byte[] value, int flags, int ttl)
        {
            return ExecSetCommand("cas", key, flags, ttl, revision, value);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private bool ExecSetCommand(string cmd, string key, int flags, int ttl, long? revision, byte[] value)
        {
            ThrowIfObjectDisposed();
            ThrowIfNotConnected();

            try
            {
                m_pClient.WriteLine($"{cmd} {key} {flags} {ttl} {value.Length} {revision}");
                m_pClient.Write(value, 0, value.Length);
                m_pClient.Write(new byte[] { (byte)'\r', (byte)'\n' }, 0, 2);

                var line = m_pClient.ReadLine();
                if (line == string.Empty)
                { line = m_pClient.ReadLine(); }

                if (line.Equals(STORED, StringComparison.InvariantCultureIgnoreCase))
                { return true; }

                if (line.Equals(EXISTS, StringComparison.InvariantCultureIgnoreCase))
                { return false; }

                if (line.Equals(NOT_STORED, StringComparison.InvariantCultureIgnoreCase))
                { return false; }

                if (line.Equals(ERROR, StringComparison.InvariantCultureIgnoreCase))
                { throw new Exception($"Error."); }

                if (line.StartsWith(CLIENT_ERROR, StringComparison.InvariantCultureIgnoreCase))
                { throw new Exception($"Client error: {line.Split(' ', 2)[1]}"); }

                if (line.StartsWith(SERVER_ERROR, StringComparison.InvariantCultureIgnoreCase))
                { throw new Exception($"Server error: {line.Split(' ', 2)[1]}"); }

                throw new NotSupportedException($"Unsupported response: {line}");
            }
            catch (SocketException ex)
            {
                m_pClient.Dispose();
                throw ex;
            }
            catch (IOException ex)
            {
                m_pClient.Dispose();
                throw ex;
            }
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool Delete(string key)
        {
            ThrowIfObjectDisposed();
            ThrowIfNotConnected();

            try
            {
                m_pClient.WriteLine($"delete {key}");

                var line = m_pClient.ReadLine();
                if (line == string.Empty)
                { line = m_pClient.ReadLine(); }

                if (line.Equals(DELETED, StringComparison.InvariantCultureIgnoreCase))
                { return true; }

                if (line.Equals(NOT_FOUND, StringComparison.InvariantCultureIgnoreCase))
                { return false; }

                if (line.Equals(ERROR, StringComparison.InvariantCultureIgnoreCase))  // not happended yet.
                { throw new Exception($"Error."); }

                if (line.StartsWith(CLIENT_ERROR, StringComparison.InvariantCultureIgnoreCase))
                { throw new Exception($"Client error: {line.Split(' ', 2)[1]}"); }

                if (line.StartsWith(SERVER_ERROR, StringComparison.InvariantCultureIgnoreCase))
                { throw new Exception($"Server error: {line.Split(' ', 2)[1]}"); }

                throw new NotSupportedException($"Unsupported response text: {line}");
            }
            catch (SocketException ex)
            {
                m_pClient.Dispose();
                throw ex;
            }
            catch (IOException ex)
            {
                m_pClient.Dispose();
                throw ex;
            }
        }


        public long Incr(string key, int delta)
        {
            return IncrOrDecr("incr", key, delta);
        }

        public long Decr(string key, int delta)
        {
            return IncrOrDecr("decr", key, delta);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private long IncrOrDecr(string cmd, string key, int delta)
        {
            ThrowIfObjectDisposed();
            ThrowIfNotConnected();

            try
            {
                m_pClient.WriteLine($"{cmd} {key} {delta}");

                var line = m_pClient.ReadLine();
                if (line == string.Empty)
                { line = m_pClient.ReadLine(); }

                if (m_pReNumber.IsMatch(line))
                { return long.Parse(line); }

                if (line.Equals(NOT_FOUND, StringComparison.InvariantCultureIgnoreCase))
                { throw new KeyNotFoundException($"Key '{key}' can not be found."); }

                if (line.Equals(ERROR, StringComparison.InvariantCultureIgnoreCase))  // not happended yet.
                { throw new Exception($"Error."); }

                if (line.StartsWith(CLIENT_ERROR, StringComparison.InvariantCultureIgnoreCase))
                { throw new Exception($"Client error: {line.Split(' ', 2)[1]}"); }

                if (line.StartsWith(SERVER_ERROR, StringComparison.InvariantCultureIgnoreCase))
                { throw new Exception($"Server error: {line.Split(' ', 2)[1]}"); }

                throw new NotSupportedException($"Unsupported response: {line}");
            }
            catch (SocketException ex)
            {
                m_pClient.Dispose();
                throw ex;
            }
            catch (IOException ex)
            {
                m_pClient.Dispose();
                throw ex;
            }
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool Touch(string key, int ttl)
        {
            ThrowIfObjectDisposed();
            ThrowIfNotConnected();

            try
            {
                m_pClient.WriteLine($"touch {key} {ttl}");

                var line = m_pClient.ReadLine();
                if (line == string.Empty)
                { line = m_pClient.ReadLine(); }

                if (line.Equals(TOUCHED, StringComparison.InvariantCultureIgnoreCase))
                { return true; }

                if (line.Equals(NOT_FOUND, StringComparison.InvariantCultureIgnoreCase))
                { return false; }

                if (line.Equals(ERROR, StringComparison.InvariantCultureIgnoreCase))  // not happended yet.
                { throw new Exception($"Error."); }

                if (line.StartsWith(CLIENT_ERROR, StringComparison.InvariantCultureIgnoreCase))
                { throw new Exception($"Client error: {line.Split(' ', 2)[1]}"); }

                if (line.StartsWith(SERVER_ERROR, StringComparison.InvariantCultureIgnoreCase))
                { throw new Exception($"Server error: {line.Split(' ', 2)[1]}"); }

                throw new NotSupportedException($"Unsupported response: {line}");
            }
            catch (SocketException ex)
            {
                m_pClient.Dispose();
                throw ex;
            }
            catch (IOException ex)
            {
                m_pClient.Dispose();
                throw ex;
            }
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool FlushAll()
        {
            ThrowIfObjectDisposed();
            ThrowIfNotConnected();

            try
            {
                m_pClient.WriteLine($"flush_all");

                var line = m_pClient.ReadLine();
                if (line == string.Empty)
                { line = m_pClient.ReadLine(); }

                if (line.Equals(OK, StringComparison.InvariantCultureIgnoreCase))
                { return true; }

                if (line.Equals(ERROR, StringComparison.InvariantCultureIgnoreCase))  // not happended yet.
                { throw new Exception($"Error."); }

                if (line.StartsWith(CLIENT_ERROR, StringComparison.InvariantCultureIgnoreCase))
                { throw new Exception($"Client error: {line.Split(' ', 2)[1]}"); }

                if (line.StartsWith(SERVER_ERROR, StringComparison.InvariantCultureIgnoreCase))
                { throw new Exception($"Server error: {line.Split(' ', 2)[1]}"); }

                throw new NotSupportedException($"Unsupported response: {line}");
            }
            catch (SocketException ex)
            {
                m_pClient.Dispose();
                throw ex;
            }
            catch (IOException ex)
            {
                m_pClient.Dispose();
                throw ex;
            }
        }


        #region Properties Implemention

        public override bool IsDisposed
        {
            get { return m_pClient.IsDisposed; }
        }

        public override bool IsConnected
        {
            get { return m_pClient.IsConnected; }
        }

        public override DateTime ConnectTime
        {
            get { return m_pClient.ConnectTime; }
        }

        public override DateTime LastActivity
        {
            get { return m_pClient.LastActivity; }
        }

        public override int ReadWriteTimeout
        {
            get { return m_pClient.ReadWriteTimeout; }

            set { m_pClient.ReadWriteTimeout = value; }
        }

        #endregion
    }
}
