using System;
using System.Collections.Generic;
using ECode.Utility;

namespace ECode.Data
{
    public class SimpleConnectionManager : IConnectionManager
    {
        private Dictionary<string, string>      connectionStrings   = null;


        public SimpleConnectionManager(Dictionary<string, string> connectionStrings)
        {
            AssertUtil.ArgumentNotNull(connectionStrings, nameof(connectionStrings));
            if (connectionStrings.Count == 0)
            {
                throw new ArgumentException($"Argument '{nameof(connectionStrings)}' cannot be empty.");
            }

            this.connectionStrings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            foreach (string shardNo in connectionStrings.Keys)
            {
                this.connectionStrings[shardNo?.Trim()] = connectionStrings[shardNo];
            }
        }


        public string GetConnectionString(string shardNo = null, bool writable = true)
        {
            shardNo = (shardNo ?? string.Empty).Trim();

            if (connectionStrings.ContainsKey(shardNo))
            {
                return connectionStrings[shardNo];
            }

            throw new KeyNotFoundException($"Shard connection '{shardNo}' cannot be found.");
        }
    }
}
