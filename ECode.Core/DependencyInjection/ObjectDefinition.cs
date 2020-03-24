using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using ECode.Logging;
using ECode.TypeConversion;
using ECode.TypeResolution;

namespace ECode.DependencyInjection
{
    class ObjectDefinition : DefinitionBase
    {
        static readonly Logger  Log     = LogManager.GetLogger("DependencyInjection");


        private object              singleInstance      = null;

        private ConstructorInfo     ctorMethod          = null;
        private MethodInfo          initMethod          = null;
        private MethodInfo          factoryMethod       = null;

        private Dictionary<PropertyDefinition, PropertyInfo>    propertyMaps
            = new Dictionary<PropertyDefinition, PropertyInfo>();

        private Dictionary<ListenerDefinition, EventInfo>       listenerMaps
            = new Dictionary<ListenerDefinition, EventInfo>();


        public string ID
        { get; set; }

        public string Type
        { get; set; }

        public bool IsSingleton
        { get; set; }

        /// <summary>
        /// Not support.
        /// </summary>
        public bool IsLazyInit
        { get; set; }

        public string InitMethod
        { get; set; }

        /// <summary>
        /// Not support.
        /// </summary>
        public string DestroyMethod
        { get; set; }


        public DefinitionBase FactoryObject
        { get; set; }

        public string FactoryMethod
        { get; set; }


        public List<ArgumentDefinition> ConstructorArgs
        { get; set; } = new List<ArgumentDefinition>();

        public List<ArgumentDefinition> InitializeArgs
        { get; set; } = new List<ArgumentDefinition>();

        public List<ArgumentDefinition> FactoryArgs
        { get; set; } = new List<ArgumentDefinition>();

        public List<PropertyDefinition> Properties
        { get; set; } = new List<PropertyDefinition>();

        public List<ListenerDefinition> Listeners
        { get; set; } = new List<ListenerDefinition>();


        private object CreateInstance()
        {
            object instance = null;

            var watch = new Stopwatch();
            watch.Start();

            if (this.FactoryObject != null)
            {
                instance = this.factoryMethod.Invoke(this.FactoryObject.GetValue(), GenerateArguments(this.factoryMethod.GetParameters(), this.FactoryArgs));
            }
            else if (this.factoryMethod != null)
            {
                instance = this.factoryMethod.Invoke(null, GenerateArguments(this.factoryMethod.GetParameters(), this.FactoryArgs));
            }
            else if (this.ctorMethod != null)
            {
                instance = this.ctorMethod.Invoke(GenerateArguments(this.ctorMethod.GetParameters(), this.ConstructorArgs));
            }
            else
            {
                instance = Activator.CreateInstance(this.ResolvedType);
            }

            foreach (var property in this.propertyMaps)
            {
                property.Value.SetValue(instance, TypeConversionUtil.ConvertValueIfNecessary(property.Value.PropertyType, property.Key.GetValue()));
            }

            foreach (var listener in this.listenerMaps)
            {
                listener.Value.AddEventHandler(instance, listener.Key.CreateDelegate(listener.Value.EventHandlerType));
            }

            if (this.initMethod != null)
            {
                this.initMethod.Invoke(instance, GenerateArguments(this.initMethod.GetParameters(), this.InitializeArgs));
            }

            watch.Stop();

            if (!string.IsNullOrWhiteSpace(this.ID))
            { Log.Debug($"Object '{this.ID}' created, {watch.ElapsedMilliseconds}ms time took."); }
            else
            { Log.Debug($"Anonymous object created, {watch.ElapsedMilliseconds}ms time took."); }

            return instance;
        }

        private object[] GenerateArguments(ParameterInfo[] parameters, List<ArgumentDefinition> arguments)
        {
            var paramValues = new List<object>();
            for (int i = 0; i < parameters.Length; i++)
            {
                var arg = arguments.Find(t => t.Index.HasValue && t.Index.Value == i);
                if (arg == null)
                {
                    arg = arguments.Find(t => t.Name == parameters[i].Name);
                }

                if (arg != null)
                {
                    paramValues.Add(TypeConversionUtil.ConvertValueIfNecessary(parameters[i].ParameterType, arg.GetValue()));
                }
                else
                {
                    // use default value
                    paramValues.Add(parameters[i].DefaultValue);
                }
            }

            return paramValues.ToArray();
        }

        private bool ValidateParametersMatched(ParameterInfo[] parameters, List<ArgumentDefinition> arguments)
        {
            if (arguments.Count > parameters.Length)
            { return false; }

            bool parametersMatched = true;
            for (int i = 0; i < parameters.Length; i++)
            {
                var arg = arguments.Find(t => t.Index.HasValue && t.Index.Value == i);
                if (arg == null)
                {
                    arg = arguments.Find(t => t.Name == parameters[i].Name);
                }
                else if (arguments.Find(t => t.Name == parameters[i].Name) != null)
                {
                    // contains name and index conflict
                    parametersMatched = false;
                    break;
                }

                if (arg == null)
                {
                    if (!parameters[i].HasDefaultValue)
                    {
                        parametersMatched = false;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }

                if (arg.ResolvedType != null)
                {
                    if (!parameters[i].ParameterType.IsAssignableFrom(arg.ResolvedType))
                    {
                        parametersMatched = false;
                        break;
                    }
                }
                else
                {
                    if (!arg.CanConvertTo(parameters[i].ParameterType))
                    {
                        parametersMatched = false;
                        break;
                    }
                }
            }

            return parametersMatched;
        }


        public override void Validate()
        {
            if (this.ResolvedType != null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(this.Type))
            {
                this.ResolvedType = TypeResolutionUtil.ResolveType(this.Type);
                if (!string.IsNullOrWhiteSpace(this.FactoryMethod))
                {
                    var nameMatchedMethods = new List<MethodInfo>();
                    var methods = this.ResolvedType.GetMethods(BindingFlags.Static | BindingFlags.Public);
                    foreach (var methodInfo in methods)
                    {
                        if (methodInfo.Name == this.FactoryMethod)
                        {
                            nameMatchedMethods.Add(methodInfo);
                        }
                    }

                    if (nameMatchedMethods.Count == 0)
                    {
                        throw new InvalidOperationException($"Cannot find public static named method '{this.FactoryMethod}' on type '{this.ResolvedType.FullName}'.");
                    }

                    foreach (var methodInfo in nameMatchedMethods)
                    {
                        var parms = methodInfo.GetParameters();
                        if (!ValidateParametersMatched(parms, this.FactoryArgs))
                        { continue; }

                        this.factoryMethod = methodInfo;
                        break;
                    }

                    if (this.factoryMethod == null)
                    {
                        throw new InvalidOperationException($"Doesnot contain matched named method '{this.FactoryMethod}' with arguments.");
                    }

                    this.ResolvedType = this.factoryMethod.ReturnType;
                }
                else
                {
                    var ctorMethods = this.ResolvedType.GetConstructors();
                    if (this.ConstructorArgs.Count > 0)
                    {
                        foreach (var ctorMethodInfo in ctorMethods)
                        {
                            var parms = ctorMethodInfo.GetParameters();
                            if (!ValidateParametersMatched(parms, this.ConstructorArgs))
                            { continue; }

                            this.ctorMethod = ctorMethodInfo;
                            break;
                        }

                        if (this.ctorMethod == null)
                        {
                            throw new InvalidOperationException("Doesnot contain matched constructor method with arguments.");
                        }
                    }
                    else if (ctorMethods.Length > 0)
                    {
                        foreach (var ctorMethodInfo in ctorMethods)
                        {
                            if (ctorMethodInfo.GetParameters().Length > 0)
                            {
                                continue;
                            }

                            this.ctorMethod = ctorMethodInfo;
                            break;
                        }

                        if (this.ctorMethod == null)
                        {
                            throw new InvalidOperationException("Doesnot contain constructor method with empty arguments.");
                        }
                    }
                }
            }
            else
            {
                var nameMatchedMethods = new List<MethodInfo>();
                var methods = this.FactoryObject.ResolvedType.GetMethods(BindingFlags.Instance | BindingFlags.Public);
                foreach (var methodInfo in methods)
                {
                    if (methodInfo.Name == this.FactoryMethod)
                    {
                        nameMatchedMethods.Add(methodInfo);
                    }
                }

                if (nameMatchedMethods.Count == 0)
                {
                    throw new InvalidOperationException($"Cannot find public instance named method '{this.FactoryMethod}' on type '{this.FactoryObject.ResolvedType.FullName}'.");
                }

                foreach (var methodInfo in nameMatchedMethods)
                {
                    var parms = methodInfo.GetParameters();
                    if (!ValidateParametersMatched(parms, this.FactoryArgs))
                    { continue; }

                    this.factoryMethod = methodInfo;
                    break;
                }

                if (this.factoryMethod == null)
                {
                    throw new InvalidOperationException($"Doesnot contain matched named method '{this.FactoryMethod}' with arguments on type '{this.FactoryObject.ResolvedType.FullName}'.");
                }

                this.ResolvedType = this.factoryMethod.ReturnType;
            }

            foreach (var property in this.Properties)
            {
                var propInfo = this.ResolvedType.GetProperty(property.Name, BindingFlags.Instance | BindingFlags.Public);
                if (propInfo == null)
                {
                    throw new InvalidOperationException($"Doesnot contain instance property '{property.Name}' on type '{this.ResolvedType.FullName}'.");
                }
                else
                {
                    if (!propInfo.CanWrite)
                    {
                        throw new InvalidOperationException($"Property '{property.Name}' cannot be set on type '{this.ResolvedType.FullName}'.");
                    }

                    if (!property.CanConvertTo(propInfo.PropertyType))
                    {
                        throw new InvalidOperationException($"Cannot assign value '{property.GetValue()}' to property '{propInfo.Name}'.");
                    }

                    propertyMaps[property] = propInfo;
                }
            }

            foreach (var listener in this.Listeners)
            {
                var eventInfo = this.ResolvedType.GetEvent(listener.Event, BindingFlags.Instance | BindingFlags.Public);
                if (eventInfo == null)
                {
                    throw new InvalidOperationException($"Doesnot contain instance event '{listener.Event}' on type '{this.ResolvedType.FullName}'.");
                }
                else
                {
                    listener.ValidateListener(eventInfo.EventHandlerType);
                    listenerMaps[listener] = eventInfo;
                }
            }

            if (!string.IsNullOrWhiteSpace(this.InitMethod))
            {
                var nameMatchedMethods = new List<MethodInfo>();
                var methods = this.ResolvedType.GetMethods(BindingFlags.Instance | BindingFlags.Public);
                foreach (var methodInfo in methods)
                {
                    if (methodInfo.Name == this.InitMethod)
                    {
                        nameMatchedMethods.Add(methodInfo);
                    }
                }

                if (nameMatchedMethods.Count == 0)
                {
                    throw new InvalidOperationException($"Cannot find public instance named method '{this.InitMethod}' on type '{this.ResolvedType.FullName}'.");
                }

                foreach (var methodInfo in nameMatchedMethods)
                {
                    var parms = methodInfo.GetParameters();
                    if (!ValidateParametersMatched(parms, this.InitializeArgs))
                    { continue; }

                    this.initMethod = methodInfo;
                    break;
                }

                if (this.initMethod == null)
                {
                    throw new InvalidOperationException($"Doesnot contain matched named method '{this.InitMethod}' with arguments on type '{this.ResolvedType.FullName}'.");
                }
            }
        }

        public override object GetValue()
        {
            if (this.IsSingleton)
            {
                if (this.singleInstance != null)
                {
                    return this.singleInstance;
                }

                lock (this)
                {
                    if (this.singleInstance != null)
                    {
                        return this.singleInstance;
                    }

                    this.singleInstance = CreateInstance();

                    return this.singleInstance;
                }
            }

            return CreateInstance();
        }
    }
}
