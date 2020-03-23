using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using ECode.Core;

namespace ECode.TypeConversion
{
    /// <summary>
    /// Utility methods that are used to convert objects from one type into another.
    /// </summary>
    public static class TypeConversionUtil
    {
        /// <summary>
        /// Convert the value to the required <see cref="System.Type"/> (if necessary from a string).
        /// </summary>
        /// <param name="sourceValue">The proposed change value.</param>
        /// <param name="requiredType">
        /// The <see cref="System.Type"/> we must convert to.
        /// </param>
        /// <returns>The new value, possibly the result of type conversion.</returns>
        public static object ConvertValueIfNecessary(Type requiredType, object sourceValue)
        {
            if (sourceValue == null)
            { return null; }

            // if it is assignable, return the value right away
            if (IsAssignableFrom(sourceValue, requiredType))
            { return sourceValue; }

            // if required type is an array, convert all the elements
            if (requiredType != null && requiredType.IsArray)
            {
                // convert individual elements to array elements
                var componentType = requiredType.GetElementType();
                if (sourceValue is ICollection)
                {
                    var elements = (ICollection)sourceValue;
                    return ToArrayWithTypeConversion(componentType, elements);
                }
                else if (sourceValue is string)
                {
                    if (requiredType.Equals(typeof(char[])))
                    {
                        return ((string)sourceValue).ToCharArray();
                    }
                    else
                    {
                        var elements = ((string)sourceValue).Split(",", false, false, "\"\""); ;
                        return ToArrayWithTypeConversion(componentType, elements);
                    }
                }
                else if (!sourceValue.GetType().IsArray)
                {
                    // A plain value: convert it to an array with a single component.
                    var result = Array.CreateInstance(componentType, 1);
                    var val = ConvertValueIfNecessary(componentType, sourceValue);
                    result.SetValue(val, 0);
                    return result;
                }
            }

            // if required type is some ISet<T>, convert all the elements
            if (requiredType != null && requiredType.GetTypeInfo().IsGenericType && TypeImplementsGenericInterface(requiredType, typeof(ISet<>)))
            {
                // convert individual elements to array elements
                var componentType = requiredType.GetGenericArguments()[0];
                if (sourceValue is ICollection)
                {
                    var elements = (ICollection)sourceValue;
                    return ToTypedCollectionWithTypeConversion(typeof(HashSet<>), componentType, elements);
                }
            }

            // if required type is some IList<T>, convert all the elements
            if (requiredType != null && requiredType.GetTypeInfo().IsGenericType && TypeImplementsGenericInterface(requiredType, typeof(IList<>)))
            {
                // convert individual elements to array elements
                var componentType = requiredType.GetGenericArguments()[0];
                if (sourceValue is ICollection)
                {
                    var elements = (ICollection)sourceValue;
                    return ToTypedCollectionWithTypeConversion(typeof(List<>), componentType, elements);
                }
            }

            // try to convert using type converter
            try
            {
                var typeConverter = TypeConverterRegistry.GetConverter(requiredType);
                if (typeConverter != null && typeConverter.CanConvertFrom(sourceValue.GetType()))
                {
                    try
                    {
                        sourceValue = typeConverter.ConvertFrom(sourceValue);
                    }
                    catch
                    {
                        if (sourceValue is string)
                        {
                            sourceValue = typeConverter.ConvertFromInvariantString((string)sourceValue);
                        }
                    }
                }
                else
                {
                    typeConverter = TypeConverterRegistry.GetConverter(sourceValue.GetType());
                    if (typeConverter != null && typeConverter.CanConvertTo(requiredType))
                    {
                        sourceValue = typeConverter.ConvertTo(sourceValue, requiredType);
                    }
                    else
                    {
                        // look if it's an enum
                        if (requiredType != null
                            && requiredType.GetTypeInfo().IsEnum
                            && (!(sourceValue is float)
                                && (!(sourceValue is double))))
                        {
                            // convert numeric value into enum's underlying type
                            var numericType = Enum.GetUnderlyingType(requiredType);
                            sourceValue = Convert.ChangeType(sourceValue, numericType);

                            if (Enum.IsDefined(requiredType, sourceValue))
                            {
                                sourceValue = Enum.ToObject(requiredType, sourceValue);
                            }
                            else
                            {
                                throw new TypeConvertException(sourceValue, requiredType);
                            }
                        }
                        else if (sourceValue is IConvertible)
                        {
                            // last resort - try ChangeType
                            sourceValue = Convert.ChangeType(sourceValue, requiredType);
                        }
                        else
                        {
                            throw new TypeConvertException(sourceValue, requiredType);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new TypeConvertException(sourceValue, requiredType, ex);
            }

            if (sourceValue == null
                && (requiredType == null
                    || !Type.GetType("System.Nullable`1").Equals(requiredType.GetGenericTypeDefinition())))
            {
                throw new TypeConvertException(sourceValue, requiredType);
            }

            return sourceValue;
        }

        private static object ToArrayWithTypeConversion(Type componentType, ICollection elements)
        {
            var destination = Array.CreateInstance(componentType, elements.Count);

            int i = 0;
            foreach (object element in elements)
            {
                object value = ConvertValueIfNecessary(componentType, element);
                destination.SetValue(value, i);
                i++;
            }

            return destination;
        }

        private static object ToTypedCollectionWithTypeConversion(Type targetCollectionType, Type componentType, ICollection elements)
        {
            if (!TypeImplementsGenericInterface(targetCollectionType, typeof(ICollection<>)))
            {
                throw new ArgumentException($"Argument '{nameof(targetCollectionType)}' must be a type that derives from ICollection<T>.");
            }


            var collectionType = targetCollectionType.MakeGenericType(new Type[] { componentType });

            var typedCollection = Activator.CreateInstance(collectionType);

            foreach (object element in elements)
            {
                var value = ConvertValueIfNecessary(componentType, element);
                collectionType.GetMethod("Add").Invoke(typedCollection, new object[] { value });
            }

            return typedCollection;
        }

        private static bool IsAssignableFrom(object sourceValue, Type requiredType)
        {
            if (requiredType == null)
            {
                return false;
            }

            return requiredType.IsAssignableFrom(sourceValue.GetType());
        }

        /// <summary>
        /// Determines if a Type implements a specific generic interface.
        /// </summary>
        /// <param name="candidateType">Candidate <see lang="Type"/> to evaluate.</param>
        /// <param name="matchingInterface">The <see lang="interface"/> to test for in the Candidate <see lang="Type"/>.</param>
        /// <returns><see lang="true" /> if a match, else <see lang="false"/></returns>
        private static bool TypeImplementsGenericInterface(Type candidateType, Type matchingInterface)
        {
            if (!matchingInterface.GetTypeInfo().IsInterface)
            {
                throw new ArgumentException($"Type '{matchingInterface}' must be an Interface Type.");
            }

            foreach (var interfaceType in candidateType.GetInterfaces())
            {
                if (false == interfaceType.GetTypeInfo().IsGenericType)
                {
                    continue;
                }

                var genericType = interfaceType.GetGenericTypeDefinition();
                if (genericType == matchingInterface)
                {
                    return true;
                }
            }

            return false;
        }
    }
}