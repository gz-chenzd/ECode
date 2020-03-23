using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using ECode.Core;
using ECode.TypeResolution;
using ECode.Utility;

namespace ECode.TypeConversion
{
    /// <summary>
    /// Registry class that allows users to register and retrieve type converters.
    /// </summary>
    public static class TypeConverterRegistry
    {
        static IDictionary  converters  = new HybridDictionary();


        static TypeConverterRegistry()
        {
            lock (converters.SyncRoot)
            {
                //converters[typeof(bool)] = new BooleanConverter();
                converters[typeof(ICredentials)] = new CredentialConverter();
                converters[typeof(NetworkCredential)] = new CredentialConverter();
                converters[typeof(Encoding)] = new EncodingConverter();
                converters[typeof(FileInfo)] = new FileInfoConverter();
                converters[typeof(IPAddress)] = new IPAddressConverter();
                converters[typeof(IPEndPoint)] = new IPEndPointConverter();
                converters[typeof(NameValueCollection)] = new NameValueConverter();
                //converters[typeof(Int16)] = new NumericConverter(typeof(Int16), null, false);
                //converters[typeof(UInt16)] = new NumericConverter(typeof(UInt16), null, false);
                //converters[typeof(Int32)] = new NumericConverter(typeof(Int32), null, false);
                //converters[typeof(UInt32)] = new NumericConverter(typeof(UInt32), null, false);
                //converters[typeof(Int64)] = new NumericConverter(typeof(Int64), null, false);
                //converters[typeof(UInt64)] = new NumericConverter(typeof(UInt64), null, false);
                //converters[typeof(Single)] = new NumericConverter(typeof(Single), null, false);
                //converters[typeof(Double)] = new NumericConverter(typeof(Double), null, false);
                //converters[typeof(Decimal)] = new NumericConverter(typeof(Decimal), null, false);
                converters[typeof(Regex)] = new RegexConverter();
                converters[typeof(ResourceManager)] = new ResourceManagerConverter();
                converters[typeof(Type)] = new RuntimeTypeConverter();
                converters[typeof(string[])] = new StringArrayConverter();
                converters[typeof(TimeSpan)] = new TimeSpanConverter();
                converters[typeof(Uri)] = new UriConverter();
            }
        }


        /// <summary>
        /// Returns <see cref="TypeConverter"/> for the specified type.
        /// </summary>
        /// <param name="type">Type to get the converter for.</param>
        /// <returns>a type converter for the specified type.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="type"/> is <c>null</c>.</exception>
        public static TypeConverter GetConverter(Type type)
        {
            if (type == null)
            { throw new ArgumentNullException(nameof(type)); }

            var converter = (TypeConverter)converters[type];
            if (converter == null)
            {
                if (type.GetTypeInfo().IsEnum)
                {
                    converter = new EnumConverter(type);
                }
                else
                {
                    converter = TypeDescriptor.GetConverter(type);
                }
            }

            return converter;
        }

        /// <summary>
        /// Registers <see cref="TypeConverter"/> for the specified type.
        /// </summary>
        /// <param name="type">Type to register the converter for.</param>
        /// <param name="converter">Type converter to register.</param>
        /// <exception cref="ArgumentNullException">If either of arguments is <c>null</c>.</exception>
        public static void RegisterConverter(Type type, TypeConverter converter)
        {
            if (type == null)
            { throw new ArgumentNullException(nameof(type)); }

            if (converter == null)
            { throw new ArgumentNullException(nameof(converter)); }

            lock (converters.SyncRoot)
            {
                converters[type] = converter;
            }
        }

        /// <summary>
        /// Registers <see cref="TypeConverter"/> for the specified type.
        /// </summary>
        /// <remarks>
        /// This is a convinience method that accepts the names of both
        /// type to register converter for and the converter itself,
        /// resolves them using <see cref="ECode.TypeResolution.TypeResolverRegistry"/>, creates an
        /// instance of type converter and calls overloaded
        /// <see cref="RegisterConverter(Type, TypeConverter)"/> method.
        /// </remarks>
        /// <param name="typeName">Type name of the type to register the converter for (can be a type alias).</param>
        /// <param name="converterTypeName">Type name of the type converter to register (can be a type alias).</param>
        /// <exception cref="ArgumentNullException">If either of arguments is <c>null</c> or empty string.</exception>
        /// <exception cref="TypeLoadException">
        /// If either of arguments fails to resolve to a valid <see cref="Type"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If type converter does not derive from <see cref="TypeConverter"/> or if it cannot be instantiated.
        /// </exception>
        public static void RegisterConverter(string typeName, string converterTypeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
            { throw new ArgumentNullException(nameof(typeName)); }

            if (string.IsNullOrWhiteSpace(converterTypeName))
            { throw new ArgumentNullException(nameof(converterTypeName)); }

            try
            {
                var type = TypeResolutionUtil.ResolveType(typeName);
                var converterType = TypeResolutionUtil.ResolveType(converterTypeName);
                if (!typeof(TypeConverter).GetTypeInfo().IsAssignableFrom(converterType))
                { throw new ArgumentException($"Type specified as a '{converterTypeName}' does not inherit from System.ComponentModel.TypeConverter"); }

                RegisterConverter(type, (TypeConverter)ObjectUtil.InstantiateType(converterType));
            }
            catch (ReflectionException ex)
            { throw new ArgumentException("Failed to create an instance of the specified type converter.", ex); }
        }
    }
}