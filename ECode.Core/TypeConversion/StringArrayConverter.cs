using System;
using System.ComponentModel;
using System.Globalization;
using ECode.Core;

namespace ECode.TypeConversion
{
    /// <summary>
    /// Converts a separated <see cref="System.String"/> to a <see cref="System.String"/> array.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Defaults to using the <c>,</c> (comma) as the list separator. Note that the value
    /// of the current <see cref="System.Globalization.CultureInfo.CurrentCulture"/> is <b>not</b> used.
    /// </p>
    /// <p>
    /// If you want to provide your own list separator, you can set the value of the
    /// <see cref="ECode.TypeConversion.StringArrayConverter.Separator"/>
    /// property to the value that you want. Please note that this value will be used
    /// for <i>all</i> future conversions in preference to the default list separator.
    /// </p>
    /// <p>
    /// Please note that the individual elements of a string will be passed
    /// through <i>as is</i> (i.e. no conversion or trimming of surrounding
    /// whitespace will be performed).
    /// </p>
    /// </remarks>
    /// <example>
    /// <code language="C#">
    /// public class StringArrayConverterExample 
    /// {     
    ///     public static void Main()
    ///     {
    ///         StringArrayConverter converter = new StringArrayConverter();
    ///			
    ///			string csvWords = "This,Is,It";
    ///			string[] frankBoothWords = converter.ConvertFrom(csvWords);
    ///
    ///			// the 'frankBoothWords' array will have 3 elements, namely
    ///			// "This", "Is", "It".
    ///			
    ///			// please note that extraneous whitespace is NOT trimmed off
    ///			// in the current implementation...
    ///			string csv = "  Cogito ,ergo ,sum ";
    ///			string[] descartesWords = converter.ConvertFrom(csv);
    ///			
    ///			// the 'descartesWords' array will have 3 elements, namely
    ///			// "  Cogito ", "ergo ", "sum ".
    ///			// notice how the whitespace has NOT been trimmed.
    ///     }
    /// }
    /// </code>
    /// </example>
    public class StringArrayConverter : TypeConverter
    {
        const string            DEFAULT_SEPARATOR       = ",";

        private string          separator               = DEFAULT_SEPARATOR;


        /// <summary>
        /// The value that will be used as the list separator when performing conversions.
        /// </summary>
        /// <value>
        /// A 'single' string character that will be used as the list separator
        /// when performing conversions.
        /// </value>
        /// <exception cref="System.ArgumentException">
        /// If the supplied value is not <cref lang="null"/> and is an empty
        /// string, or has more than one character.
        /// </exception>
        public string Separator
        {
            get { return this.separator; }
            set
            {
                if (value != null)
                {
                    if (value.Length != 1)
                    { throw new ArgumentException($"The '{nameof(Separator)}' must be exactly one character in length."); }

                    separator = value;
                }
                else
                { separator = DEFAULT_SEPARATOR; }
            }
        }


        /// <summary>
        /// Can we convert from a the sourceType to a <see cref="System.String"/> array?
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
        /// Convert from a <see cref="System.String"/> value to a <see cref="System.String"/> array.
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
        /// A <see cref="System.String"/> array if successful.
        /// </returns>        
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                try
                {
                    string text = value as string;
                    if (text == null)
                    {
                        return new string[0];
                    }

                    return text.Split(new string[] { this.Separator }, StringSplitOptions.None);
                }
                catch (Exception ex)
                { throw new TypeConvertException(value, typeof(string[]), ex); }
            }
            else
            { throw new TypeConvertException(value, typeof(string[])); }
        }
    }
}