using System;
using System.ComponentModel;
using System.Globalization;
using ECode.Core;

namespace ECode.TypeConversion
{
    /// <summary>
    /// Converts string representation into an instance of <see cref="System.Uri"/>.
    /// </summary>
    public class UriConverter : TypeConverter
    {
        /// <summary>
        /// Can we convert from the sourceType to a <see cref="System.Uri"/> instance?
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
        /// Convert from a string value to a <see cref="System.Uri"/> instance.
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
        /// A <see cref="System.Uri"/> if successful. 
        /// </returns>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                try
                {
                    return new Uri(value as string);
                }
                catch (Exception ex)
                { throw new TypeConvertException(value, typeof(Uri), ex); }
            }
            else
            { throw new TypeConvertException(value, typeof(Uri)); }
        }
    }
}
