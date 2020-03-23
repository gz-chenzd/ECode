using System;
using System.Collections.Generic;
using System.Reflection;
using ECode.TypeResolution;

namespace ECode.DependencyInjection
{
    class DictionaryDefinition : DefinitionBase
    {
        private Type            resolvedKeyType     = null;
        private Type            resolvedValueType   = null;
        private MethodInfo      addEntryMethod      = null;


        public string KeyType
        { get; set; }

        public string ValueType
        { get; set; }

        public Dictionary<DefinitionBase, DefinitionBase> Entries
        { get; set; } = new Dictionary<DefinitionBase, DefinitionBase>();


        public override void Validate()
        {
            if (this.ResolvedType != null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(this.KeyType))
            {
                this.resolvedKeyType = TypeResolutionUtil.ResolveType(this.KeyType);
                foreach (var keyDefinition in this.Entries.Keys)
                {
                    if (!keyDefinition.CanConvertTo(this.resolvedKeyType))
                    {
                        if (keyDefinition.ResolvedType != null)
                        {
                            throw new InvalidCastException($"Type '{keyDefinition.ResolvedType.FullName}' cannot convert to target type '{this.resolvedKeyType.FullName}'.");
                        }
                        else
                        {
                            throw new InvalidCastException($"Value '{keyDefinition.GetValue()}' cannot convert to target type '{this.resolvedKeyType.FullName}'.");
                        }
                    }
                }
            }
            else
            {
                Type possibleKeyType = null;  // string or other ref object type
                foreach (var keyDefinition in this.Entries.Keys)
                {
                    if (keyDefinition.ResolvedType == null)
                    {
                        if (possibleKeyType == null)
                        {
                            possibleKeyType = typeof(string);
                        }
                        else
                        {
                            if (possibleKeyType == typeof(string))
                            {
                                continue;
                            }

                            if (possibleKeyType.IsAssignableFrom(typeof(string)))
                            {
                                // use current possible type.
                            }
                            else if (typeof(string).IsAssignableFrom(possibleKeyType))
                            {
                                possibleKeyType = typeof(string);
                            }
                            else
                            {
                                possibleKeyType = typeof(object); // final type.
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (possibleKeyType == null)
                        {
                            possibleKeyType = keyDefinition.ResolvedType;
                        }
                        else
                        {
                            if (possibleKeyType.IsAssignableFrom(keyDefinition.ResolvedType))
                            {
                                // use current possible type.
                            }
                            else if (keyDefinition.ResolvedType.IsAssignableFrom(possibleKeyType))
                            {
                                possibleKeyType = keyDefinition.ResolvedType;
                            }
                            else
                            {
                                possibleKeyType = typeof(object); // final type.
                                break;
                            }
                        }
                    }
                }

                this.resolvedKeyType = possibleKeyType ?? typeof(object);
            }

            if (!string.IsNullOrWhiteSpace(this.ValueType))
            {
                this.resolvedValueType = TypeResolutionUtil.ResolveType(this.ValueType);
                foreach (var valueDefinition in this.Entries.Values)
                {
                    if (!valueDefinition.CanConvertTo(this.resolvedValueType))
                    {
                        if (valueDefinition.ResolvedType != null)
                        {
                            throw new InvalidCastException($"Type '{valueDefinition.ResolvedType.FullName}' cannot convert to target type '{this.resolvedValueType.FullName}'.");
                        }
                        else
                        {
                            throw new InvalidCastException($"Value '{valueDefinition.GetValue()}' cannot convert to target type '{this.resolvedValueType.FullName}'.");
                        }
                    }
                }
            }
            else
            {
                bool containsNullValue = false;
                Type possibleValueType = null;  // string or other ref object type
                foreach (var valueDefinition in this.Entries.Values)
                {
                    if (valueDefinition.ResolvedType == null)
                    {
                        if (valueDefinition == ValueDefinition.NULL)
                        {
                            containsNullValue = true;
                            if (possibleValueType == null)
                            {
                                continue;
                            }
                        }

                        if (possibleValueType == null)
                        {
                            possibleValueType = typeof(string);
                        }
                        else
                        {
                            if (possibleValueType == typeof(string))
                            {
                                continue;
                            }

                            if (possibleValueType.IsAssignableFrom(typeof(string)))
                            {
                                // use current possible type.
                            }
                            else if (typeof(string).IsAssignableFrom(possibleValueType))
                            {
                                possibleValueType = typeof(string);
                            }
                            else
                            {
                                possibleValueType = typeof(object); // final type.
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (possibleValueType == null)
                        {
                            possibleValueType = valueDefinition.ResolvedType;
                        }
                        else
                        {
                            if (possibleValueType.IsAssignableFrom(valueDefinition.ResolvedType))
                            {
                                // use current possible type.
                            }
                            else if (valueDefinition.ResolvedType.IsAssignableFrom(possibleValueType))
                            {
                                possibleValueType = valueDefinition.ResolvedType;
                            }
                            else
                            {
                                possibleValueType = typeof(object); // final type.
                                break;
                            }
                        }
                    }
                }

                if (possibleValueType == null)
                {
                    possibleValueType = typeof(object);
                }
                else if (containsNullValue == true)
                {
                    if (possibleValueType.IsPrimitive)
                    {
                        possibleValueType = typeof(Nullable<>).MakeGenericType(possibleValueType);
                    }
                    else if (possibleValueType.IsValueType)
                    {
                        if (possibleValueType.IsGenericType && possibleValueType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            // use current possible type.
                        }
                        else
                        {
                            possibleValueType = typeof(Nullable<>).MakeGenericType(possibleValueType);
                        }
                    }
                }

                this.resolvedValueType = possibleValueType;
            }


            this.ResolvedType = typeof(Dictionary<,>).MakeGenericType(this.resolvedKeyType, this.resolvedValueType);
            this.addEntryMethod = this.ResolvedType.GetMethod("Add", BindingFlags.Instance | BindingFlags.Public);
        }

        public override object GetValue()
        {
            var dict = Activator.CreateInstance(this.ResolvedType);
            foreach (var pair in this.Entries)
            {
                var key = pair.Key.GetValue();
                var val = pair.Value.GetValue();

                this.addEntryMethod.Invoke(dict, new[] { key, val });
            }

            return dict;
        }
    }
}
