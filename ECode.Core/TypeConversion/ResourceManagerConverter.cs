using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.Loader;
using ECode.Core;
using ECode.TypeResolution;

namespace ECode.TypeConversion
{
    /// <summary>
    /// Converts a two part string, (resource name, assembly name) to a ResourceManager instance.
    /// </summary>
    public class ResourceManagerConverter : TypeConverter
    {
        /// <summary>
        /// This constant represents the name of the folder/assembly containing global resources.
        /// </summary>
        static readonly string APP_GLOBALRESOURCES_ASSEMBLYNAME = "App_GlobalResources";


        /// <summary>
        /// Can we convert from the sourceType to a <see cref="System.Resources.ResourceManager"/> instance?
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
        /// Convert from a <see cref="System.String"/> value to a <see cref="System.Resources.ResourceManager"/> instance.
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
        /// A <see cref="System.Resources.ResourceManager"/> if successful. 
        /// </returns>
        /// <exception cref="ArgumentException">If the specified <paramref name="value"/> does not denote a valid resource</exception>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                try
                {
                    // convert incoming string into ResourceManager...
                    var resourceManagerDescription = ((string)value).Split( ',' );
                    if (resourceManagerDescription.Length != 2)
                    {
                        throw new ArgumentException("The string to specify a ResourceManager must be a comma delimited list of length two.  i.e. resourcename, assembly parial name.");
                    }

                    var resourceName = resourceManagerDescription[0].Trim();
                    if (string.IsNullOrWhiteSpace(resourceName))
                    {
                        throw new ArgumentException("Empty value set for the resource name in ResourceManager string.");
                    }

                    var assemblyName = resourceManagerDescription[1].Trim();
                    if (string.IsNullOrWhiteSpace(assemblyName))
                    {
                        throw new ArgumentException("Empty value set for the assembly name in ResourceManager string.");
                    }


                    if (assemblyName == APP_GLOBALRESOURCES_ASSEMBLYNAME)
                    {
                        try
                        {
                            var resourcesType = TypeResolutionUtil.ResolveType(resourceName);
                            // look both, NonPublic and Public properties (SPRNET-861)
                            var resourceManagerProperty = resourcesType.GetProperty("ResourceManager", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
                            return (ResourceManager)resourceManagerProperty.GetValue(resourcesType, null);
                        }
                        catch (TypeLoadException ex)
                        {
                            throw new ArgumentException($"Could not load resources '{resourceName}'", ex);
                        }
                    }

                    //Assembly ass = Assembly.LoadWithPartialName(assemblyName);
                    var ass = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyName);
                    if (ass == null)
                    { throw new ArgumentException($"Could not find assembly with name '{assemblyName}'."); }

                    return new ResourceManager(resourceName, ass);
                }
                catch (Exception ex)
                { throw new TypeConvertException(value, typeof(ResourceManager), ex); }
            }
            else
            { throw new TypeConvertException(value, typeof(ResourceManager)); }
        }
    }
}
