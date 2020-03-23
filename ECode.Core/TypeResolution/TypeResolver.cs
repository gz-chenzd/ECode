using System;
using System.Reflection;

namespace ECode.TypeResolution
{
    /// <summary>
    /// Resolves a <see cref="System.Type"/> by name.
    /// </summary>
    public class TypeResolver : ITypeResolver
    {
        /// <summary>
        /// Resolves the supplied <paramref name="typeName"/> to a
        /// <see cref="System.Type"/> instance.
        /// </summary>
        /// <param name="typeName">
        /// The unresolved (possibly partially assembly qualified) name 
        /// of a <see cref="System.Type"/>.
        /// </param>
        /// <returns>
        /// A resolved <see cref="System.Type"/> instance.
        /// </returns>
        /// <exception cref="System.TypeLoadException">
        /// If the supplied <paramref name="typeName"/> could not be resolved
        /// to a <see cref="System.Type"/>.
        /// </exception>
        public virtual Type Resolve(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
            {
                throw BuildTypeLoadException(typeName);
            }

            Type resolvedType = null;
            var typeInfo = new TypeAssemblyHolder(typeName);

            try
            {
                resolvedType = (typeInfo.IsAssemblyQualified) ?
                     LoadTypeDirectlyFromAssembly(typeInfo) :
                     LoadTypeByIteratingOverAllLoadedAssemblies(typeInfo);
            }
            catch (Exception ex)
            {
                if (ex is TypeLoadException)
                {
                    throw;
                }

                throw BuildTypeLoadException(typeName, ex);
            }

            if (resolvedType == null)
            {
                throw BuildTypeLoadException(typeName);
            }

            return resolvedType;
        }

        /// <summary>
        /// Uses <see cref="System.Reflection.Assembly.Load(AssemblyName)"/>
        /// to load an <see cref="System.Reflection.Assembly"/> and then the attendant
        /// <see cref="System.Type"/> referred to by the <paramref name="typeInfo"/>
        /// parameter.
        /// </summary>
        /// <param name="typeInfo">
        /// The assembly and type to be loaded.
        /// </param>
        /// <returns>
        /// A <see cref="System.Type"/>, or <see lang="null"/>.
        /// </returns>
        /// <exception cref="System.Exception">
        /// <see cref="System.Reflection.Assembly.Load(AssemblyName)"/>
        /// </exception>
        private static Type LoadTypeDirectlyFromAssembly(TypeAssemblyHolder typeInfo)
        {
            Type resolvedType = null;

            var assembly = AssemblyResolver.Resolve(typeInfo.AssemblyName);
            if (assembly != null)
            {
                resolvedType = assembly.GetType(typeInfo.TypeName, true, true);
            }

            return resolvedType;
        }

        /// <summary>
        /// Uses <see cref="System.Reflection.Assembly.GetEntryAssembly().GetReferencedAssemblies()"/>
        /// to load the attendant <see cref="System.Type"/> referred to by 
        /// the <paramref name="typeInfo"/> parameter.
        /// </summary>
        /// <param name="typeInfo">
        /// The type to be loaded.
        /// </param>
        /// <returns>
        /// A <see cref="System.Type"/>, or <see lang="null"/>.
        /// </returns>
        private static Type LoadTypeByIteratingOverAllLoadedAssemblies(TypeAssemblyHolder typeInfo)
        {
            Type resolvedType = null;

            var anames = Assembly.GetEntryAssembly().GetReferencedAssemblies();
            foreach (var aname in Assembly.GetEntryAssembly().GetReferencedAssemblies())
            {
                var assembly = Assembly.Load(aname);
                resolvedType = assembly.GetType(typeInfo.TypeName, false, false);
                if (resolvedType != null)
                {
                    break;
                }
            }

            return resolvedType;
        }

        /// <summary>
        /// Creates a new <see cref="System.TypeLoadException"/> instance
        /// from the given <paramref name="typeName"/> with the given inner <see cref="Exception"/> 
        /// </summary>
        protected static TypeLoadException BuildTypeLoadException(string typeName, Exception ex = null)
        {
            return new TypeLoadException($"Could not load type from string '{typeName}'.", ex);
        }
    }
}
