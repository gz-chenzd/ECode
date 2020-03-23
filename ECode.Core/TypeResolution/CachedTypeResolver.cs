using System;
using System.Collections;
using System.Collections.Specialized;

namespace ECode.TypeResolution
{
    /// <summary>
    /// Resolves (instantiates) a <see cref="System.Type"/> by it's (possibly
    /// assembly qualified) name, and caches the <see cref="System.Type"/>
    /// instance against the type name.
    /// </summary>
    public class CachedTypeResolver : ITypeResolver
    {
        private ITypeResolver   typeResolver    = null;

        /// <summary>
        /// The cache, mapping type names (<see cref="System.String"/> instances) against
        /// <see cref="System.Type"/> instances.
        /// </summary>
        private IDictionary     cachedTypes     = new HybridDictionary();


        /// <summary>
        /// Creates a new instance of the <see cref="ECode.TypeResolution.CachedTypeResolver"/> class.
        /// </summary>
        /// <param name="typeResolver">
        /// The <see cref="ECode.TypeResolution.ITypeResolver"/> that this instance will delegate
        /// actual <see cref="System.Type"/> resolution to if a <see cref="System.Type"/>
        /// cannot be found in this instance's <see cref="System.Type"/> cache.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="typeResolver"/> is <see langword="null"/>.
        /// </exception>
        public CachedTypeResolver(ITypeResolver typeResolver)
        {
            if (typeResolver == null)
            {
                throw new ArgumentNullException(nameof(typeResolver));
            }

            this.typeResolver = typeResolver;
        }

        /// <summary>
        /// Resolves the supplied <paramref name="typeName"/> to a <see cref="System.Type"/>
        /// instance.
        /// </summary>
        /// <param name="typeName">
        /// The (possibly partially assembly qualified) name of a <see cref="System.Type"/>.
        /// </param>
        /// <returns>
        /// A resolved <see cref="System.Type"/> instance.
        /// </returns>
        /// <exception cref="System.TypeLoadException">
        /// If the supplied <paramref name="typeName"/> could not be resolved
        /// to a <see cref="System.Type"/>.
        /// </exception>
        public Type Resolve(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
            {
                throw BuildTypeLoadException(typeName);
            }

            Type type = null;
            try
            {
                lock (this.cachedTypes.SyncRoot)
                {
                    type = this.cachedTypes[typeName] as Type;
                    if (type == null)
                    {
                        type = this.typeResolver.Resolve(typeName);
                        this.cachedTypes[typeName] = type;
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is TypeLoadException)
                {
                    throw;
                }

                throw BuildTypeLoadException(typeName, ex);
            }

            return type;
        }


        private static TypeLoadException BuildTypeLoadException(string typeName, Exception ex = null)
        {
            return new TypeLoadException($"Could not load type from string '{typeName}'.", ex);
        }
    }
}
