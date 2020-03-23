using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using ECode.Core;

namespace ECode.DependencyInjection
{
    class XmlConfigParser
    {
        #region Xml Tags and Attrs

        const string ROOT_TAG               = "objects";

        const string OBJECT_TAG             = "object";
        const string ALIAS_TAG              = "alias";
        const string CONSTRUCTOR_ARG_TAG    = "constructor-arg";
        const string PROPERTY_TAG           = "property";
        const string LISTENER_TAG           = "listener";
        const string LIST_TAG               = "list";
        const string SET_TAG                = "set";
        const string DICTIONARY_TAG         = "dictionary";
        const string NAME_VALUES_TAG        = "name-values";
        const string ADD_TAG                = "add";
        const string VALUE_TAG              = "value";
        const string ENTRY_TAG              = "entry";
        const string REF_TAG                = "ref";
        const string NULL_TAG               = "null";

        const string ID_ATTR                = "id";
        const string TYPE_ATTR              = "type";
        const string INDEX_ATTR             = "index";
        const string NAME_ATTR              = "name";
        const string KEY_ATTR               = "key";
        const string VALUE_ATTR             = "value";
        const string REF_ATTR               = "ref";
        const string OBJECT_ATTR            = "object";
        const string SINGLETON_ATTR         = "singleton";
        const string LAZY_INIT_ATTR         = "lazy-init";
        const string INIT_METHOD_ATTR       = "init-method";
        const string DESTROY_METHOD_ATTR    = "destroy-method";

        const string FACTORY_OBJECT_ATTR    = "factory-object";
        const string FACTORY_METHOD_ATTR    = "factory-method";

        const string KEY_TYPE_ATTR          = "key-type";
        const string VALUE_TYPE_ATTR        = "value-type";
        const string ELEMENT_TYPE_ATTR      = "element-type";
        const string KEY_REF_ATTR           = "key-ref";
        const string VALUE_REF_ATTR         = "value-ref";

        const string EVENT_ATTR             = "event";
        const string METHOD_ATTR            = "method";

        #endregion

        #region Parse Methods

        private XmlDocument Document
        { get; set; }

        private Dictionary<string, XmlElement> Elements
        { get; set; } = new Dictionary<string, XmlElement>();

        private Dictionary<string, DefinitionBase> Definitions
        { get; set; } = new Dictionary<string, DefinitionBase>();


        private XmlConfigParser(string xml)
        {
            this.Document = new XmlDocument();
            this.Document.XmlResolver = null;

            this.Document.LoadXml(xml);
        }

        private XmlConfigParser(Stream stream)
        {
            this.Document = new XmlDocument();
            this.Document.XmlResolver = null;

            this.Document.Load(stream);
        }

        private XmlConfigParser(TextReader reader)
        {
            this.Document = new XmlDocument();
            this.Document.XmlResolver = null;

            this.Document.Load(reader);
        }


        private void Parse()
        {
            var root = this.Document.DocumentElement;
            if (root.LocalName != ROOT_TAG)
            { throw new ConfigurationException($"Root element is not '{ROOT_TAG}'."); }

            foreach (XmlNode node in root.ChildNodes)
            {
                if (node.NodeType == XmlNodeType.Element)
                {
                    var element = (XmlElement)node;
                    if (element.LocalName == OBJECT_TAG)
                    {
                        if (!element.HasAttribute(ID_ATTR) || string.IsNullOrWhiteSpace(element.GetAttribute(ID_ATTR)))
                        {
                            throw new ConfigurationException("Attribute 'id' is required.", element.OuterXml);
                        }

                        var objectId = element.GetAttribute(ID_ATTR);
                        if (Elements.ContainsKey(objectId))
                        {
                            throw new ConfigurationException($"Object '{objectId}' duplicate with other object or alias.", element.OuterXml);
                        }

                        Elements[objectId] = element;
                    }
                    else if (element.LocalName == ALIAS_TAG)
                    {
                        if (!element.HasAttribute(NAME_ATTR) || string.IsNullOrWhiteSpace(element.GetAttribute(NAME_ATTR)))
                        {
                            throw new ConfigurationException("Attribute 'name' is required.", element.OuterXml);
                        }

                        var aliasName = element.GetAttribute(NAME_ATTR);
                        if (Elements.ContainsKey(aliasName))
                        {
                            throw new ConfigurationException($"Alias '{aliasName}' duplicate with other object or alias.", element.OuterXml);
                        }

                        Elements[aliasName] = element;
                    }
                    else
                    {
                        throw new ConfigurationException($"Unsupported element '{element.LocalName}'.", element.OuterXml);
                    }
                }
            }

            foreach (var element in Elements.Values)
            {
                if (element.LocalName == OBJECT_TAG)
                {
                    var definition = ParseObject(element);
                    Definitions[definition.ID] = definition;
                }
                else
                {
                    var definition = ParseAlias(element);
                    Definitions[definition.Name] = definition;
                }
            }
        }

        private ValueDefinition ParseValue(XmlElement element)
        {
            var definition = new ValueDefinition();
            if (element.HasAttribute(TYPE_ATTR))
            {
                definition.Type = element.GetAttribute(TYPE_ATTR);
                if (string.IsNullOrWhiteSpace(definition.Type))
                {
                    throw new ConfigurationException("Attribute 'type' is required.", element.OuterXml);
                }
            }

            definition.Value = element.InnerText;
            definition.Validate();
            return definition;
        }

        private ArgumentDefinition ParseArgument(XmlElement element)
        {
            if (element.HasAttribute(INDEX_ATTR) && element.HasAttribute(NAME_ATTR))
            {
                throw new ConfigurationException("Attribute 'index' and 'name' cannot be assigned at the same time.", element.OuterXml);
            }

            if (element.HasAttribute(REF_ATTR) && element.HasAttribute(VALUE_ATTR))
            {
                throw new ConfigurationException("Attribute 'ref' and 'value' cannot be assigned at the same time.", element.OuterXml);
            }

            if (element.HasAttribute(REF_ATTR) && element.HasChildNodes)
            {
                throw new ConfigurationException("Attribute 'ref' and child node cannot be assigned at the same time.", element.OuterXml);
            }

            if (element.HasAttribute(VALUE_ATTR) && element.HasChildNodes)
            {
                throw new ConfigurationException("Attribute 'value' and child node cannot be assigned at the same time.", element.OuterXml);
            }

            if (element.HasChildNodes && element.ChildNodes.Count > 1)
            {
                throw new ConfigurationException("Only one child node is allowed.", element.OuterXml);
            }


            var definition = new ArgumentDefinition();
            if (element.HasAttribute(INDEX_ATTR))
            {
                definition.Index = int.Parse(element.GetAttribute(INDEX_ATTR));
                if (definition.Index < 0)
                {
                    throw new ConfigurationException("Attribute 'index' value must be >= 0.", element.OuterXml);
                }
            }
            else if (element.HasAttribute(NAME_ATTR))
            {
                definition.Name = element.GetAttribute(NAME_ATTR);
                if (string.IsNullOrWhiteSpace(definition.Name))
                {
                    throw new ConfigurationException("Attribute 'name' is required.", element.OuterXml);
                }
            }

            if (element.HasAttribute(TYPE_ATTR))
            {
                definition.Type = element.GetAttribute(TYPE_ATTR);
                if (string.IsNullOrWhiteSpace(definition.Type))
                {
                    throw new ConfigurationException("Attribute 'type' is required.", element.OuterXml);
                }
            }

            if (element.HasAttribute(REF_ATTR))
            {
                var objectId = element.GetAttribute(REF_ATTR);
                if (string.IsNullOrWhiteSpace(objectId))
                {
                    throw new ConfigurationException("Attribute 'ref' is required.", element.OuterXml);
                }

                definition.ValueDefinition = FindReferenceObject(objectId);
            }
            else if (element.HasAttribute(VALUE_ATTR))
            {
                definition.ValueDefinition = new ValueDefinition()
                {
                    Type = definition.Type,
                    Value = element.GetAttribute(VALUE_ATTR)
                };
                definition.ValueDefinition.Validate();
            }
            else if (element.HasChildNodes)
            {
                definition.ValueDefinition = ParseChildNode((XmlElement)element.FirstChild);
            }
            else
            {
                definition.ValueDefinition = ValueDefinition.NULL;
            }

            definition.Validate();
            return definition;
        }

        private PropertyDefinition ParseProperty(XmlElement element)
        {
            if (!element.HasAttribute(NAME_ATTR))
            {
                throw new ConfigurationException("Attribute 'name' is required.", element.OuterXml);
            }

            if (element.HasAttribute(REF_ATTR) && element.HasAttribute(VALUE_ATTR))
            {
                throw new ConfigurationException("Attribute 'ref' and 'value' cannot be assigned at the same time.", element.OuterXml);
            }

            if (element.HasAttribute(REF_ATTR) && element.HasChildNodes)
            {
                throw new ConfigurationException("Attribute 'ref' and child node cannot be assigned at the same time.", element.OuterXml);
            }

            if (element.HasAttribute(VALUE_ATTR) && element.HasChildNodes)
            {
                throw new ConfigurationException("Attribute 'value' and child node cannot be assigned at the same time.", element.OuterXml);
            }

            if (element.HasChildNodes && element.ChildNodes.Count > 1)
            {
                throw new ConfigurationException($"Only one child node is allowed.", element.OuterXml);
            }


            var definition = new PropertyDefinition();
            definition.Name = element.GetAttribute(NAME_ATTR);
            if (string.IsNullOrWhiteSpace(definition.Name))
            {
                throw new ConfigurationException("Attribute 'name' is required.", element.OuterXml);
            }

            if (element.HasAttribute(REF_ATTR))
            {
                var objectId = element.GetAttribute(REF_ATTR);
                if (string.IsNullOrWhiteSpace(objectId))
                {
                    throw new ConfigurationException("Attribute 'ref' is required.", element.OuterXml);
                }

                definition.ValueDefinition = FindReferenceObject(objectId);
            }
            else if (element.HasAttribute(VALUE_ATTR))
            {
                definition.ValueDefinition = new ValueDefinition()
                {
                    Value = element.GetAttribute(VALUE_ATTR)
                };
                definition.ValueDefinition.Validate();
            }
            else if (element.HasChildNodes)
            {
                definition.ValueDefinition = ParseChildNode((XmlElement)element.FirstChild);
            }
            else
            {
                definition.ValueDefinition = ValueDefinition.NULL;
            }

            definition.Validate();
            return definition;
        }

        private ListenerDefinition ParseListener(XmlElement element)
        {
            if (!element.HasAttribute(EVENT_ATTR))
            {
                throw new ConfigurationException("Attribute 'event' is required.", element.OuterXml);
            }

            if (!element.HasAttribute(METHOD_ATTR))
            {
                throw new ConfigurationException("Attribute 'method' is required.", element.OuterXml);
            }

            if (!element.HasChildNodes)
            {
                throw new ConfigurationException($"Child node 'ref' is required.", element.OuterXml);
            }

            if (element.ChildNodes.Count > 1)
            {
                throw new ConfigurationException("Only one child node is allowed.", element.OuterXml);
            }


            var definition = new ListenerDefinition();
            definition.Event = element.GetAttribute(EVENT_ATTR);
            if (string.IsNullOrWhiteSpace(definition.Event))
            {
                throw new ConfigurationException("Attribute 'event' is required.", element.OuterXml);
            }

            definition.Method = element.GetAttribute(METHOD_ATTR);
            if (string.IsNullOrWhiteSpace(definition.Method))
            {
                throw new ConfigurationException("Attribute 'method' is required.", element.OuterXml);
            }

            var refElement = (XmlElement)element.FirstChild;
            if (refElement.LocalName != REF_TAG)
            {
                throw new ConfigurationException($"Unsupported element '{refElement.LocalName}'.", element.OuterXml);
            }

            if (refElement.HasAttribute(TYPE_ATTR) && refElement.HasAttribute(OBJECT_ATTR))
            {
                throw new ConfigurationException("Attribute 'type' and 'object' cannot be assigned at the same time.", element.OuterXml);
            }

            if (refElement.HasAttribute(TYPE_ATTR))
            {
                var type = refElement.GetAttribute(TYPE_ATTR);
                if (string.IsNullOrWhiteSpace(type))
                {
                    throw new ConfigurationException("Attribute 'type' is required.", element.OuterXml);
                }

                definition.RefDefinition = new TypeDefinition() { Type = type };
                definition.RefDefinition.Validate();
            }
            else if (refElement.HasAttribute(OBJECT_ATTR))
            {
                var objectId = refElement.GetAttribute(OBJECT_ATTR);
                if (string.IsNullOrWhiteSpace(objectId))
                {
                    throw new ConfigurationException("Attribute 'object' is required.", element.OuterXml);
                }

                definition.RefDefinition = FindReferenceObject(objectId);
            }

            definition.Validate();
            return definition;
        }

        private ListDefinition ParseList(XmlElement element)
        {
            var definition = new ListDefinition();
            if (element.HasAttribute(ELEMENT_TYPE_ATTR))
            {
                definition.ElementType = element.GetAttribute(ELEMENT_TYPE_ATTR);
                if (string.IsNullOrWhiteSpace(definition.ElementType))
                {
                    throw new ConfigurationException("Attribute 'element-type' is required.", element.OuterXml);
                }
            }

            foreach (XmlNode valueNode in element.ChildNodes)
            {
                if (valueNode.NodeType == XmlNodeType.Element)
                {
                    var valueElement = (XmlElement)valueNode;
                    if (valueElement.LocalName == VALUE_TAG)
                    {
                        definition.Items.Add(ParseValue(valueElement));
                    }
                    else if (valueElement.LocalName == REF_TAG)
                    {
                        var objectId = valueElement.GetAttribute(OBJECT_ATTR);
                        if (string.IsNullOrWhiteSpace(objectId))
                        {
                            throw new ConfigurationException("Attribute 'object' is required.", element.OuterXml);
                        }

                        definition.Items.Add(FindReferenceObject(objectId));
                    }
                    else
                    {
                        throw new ConfigurationException($"Unsupported element '{valueElement.LocalName}'.", element.OuterXml);
                    }
                }
            }

            definition.Validate();
            return definition;
        }

        private SetDefinition ParseSet(XmlElement element)
        {
            var definition = new SetDefinition();
            if (element.HasAttribute(ELEMENT_TYPE_ATTR))
            {
                definition.ElementType = element.GetAttribute(ELEMENT_TYPE_ATTR);
                if (string.IsNullOrWhiteSpace(definition.ElementType))
                {
                    throw new ConfigurationException("Attribute 'element-type' is required.", element.OuterXml);
                }
            }

            foreach (XmlNode valueNode in element.ChildNodes)
            {
                if (valueNode.NodeType == XmlNodeType.Element)
                {
                    var valueElement = (XmlElement)valueNode;
                    if (valueElement.LocalName == VALUE_TAG)
                    {
                        definition.Items.Add(ParseValue(valueElement));
                    }
                    else if (valueElement.LocalName == REF_TAG)
                    {
                        var objectId = valueElement.GetAttribute(OBJECT_ATTR);
                        if (string.IsNullOrWhiteSpace(objectId))
                        {
                            throw new ConfigurationException("Attribute 'object' is required.", element.OuterXml);
                        }

                        definition.Items.Add(FindReferenceObject(objectId));
                    }
                    else
                    {
                        throw new ConfigurationException($"Unsupported element '{valueElement.LocalName}'.", element.OuterXml);
                    }
                }
            }

            definition.Validate();
            return definition;
        }

        private DictionaryDefinition ParseDictionary(XmlElement element)
        {
            var definition = new DictionaryDefinition();
            if (element.HasAttribute(KEY_TYPE_ATTR))
            {
                definition.KeyType = element.GetAttribute(KEY_TYPE_ATTR);
                if (string.IsNullOrWhiteSpace(definition.KeyType))
                {
                    throw new ConfigurationException("Attribute 'key-type' is required.", element.OuterXml);
                }
            }

            if (element.HasAttribute(VALUE_TYPE_ATTR))
            {
                definition.ValueType = element.GetAttribute(VALUE_TYPE_ATTR);
                if (string.IsNullOrWhiteSpace(definition.ValueType))
                {
                    throw new ConfigurationException("Attribute 'value-type' is required.", element.OuterXml);
                }
            }

            foreach (XmlNode entryNode in element.ChildNodes)
            {
                if (entryNode.NodeType == XmlNodeType.Element)
                {
                    var entryElement = (XmlElement)entryNode;
                    if (entryElement.LocalName == ENTRY_TAG)
                    {
                        ParseEntry(entryElement, out DefinitionBase keyDefinition, out DefinitionBase valueDefinition);

                        definition.Entries[keyDefinition] = valueDefinition;
                    }
                    else
                    {
                        throw new ConfigurationException($"Unsupported element '{entryElement.LocalName}'.", element.OuterXml);
                    }
                }
            }

            definition.Validate();
            return definition;
        }

        private void ParseEntry(XmlElement element, out DefinitionBase keyDefinition, out DefinitionBase valueDefinition)
        {
            if (element.HasAttribute(KEY_ATTR) && element.HasAttribute(KEY_REF_ATTR))
            {
                throw new ConfigurationException("Attribute 'key' and 'key-ref' cannot be assigned at the same time.", element.OuterXml);
            }

            if (element.HasAttribute(VALUE_ATTR) && element.HasAttribute(VALUE_REF_ATTR))
            {
                throw new ConfigurationException("Attribute 'value' and 'value-ref' cannot be assigned at the same time.", element.OuterXml);
            }

            if (element.HasAttribute(VALUE_ATTR) && element.HasChildNodes)
            {
                throw new ConfigurationException("Attribute 'value' and child node cannot be assigned at the same time.", element.OuterXml);
            }

            if (element.HasAttribute(VALUE_REF_ATTR) && element.HasChildNodes)
            {
                throw new ConfigurationException("Attribute 'value-ref' and child node cannot be assigned at the same time.", element.OuterXml);
            }

            if (element.HasChildNodes && element.ChildNodes.Count > 1)
            {
                throw new ConfigurationException("Only one child node is allowed.", element.OuterXml);
            }


            if (element.HasAttribute(KEY_ATTR))
            {
                keyDefinition = new ValueDefinition()
                {
                    Value = element.GetAttribute(KEY_ATTR)
                };
                keyDefinition.Validate();
            }
            else if (element.HasAttribute(KEY_REF_ATTR))
            {
                var objectId = element.GetAttribute(KEY_REF_ATTR);
                if (string.IsNullOrWhiteSpace(objectId))
                {
                    throw new ConfigurationException("Attribute 'key-ref' is required.", element.OuterXml);
                }

                keyDefinition = FindReferenceObject(objectId);
            }
            else
            {
                throw new ConfigurationException("Attribute 'key' and 'key-ref' cannot be empty at the same time.", element.OuterXml);
            }

            if (element.HasAttribute(VALUE_ATTR))
            {
                valueDefinition = new ValueDefinition()
                {
                    Value = element.GetAttribute(VALUE_ATTR)
                };
                valueDefinition.Validate();
            }
            else if (element.HasAttribute(VALUE_REF_ATTR))
            {
                var objectId = element.GetAttribute(VALUE_REF_ATTR);
                if (string.IsNullOrWhiteSpace(objectId))
                {
                    throw new ConfigurationException("Attribute 'value-ref' is required.", element.OuterXml);
                }

                valueDefinition = FindReferenceObject(objectId);
            }
            else if (element.HasChildNodes)
            {
                valueDefinition = ParseChildNode((XmlElement)element.FirstChild);
            }
            else
            {
                valueDefinition = ValueDefinition.NULL;
            }
        }

        private NameValuesDefinition ParseNameValues(XmlElement element)
        {
            var definition = new NameValuesDefinition();

            foreach (XmlNode addNode in element.ChildNodes)
            {
                if (addNode.NodeType == XmlNodeType.Element)
                {
                    var addElement = (XmlElement)addNode;
                    if (addElement.LocalName == ADD_TAG)
                    {
                        if (!addElement.HasAttribute(KEY_ATTR))
                        {
                            throw new ConfigurationException("Attribute 'key' is required.", element.OuterXml);
                        }

                        var key = addElement.GetAttribute(KEY_ATTR);
                        if (addElement.HasAttribute(VALUE_ATTR))
                        {
                            definition.NameValues[key] = addElement.GetAttribute(VALUE_ATTR);
                        }
                        else
                        {
                            definition.NameValues[key] = null;
                        }
                    }
                    else
                    {
                        throw new ConfigurationException($"Unsupported element '{addElement.LocalName}'.", element.OuterXml);
                    }
                }
            }

            definition.Validate();
            return definition;
        }

        private AliasDefinition ParseAlias(XmlElement element)
        {
            if (!element.HasAttribute(NAME_ATTR))
            {
                throw new ConfigurationException("Attribute 'name' is required.", element.OuterXml);
            }

            var definition = new AliasDefinition();
            definition.Name = element.GetAttribute(NAME_ATTR);
            if (string.IsNullOrWhiteSpace(definition.Name))
            {
                throw new ConfigurationException("Attribute 'name' is required.", element.OuterXml);
            }

            if (!element.HasAttribute(REF_ATTR))
            {
                throw new ConfigurationException("Attribute 'ref' is required.", element.OuterXml);
            }

            var objectId = element.GetAttribute(REF_ATTR);
            if (string.IsNullOrWhiteSpace(objectId))
            {
                throw new ConfigurationException("Attribute 'ref' is required.", element.OuterXml);
            }

            definition.RefDefinition = FindReferenceObject(objectId);
            definition.Validate();
            return definition;
        }

        private ObjectDefinition ParseObject(XmlElement element)
        {
            var definition = new ObjectDefinition();
            if (element.HasAttribute(ID_ATTR))
            {
                definition.ID = element.GetAttribute(ID_ATTR);
                if (string.IsNullOrWhiteSpace(definition.ID))
                {
                    throw new ConfigurationException("Attribute 'id' is required.", element.OuterXml);
                }
            }

            if (element.HasAttribute(TYPE_ATTR))
            {
                definition.Type = element.GetAttribute(TYPE_ATTR);
                if (string.IsNullOrWhiteSpace(definition.Type))
                {
                    throw new ConfigurationException("Attribute 'type' is required.", element.OuterXml);
                }
            }

            if (element.HasAttribute(SINGLETON_ATTR))
            {
                definition.IsSingleton = bool.Parse(element.GetAttribute(SINGLETON_ATTR));
            }

            if (element.HasAttribute(LAZY_INIT_ATTR))
            {
                definition.IsLazyInit = bool.Parse(element.GetAttribute(LAZY_INIT_ATTR));
            }

            if (element.HasAttribute(INIT_METHOD_ATTR))
            {
                definition.InitMethod = element.GetAttribute(INIT_METHOD_ATTR);
                if (string.IsNullOrWhiteSpace(definition.InitMethod))
                {
                    throw new ConfigurationException("Attribute 'init-method' is required.", element.OuterXml);
                }
            }

            if (element.HasAttribute(DESTROY_METHOD_ATTR))
            {
                definition.DestroyMethod = element.GetAttribute(DESTROY_METHOD_ATTR);
                if (string.IsNullOrWhiteSpace(definition.DestroyMethod))
                {
                    throw new ConfigurationException("Attribute 'destroy-method' is required.", element.OuterXml);
                }
            }

            if (element.HasAttribute(FACTORY_METHOD_ATTR))
            {
                definition.FactoryMethod = element.GetAttribute(FACTORY_METHOD_ATTR);
                if (string.IsNullOrWhiteSpace(definition.FactoryMethod))
                {
                    throw new ConfigurationException("Attribute 'factory-method' is required.", element.OuterXml);
                }
            }

            if (element.HasAttribute(FACTORY_OBJECT_ATTR))
            {
                if (definition.Type != null)
                {
                    throw new ConfigurationException("Attribute 'type' cannot appear with attribute 'factory-object' at the same time.", element.OuterXml);
                }

                if (definition.FactoryMethod == null)
                {
                    throw new ConfigurationException("Attribute 'factory-object' must appear with 'factory-method'.", element.OuterXml);
                }

                var objectId = element.GetAttribute(FACTORY_OBJECT_ATTR);
                if (string.IsNullOrWhiteSpace(objectId))
                {
                    throw new ConfigurationException("Attribute 'factory-object' is required.", element.OuterXml);
                }

                definition.FactoryObject = FindReferenceObject(objectId);
            }
            else
            {
                if (definition.Type == null)
                {
                    throw new ConfigurationException("Attribute 'type' is required.", element.OuterXml);
                }
            }

            foreach (XmlNode childNode in element.ChildNodes)
            {
                if (childNode.NodeType == XmlNodeType.Element)
                {
                    var childElement = (XmlElement)childNode;
                    if (childElement.LocalName == CONSTRUCTOR_ARG_TAG)
                    {
                        var arg = ParseArgument(childElement);
                        if (arg.Index.HasValue)
                        {
                            if (definition.ConstructorArgs.Find(t => t.Index.HasValue && t.Index.Value == arg.Index.Value) != null)
                            {
                                throw new ConfigurationException($"Argument index '{arg.Index.Value}' duplicate.", element.OuterXml);
                            }
                        }
                        else if (!string.IsNullOrWhiteSpace(arg.Name))
                        {
                            if (definition.ConstructorArgs.Find(t => t.Name == arg.Name) != null)
                            {
                                throw new ConfigurationException($"Argument name '{arg.Name}' duplicate.", element.OuterXml);
                            }
                        }
                        else
                        {
                            if (definition.ConstructorArgs.FindAll(t => t.Name != null).Count > 0)
                            {
                                throw new ConfigurationException($"Argument '{childElement.OuterXml}' doesnot specify name or index after other named arguments.", element.OuterXml);
                            }
                            else
                            {
                                arg.Index = definition.ConstructorArgs.Count == 0 ? 0 : definition.ConstructorArgs.Last().Index + 1;
                            }
                        }

                        definition.ConstructorArgs.Add(arg);
                    }
                    else if (childNode.LocalName == PROPERTY_TAG)
                    {
                        var property = ParseProperty(childElement);
                        if (definition.Properties.Find(t => t.Name == property.Name) != null)
                        {
                            throw new ConfigurationException($"Property '{property.Name}' value set more than once.", element.OuterXml);
                        }

                        definition.Properties.Add(property);
                    }
                    else if (childNode.LocalName == LISTENER_TAG)
                    {
                        definition.Listeners.Add(ParseListener(childElement));
                    }
                    else
                    {
                        throw new ConfigurationException($"Unsupported element '{element.LocalName}'.", element.OuterXml);
                    }
                }
            }

            definition.Validate();
            return definition;
        }

        private DefinitionBase ParseChildNode(XmlElement element)
        {
            switch (element.LocalName)
            {
                case NULL_TAG:
                    return ValueDefinition.NULL;

                case VALUE_TAG:
                    return ParseValue(element);

                case OBJECT_TAG:
                    return ParseObject(element);

                case LIST_TAG:
                    return ParseList(element);

                case SET_TAG:
                    return ParseSet(element);

                case DICTIONARY_TAG:
                    return ParseDictionary(element);

                case NAME_VALUES_TAG:
                    return ParseNameValues(element);

                default:
                    throw new ConfigurationException($"Unsupported element '{element.LocalName}'.", element.OuterXml);
            }
        }


        private List<string> inParsingObjects = new List<string>();

        private DefinitionBase FindReferenceObject(string objectId)
        {
            if (Definitions.ContainsKey(objectId))
            {
                return Definitions[objectId];
            }

            if (inParsingObjects.Contains(objectId))
            {
                throw new ConfigurationException($"Contains loop reference object '{objectId}'.");
            }

            if (Elements.ContainsKey(objectId))
            {
                inParsingObjects.Add(objectId);

                DefinitionBase definition = null;
                var element = Elements[objectId];
                if (element.LocalName == OBJECT_TAG)
                {
                    definition = ParseObject(element);
                }
                else
                {
                    definition = ParseAlias(element);
                }

                Definitions[objectId] = definition;
                inParsingObjects.Remove(objectId);

                return definition;
            }

            throw new ConfigurationException($"Cannot find reference object '{objectId}'.");
        }

        #endregion


        public static Dictionary<string, DefinitionBase> Parse(string xml)
        {
            var parser = new XmlConfigParser(xml);
            parser.Parse();

            return parser.Definitions;
        }

        public static Dictionary<string, DefinitionBase> Parse(Stream stream)
        {
            var parser = new XmlConfigParser(stream);
            parser.Parse();

            return parser.Definitions;
        }

        public static Dictionary<string, DefinitionBase> Parse(TextReader reader)
        {
            var parser = new XmlConfigParser(reader);
            parser.Parse();

            return parser.Definitions;
        }
    }
}