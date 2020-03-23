using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ECode.Configuration;
using ECode.Core;
using ECode.Json;
using ECode.Net;
using ECode.Utility;

namespace ECode.Caching
{
    public class MemcachedClientManager : IMemcachedClientManager, IDisposable
    {
        class JsonFormatConfigItem
        {
            public string ShardNo
            { get; set; }

            public string ConnectionString
            { get; set; }
        }

        class ConnectionPoolConfig
        {
            public string ShardNo
            { get; set; } = string.Empty;

            public string Host
            { get; set; }

            public int Port
            { get; set; } = 11211;

            public bool Ssl
            { get; set; } = false;

            public int MinPoolSize
            { get; set; } = -1;

            public int MaxPoolSize
            { get; set; } = -1;

            public int ConnectTimeout
            { get; set; } = -1;

            public int ReadWriteTimeout
            { get; set; } = -1;

            public int ConnectionIdleTimeout
            { get; set; } = -1;

            public int ConnectionBusyTimeout
            { get; set; } = -1;


            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (obj == null || !(obj is ConnectionPoolConfig))
                { return false; }

                var other = (ConnectionPoolConfig)obj;
                if (this.ShardNo != other.ShardNo)
                { return false; }

                if (!string.Equals(this.Host, other.Host, StringComparison.InvariantCultureIgnoreCase))
                { return false; }

                if (this.Port != other.Port)
                { return false; }

                if (this.Ssl != other.Ssl)
                { return false; }

                return true;
            }


            public static ConnectionPoolConfig Parse(string config)
            {
                AssertUtil.ArgumentNotEmpty(config, nameof(config));

                var result = new ConnectionPoolConfig();
                foreach (string keyValuePair in config.Split(';', StringSplitOptions.RemoveEmptyEntries))
                {
                    var items = keyValuePair.Split('=', 2);
                    if (items.Length != 2)
                    { throw new ConfigurationException($"Config error: cannot parse '{keyValuePair}'."); }

                    var key = items[0].Trim().ToLower();
                    var value = items[1].Trim();
                    int resolvedValue = -1;

                    if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
                    { throw new ConfigurationException($"Config error: key and value cannot be empty string."); }

                    switch (key)
                    {
                        case "server":
                            var hostPort = value.Split(',', 2);
                            result.Host = hostPort[0].Trim();
                            if (result.Host == ".")
                            { result.Host = "localhost"; }

                            if (hostPort.Length == 2)
                            {
                                if (!int.TryParse(hostPort[1].Trim(), out resolvedValue))
                                {
                                    throw new ConfigurationException($"Config error: cannot parse 'Port' to integer value.");
                                }

                                if (resolvedValue < 0 || resolvedValue > 65535)
                                {
                                    throw new ConfigurationException($"Config error: 'Port' must be > 0 and < 65535.");
                                }

                                result.Port = resolvedValue;
                            }
                            break;

                        case "ssl":
                            if (string.Compare("True", value, true) == 0)
                            { result.Ssl = true; }
                            else if (string.Compare("False", value, true) == 0)
                            { result.Ssl = false; }
                            else
                            {
                                throw new ConfigurationException($"Config error: cannot parse 'Ssl' to bool value.");
                            }
                            break;

                        case "minpoolsize":
                            if (!int.TryParse(value, out resolvedValue))
                            {
                                throw new ConfigurationException($"Config error: cannot parse 'MinPoolSize' to integer value.");
                            }

                            if (resolvedValue < 0)
                            {
                                throw new ConfigurationException($"Config error: 'MinPoolSize' must be >= 0.");
                            }

                            result.MinPoolSize = resolvedValue;
                            break;

                        case "maxpoolsize":
                            if (!int.TryParse(value, out resolvedValue))
                            {
                                throw new ConfigurationException($"Config error: cannot parse 'MaxPoolSize' to integer value.");
                            }

                            if (resolvedValue <= 0)
                            {
                                throw new ConfigurationException($"Config error: 'MaxPoolSize' must be > 0.");
                            }

                            result.MaxPoolSize = resolvedValue;
                            break;

                        case "connecttimeout":
                            if (!int.TryParse(value, out resolvedValue))
                            {
                                throw new ConfigurationException($"Config error: cannot parse 'ConnectTimeout' to integer value.");
                            }

                            if (resolvedValue <= 0)
                            {
                                throw new ConfigurationException($"Config error: 'ConnectTimeout' must be > 0.");
                            }

                            result.ConnectTimeout = resolvedValue;
                            break;

                        case "readwritetimeout":
                            if (!int.TryParse(value, out resolvedValue))
                            {
                                throw new ConfigurationException($"Config error: cannot parse 'ReadWriteTimeout' to integer value.");
                            }

                            if (resolvedValue <= 0)
                            {
                                throw new ConfigurationException($"Config error: 'ReadWriteTimeout' must be > 0.");
                            }

                            result.ReadWriteTimeout = resolvedValue;
                            break;

                        case "connectionidletimeout":
                            if (!int.TryParse(value, out resolvedValue))
                            {
                                throw new ConfigurationException($"Config error: cannot parse 'ConnectionIdleTimeout' to integer value.");
                            }

                            if (resolvedValue <= 0)
                            {
                                throw new ConfigurationException($"Config error: 'ConnectionIdleTimeout' must be > 0.");
                            }

                            result.ConnectionIdleTimeout = resolvedValue;
                            break;

                        case "connectionbusytimeout":
                            if (!int.TryParse(value, out resolvedValue))
                            {
                                throw new ConfigurationException($"Config error: cannot parse 'ConnectionBusyTimeout' to integer value.");
                            }

                            if (resolvedValue <= 0)
                            {
                                throw new ConfigurationException($"Config error: 'ConnectionBusyTimeout' must be > 0.");
                            }

                            result.ConnectionBusyTimeout = resolvedValue;
                            break;

                        default:
                            throw new ConfigurationException($"Config error: cannot support key '{items[0].Trim()}'.");
                    }
                }

                if (string.IsNullOrWhiteSpace(result.Host))
                { throw new ConfigurationException("Config error: 'Server' is required."); }

                return result;
            }
        }


        private bool                                                    m_IsDisposed            = false;
        private IShardStrategy                                          m_pShardStrategy        = null;
        private string                                                  m_ConfigKey             = null;
        private Dictionary<string, ConnectionPoolConfig>                m_pPoolConfigs          = null;
        private Dictionary<string, ConnectionPool<MemcachedClient>>     m_pConnectionPools      = null;


        public MemcachedClientManager(string configKey)
        {
            AssertUtil.ArgumentNotEmpty(configKey, nameof(configKey));

            m_ConfigKey = configKey.Trim();
            ConfigurationManager.Changed += (e) =>
            {
                if (string.Compare(m_ConfigKey, e.Key, true) == 0)
                {
                    InitializeConnectionPools();
                }
            };


            InitializeConnectionPools();
        }

        public MemcachedClientManager(string configKey, IShardStrategy shardStrategy)
        {
            AssertUtil.ArgumentNotEmpty(configKey, nameof(configKey));
            AssertUtil.ArgumentNotNull(shardStrategy, nameof(shardStrategy));

            m_ConfigKey = configKey.Trim();
            m_pShardStrategy = shardStrategy;

            ConfigurationManager.Changed += (e) =>
            {
                if (string.Compare(m_ConfigKey, e.Key, true) == 0)
                {
                    InitializeConnectionPools();
                }
            };


            InitializeConnectionPools();
        }


        private void InitializeConnectionPools()
        {
            var config = ConfigurationManager.Get(m_ConfigKey);
            if (string.IsNullOrWhiteSpace(config))
            { throw new ConfigurationException($"Config error: cannot load config '{m_ConfigKey}'."); }

            var poolsConfig = new Dictionary<string, ConnectionPoolConfig>();
            foreach (var item in JsonUtil.Deserialize<JsonFormatConfigItem[]>(config))
            {
                var poolConfig = ConnectionPoolConfig.Parse(item.ConnectionString);
                poolConfig.ShardNo = (item.ShardNo ?? string.Empty).Trim();

                poolsConfig[poolConfig.ShardNo] = poolConfig;
            }

            if (poolsConfig.Count <= 0)
            { throw new ConfigurationException("Config error: at least one connection string is required."); }

            ReloadConnectionPools(poolsConfig);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void ReloadConnectionPools(Dictionary<string, ConnectionPoolConfig> poolsConfig)
        {
            if (m_pPoolConfigs == null)
            {
                m_pPoolConfigs = new Dictionary<string, ConnectionPoolConfig>();
                m_pConnectionPools = new Dictionary<string, ConnectionPool<MemcachedClient>>();
            }

            var newConnectionPools = new Dictionary<string, ConnectionPool<MemcachedClient>>();

            foreach (var poolConfig in poolsConfig.Values)
            {
                if (m_pPoolConfigs.ContainsKey(poolConfig.ShardNo))
                {
                    var prePoolConfig = m_pPoolConfigs[poolConfig.ShardNo];
                    var preConnectionPool = m_pConnectionPools[poolConfig.ShardNo];
                    if (poolConfig == prePoolConfig)
                    {
                        newConnectionPools[poolConfig.ShardNo] = preConnectionPool;

                        if (poolConfig.ConnectTimeout > 0)
                        { preConnectionPool.ConnectTimeout = poolConfig.ConnectTimeout; }

                        if (poolConfig.ReadWriteTimeout > 0)
                        { preConnectionPool.ReadWriteTimeout = poolConfig.ReadWriteTimeout; }

                        if (poolConfig.MinPoolSize >= 0)
                        { preConnectionPool.MinPoolSize = poolConfig.MinPoolSize; }

                        if (poolConfig.MaxPoolSize > 0)
                        { preConnectionPool.MaxPoolSize = poolConfig.MaxPoolSize; }

                        if (poolConfig.ConnectionIdleTimeout > 0)
                        { preConnectionPool.ConnectionIdleTimeout = poolConfig.ConnectionIdleTimeout; }

                        if (poolConfig.ConnectionBusyTimeout > 0)
                        { preConnectionPool.ConnectionBusyTimeout = poolConfig.ConnectionBusyTimeout; }

                        continue;
                    }
                    else
                    {
                        try { preConnectionPool.Dispose(); }
                        catch (Exception ex)
                        { string dummy = ex.Message; }

                        m_pPoolConfigs.Remove(poolConfig.ShardNo);
                        m_pConnectionPools.Remove(poolConfig.ShardNo);
                    }
                }

                var newConnectionPool = new ConnectionPool<MemcachedClient>(poolConfig.Host, poolConfig.Port, poolConfig.Ssl);
                newConnectionPools[poolConfig.ShardNo] = newConnectionPool;

                if (poolConfig.ConnectTimeout > 0)
                { newConnectionPool.ConnectTimeout = poolConfig.ConnectTimeout; }

                if (poolConfig.ReadWriteTimeout > 0)
                { newConnectionPool.ReadWriteTimeout = poolConfig.ReadWriteTimeout; }

                if (poolConfig.MinPoolSize >= 0)
                { newConnectionPool.MinPoolSize = poolConfig.MinPoolSize; }

                if (poolConfig.MaxPoolSize > 0)
                { newConnectionPool.MaxPoolSize = poolConfig.MaxPoolSize; }

                if (poolConfig.ConnectionIdleTimeout > 0)
                { newConnectionPool.ConnectionIdleTimeout = poolConfig.ConnectionIdleTimeout; }

                if (poolConfig.ConnectionBusyTimeout > 0)
                { newConnectionPool.ConnectionBusyTimeout = poolConfig.ConnectionBusyTimeout; }
            }

            foreach (var preConnectionPool in m_pConnectionPools.Values)
            {
                try { preConnectionPool.Dispose(); }
                catch (Exception ex)
                { string dummy = ex.Message; }
            }

            m_pPoolConfigs = poolsConfig;
            m_pConnectionPools = newConnectionPools;
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Dispose()
        {
            if (m_IsDisposed)
            { return; }

            m_IsDisposed = true;

            foreach (var pool in m_pConnectionPools.Values)
            { pool.Dispose(); }

            m_pConnectionPools.Clear();
            m_pConnectionPools = null;
        }


        public MemcachedClient GetClient(object shardObject, bool writable)
        {
            return GetConnectionPool(shardObject, writable).GetConnector();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private ConnectionPool<MemcachedClient> GetConnectionPool(object shardObject, bool writable)
        {
            string shardNo = string.Empty;
            if (m_pShardStrategy != null)
            {
                shardNo = m_pShardStrategy.GetShardNo(shardObject);
            }

            if (!m_pConnectionPools.ContainsKey(shardNo))
            {
                throw new ConfigurationException($"Config error: shard no '{shardNo}' not exists.");
            }

            return m_pConnectionPools[shardNo];
        }
    }
}
