using System;
using System.IO;
using System.Reflection;

namespace ECode.TypeResolution
{
    static class AssemblyResolver
    {
        static string       appBasePath     = null;


        static AssemblyResolver()
        {
            appBasePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
        }

        private static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            var assName = new AssemblyName(args.Name);
            if (File.Exists(Path.Combine(appBasePath, $"{assName.Name}.dll")))
            {
                return Assembly.LoadFile(Path.Combine(appBasePath, $"{assName.Name}.dll"));
            }

            return null;
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
        public static Assembly Resolve(string assemblyName)
        {
            return Resolve(new AssemblyName(assemblyName));
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
        public static Assembly Resolve(AssemblyName assemblyName)
        {
            return Assembly.Load(assemblyName);
        }
    }
}
