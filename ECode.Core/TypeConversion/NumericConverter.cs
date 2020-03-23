using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using ECode.Core;

namespace ECode.TypeConversion
{
    /// <summary>
    /// A custom <see cref="System.ComponentModel.TypeConverter"/> for any
    /// primitive numeric type such as <see cref="System.Int32"/>,
    /// <see cref="System.Single"/>, <see cref="System.Double"/>, etc.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Can use a given <see cref="System.Globalization.NumberFormatInfo"/> for
    /// (locale-specific) parsing and rendering.
    /// </p>
    /// <p>
    /// This is not meant to be used as a system
    /// <see cref="System.ComponentModel.TypeConverter"/> but rather as a
    /// locale-specific number converter within custom controller code, to
    /// parse user-entered number strings into number properties of objects,
    /// and render them in a UI form.
    /// </p>
    /// </remarks>
    public class NumericConverter : TypeConverter
    {
        Type                numericType         = null;
        NumberFormatInfo    numberFormat        = null;
        bool                allowedEmpty        = false;


        /// <summary>
        /// Creates a new instance of the <see cref="NumericConverter"/> class.
        /// </summary>
        /// <param name="type">
        /// The primitive numeric <see cref="System.Type"/> to convert to.
        /// </param>
        /// <param name="format">
        /// The <see cref="System.Globalization.NumberFormatInfo"/> to use for
        /// (locale-specific) parsing and rendering
        /// </param>
        /// <param name="allowEmpty">
        /// Is an empty string allowed to be converted? 
        /// If <see langword="true"/>, an empty string value will be converted to
        /// numeric 0.</param>
        /// <exception cref="System.ArgumentException">
        /// If the supplied <paramref name="type"/> is not a primitive <see cref="System.Type"/>.
        /// </exception>
        public NumericConverter(Type type, NumberFormatInfo format, bool allowEmpty)
        {
            if (!type.GetTypeInfo().IsPrimitive)
            { throw new ArgumentException($"Argument '{nameof(type)}' must be a primitive type."); }

            this.numericType = type;
            this.numberFormat = format;
            this.allowedEmpty = allowEmpty;
        }


        /// <summary>
        /// Can we convert from the sourceType to a required primitive type value?
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
        /// <returns>
        /// <see langword="true"/> if the conversion is possible.
        /// </returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        /// <summary>
        /// Convert from a <see cref="System.String"/> value to the required primitive type.
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
        /// <returns>A primitive representation of the string value.</returns>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                try
                {
                    string strValue = value as string;
                    if (string.IsNullOrWhiteSpace(strValue) && allowedEmpty)
                    {
                        strValue = "0";
                    }

                    if (numericType.Equals(typeof(Int16)))
                    {
                        return Convert.ToInt16(strValue, numberFormat);
                    }
                    else if (numericType.Equals(typeof(UInt16)))
                    {
                        return Convert.ToUInt16(strValue, numberFormat);
                    }
                    else if (numericType.Equals(typeof(Int32)))
                    {
                        return Convert.ToInt32(strValue, numberFormat);
                    }
                    else if (numericType.Equals(typeof(UInt32)))
                    {
                        return Convert.ToUInt32(strValue, numberFormat);
                    }
                    else if (numericType.Equals(typeof(Int64)))
                    {
                        return Convert.ToInt64(strValue, numberFormat);
                    }
                    else if (numericType.Equals(typeof(UInt64)))
                    {
                        return Convert.ToUInt64(strValue, numberFormat);
                    }
                    else if (numericType.Equals(typeof(Single)))
                    {
                        return Convert.ToSingle(strValue, numberFormat);
                    }
                    else if (numericType.Equals(typeof(Double)))
                    {
                        return Convert.ToDouble(strValue, numberFormat);
                    }
                    else if (numericType.Equals(typeof(Decimal)))
                    {
                        return Convert.ToDecimal(strValue, numberFormat);
                    }

                    throw new NotSupportedException("Unsupported numeric type.");
                }
                catch (Exception ex)
                { throw new TypeConvertException(value, numericType, ex); }
            }
            else
            { throw new TypeConvertException(value, numericType); }
        }
    }
}
