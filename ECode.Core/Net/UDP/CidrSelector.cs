using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using ECode.Collections;
using ECode.Core;
using ECode.Json;
using ECode.Utility;

namespace ECode.Net.Udp
{
    public class CidrSelector : IBindSelector
    {
        class LocalMapRule
        {
            public Range_ipv4 LocalRule
            { get; set; }

            public bool IsDefaultRule
            { get; set; }

            public List<RemoteMapRule> RemoteRules
            { get; } = new List<RemoteMapRule>();
        }

        class RemoteMapRule
        {
            public Range_ipv4 RemoteRule
            { get; set; }

            public CycleCollection<string> LocalBindings
            { get; } = new CycleCollection<string>();
        }


        private List<LocalMapRule>                      m_pLocalRules           = null;
        private List<RemoteMapRule>                     m_pRemoteRules          = null;
        private Dictionary<string, LocalMapRule>        m_pDictLocalRules       = null;
        private Dictionary<string, RemoteMapRule>       m_pDictRemoteRules      = null;
        private CycleCollection<string>                 m_pDefaultBindings      = null;


        public CidrSelector(string cidrMaps)
        {
            AssertUtil.ArgumentNotEmpty(cidrMaps, nameof(cidrMaps));

            m_pLocalRules = new List<LocalMapRule>();
            m_pRemoteRules = new List<RemoteMapRule>();
            m_pDictLocalRules = new Dictionary<string, LocalMapRule>();
            m_pDictRemoteRules = new Dictionary<string, RemoteMapRule>();

            bool containsDefaultMapRule = false;
            foreach (dynamic rule in JsonUtil.Deserialize<dynamic[]>(cidrMaps))
            {
                if (!string.IsNullOrWhiteSpace((string)rule["default"]))
                {
                    containsDefaultMapRule = true;

                    var cidr = ((string)rule["default"]).Trim();
                    var localRule = GetLocalMapRule(cidr);
                    localRule.IsDefaultRule = true;
                }
                else if (!string.IsNullOrWhiteSpace((string)rule["both"]))
                {
                    var cidr = ((string)rule["both"]).Trim();
                    var localRule = GetLocalMapRule(cidr);
                    var remoteRule = GetRemoteMapRule(cidr);

                    localRule.RemoteRules.Add(remoteRule);
                }
                else if (!string.IsNullOrWhiteSpace((string)rule["local"])
                         && !string.IsNullOrWhiteSpace((string)rule["remote"]))
                {
                    var cidr = ((string)rule["local"]).Trim();
                    var localRule = GetLocalMapRule(cidr);

                    cidr = ((string)rule["remote"]).Trim();
                    var remoteRule = GetRemoteMapRule(cidr);

                    localRule.RemoteRules.Add(remoteRule);
                }
                else
                { throw new ArgumentException($"Argument '{nameof(cidrMaps)}' contains invalid rule '{rule}'."); }
            }

            if (!containsDefaultMapRule)
            { throw new ArgumentException($"Argument '{nameof(cidrMaps)}' must contain a default rule."); }
        }


        private Range_ipv4 ParseRangeIPv4(string cidr)
        {
            string[] items = cidr.Split('/', 2);

            if (!IPAddress.TryParse(items[0], out IPAddress ip) || ip.AddressFamily != AddressFamily.InterNetwork)
            { throw new ArgumentException($"Argument 'cidrMaps' contains invalid ipv4 '{items[0]}'."); }

            if (!int.TryParse(items[1], out int mask) || mask < 8 || mask > 32)
            { throw new ArgumentException($"Argument 'cidrMaps' contains invalid cidr '{cidr}'."); }

            return new Range_ipv4(ip, mask);
        }

        private LocalMapRule GetLocalMapRule(string cidr)
        {
            if (m_pDictLocalRules.ContainsKey(cidr))
            { return m_pDictLocalRules[cidr]; }

            var rule = new LocalMapRule();
            rule.LocalRule = ParseRangeIPv4(cidr);

            m_pLocalRules.Add(rule);
            m_pDictLocalRules[cidr] = rule;

            return rule;
        }

        private RemoteMapRule GetRemoteMapRule(string cidr)
        {
            if (m_pDictRemoteRules.ContainsKey(cidr))
            { return m_pDictRemoteRules[cidr]; }

            var rule = new RemoteMapRule();
            rule.RemoteRule = ParseRangeIPv4(cidr);

            m_pRemoteRules.Add(rule);
            m_pDictRemoteRules[cidr] = rule;

            return rule;
        }


        public void Load(string[] bindings)
        {
            m_pDefaultBindings = new CycleCollection<string>();
            foreach (var remoteRule in m_pRemoteRules)
            {
                remoteRule.LocalBindings.Clear();
            }

            foreach (string binding in bindings)
            {
                var items = binding.Split(":", true, true, "[]");
                string addr = items[0].TrimStart('[').TrimEnd(']');
                string port = items[1];

                var ip = IPAddress.Parse(addr);
                if (ip.AddressFamily != AddressFamily.InterNetwork)
                { continue; }

                foreach (var localRule in m_pLocalRules)
                {
                    if (!localRule.LocalRule.Contains(ip))
                    { continue; }

                    if (localRule.IsDefaultRule)
                    { m_pDefaultBindings.Add(binding); }

                    foreach (var remoteRule in localRule.RemoteRules)
                    {
                        remoteRule.LocalBindings.Add(binding);
                    }
                }
            }
        }

        public string Select(IPEndPoint remoteEP)
        {
            if (remoteEP.Address.AddressFamily != AddressFamily.InterNetwork)
            { return null; }

            foreach (var remoteRule in m_pRemoteRules)
            {
                if (remoteRule.LocalBindings.Count == 0)
                { continue; }

                if (remoteRule.RemoteRule.Contains(remoteEP.Address))
                {
                    return remoteRule.LocalBindings.Next();
                }
            }

            return m_pDefaultBindings.Next();
        }
    }
}
