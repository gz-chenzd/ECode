using System;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using ECode.Core;

namespace ECode.TypeConversion
{
    /// <summary>
    /// Converts string representation into an instance of <see cref="System.Net.IPEndPoint"/>.
    /// </summary>
    public class IPEndPointConverter : TypeConverter
    {
        static readonly Regex   EndPointRegex   = new Regex(
            @"^(?<host>([\w\-]+(\.[\w\-]+)*)):(?<port>(\d+))$",
            RegexOptions.Compiled);


        /// <summary>
        /// Can we convert from the sourceType to a <see cref="System.Net.IPEndPoint"/> instance?
        /// </summary>
        /// <remarks>
        /// <p>
        /// Currently only supports conversion from a <see cref="System.String"/> value.
        /// </p>
        /// </remarks>
        /// <param name="context">
        /// A <see cref="System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.
        /// </param>
        /// <param name="sourceType">
        /// A <see cref="System.Type"/> that represents what you want to convert from.
        /// </param>
        /// <returns>True if the conversion is possible.</returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        /// <summary>
        /// Convert from a <see cref="System.String"/> value to a <see cref="System.Net.IPEndPoint"/> instance.
        /// </summary>
        /// <param name="context">
        /// A <see cref="System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.
        /// </param>
        /// <param name="culture">
        /// The <see cref="System.Globalization.CultureInfo"/> to use as the current culture. 
        /// </param>
        /// <param name="value">
        /// The value that is to be converted.
        /// </param>
        /// <returns>
        /// A <see cref="System.Net.IPEndPoint"/> if successful. 
        /// </returns>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                try
                {
                    string endpoint = (string)value;

                    Match m = EndPointRegex.Match(endpoint);
                    if (!m.Success)
                    { throw new ArgumentException($"Cannot parse '{endpoint}' to a valid IPEndPoint."); }


                    // Get host
                    string host = m.Groups["host"].Value;

                    // Try to resolve via DNS. This is a blocking call. 
                    // GetHostEntry works with either an IPAddress string or a host name
                    var resolvedHost = Dns.GetHostEntry(host);
                    if (resolvedHost == null ||
                        resolvedHost.AddressList == null ||
                        resolvedHost.AddressList.Length < 1)
                    { throw new ArgumentException($"Host '{host}' cannot be resolved."); }

                    var selectedIpAddr = resolvedHost.AddressList[0];
                    foreach (var ip in resolvedHost.AddressList)
                    {
                        if (ip.ToString() == host)
                        {
                            selectedIpAddr = ip;
                            break;
                        }
                    }


                    // Get port
                    int port = int.Parse(m.Groups["port"].Value);
                    if (port < 1 || port > 65535)
                    { throw new ArgumentOutOfRangeException($"Port '{port}' isnot in valid range."); }

                    return new IPEndPoint(selectedIpAddr, port);
                }
                catch (Exception ex)
                { throw new TypeConvertException(value, typeof(IPEndPoint), ex); }
            }
            else
            { throw new TypeConvertException(value, typeof(IPEndPoint)); }
        }
    }
}