using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using ECode.Core;

namespace ECode.Utility
{
    public static class NetworkUtil
    {
        /// <summary>
        /// Gets local host name.
        /// </summary>
        public static string GetHostName()
        {
            return Dns.GetHostName();
        }

        /// <summary>
        /// Gets local host ips.
        /// </summary>
        public static IPAddress[] GetIPAddresses()
        {
            var list = new List<IPAddress>();
            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus != OperationalStatus.Up)
                { continue; }

                foreach (var addr in nic.GetIPProperties().UnicastAddresses)
                {
                    list.Add(addr.Address);
                }
            }

            return list.ToArray();
        }

        /// <summary>
        /// Gets mailbox's domain.
        /// </summary>
        public static string GetMxDomain(string mailbox)
        {
            AssertUtil.ArgumentNotEmpty(mailbox, nameof(mailbox));

            if (!ValidateUtil.IsMailAddress(mailbox))
            { throw new ArgumentException($"Argument '{mailbox}' is not valid mail address."); }

            return mailbox.Split('@', 2)[1];
        }

        /// <summary>
        /// Gets if the specified string value is IP address.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <returns>Returns true if specified value is IP address.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>value</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public static bool IsIPAddress(string value)
        {
            AssertUtil.ArgumentNotEmpty(value, nameof(value));

            return IPAddress.TryParse(value, out IPAddress ip);
        }

        /// <summary>
        /// Gets if specified IP address is private LAN IP address. For example 192.168.x.x is private ip.
        /// </summary>
        /// <param name="ip">IP address to check.</param>
        /// <returns>Returns true if IP is private IP.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>ip</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public static bool IsPrivateIPAddress(string ip)
        {
            AssertUtil.ArgumentNotEmpty(ip, nameof(ip));

            if (!IsIPAddress(ip))
            { throw new ArgumentException($"Argument '{ip}' is not valid ip address."); }

            return IsPrivateIPAddress(IPAddress.Parse(ip));
        }

        /// <summary>
        /// Gets if specified IP address is private LAN IP address. For example 192.168.x.x is private ip.
        /// </summary>
        /// <param name="ip">IP address to check.</param>
        /// <returns>Returns true if IP is private IP.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>ip</b> is null reference.</exception>
        public static bool IsPrivateIPAddress(IPAddress ip)
        {
            AssertUtil.ArgumentNotNull(ip, nameof(ip));

            if (ip.AddressFamily != AddressFamily.InterNetwork)
            { return false; }

            var ipBytes = ip.GetAddressBytes();

            /* 
              Private IPs:
                First Octet = 192 AND Second Octet = 168 (Example: 192.168.X.X) 
                First Octet = 172 AND (Second Octet >= 16 AND Second Octet <= 31) (Example: 172.16.X.X - 172.31.X.X)
                First Octet = 10 (Example: 10.X.X.X)
                First Octet = 169 AND Second Octet = 254 (Example: 169.254.X.X)
            */

            if (ipBytes[0] == 192 && ipBytes[1] == 168)
            { return true; }

            if (ipBytes[0] == 172 && ipBytes[1] >= 16 && ipBytes[1] <= 31)
            { return true; }

            if (ipBytes[0] == 10)
            { return true; }

            if (ipBytes[0] == 169 && ipBytes[1] == 254)
            { return true; }

            return false;
        }

        /// <summary>
        /// Gets if the specified IP address is multicast address.
        /// </summary>
        /// <param name="ip">IP address.</param>
        /// <returns>Returns true if <b>ip</b> is muticast address, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>ip</b> s null reference.</exception>
        public static bool IsMulticastIPAddress(IPAddress ip)
        {
            AssertUtil.ArgumentNotNull(ip, nameof(ip));

            // IPv4 multicast 224.0.0.0 to 239.255.255.255

            if (ip.IsIPv6Multicast)
            { return true; }

            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                var ipBytes = ip.GetAddressBytes();
                if (ipBytes[0] >= 224 && ipBytes[0] <= 239)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Compares 2 IP addresses.
        /// </summary>
        /// <param name="ipA">The first IP address to compare.</param>
        /// <param name="ipB">The second IP address to compare.</param>
        /// <returns>
        /// Returns 0 if IPs are equal, 
        /// returns positive value if the second IP is bigger than the first IP,
        /// returns negative value if the second IP is smaller than the first IP.
        /// </returns>
        public static int Compare(IPAddress ipA, IPAddress ipB)
        {
            AssertUtil.ArgumentNotNull(ipA, nameof(ipA));
            AssertUtil.ArgumentNotNull(ipB, nameof(ipB));

            byte[] ipABytes     = ipA.GetAddressBytes();
            byte[] ipBBytes     = ipB.GetAddressBytes();

            // IPv4 and IPv6
            if (ipABytes.Length < ipBBytes.Length)
            { return 1; }

            // IPv6 and IPv4
            if (ipABytes.Length > ipBBytes.Length)
            { return -1; }

            // IPv4 and IPv4 OR IPv6 and IPv6
            for (int i = 0; i < ipABytes.Length; i++)
            {
                if (ipABytes[i] < ipBBytes[i])
                {
                    return 1;
                }
                else if (ipABytes[i] > ipBBytes[i])
                {
                    return -1;
                }
            }

            return 0;
        }

        /// <summary>
        /// Gets if the specified IP address is in the range.
        /// </summary>
        /// <param name="ip">The IP address to check.</param>
        /// <param name="range">The IP address range.</param>
        /// <returns>Returns true if the IP address is in the range.</returns>
        public static bool IsIPAddressInRange(IPAddress ip, string range)
        {
            AssertUtil.ArgumentNotNull(ip, nameof(ip));
            AssertUtil.ArgumentNotEmpty(range, nameof(range));

            string ipString = ip.ToString();
            if (ipString == range)
            { return true; }

            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                if (range.IndexOf('-') > 0)
                {
                    string[] items = range.Split(new char[] { '-' }, 2);

                    if (!IPAddress.TryParse(items[0], out IPAddress ipStart))
                    {
                        throw new ArgumentException($"Agument '{range}' value is not valid ip range.");
                    }

                    if (!int.TryParse(items[1], out int endValue) || endValue > 255)
                    {
                        throw new ArgumentException($"Agument '{range}' value is not valid ip range.");
                    }

                    byte[] ipBytes = ip.GetAddressBytes();
                    byte[] startBytes = ipStart.GetAddressBytes();
                    for (int i = 0; i < 4; i++)
                    {
                        if (i == 3)
                        {
                            return ipBytes[i] >= startBytes[i] && ipBytes[i] <= endValue;
                        }
                        else if (ipBytes[i] != startBytes[i])
                        {
                            return false;
                        }
                    }
                }
                else if (range.IndexOf('/') > 0)
                {
                    string[] items = range.Split(new char[] { '/' }, 2);

                    if (!IPAddress.TryParse(items[0], out IPAddress ipStart))
                    {
                        throw new ArgumentException($"Agument '{range}' value is not valid ip range.");
                    }

                    if (!int.TryParse(items[1], out int maskValue) || maskValue > 32)
                    {
                        throw new ArgumentException($"Agument '{range}' value is not valid ip range.");
                    }

                    byte[] ipBytes = ip.GetAddressBytes();
                    byte[] startBytes = ipStart.GetAddressBytes();
                    for (int i = 0; i < 4; i++)
                    {
                        int endValue = startBytes[i];
                        if (((i + 1) * 8 - maskValue) > 0)
                        {
                            endValue += (int)Math.Pow(2, (i + 1) * 8 - maskValue) - 1;
                        }

                        if (ipBytes[i] < startBytes[i] || ipBytes[i] > endValue)
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }
            else if (ip.AddressFamily == AddressFamily.InterNetworkV6)
            {
                if (range.IndexOf('-') > 0)
                {
                    string[] items = range.Split(new char[] { '-' }, 2);

                    if (!IPAddress.TryParse(items[0], out IPAddress ipStart))
                    {
                        throw new ArgumentException($"Agument '{range}' value is not valid ip range.");
                    }

                    if (items[1].Length > 4)
                    {
                        throw new ArgumentException($"Agument '{range}' value is not valid ip range.");
                    }

                    byte[] last2Bytes = items[1].PadLeft(4, '0').FromHex();

                    byte[] ipBytes = ip.GetAddressBytes();
                    byte[] startBytes = ipStart.GetAddressBytes();
                    for (int i = 0; i < 16; i++)
                    {
                        if (i >= 14)
                        {
                            if (ipBytes[i] < startBytes[i] || ipBytes[i] > last2Bytes[i - 14])
                            {
                                return false;
                            }
                        }
                        else if (ipBytes[i] != startBytes[i])
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Parses IPEndPoint from the specified string value.
        /// </summary>
        /// <param name="value">IPEndPoint string value.</param>
        /// <returns>Returns parsed IPEndPoint.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>value</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public static IPEndPoint ParseIPEndPoint(string value)
        {
            AssertUtil.ArgumentNotEmpty(value, nameof(value));

            try
            {
                string[] ip_port = value.Split(":", true, false, "[]");

                if (ip_port[0].StartsWith('['))
                { ip_port[0] = ip_port[0].TrimStart('[').TrimEnd(']'); }

                return new IPEndPoint(IPAddress.Parse(ip_port[0]), Convert.ToInt32(ip_port[1]));
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Argument '{value}' is not valid IPEndPoint value.", ex);
            }
        }

        /// <summary>
        /// Gets if socket async methods supported by OS.
        /// </summary>
        /// <returns>returns ture if supported, otherwise false.</returns>
        public static bool IsSocketAsyncSupported()
        {
            try
            {
                using (var e = new SocketAsyncEventArgs())
                { return true; }
            }
            catch (NotSupportedException ex)
            {
                string dummy = ex.Message;

                return false;
            }
        }

        /// <summary>
        /// Creates a new socket for the specified end point.
        /// </summary>
        /// <param name="localEP">Local end point.</param>
        /// <param name="protocol">Protocol type.</param>
        /// <returns>Retruns newly created socket.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>localEP</b> is null.</exception>
        public static Socket CreateSocket(IPEndPoint localEP, ProtocolType protocol)
        {
            AssertUtil.ArgumentNotNull(localEP, nameof(localEP));

            var socketType = SocketType.Stream;
            if (protocol == ProtocolType.Udp)
            { socketType = SocketType.Dgram; }

            if (localEP.AddressFamily == AddressFamily.InterNetwork)
            {
                var socket = new Socket(AddressFamily.InterNetwork, socketType, protocol);
                socket.Bind(localEP);

                return socket;
            }
            else if (localEP.AddressFamily == AddressFamily.InterNetworkV6)
            {
                var socket = new Socket(AddressFamily.InterNetworkV6, socketType, protocol);
                socket.Bind(localEP);

                return socket;
            }
            else
            { throw new ArgumentException($"Argument '{localEP.ToString()}' is not valid IPEndPoint value."); }
        }
    }
}
