using System;
using System.Collections.Generic;
using System.Reflection;
using ECode.TypeConversion;
using ECode.TypeResolution;

namespace ECode.DependencyInjection
{
    class ListDefinition : DefinitionBase
    {
        private Type            resolvedElementType     = null;
        private MethodInfo      addItemMethod           = null;


        public string ElementType
        { get; set; }

        public List<DefinitionBase> Items
        { get; set; } = new List<DefinitionBase>();


        public override bool CanConvertTo(Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException(nameof(destinationType));
            }

            if (this.ResolvedType != null && destinationType.IsAssignableFrom(this.ResolvedType))
            {
                return true;
            }

            Type srcElementType = typeof(string);
            if (this.resolvedElementType != null)
            {
                srcElementType = this.resolvedElementType;
            }

            if (destinationType.IsArray)
            {
                var dstElementType = destinationType.GetElementType();
                return dstElementType.IsAssignableFrom(srcElementType);
            }
            else if (destinationType.IsGenericType && TypeImplementsGenericInterface(destinationType, typeof(IList<>)))
            {
                var dstElementType = destinationType.GetGenericArguments()[0];
                return dstElementType.IsAssignableFrom(srcElementType);
            }
            else if (destinationType.IsGenericType && TypeImplementsGenericInterface(destinationType, typeof(HashSet<>)))
            {
                var dstElementType = destinationType.GetGenericArguments()[0];
                return dstElementType.IsAssignableFrom(srcElementType);
            }
            else
            {
                return false;
            }
        }

        private bool TypeImplementsGenericInterface(Type candidateType, Type matchingInterface)
        {
            if (!matchingInterface.IsInterface)
            {
                throw new ArgumentException($"Type '{matchingInterface}' must be an Interface Type.");
            }

            foreach (var interfaceType in candidateType.GetInterfaces())
            {
                if (false == interfaceType.IsGenericType)
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


        public override void Validate()
        {
            if (this.ResolvedType != null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(this.ElementType))
            {
                this.resolvedElementType = TypeResolutionUtil.ResolveType(this.ElementType);
                foreach (var itemDefinition in this.Items)
                {
                    if (!itemDefinition.CanConvertTo(this.resolvedElementType))
                    {
                        if (itemDefinition.ResolvedType != null)
                        {
                            throw new InvalidCastException($"Type '{itemDefinition.ResolvedType.FullName}' cannot convert to target type '{this.resolvedElementType.FullName}'.");
                        }
                        else
                        {
                            throw new InvalidCastException($"Value '{itemDefinition.GetValue()}' cannot convert to target type '{this.resolvedElementType.FullName}'.");
                        }
                    }
                }
            }
            else
            {
                bool containsNullValue = false;
                Type possibleElementType = null;  // string or other ref object type
                foreach (var itemDefinition in this.Items)
                {
                    if (itemDefinition.ResolvedType == null)
                    {
                        if (itemDefinition == ValueDefinition.NULL)
                        {
                            containsNullValue = true;
                            if (possibleElementType == null)
                            {
                                continue;
                            }
                        }

                        if (possibleElementType == null)
                        {
                            possibleElementType = typeof(string);
                        }
                        else
                        {
                            if (possibleElementType == typeof(string))
                            {
                                continue;
                            }

                            if (possibleElementType.IsAssignableFrom(typeof(string)))
                            {
                                // use current possible type.
                            }
                            else if (typeof(string).IsAssignableFrom(possibleElementType))
                            {
                                possibleElementType = typeof(string);
                            }
                            else
                            {
                                possibleElementType = typeof(object); // final type.
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (possibleElementType == null)
                        {
                            possibleElementType = itemDefinition.ResolvedType;
                        }
                        else
                        {
                            if (possibleElementType.IsAssignableFrom(itemDefinition.ResolvedType))
                            {
                                // use current possible type.
                            }
                            else if (itemDefinition.ResolvedType.IsAssignableFrom(possibleElementType))
                            {
                                possibleElementType = itemDefinition.ResolvedType;
                            }
                            else
                            {
                                possibleElementType = typeof(object); // final type.
                                break;
                            }
                        }
                    }
                }

                if (possibleElementType == null)
                {
                    possibleElementType = typeof(object);
                }
                else if (containsNullValue == true)
                {
                    if (possibleElementType.IsPrimitive)
                    {
                        possibleElementType = typeof(Nullable<>).MakeGenericType(possibleElementType);
                    }
                    else if (possibleElementType.IsValueType)
                    {
                        if (possibleElementType.IsGenericType && possibleElementType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            // use current possible type.
                        }
                        else
                        {
                            possibleElementType = typeof(Nullable<>).MakeGenericType(possibleElementType);
                        }
                    }
                }

                this.resolvedElementType = possibleElementType;
            }


            this.ResolvedType = typeof(List<>).MakeGenericType(this.resolvedElementType);
            this.addItemMethod = this.ResolvedType.GetMethod("Add", BindingFlags.Instance | BindingFlags.Public);
        }

        public override object GetValue()
        {
            var list = Activator.CreateInstance(this.ResolvedType);
            foreach (var item in this.Items)
            {
                var val = TypeConversionUtil.ConvertValueIfNecessary(this.resolvedElementType, item.GetValue());

                this.addItemMethod.Invoke(list, new[] { val });
            }

            return list;
        }
    }
}
