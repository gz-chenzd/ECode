using System;

namespace ECode.TypeResolution
{
    /// <summary>
    /// Holds data about a <see cref="System.Type"/> and it's
    /// attendant <see cref="System.Reflection.Assembly"/>.
    /// </summary>
    public class TypeAssemblyHolder
    {
        /// <summary>
        /// The string that separates a <see cref="System.Type"/> name
        /// from the name of it's attendant <see cref="System.Reflection.Assembly"/>
        /// in an assembly qualified type name.
        /// </summary>
        public const string TYPE_ASSEMBLY_SEPARATOR = ",";


        private string  unresolvedAssemblyName      = null;
        private string  unresolvedTypeName          = null;


        /// <summary>
        /// Creates a new instance of the TypeAssemblyHolder class.
        /// </summary>
        /// <param name="unresolvedTypeName">
        /// The unresolved name of a <see cref="System.Type"/>.
        /// </param>
        public TypeAssemblyHolder(string unresolvedTypeName)
        {
            SplitTypeAndAssemblyNames(unresolvedTypeName);
        }


        /// <summary>
        /// The (unresolved) type name portion of the original type name.
        /// </summary>
        public string TypeName
        {
            get { return unresolvedTypeName; }
        }

        /// <summary>
        /// The (unresolved, possibly partial) name of the attandant assembly.
        /// </summary>
        public string AssemblyName
        {
            get { return unresolvedAssemblyName; }
        }

        /// <summary>
        /// Is the type name being resolved assembly qualified?
        /// </summary>
        public bool IsAssemblyQualified
        {
            get { return !string.IsNullOrWhiteSpace(AssemblyName); }
        }

        private void SplitTypeAndAssemblyNames(string originalTypeName)
        {
            // generic types may look like:
            // ECode.TestGenericObject`2[[System.Int32, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]][] , ECode.Tests, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
            //
            // start searching for assembly separator after the last bracket if any
            int typeAssemblyIndex = originalTypeName.LastIndexOf(']');
            typeAssemblyIndex = originalTypeName.IndexOf(TYPE_ASSEMBLY_SEPARATOR, typeAssemblyIndex + 1, StringComparison.Ordinal);
            if (typeAssemblyIndex < 0)
            {
                unresolvedTypeName = originalTypeName;
            }
            else
            {
                unresolvedTypeName = originalTypeName.Substring(
                    0, typeAssemblyIndex).Trim();
                unresolvedAssemblyName = originalTypeName.Substring(
                    typeAssemblyIndex + 1).Trim();
            }
        }
    }
}
