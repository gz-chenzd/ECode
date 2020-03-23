using System;
using System.Collections.Generic;
using System.Reflection;
using ECode.Core;
using ECode.TypeResolution;
using ECode.Utility;

namespace ECode.EventFramework
{
    public static class EventRegistration
    {
        static List<Assembly> loadedAssemblies = new List<Assembly>();


        public static void RegisterAssembly(Assembly assembly)
        {
            AssertUtil.ArgumentNotNull(assembly, nameof(assembly));

            if (loadedAssemblies.Contains(assembly))
            { return; }

            loadedAssemblies.Add(assembly);

            var interfaceType = typeof(IEventHandler);
            foreach (Type handlerType in assembly.GetTypes())
            {
                if (!interfaceType.IsAssignableFrom(handlerType))
                { continue; }

                var attrs = handlerType.GetCustomAttributes(typeof(EventNameAttribute), false);
                if (attrs == null || attrs.Length == 0)
                { continue; }

                var wrappedHandler = new WrappedHandler(handlerType);

                var eventNameAttr = attrs[0] as EventNameAttribute;
                foreach (string eventName in eventNameAttr.EventNames)
                {
                    EventCore.RegisterHandler(eventName, wrappedHandler);
                }
            }
        }

        public static void RegisterAssemblies(string[] assemblyNames)
        {
            foreach (string assemblyName in assemblyNames)
            {
                var assembly = TypeResolutionUtil.ResolveAssembly(assemblyName);
                if (assembly == null)
                { throw new AssemblyLoadException($"Cannot load assembly '{assemblyName}'."); }

                RegisterAssembly(assembly);
            }
        }
    }
}