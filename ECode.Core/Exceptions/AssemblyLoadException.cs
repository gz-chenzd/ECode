using System;

namespace ECode.Core
{
    public class AssemblyLoadException : Exception
    {
        public AssemblyLoadException(string message)
            : base(message)
        {

        }
    }
}
