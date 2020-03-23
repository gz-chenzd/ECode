using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Xml;
using ECode.Core;

namespace ECode.TypeConversion
{
    /// <summary>
    /// Converts string representation into an instance of <see cref="System.Collections.Specialized.NameValueCollection"/>.
    /// </summary>
    /// <example>
    /// <p>
    /// Find below some examples of the XML formatted strings that this
    /// converter will sucessfully convert. Note that the name of the top level
    /// (document) element is quite arbitrary... it is only the content that
    /// matters (and which must be in the format
    /// <c>&lt;add key="..." value="..."/&gt;</c>. For your continued sanity
    /// though, you may wish to standardize on the top level name of
    /// <c>'dictionary'</c> (although you are of course free to not do so).
    /// </p>
    /// <code escaped="true">
    /// <dictionary>
    ///		<add key="host" value="localhost"/>
    ///		<add key="port" value="8080"/>
    /// </dictionary>
    /// </code>
    /// <p>
    /// The following example uses a different top level (document) element
    /// name, but is equivalent to the first example.
    /// </p>
    /// <code escaped="true">
    /// <web-configuration-parameters>
    ///		<add key="host" value="localhost"/>
    ///		<add key="port" value="8080"/>
    /// </web-configuration-parameters>
    /// </code>
    /// </example>
    public class NameValueConverter : TypeConverter
    {
        /// <summary>
        /// Can we convert from the sourceType to a <see cref="System.Collections.Specialized.NameValueCollection"/> instance?
        /// </summary>
        /// <remarks>
        /// <p>
        /// Currently only supports conversion from an <b>XML formatted</b> <see cref="System.String"/> value.
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
        /// Convert from a <see cref="System.String"/> value to a <see cref="System.Collections.Specialized.NameValueCollection"/> instance.
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
        /// A <see cref="System.Collections.Specialized.NameValueCollection"/> if successful. 
        /// </returns>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string text = value as string;
            if (!string.IsNullOrWhiteSpace(text))
            {
                try
                {
                    var doc = new XmlDocument();
                    doc.XmlResolver = null;
                    doc.LoadXml(text);

                    var dict = new NameValueCollection();
                    foreach (XmlElement element in doc.DocumentElement.ChildNodes)
                    {
                        if (element.LocalName == "add")
                        {
                            string k = element.GetAttribute("key");
                            string v = element.GetAttribute("value");

                            if (k != null)
                            { dict[k] = v; }
                        }
                    }

                    return dict;
                }
                catch (Exception ex)
                { throw new TypeConvertException(value, typeof(NameValueCollection), ex); }
            }
            else
            { throw new TypeConvertException(value, typeof(NameValueCollection)); }
        }
    }
}