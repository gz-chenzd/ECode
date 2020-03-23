using System;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using ECode.Utility;

namespace ECode.TypeResolution
{
    /// <summary>
    /// Helper methods with regard to type resolution.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Not intended to be used directly by applications.
    /// </p>
    /// </remarks>
    public static class TypeResolutionUtil
    {
        static readonly ITypeResolver   internalTypeResolver
            = new CachedTypeResolver(new GenericTypeResolver());


        /// <summary>
        /// Resolves the supplied type name into a <see cref="System.Type"/>
        /// instance.
        /// </summary>
        /// <remarks>
        /// <p>
        /// If you require special <see cref="System.Type"/> resolution, do
        /// <b>not</b> use this method, but rather instantiate
        /// your own <see cref="ECode.TypeResolution.TypeResolver"/>.
        /// </p>
        /// </remarks>
        /// <param name="typeName">
        /// The (possibly partially assembly qualified) name of a
        /// <see cref="System.Type"/>.
        /// </param>
        /// <returns>
        /// A resolved <see cref="System.Type"/> instance.
        /// </returns>
        /// <exception cref="System.TypeLoadException">
        /// If the type cannot be resolved.
        /// </exception>
        public static Type ResolveType(string typeName)
        {
            var type = TypeResolverRegistry.ResolveType(typeName);
            if (type == null)
            {
                type = internalTypeResolver.Resolve(typeName);
            }

            return type;
        }

        /// <summary>
        /// Resolves the supplied <paramref name="assemblyName"/> to a
        /// <see cref="System.Reflection.Assembly"/> instance.
        /// </summary>
        /// <param name="assemblyName">
        /// The unresolved name of a <see cref="System.Reflection.Assembly"/>.
        /// </param>
        /// <returns>
        /// A resolved <see cref="System.Reflection.Assembly"/> instance or null if not exists.
        /// </returns>
        public static Assembly ResolveAssembly(string assemblyName)
        {
            return AssemblyResolver.Resolve(assemblyName);
        }

        /// <summary>
        /// Resolves a string array of interface names to
        /// a <see cref="System.Type"/> array.
        /// </summary>
        /// <param name="interfaceNames">
        /// An array of valid interface names. Each name must include the full
        /// interface and assembly name.
        /// </param>
        /// <returns>An array of interface <see cref="System.Type"/>s.</returns>
        /// <exception cref="System.TypeLoadException">
        /// If any of the interfaces can't be loaded.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If any of the <see cref="System.Type"/>s specified is not an interface.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="interfaceNames"/> (or any of its elements ) is
        /// <see langword="null"/>.
        /// </exception>
        public static Type[] ResolveInterfaceArray(string[] interfaceNames)
        {
            AssertUtil.ArgumentNotNull(interfaceNames, "interfaceNames");

            var interfaces = new ArrayList();
            for (int i = 0; i < interfaceNames.Length; i++)
            {
                string interfaceName = interfaceNames[i];
                AssertUtil.ArgumentNotNull(interfaceName,
                                            string.Format(CultureInfo.InvariantCulture, "interfaceNames[{0}]", i));

                var interfaceType = ResolveType(interfaceName);
                if (!interfaceType.GetTypeInfo().IsInterface)
                {
                    throw new ArgumentException(
                        string.Format(CultureInfo.InvariantCulture,
                                      "[{0}] is a class.",
                                      interfaceType.FullName));
                }

                interfaces.Add(interfaceType);
                interfaces.AddRange(interfaceType.GetInterfaces());
            }
            return (Type[])interfaces.ToArray(typeof(Type));
        }


        // TODO : Use the future Pointcut expression language instead

        readonly static Regex methodMatchRegex = new Regex(
            @"(?<methodName>([\w]+\.)*[\w\*]+)(?<parameters>(\((?<parameterTypes>[\w\.]+(,[\w\.]+)*)*\))?)",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        /// <summary>
        /// Match a method against the given pattern.
        /// </summary>
        /// <param name="pattern">the pattern to match against.</param>
        /// <param name="method">the method to match.</param>
        /// <returns>
        /// <see lang="true"/> if the method matches the given pattern; otherwise <see lang="false"/>.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// If the supplied <paramref name="pattern"/> is invalid.
        /// </exception>
        public static bool MethodMatch(string pattern, MethodInfo method)
        {
            Match m = methodMatchRegex.Match(pattern);
            if (!m.Success)
            { throw new ArgumentException($"The pattern '{pattern}' is not well-formed."); }

            // Check method name
            var methodNamePattern = m.Groups["methodName"].Value;
            if (!StringUtil.IsMatch(methodNamePattern, method.Name))
            { return false; }

            if (m.Groups["parameters"].Value.Length > 0)
            {
                // Check parameter types
                var parameters = m.Groups["parameterTypes"].Value;
                var paramTypes = (parameters.Length == 0) ? new string[0] : parameters.Split(",");
                var paramInfos = method.GetParameters();

                // Verify parameter count
                if (paramTypes.Length != paramInfos.Length)
                { return false; }

                // Match parameter types
                for (int i = 0; i < paramInfos.Length; i++)
                {
                    if (paramInfos[i].ParameterType != ResolveType(paramTypes[i]))
                    { return false; }
                }
            }

            return true;
        }
    }
}