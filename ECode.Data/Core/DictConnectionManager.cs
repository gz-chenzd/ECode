using System;
using System.Collections.Generic;
using ECode.Utility;

namespace ECode.Data
{
    public class DictConnectionManager : IConnectionManager
    {
        private Dictionary<string, string>      m_pConnectionStrings    = null;


        public DictConnectionManager(Dictionary<string, string> connectionStrings)
        {
            AssertUtil.ArgumentNotNull(connectionStrings, nameof(connectionStrings));

            if (connectionStrings.Count == 0)
            { throw new ArgumentException($"Argument '{nameof(connectionStrings)}' cannot be empty."); }

            m_pConnectionStrings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (string shardNo in connectionStrings.Keys)
            {
                m_pConnectionStrings[(shardNo ?? string.Empty).Trim()] = connectionStrings[shardNo];
            }
        }


        public string GetConnectionString(string shardNo = null, bool writable = true)
        {
            shardNo = (shardNo ?? string.Empty).Trim();

            if (m_pConnectionStrings.ContainsKey(shardNo))
            { return m_pConnectionStrings[shardNo]; }

            throw new KeyNotFoundException($"Db shard connection '{shardNo}' cannot be found.");
        }
    }
}
