using System;
using System.Collections.Generic;
using System.Reflection;

namespace ECode.DependencyInjection
{
    class ListenerDefinition : DefinitionBase
    {
        private List<MethodInfo>    nameMatchedMethods      = null;
        private MethodInfo          finalMatchedMethod      = null;


        public string Event
        { get; set; }

        public string Method
        { get; set; }

        public DefinitionBase RefDefinition
        { get; set; }


        public override void Validate()
        {
            if (this.nameMatchedMethods != null)
            {
                return;
            }

            this.nameMatchedMethods = new List<MethodInfo>();
            if (this.RefDefinition is TypeDefinition)
            {
                var methods = this.RefDefinition.ResolvedType.GetMethods(BindingFlags.Static | BindingFlags.Public);
                foreach (var methodInfo in methods)
                {
                    if (methodInfo.Name == this.Method)
                    {
                        this.nameMatchedMethods.Add(methodInfo);
                    }
                }

                if (this.nameMatchedMethods.Count == 0)
                {
                    throw new InvalidOperationException($"Cannot find public static named method '{this.Method}' on type '{this.RefDefinition.ResolvedType.FullName}'.");
                }
            }
            else
            {
                var methods = this.RefDefinition.ResolvedType.GetMethods(BindingFlags.Instance | BindingFlags.Public);
                foreach (var methodInfo in methods)
                {
                    if (methodInfo.Name == this.Method)
                    {
                        this.nameMatchedMethods.Add(methodInfo);
                    }
                }

                if (this.nameMatchedMethods.Count == 0)
                {
                    throw new InvalidOperationException($"Cannot find public instance named method '{this.Method}' on type '{this.RefDefinition.ResolvedType.FullName}'.");
                }
            }
        }

        public override object GetValue()
        {
            throw new NotImplementedException();
        }

        public void ValidateListener(Type delegateType)
        {
            if (!typeof(Delegate).IsAssignableFrom(delegateType))
            {
                throw new ArgumentException($"Type '{delegateType.FullName}' is not valid delegate type.");
            }

            var delegateParms = delegateType.GetMethod("Invoke").GetParameters();
            foreach (var methodInfo in this.nameMatchedMethods)
            {
                var methodParms = methodInfo.GetParameters();
                if (delegateParms.Length > methodParms.Length)
                {
                    continue;
                }

                bool parmMatched = true;
                for (int i = 0; i < methodParms.Length; i++)
                {
                    if (i >= delegateParms.Length)
                    {
                        if (!methodParms[i].HasDefaultValue)
                        {
                            parmMatched = false;
                            break;
                        }
                    }
                    else
                    {
                        if (delegateParms[i].ParameterType != methodParms[i].ParameterType)
                        {
                            parmMatched = false;
                            break;
                        }
                    }
                }

                if (parmMatched)
                {
                    this.finalMatchedMethod = methodInfo;
                    break;
                }
            }

            if (this.finalMatchedMethod == null)
            {
                throw new InvalidOperationException($"Doesnot contain the same signature named method '{this.Method}' with delegate '{delegateType.FullName}'.");
            }
        }

        public Delegate CreateDelegate(Type delegateType)
        {
            if (this.RefDefinition is TypeDefinition)
            {
                return this.finalMatchedMethod.CreateDelegate(delegateType);
            }
            else
            {
                return this.finalMatchedMethod.CreateDelegate(delegateType, this.RefDefinition.GetValue());
            }
        }
    }
}
