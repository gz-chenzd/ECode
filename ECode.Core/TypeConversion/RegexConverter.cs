using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using ECode.Core;

namespace ECode.TypeConversion
{
    /// <summary>
    /// Converts string representation of regular expression into an instance of <see cref="Regex"/>.
    /// </summary>
    public class RegexConverter : TypeConverter
    {
        /// <summary>
        /// Can we convert from the sourcetype to a <see cref="Regex"/> instance?
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
            return (sourceType == typeof(string));
        }

        /// <summary>
        /// Convert from a <see cref="System.String"/> value to a <see cref="Regex"/> instance.
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
        /// A <see cref="Regex"/> if successful.
        /// </returns>        
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                try
                {
                    return new Regex(value as string, RegexOptions.Compiled);
                }
                catch (Exception ex)
                { throw new TypeConvertException(value, typeof(Regex), ex); }
            }
            else
            { throw new TypeConvertException(value, typeof(Regex)); }
        }
    }
}