using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using ECode.Core;
using ECode.Utility;

namespace ECode.Configuration
{
    public class XmlConfigParser
    {
        const string    ROOT_TAG            = "configuration";

        const string    ADD_TAG             = "add";
        const string    NAMESPACE_TAG       = "namespace";

        const string    NAME_ATTR           = "name";
        const string    VALUE_ATTR          = "value";


        public static ICollection<ConfigItem> Parse(string xml)
        {
            AssertUtil.ArgumentNotEmpty(xml, nameof(xml));

            var doc = new XmlDocument();
            doc.XmlResolver = null;
            doc.LoadXml(xml);

            var root = doc.DocumentElement;
            if (root.LocalName != ROOT_TAG)
            { throw new ConfigurationException($"Root element isnot '{ROOT_TAG}'"); }

            return Parse(root).Values;
        }

        public static ICollection<ConfigItem> Parse(Stream stream)
        {
            AssertUtil.ArgumentNotNull(stream, nameof(stream));

            if (!stream.CanRead)
            { throw new ArgumentException($"Argument '{nameof(stream)}' cannot be read"); }

            var doc = new XmlDocument();
            doc.XmlResolver = null;
            doc.Load(stream);

            var root = doc.DocumentElement;
            if (root.LocalName != ROOT_TAG)
            { throw new ConfigurationException($"Root element isnot '{ROOT_TAG}'"); }

            return Parse(root).Values;
        }

        public static ICollection<ConfigItem> Parse(TextReader reader)
        {
            AssertUtil.ArgumentNotNull(reader, nameof(reader));

            var doc = new XmlDocument();
            doc.XmlResolver = null;
            doc.Load(reader);

            var root = doc.DocumentElement;
            if (root.LocalName != ROOT_TAG)
            { throw new ConfigurationException($"Root element isnot '{ROOT_TAG}'"); }

            return Parse(root).Values;
        }


        static IDictionary<string, ConfigItem> Parse(XmlElement parentElement, NamespaceItem parentItem = null)
        {
            var itemsByKey = new SortedDictionary<string, ConfigItem>(StringComparer.InvariantCultureIgnoreCase);
            foreach (XmlNode childNode in parentElement.ChildNodes)
            {
                if (childNode.NodeType != XmlNodeType.Element)
                { continue; }

                var element = (XmlElement)childNode;
                if (element.LocalName == ADD_TAG)
                {
                    if (!element.HasAttribute(NAME_ATTR) || string.IsNullOrWhiteSpace(element.GetAttribute(NAME_ATTR)))
                    { throw new ConfigurationException($"Attribute '{NAME_ATTR}' is required", element.OuterXml); }

                    var keyValueItem = new KeyValueItem(element.GetAttribute(NAME_ATTR), parentItem);
                    keyValueItem.Value = element.InnerText;

                    if (element.HasAttribute(VALUE_ATTR))
                    { keyValueItem.Value = element.GetAttribute(VALUE_ATTR); }

                    itemsByKey[keyValueItem.Key] = keyValueItem;
                }
                else if (element.LocalName == NAMESPACE_TAG)
                {
                    if (!element.HasAttribute(NAME_ATTR) || string.IsNullOrWhiteSpace(element.GetAttribute(NAME_ATTR)))
                    { throw new ConfigurationException($"Attribute '{NAME_ATTR}' is required", element.OuterXml); }

                    var namespaceItem = new NamespaceItem(element.GetAttribute(NAME_ATTR), parentItem);
                    namespaceItem.Children = Parse(element, namespaceItem);

                    itemsByKey[namespaceItem.Key] = namespaceItem;
                }
                else
                { throw new ConfigurationException($"Unsupported element '{element.LocalName}'", element.OuterXml); }
            }

            return itemsByKey;
        }
    }
}
