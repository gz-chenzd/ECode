using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ECode.Core;
using ECode.Json;
using ECode.Utility;

namespace ECode.Configuration
{
    public class JsonConfigParser
    {
        public static ICollection<ConfigItem> Parse(string json)
        {
            AssertUtil.ArgumentNotEmpty(json, nameof(json));

            using (var reader = new System.IO.StringReader(json))
            {
                return Parse(JsonParser.Parse(reader)).Values;
            }
        }

        public static ICollection<ConfigItem> Parse(Stream stream)
        {
            AssertUtil.ArgumentNotNull(stream, nameof(stream));

            if (!stream.CanRead)
            { throw new ArgumentException($"Argument '{nameof(stream)}' cannot be read"); }

            using (var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, true))
            {
                return Parse(reader);
            }
        }

        public static ICollection<ConfigItem> Parse(TextReader reader)
        {
            AssertUtil.ArgumentNotNull(reader, nameof(reader));

            return Parse(JsonParser.Parse(reader)).Values;
        }


        static IDictionary<string, ConfigItem> Parse(JToken jsonValue, NamespaceItem parentItem = null)
        {
            if (!(jsonValue is JObject))
            { throw new ConfigurationException("Json config invalid"); }

            var itemsByKey = new SortedDictionary<string, ConfigItem>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var item in (JObject)jsonValue)
            {
                if (string.IsNullOrWhiteSpace(item.Key))
                { throw new ConfigurationException($"Json config key required"); }

                if (item.Value is JObject)
                {
                    var namespaceItem = new NamespaceItem(item.Key, parentItem);
                    namespaceItem.Children = Parse(item.Value, namespaceItem);

                    itemsByKey[namespaceItem.Key] = namespaceItem;
                }
                else
                {
                    var keyValueItem = new KeyValueItem(item.Key, parentItem);
                    itemsByKey[keyValueItem.Key] = keyValueItem;

                    if (item.Value is JValue)
                    {
                        if (item.Value.ValueKind == JValueKind.Null)
                        { continue; }

                        keyValueItem.Value = ((JValue)item.Value).Value;
                    }
                    else
                    { keyValueItem.Value = item.Value.ToString(); }
                }
            }

            return itemsByKey;
        }
    }
}
