using System;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using ECode.Core;

namespace ECode.TypeConversion
{
    /// <summary>
    /// Converts string representation of authentication credential 
    /// into an instance of <see cref="System.Net.NetworkCredential"/>.
    /// </summary>
    /// <example>
    /// <p>
    /// Find below some examples of the XML formatted strings that this
    /// converter will sucessfully convert.
    /// </p>
    /// <code escaped="true">
    /// <property name="credentials" value="Domain\UserName:Password"/>
    /// </code>
    /// <code escaped="true">
    /// <property name="credentials" value="UserName:Password"/>
    /// </code>
    /// </example>
    public class CredentialConverter : TypeConverter
    {
        static readonly Regex   CredentialRegex     = new Regex(
            @"(((?<domain>[\w_.]+)\\)?)(?<userName>([\w_.]+))((:(?<password>([\w_.]+)))?)",
            RegexOptions.Compiled);


        /// <summary>
        /// Can we convert from the sourceType to a <see cref="System.Net.NetworkCredential"/> instance?
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
        /// <returns><see langword="true"/> if the conversion is possible.</returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        /// <summary>
        /// Convert from a <see cref="System.String"/> value to a <see cref="System.Net.NetworkCredential"/> instance.
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
        /// A <see cref="System.Net.NetworkCredential"/> instance if successful.
        /// </returns>        
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                try
                {
                    string credentials = (string)value;

                    var m = CredentialRegex.Match(credentials);
                    if (!m.Success || m.Value != credentials)
                    { throw new ArgumentException($"Cannot parse '{credentials}' to a valid NetworkCredential."); }

                    // Get domain
                    string domain = m.Groups["domain"].Value;

                    // Get user name
                    string userName = m.Groups["userName"].Value;

                    // Get password
                    string password = m.Groups["password"].Value;

                    return new NetworkCredential(userName, password, domain);
                }
                catch (Exception ex)
                { throw new TypeConvertException(value, typeof(NetworkCredential), ex); }
            }
            else
            { throw new TypeConvertException(value, typeof(NetworkCredential)); }
        }
    }
}