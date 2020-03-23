using System;
using System.Collections.Generic;
using ECode.Utility;

namespace ECode.Configuration
{
    public abstract class ConfigItem
    {
        public string Key { get; }

        public ConfigItem Parent { get; }


        public ConfigItem(string key, ConfigItem parent = null)
        {
            AssertUtil.ArgumentNotEmpty(key, nameof(key));

            this.Key = ConfigurationUtil.ResolveKey(key);
            this.Parent = parent;
        }


        internal string GetRelativeKey(ConfigItem relativeTo = null)
        {
            if (this.Parent != null && this.Parent != relativeTo)
            { return $"{this.Parent.GetRelativeKey(relativeTo)}{ConfigurationManager.SEPARATOR_CHAR}{this.Key}"; }

            return this.Key;
        }
    }

    public class KeyValueItem : ConfigItem
    {
        public string Value { get; set; }


        public KeyValueItem(string key, ConfigItem parent = null)
            : this(key, string.Empty, parent)
        {

        }

        public KeyValueItem(string key, string value, ConfigItem parent = null)
            : base(key, parent)
        {
            this.Value = value;
        }
    }

    public class NamespaceItem : ConfigItem
    {
        public IDictionary<string, ConfigItem> Children { get; set; }


        public NamespaceItem(string key, ConfigItem parent = null)
            : base(key, parent)
        {
            this.Children = new SortedDictionary<string, ConfigItem>(StringComparer.InvariantCultureIgnoreCase);
        }
    }
}
