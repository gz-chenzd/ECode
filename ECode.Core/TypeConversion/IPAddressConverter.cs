using System;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using ECode.Core;
using ECode.Utility;

namespace ECode.TypeConversion
{
    /// <summary>
    /// Converts string representation into an instance of <see cref="System.Net.IPAddress"/>.
    /// </summary>
    public class IPAddressConverter : TypeConverter
    {
        /// <summary>
        /// Can we convert from the sourceType to a <see cref="System.Net.IPAddress"/> instance?
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
        /// Convert from a <see cref="System.String"/> value to a <see cref="System.Net.IPAddress"/> instance.
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
        /// A <see cref="System.Net.IPAddress"/> if successful. 
        /// </returns>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                try
                {
                    string host = (string)value;
                    if (!NetworkUtil.IsIPAddress(host))
                    { throw new ArgumentException($"Cannot parse '{host}' to a valid IPAddress."); }

                    return IPAddress.Parse(host);
                }
                catch (Exception ex)
                { throw new TypeConvertException(value, typeof(IPAddress), ex); }
            }
            else
            { throw new TypeConvertException(value, typeof(IPAddress)); }
        }
    }
}