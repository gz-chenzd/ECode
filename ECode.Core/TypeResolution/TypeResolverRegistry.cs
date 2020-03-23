using System;
using System.Collections;
using System.Reflection;
using ECode.Utility;

namespace ECode.TypeResolution
{
    /// <summary> 
    /// Provides access to a central registry of aliased <see cref="System.Type"/>s.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Simplifies configuration by allowing aliases to be used instead of
    /// fully qualified type names.
    /// </p>
    /// <p>
    /// Comes 'pre-loaded' with a number of convenience alias' for the more
    /// common types; an example would be the '<c>int</c>' alias for the <see cref="System.Int32"/>
    /// type.
    /// </p>
    /// </remarks>
    public static class TypeResolverRegistry
    {
        /// <summary>
        /// The alias around the 'bool' type.
        /// </summary>
        public const string BoolAlias = "bool";

        /// <summary>
        /// The alias around the 'bool[]' array type.
        /// </summary>
        public const string BoolArrayAlias = "bool[]";

        /// <summary>
        /// The alias around the 'char' type.
        /// </summary>
        public const string CharAlias = "char";

        /// <summary>
        /// The alias around the 'char[]' array type.
        /// </summary>
        public const string CharArrayAlias = "char[]";

        /// <summary>
        /// The alias around the 'short' type.
        /// </summary>
        public const string Int16Alias = "short";

        /// <summary>
        /// The alias around the 'short[]' array type.
        /// </summary>
        public const string Int16ArrayAlias = "short[]";

        /// <summary>
        /// The alias around the 'int' type.
        /// </summary>
        public const string Int32Alias = "int";

        /// <summary>
        /// The alias around the 'int[]' array type.
        /// </summary>
        public const string Int32ArrayAlias = "int[]";

        /// <summary>
        /// The alias around the 'long' type.
        /// </summary>
        public const string Int64Alias = "long";

        /// <summary>
        /// The alias around the 'long[]' array type.
        /// </summary>
        public const string Int64ArrayAlias = "long[]";

        /// <summary>
        /// The alias around the 'float' type.
        /// </summary>
        public const string FloatAlias = "float";

        /// <summary>
        /// The alias around the 'float[]' array type.
        /// </summary>
        public const string FloatArrayAlias = "float[]";

        /// <summary>
        /// The alias around the 'double' type.
        /// </summary>
        public const string DoubleAlias = "double";

        /// <summary>
        /// The alias around the 'double[]' array type.
        /// </summary>
        public const string DoubleArrayAlias = "double[]";

        /// <summary>
        /// The alias around the 'decimal' type.
        /// </summary>
        public const string DecimalAlias = "decimal";

        /// <summary>
        /// The alias around the 'decimal[]' array type.
        /// </summary>
        public const string DecimalArrayAlias = "decimal[]";

        /// <summary>
        /// The alias around the 'unsigned short' type.
        /// </summary>
        public const string UInt16Alias = "ushort";

        /// <summary>
        /// The alias around the 'ushort[]' array type.
        /// </summary>
        public const string UInt16ArrayAlias = "ushort[]";

        /// <summary>
        /// The alias around the 'unsigned int' type.
        /// </summary>
        public const string UInt32Alias = "uint";

        /// <summary>
        /// The alias around the 'uint[]' array type.
        /// </summary>
        public const string UInt32ArrayAlias = "uint[]";

        /// <summary>
        /// The alias around the 'unsigned long' type.
        /// </summary>
        public const string UInt64Alias = "ulong";

        /// <summary>
        /// The alias around the 'ulong[]' array type.
        /// </summary>
        public const string UInt64ArrayAlias = "ulong[]";

        /// <summary>
        /// The alias around the 'DateTime' type (C# style).
        /// </summary>
        public const string DateAlias = "date";

        /// <summary>
        /// The alias around the 'DateTime[]' array type.
        /// </summary>
        public const string DateTimeArrayAliasCSharp = "date[]";

        /// <summary>
        /// The alias around the 'DateTime' type.
        /// </summary>
        public const string DateTimeAlias = "DateTime";

        /// <summary>
        /// The alias around the 'DateTime[]' array type.
        /// </summary>
        public const string DateTimeArrayAlias = "DateTime[]";

        /// <summary>
        /// The alias around the 'string' type.
        /// </summary>
        public const string StringAlias = "string";

        /// <summary>
        /// The alias around the 'string[]' array type.
        /// </summary>
        public const string StringArrayAlias = "string[]";

        /// <summary>
        /// The alias around the 'object' type.
        /// </summary>
        public const string ObjectAlias = "object";

        /// <summary>
        /// The alias around the 'object[]' array type.
        /// </summary>
        public const string ObjectArrayAlias = "object[]";

        /// <summary>
        /// The alias around the 'bool?' type.
        /// </summary>
        public const string NullableBoolAlias = "bool?";

        /// <summary>
        /// The alias around the 'bool?[]' array type.
        /// </summary>
        public const string NullableBoolArrayAlias = "bool?[]";

        /// <summary>
        /// The alias around the 'char?' type.
        /// </summary>
        public const string NullableCharAlias = "char?";

        /// <summary>
        /// The alias around the 'char?[]' array type.
        /// </summary>
        public const string NullableCharArrayAlias = "char?[]";

        /// <summary>
        /// The alias around the 'short?' type.
        /// </summary>
        public const string NullableInt16Alias = "short?";

        /// <summary>
        /// The alias around the 'short?[]' array type.
        /// </summary>
        public const string NullableInt16ArrayAlias = "short?[]";

        /// <summary>
        /// The alias around the 'int?' type.
        /// </summary>
        public const string NullableInt32Alias = "int?";

        /// <summary>
        /// The alias around the 'int?[]' array type.
        /// </summary>
        public const string NullableInt32ArrayAlias = "int?[]";

        /// <summary>
        /// The alias around the 'long?' type.
        /// </summary>
        public const string NullableInt64Alias = "long?";

        /// <summary>
        /// The alias around the 'long?[]' array type.
        /// </summary>
        public const string NullableInt64ArrayAlias = "long?[]";

        /// <summary>
        /// The alias around the 'float?' type.
        /// </summary>
        public const string NullableFloatAlias = "float?";

        /// <summary>
        /// The alias around the 'float?[]' array type.
        /// </summary>
        public const string NullableFloatArrayAlias = "float?[]";

        /// <summary>
        /// The alias around the 'double?' type.
        /// </summary>
        public const string NullableDoubleAlias = "double?";

        /// <summary>
        /// The alias around the 'double?[]' array type.
        /// </summary>
        public const string NullableDoubleArrayAlias = "double?[]";

        /// <summary>
        /// The alias around the 'decimal?' type.
        /// </summary>
        public const string NullableDecimalAlias = "decimal?";

        /// <summary>
        /// The alias around the 'decimal?[]' array type.
        /// </summary>
        public const string NullableDecimalArrayAlias = "decimal?[]";

        /// <summary>
        /// The alias around the 'unsigned short?' type.
        /// </summary>
        public const string NullableUInt16Alias = "ushort?";

        /// <summary>
        /// The alias around the 'ushort?[]' array type.
        /// </summary>
        public const string NullableUInt16ArrayAlias = "ushort?[]";

        /// <summary>
        /// The alias around the 'unsigned int?' type.
        /// </summary>
        public const string NullableUInt32Alias = "uint?";

        /// <summary>
        /// The alias around the 'uint?[]' array type.
        /// </summary>
        public const string NullableUInt32ArrayAlias = "uint?[]";

        /// <summary>
        /// The alias around the 'unsigned long?' type.
        /// </summary>
        public const string NullableUInt64Alias = "ulong?";

        /// <summary>
        /// The alias around the 'ulong?[]' array type.
        /// </summary>
        public const string NullableUInt64ArrayAlias = "ulong?[]";


        private static IDictionary types = new Hashtable();


        /// <summary>
        /// Registers standard and user-configured type aliases.
        /// </summary>
        static TypeResolverRegistry()
        {
            lock (types.SyncRoot)
            {
                types[BoolAlias] = typeof(bool);
                types[BoolArrayAlias] = typeof(bool[]);

                types[CharAlias] = typeof(char);
                types[CharArrayAlias] = typeof(char[]);

                types["Int16"] = typeof(Int16);
                types[Int16Alias] = typeof(Int16);
                types[Int16ArrayAlias] = typeof(Int16[]);

                types["Int32"] = typeof(Int32);
                types[Int32Alias] = typeof(Int32);
                types[Int32ArrayAlias] = typeof(Int32[]);

                types["Int64"] = typeof(Int64);
                types[Int64Alias] = typeof(Int64);
                types[Int64ArrayAlias] = typeof(Int64[]);

                types[FloatAlias] = typeof(float);
                types[FloatArrayAlias] = typeof(float[]);

                types[DoubleAlias] = typeof(double);
                types[DoubleArrayAlias] = typeof(double[]);

                types[DecimalAlias] = typeof(decimal);
                types[DecimalArrayAlias] = typeof(decimal[]);

                types["UInt16"] = typeof(UInt16);
                types[UInt16Alias] = typeof(UInt16);
                types[UInt16ArrayAlias] = typeof(UInt16[]);

                types["UInt32"] = typeof(UInt32);
                types[UInt32Alias] = typeof(UInt32);
                types[UInt32ArrayAlias] = typeof(UInt32[]);

                types["UInt64"] = typeof(UInt64);
                types[UInt64Alias] = typeof(UInt64);
                types[UInt64ArrayAlias] = typeof(UInt64[]);

                types[DateAlias] = typeof(DateTime);
                types[DateTimeAlias] = typeof(DateTime);
                types[DateTimeArrayAlias] = typeof(DateTime[]);
                types[DateTimeArrayAliasCSharp] = typeof(DateTime[]);

                types[StringAlias] = typeof(string);
                types[StringArrayAlias] = typeof(string[]);

                types[ObjectAlias] = typeof(object);
                types[ObjectArrayAlias] = typeof(object[]);

                types[NullableBoolAlias] = typeof(bool?);
                types[NullableBoolArrayAlias] = typeof(bool?[]);

                types[NullableCharAlias] = typeof(char?);
                types[NullableCharArrayAlias] = typeof(char?[]);

                types[NullableInt16Alias] = typeof(short?);
                types[NullableInt16ArrayAlias] = typeof(short?[]);

                types[NullableInt32Alias] = typeof(int?);
                types[NullableInt32ArrayAlias] = typeof(int?[]);

                types[NullableInt64Alias] = typeof(long?);
                types[NullableInt64ArrayAlias] = typeof(long?[]);

                types[NullableFloatAlias] = typeof(float?);
                types[NullableFloatArrayAlias] = typeof(float?[]);

                types[NullableDoubleAlias] = typeof(double?);
                types[NullableDoubleArrayAlias] = typeof(double?[]);

                types[NullableDecimalAlias] = typeof(decimal?);
                types[NullableDecimalArrayAlias] = typeof(decimal?[]);

                types[NullableUInt16Alias] = typeof(ushort?);
                types[NullableUInt16ArrayAlias] = typeof(ushort?[]);

                types[NullableUInt32Alias] = typeof(uint?);
                types[NullableUInt32ArrayAlias] = typeof(uint?[]);

                types[NullableUInt64Alias] = typeof(ulong?);
                types[NullableUInt64ArrayAlias] = typeof(ulong?[]);
            }
        }


        /// <summary> 
        /// Registers an alias for the specified <see cref="System.Type"/>. 
        /// </summary>
        /// <remarks>
        /// <p>
        /// This overload does eager resolution of the <see cref="System.Type"/>
        /// referred to by the <paramref name="typeName"/> parameter. It will throw a
        /// <see cref="System.TypeLoadException"/> if the <see cref="System.Type"/> referred
        /// to by the <paramref name="typeName"/> parameter cannot be resolved.
        /// </p>
        /// </remarks>
        /// <param name="alias">
        /// A string that will be used as an alias for the specified
        /// <see cref="System.Type"/>.
        /// </param>
        /// <param name="typeName">
        /// The (possibly partially assembly qualified) name of the
        /// <see cref="System.Type"/> to register the alias for.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// If either of the supplied parameters is <see langword="null"/> or
        /// contains only whitespace character(s).
        /// </exception>
        /// <exception cref="System.TypeLoadException">
        /// If the <see cref="System.Type"/> referred to by the supplied
        /// <paramref name="typeName"/> cannot be loaded.
        /// </exception>
        public static void RegisterType(string alias, string typeName)
        {
            AssertUtil.ArgumentNotEmpty(alias, "alias");
            AssertUtil.ArgumentNotEmpty(typeName, "typeName");

            var type = TypeResolutionUtil.ResolveType(typeName);
            if (type.GetTypeInfo().IsGenericTypeDefinition)
            { alias += ("`" + type.GetGenericArguments().Length); }

            RegisterType(alias, type);
        }

        /// <summary> 
        /// Registers short type name as an alias for 
        /// the supplied <see cref="System.Type"/>. 
        /// </summary> 
        /// <param name="type">
        /// The <see cref="System.Type"/> to register.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="type"/> is <see langword="null"/>.
        /// </exception>
        public static void RegisterType(Type type)
        {
            AssertUtil.ArgumentNotNull(type, "type");

            lock (types.SyncRoot)
            {
                types[type.Name] = type;
            }
        }

        /// <summary> 
        /// Registers an alias for the supplied <see cref="System.Type"/>. 
        /// </summary> 
        /// <param name="alias">
        /// The alias for the supplied <see cref="System.Type"/>.
        /// </param>
        /// <param name="type">
        /// The <see cref="System.Type"/> to register the supplied <paramref name="alias"/> under.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="type"/> is <see langword="null"/>; or if
        /// the supplied <paramref name="alias"/> is <see langword="null"/> or
        /// contains only whitespace character(s).
        /// </exception>
        public static void RegisterType(string alias, Type type)
        {
            AssertUtil.ArgumentNotEmpty(alias, "alias");
            AssertUtil.ArgumentNotNull(type, "type");

            lock (types.SyncRoot)
            {
                types[alias] = type;
            }
        }

        /// <summary> 
        /// Resolves the supplied <paramref name="alias"/> to a <see cref="System.Type"/>. 
        /// </summary> 
        /// <param name="alias">
        /// The alias to resolve.
        /// </param>
        /// <returns>
        /// The <see cref="System.Type"/> the supplied <paramref name="alias"/> was
        /// associated with, or <see lang="null"/> if no <see cref="System.Type"/> 
        /// was previously registered for the supplied <paramref name="alias"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="alias"/> is <see langword="null"/> or
        /// contains only whitespace character(s).
        /// </exception>
        public static Type ResolveType(string alias)
        {
            AssertUtil.ArgumentNotEmpty(alias, "alias");
            return (Type)types[alias];
        }

        /// <summary>
        /// Returns a flag specifying whether <b>TypeResolverRegistry</b> contains
        /// specified alias or not.
        /// </summary>
        /// <param name="alias">
        /// Alias to check.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified type alias is registered, 
        /// <c>false</c> otherwise.
        /// </returns>
        public static bool ContainsAlias(string alias)
        {
            return types.Contains(alias);
        }
    }
}