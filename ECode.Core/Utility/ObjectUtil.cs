using System;
using System.Reflection;
using ECode.Core;

namespace ECode.Utility
{
    public static class ObjectUtil
    {
        /// <summary>
        /// Convenience method to instantiate a <see cref="System.Type"/> using
        /// its no-arg constructor.
        /// </summary>
        /// <remarks>
        /// <p>
        /// As this method doesn't try to instantiate <see cref="System.Type"/>s
        /// by name, it should avoid <see cref="System.Type"/> loading issues.
        /// </p>
        /// </remarks>
        /// <param name="type">
        /// The <see cref="System.Type"/> to instantiate.
        /// </param>
        /// <returns>A new instance of the <see cref="System.Type"/>.</returns>
        /// <exception cref="System.ArgumentNullException"> 
        /// If the <paramref name="type"/> is <see langword="null"/>
        /// </exception>
        /// <exception cref="ECode.Core.ReflectionException">
        /// If the <paramref name="type"/> is an abstract class, an interface, 
        /// an open generic type or does not have a public no-argument constructor.
        /// </exception>
        public static object InstantiateType(Type type)
        {
            AssertUtil.ArgumentNotNull(type, nameof(type));

            var constructor = GetZeroArgConstructorInfo(type);
            if (constructor == null)
            {
                throw new ReflectionException($"Cannot instantiate a class that does not have a public no-argument constructor '{type}'.");
            }

            return InstantiateType(constructor, new object[] { });
        }

        /// <summary>
        /// Instantiates the type using the assembly specified to load the type.
        /// </summary>
        /// <remarks>This is a convenience in the case of needing to instantiate a type but not
        /// wanting to specify in the string the version, culture and public key token.</remarks>
        /// <param name="assembly">The assembly.</param>
        /// <param name="typeName">Name of the type.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"> 
        /// If the <paramref name="assembly"/> or <paramref name="typeName"/> is <see langword="null"/>
        /// </exception>
        /// <exception cref="ECode.Core.ReflectionException">
        /// If cannot load the type from the assembly or the call to <code>InstantiateType(Type)</code> fails.
        /// </exception>
        public static object InstantiateType(Assembly assembly, string typeName)
        {
            AssertUtil.ArgumentNotNull(assembly, nameof(assembly));
            AssertUtil.ArgumentNotEmpty(typeName, nameof(typeName));

            var resolvedType = assembly.GetType(typeName, false, false);
            if (resolvedType == null)
            {
                throw new ReflectionException($"Cannot load type '{typeName}' from assembly '{assembly}'.");
            }

            return InstantiateType(resolvedType);
        }

        /// <summary>
        /// Convenience method to instantiate a <see cref="System.Type"/> using
        /// the given constructor.
        /// </summary>
        /// <remarks>
        /// <p>
        /// As this method doesn't try to instantiate <see cref="System.Type"/>s
        /// by name, it should avoid <see cref="System.Type"/> loading issues.
        /// </p>
        /// </remarks>
        /// <param name="constructor">
        /// The constructor to use for the instantiation.
        /// </param>
        /// <param name="arguments">
        /// The arguments to be passed to the constructor.
        /// </param>
        /// <returns>A new instance.</returns>
        /// <exception cref="System.ArgumentNullException"> 
        /// If the <paramref name="constructor"/> is <see langword="null"/>
        /// </exception>
        /// <exception cref="ECode.Core.ReflectionException">
        /// If the <paramref name="constructor"/>'s declaring type is an abstract class, 
        /// an interface, an open generic type or does not have a public no-argument constructor.
        /// </exception>
        public static object InstantiateType(ConstructorInfo constructor, object[] arguments)
        {
            AssertUtil.ArgumentNotNull(constructor, nameof(constructor));

            if (constructor.DeclaringType.IsInterface)
            {
                throw new ReflectionException($"Cannot instantiate an interface '{constructor.DeclaringType}'.");
            }

            if (constructor.DeclaringType.IsAbstract)
            {
                throw new ReflectionException($"Cannot instantiate an abstract class '{constructor.DeclaringType}'.");
            }

            if (constructor.DeclaringType.ContainsGenericParameters)
            {
                throw new ReflectionException($"Cannot instantiate an open generic type '{constructor.DeclaringType}'.");
            }

            try
            {
                return constructor.Invoke(arguments);
            }
            catch (Exception ex)
            {
                var ctorType = constructor.DeclaringType;
                throw new ReflectionException(
                    $"Cannot instantiate type '{constructor.DeclaringType}' with ctor '{constructor}'.",
                    ex);
            }
        }

        /// <summary>
        /// Gets the zero arg ConstructorInfo object, if the type offers such functionality.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>Zero argument ConstructorInfo</returns>
        /// <exception cref="ECode.Core.ReflectionException">
        /// If the type is an interface, abstract, open generic type.
        /// </exception>
        public static ConstructorInfo GetZeroArgConstructorInfo(Type type)
        {
            IsInstantiable(type);

            return type.GetConstructor(Type.EmptyTypes);
        }

        /// <summary>
        /// Determines whether the specified type is instantiable, i.e. not an interface, abstract class or contains
        /// open generic type parameters.
        /// </summary>
        /// <param name="type">The type.</param>
        public static void IsInstantiable(Type type)
        {
            if (type.IsInterface)
            {
                throw new ReflectionException($"Cannot instantiate an interface '{type}'.");
            }

            if (type.IsAbstract)
            {
                throw new ReflectionException($"Cannot instantiate an abstract class '{type}'.");
            }

            if (type.ContainsGenericParameters)
            {
                throw new ReflectionException($"Cannot instantiate an open generic type '{type}'.");
            }
        }

        /// <summary>
        /// Determine if the given <see cref="System.Type"/> is assignable from the
        /// given value, assuming setting by reflection and taking care of transparent proxies.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Considers primitive wrapper classes as assignable to the
        /// corresponding primitive types.
        /// </p>
        /// <p>
        /// For example used in an object factory's constructor resolution.
        /// </p>
        /// </remarks>
        /// <param name="type">The target <see cref="System.Type"/>.</param>
        /// <param name="obj">The value that should be assigned to the type.</param>
        /// <returns>True if the type is assignable from the value.</returns>
        public static bool IsAssignable(Type type, object obj)
        {
            AssertUtil.ArgumentNotNull(type, nameof(type));

            if (!type.IsPrimitive && obj == null)
            {
                return true;
            }

            return (type.IsInstanceOfType(obj) ||
                    (type.Equals(typeof(bool)) && obj is Boolean) ||
                    (type.Equals(typeof(byte)) && obj is Byte) ||
                    (type.Equals(typeof(sbyte)) && obj is SByte) ||
                    (type.Equals(typeof(char)) && obj is Char) ||
                    (type.Equals(typeof(short)) && obj is Int16) ||
                    (type.Equals(typeof(int)) && obj is Int32) ||
                    (type.Equals(typeof(long)) && obj is Int64) ||
                    (type.Equals(typeof(float)) && obj is Single) ||
                    (type.Equals(typeof(double)) && obj is Double));
        }

        /// <summary>
        /// Check if the given <see cref="System.Type"/> represents a "simple" property,
        /// i.e. a primitive, a <see cref="System.String"/>, a <see cref="System.Type"/>, or a corresponding array.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Used to determine properties to check for a "simple" dependency-check.
        /// </p>
        /// </remarks>
        /// <param name="type">
        /// The <see cref="System.Type"/> to check.
        /// </param>
        public static bool IsSimpleProperty(Type type)
        {
            return type.IsPrimitive
                   || type.Equals(typeof(string))
                   || type.Equals(typeof(string[]))
                   || IsPrimitiveArray(type)
                   || type.Equals(typeof(Type))
                   || type.Equals(typeof(Type[]));
        }

        /// <summary>
        /// Check if the given class represents a primitive array,
        /// i.e. boolean, byte, char, short, int, long, float, or double.
        /// </summary>
        public static bool IsPrimitiveArray(Type type)
        {
            return typeof(bool[]).Equals(type)
                   || typeof(sbyte[]).Equals(type)
                   || typeof(char[]).Equals(type)
                   || typeof(short[]).Equals(type)
                   || typeof(int[]).Equals(type)
                   || typeof(long[]).Equals(type)
                   || typeof(float[]).Equals(type)
                   || typeof(double[]).Equals(type);
        }

        /// <summary>
        /// Determine if the given objects are equal, returning <see langword="true"/>
        /// if both are <see langword="null"/> respectively <see langword="false"/>
        /// if only one is <see langword="null"/>.
        /// </summary>
        /// <param name="o1">The first object to compare.</param>
        /// <param name="o2">The second object to compare.</param>
        /// <returns>
        /// <see langword="true"/> if the given objects are equal.
        /// </returns>
        public static bool NullSafeEquals(object o1, object o2)
        {
            return (o1 == o2 || (o1 != null && o1.Equals(o2)));
        }

        /// <summary>
        /// Gets the qualified name of the given method, consisting of 
        /// fully qualified interface/class name + "." method name.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns>qualified name of the method.</returns>
        public static string GetQualifiedMethodName(MethodInfo method)
        {
            AssertUtil.ArgumentNotNull(method, nameof(method));

            return method.DeclaringType.FullName + "." + method.Name;
        }
    }
}
