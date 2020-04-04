using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using ECode.IO;
using ECode.Net;
using ECode.Net.Tcp;

namespace ECode.Caching
{
    public class RedisClient : Connector
    {
        private bool            m_IsAuthenticated   = false;
        private int             m_Database          = -1;
        private bool            m_ScriptEnabled     = true;
        private TCP_Client      m_pClient           = new TCP_Client();
        private Regex           m_pReNumber         = new Regex(@"^(:|\$|\*)(?<number>[-]?\d+)$", RegexOptions.Compiled);


        [MethodImpl(MethodImplOptions.Synchronized)]
        protected override void OnDispose()
        {
            try
            {
                if (!m_pClient.IsDisposed && m_pClient.IsConnected)
                {
                    m_pClient.WriteLine("QUIT");
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

        /// <summary>
        /// Qoutes string and escapes fishy('\',"') chars.
        /// </summary>
        private static string QuoteString(string text)
        {
            var retVal = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (c == '\\')
                {
                    retVal.Append("\\\\");
                }
                else if (c == '\"')
                {
                    retVal.Append("\\\"");
                }
                else
                {
                    retVal.Append(c);
                }
            }

            return "\"" + retVal.ToString() + "\"";
        }


        private void SendCommand(params string[] args)
        {
            m_pClient.WriteLine($"*{args.Length}");
            for (int i = 0; i < args.Length; i++)
            {
                m_pClient.WriteLine($"${args[i].Length}");
                m_pClient.WriteLine($"{args[i]}");
            }
        }

        private (object Value, string Error) ReadResponse()
        {
            var line = m_pClient.ReadLine();
            switch (line[0])
            {
                case '*':  // multi-bulk reply
                    int count = int.Parse(line.Substring(1));
                    if (count <= 0)
                    { return (null, null); }

                    var list = new object[count];
                    for (int i = 0; i < count; i++)
                    {
                        var record = ReadResponse();
                        if (record.Error != null)
                        { return (null, record.Error); }

                        list[i] = record.Value;
                    }

                    return (list, null);

                case '$':  // bulk reply
                    int size = int.Parse(line.Substring(1));
                    if (size <= 0)
                    {
                        if (size == 0)
                        { return (new byte[0], null); }

                        return (null, null);
                    }

                    var buffer = new byte[size];
                    using (var toStream = new MemoryStream())
                    {
                        FixedCountReader.Read(m_pClient.TcpStream, toStream, size);
                        buffer = toStream.ToArray();
                    }

                    m_pClient.ReadLine();  // remove end crlf.

                    return (buffer, null);

                case '+':  // status reply
                    return (line.Substring(1), null);

                case ':':  // integer reply
                    return (int.Parse(line.Substring(1)), null);

                case '-':  // error reply
                    return (null, line.Split(' ', 2)[1]);

                default:  // unknown reply
                    return (null, $"unknown response: {line}");
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
                SendCommand("QUIT");

                //ReadResponse();  // ignore response

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


        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Auth(string password)
        {
            ThrowIfObjectDisposed();
            ThrowIfNotConnected();

            try
            {
                SendCommand("AUTH", password);

                var response = ReadResponse();
                if (response.Error != null)
                { throw new Exception(response.Error); }

                m_IsAuthenticated = true;
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
        public void Ping()
        {
            ThrowIfObjectDisposed();
            ThrowIfNotConnected();

            try
            {
                SendCommand("PING");

                var response = ReadResponse();
                if (response.Error != null)
                { throw new Exception(response.Error); }
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
        public void Select(int db)
        {
            ThrowIfObjectDisposed();
            ThrowIfNotConnected();

            try
            {
                SendCommand("SELECT", db.ToString());

                var response = ReadResponse();
                if (response.Error != null)
                { throw new Exception(response.Error); }

                m_Database = db;
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
        public int Ttl(string key)
        {
            ThrowIfObjectDisposed();
            ThrowIfNotConnected();

            try
            {
                SendCommand("TTL", key);

                var response = ReadResponse();
                if (response.Error != null)
                { throw new Exception(response.Error); }

                if (-2 == (int)response.Value)  // not exists.
                { throw new KeyNotFoundException(); }

                return (int)response.Value;
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
        public bool Exists(string key)
        {
            ThrowIfObjectDisposed();
            ThrowIfNotConnected();

            try
            {
                SendCommand("EXISTS", key);

                var response = ReadResponse();
                if (response.Error != null)
                { throw new Exception(response.Error); }

                return (int)response.Value > 0;
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
        public bool Expire(string key, int ttl)
        {
            ThrowIfObjectDisposed();
            ThrowIfNotConnected();

            try
            {
                SendCommand("EXPIRE", key, ttl.ToString());

                var response = ReadResponse();
                if (response.Error != null)
                { throw new Exception(response.Error); }

                return (int)response.Value > 0;
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
        public RedisKvCacheItem Get(string key)
        {
            ThrowIfObjectDisposed();
            ThrowIfNotConnected();

            try
            {
                SendCommand("GET", key);

                var response = ReadResponse();
                if (response.Error != null)
                { throw new Exception(response.Error); }

                if (response.Value == null)
                { throw new KeyNotFoundException(); }

                var kvItem  = new RedisKvCacheItem();
                kvItem.Key = key;
                kvItem.ValueBytes = (byte[])response.Value;

                return kvItem;
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
        public RedisKvCacheItem[] Get(string[] keys)
        {
            ThrowIfObjectDisposed();
            ThrowIfNotConnected();

            try
            {
                var args = new List<string>(keys.Length + 1);
                args.Add("MGET");
                args.AddRange(keys);

                SendCommand(args.ToArray());

                var response = ReadResponse();
                if (response.Error != null)
                { throw new Exception(response.Error); }

                var values = (object[])response.Value;
                var list = new List<RedisKvCacheItem>();
                for (int i = 0; i < keys.Length; i++)
                {
                    var kvItem  = new RedisKvCacheItem();
                    kvItem.Key = keys[i];
                    kvItem.ValueBytes = (byte[])values[i];

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


        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool Add(string key, string value, int ttl)
        {
            ThrowIfObjectDisposed();
            ThrowIfNotConnected();

            try
            {
                SendCommand("SET", key, value, "EX", ttl.ToString(), "NX");

                var response = ReadResponse();
                if (response.Error != null)
                { throw new Exception(response.Error); }

                return string.Compare(response.Value as string, "OK", true) == 0;
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
        public bool Set(string key, string value, int ttl)
        {
            ThrowIfObjectDisposed();
            ThrowIfNotConnected();

            try
            {
                SendCommand("SET", key, value, "EX", ttl.ToString());

                var response = ReadResponse();
                if (response.Error != null)
                { throw new Exception(response.Error); }

                return string.Compare(response.Value as string, "OK", true) == 0;
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
        public bool Replace(string key, string value, int ttl)
        {
            ThrowIfObjectDisposed();
            ThrowIfNotConnected();

            try
            {
                SendCommand("SET", key, value, "EX", ttl.ToString(), "XX");

                var response = ReadResponse();
                if (response.Error != null)
                { throw new Exception(response.Error); }

                return string.Compare(response.Value as string, "OK", true) == 0;
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
        public bool Append(string key, string value, int ttl)
        {
            ThrowIfObjectDisposed();
            ThrowIfNotConnected();

            try
            {
                m_pClient.WriteLine($"APPEND {QuoteString(key)} {QuoteString(value)}");

                var line = m_pClient.ReadLine();
                var match = m_pReNumber.Match(line);
                if (match == null || !match.Success)
                {
                    var items = line.Split(' ', 2);
                    if (items[0].Equals("-ERR", StringComparison.InvariantCultureIgnoreCase))
                    { throw new Exception($"Error: {items[1]}"); }

                    throw new NotSupportedException($"Unsupported response: {line}");
                }


                m_pClient.WriteLine($"EXPIRE {key} {ttl}");

                line = m_pClient.ReadLine();
                match = m_pReNumber.Match(line);
                if (match == null || !match.Success)
                {
                    var items = line.Split(' ', 2);
                    if (items[0].Equals("-ERR", StringComparison.InvariantCultureIgnoreCase))
                    { throw new Exception($"Error: {items[1]}"); }

                    throw new NotSupportedException($"Unsupported response: {line}");
                }

                return true;
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
                m_pClient.WriteLine($"DEL {QuoteString(key)}");

                var line = m_pClient.ReadLine();

                var match = m_pReNumber.Match(line);
                if (match == null || !match.Success)
                {
                    string[] items = line.Split(' ', 2);
                    if (items[0].Equals("-ERR", StringComparison.InvariantCultureIgnoreCase))
                    { throw new Exception($"Error: {items[1]}"); }

                    throw new NotSupportedException($"Unsupported response: {line}");
                }

                return int.Parse(match.Groups["number"].Value) > 0;
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
            return IncrOrDecr("INCRBY", key, delta);
        }

        public long Decr(string key, int delta)
        {
            return IncrOrDecr("DECRBY", key, delta);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private long IncrOrDecr(string cmd, string key, int delta)
        {
            ThrowIfObjectDisposed();
            ThrowIfNotConnected();

            try
            {
                m_pClient.WriteLine($"{cmd} {QuoteString(key)} {delta}");

                var line = m_pClient.ReadLine();

                var match = m_pReNumber.Match(line);
                if (match != null && match.Success)
                { return long.Parse(match.Groups["number"].Value); }

                var items = line.Split(' ', 2);
                if (items[0].Equals("-ERR", StringComparison.InvariantCultureIgnoreCase))
                { throw new Exception($"Error: {items[1]}"); }

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
                m_pClient.WriteLine($"FLUSHALL");

                var line = m_pClient.ReadLine();

                if (line.Equals("+OK", StringComparison.InvariantCultureIgnoreCase))
                { return true; }

                var items = line.Split(' ', 2);
                if (items[0].Equals("-ERR", StringComparison.InvariantCultureIgnoreCase))
                { throw new Exception($"Error: {items[1]}"); }

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

        public bool IsAuthenticated
        {
            get
            {
                ThrowIfObjectDisposed();
                ThrowIfNotConnected();

                return m_IsAuthenticated;
            }
        }

        public int CurrentDatabase
        {
            get
            {
                ThrowIfObjectDisposed();
                ThrowIfNotConnected();

                return m_Database;
            }
        }

        public bool ScriptEnabled
        {
            get
            {
                ThrowIfObjectDisposed();
                ThrowIfNotConnected();

                return m_ScriptEnabled;
            }
        }

        #endregion
    }
}
