using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ECode.Utility;

namespace ECode.Configuration
{
    public class Configuration : IEnumerable<KeyValuePair<string, string>>
    {
        class KeyValueEnumerator : IEnumerator<KeyValuePair<string, string>>
        {
            ConfigItem                              Owner               = null;
            ICollection<ConfigItem>                 ConfigItems         = null;
            KeyValueItem                            currentItem         = null;
            KeyValuePair<string, string>            currentPair         = new KeyValuePair<string, string>(null, null);
            IEnumerator<ConfigItem>                 enumerator          = null;
            Stack<IEnumerator<ConfigItem>>          enumeratorStack     = null;


            object IEnumerator.Current
            {
                get { return currentPair; }
            }

            public KeyValuePair<string, string> Current
            {
                get { return currentPair; }
            }


            public KeyValueEnumerator(ConfigItem owner, ICollection<ConfigItem> configItems)
            {
                AssertUtil.ArgumentNotNull(configItems, nameof(configItems));

                Owner = owner;
                ConfigItems = configItems;

                currentItem = null;
                currentPair = new KeyValuePair<string, string>(null, null);
                enumerator = configItems.GetEnumerator();
                enumeratorStack = new Stack<IEnumerator<ConfigItem>>();
            }


            public bool MoveNext()
            {
                while (true)
                {
                    if (enumerator == null)
                    { break; }

                    if (enumerator.MoveNext())
                    {
                        if (enumerator.Current is NamespaceItem)
                        {
                            enumeratorStack.Push(enumerator);
                            enumerator = (enumerator.Current as NamespaceItem).Children.Values.GetEnumerator();

                            continue;
                        }

                        currentItem = enumerator.Current as KeyValueItem;
                        currentPair = new KeyValuePair<string, string>(currentItem.GetRelativeKey(Owner), currentItem.Value);

                        return true;
                    }


                    enumerator.Dispose();
                    enumerator = null;

                    if (enumeratorStack.Count > 0)
                    {
                        enumerator = enumeratorStack.Pop();
                        continue;
                    }

                    currentItem = null;
                    currentPair = new KeyValuePair<string, string>(null, null);

                    break;
                }

                return false;
            }

            public void Reset()
            {
                Dispose();

                enumerator = ConfigItems.GetEnumerator();
                enumeratorStack = new Stack<IEnumerator<ConfigItem>>();
            }

            public void Dispose()
            {
                currentItem = null;
                currentPair = new KeyValuePair<string, string>(null, null);

                if (enumerator != null)
                {
                    enumerator.Dispose();
                    enumerator = null;
                }

                while (enumeratorStack.Count > 0)
                {
                    enumeratorStack.Pop().Dispose();
                }

                enumeratorStack.Clear();
                enumeratorStack = null;
            }
        }


        private ConfigItem                          Owner           = null;
        private ICollection<ConfigItem>             ConfigItems     = null;
        private IDictionary<string, ConfigItem>     ItemsByKey      = null;


        internal Configuration(ConfigItem owner, ICollection<ConfigItem> configItems)
        {
            AssertUtil.ArgumentNotNull(configItems, nameof(configItems));

            Owner = owner;
            ConfigItems = configItems;

            if (owner != null)
            { ItemsByKey = ((NamespaceItem)owner).Children; }
            else
            { ItemsByKey = configItems.ToDictionary(t => t.Key, StringComparer.InvariantCultureIgnoreCase); }
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return new KeyValueEnumerator(Owner, ConfigItems);
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return new KeyValueEnumerator(Owner, ConfigItems);
        }


        public string Get(string key)
        {
            string[] keyParts = key.Split(ConfigurationManager.SEPARATOR_CHAR, 2);

            var resolvedKey = keyParts[0];
            //var resolvedKey = ConfigurationUtil.ResolveKey(keyParts[0]);

            if (!ItemsByKey.ContainsKey(resolvedKey))
            { return null; }

            if (keyParts.Length == 1)
            {
                if (ItemsByKey[resolvedKey] is KeyValueItem)
                { return ((KeyValueItem)ItemsByKey[resolvedKey]).Value; }

                return null;
            }

            return Get((NamespaceItem)ItemsByKey[resolvedKey], keyParts[1]);
        }

        private string Get(NamespaceItem owner, string key)
        {
            string[] keyParts = key.Split(ConfigurationManager.SEPARATOR_CHAR, 2);

            var resolvedKey = keyParts[0];
            //var resolvedKey = ConfigurationUtil.ResolveKey(keyParts[0]);

            if (!owner.Children.ContainsKey(resolvedKey))
            { return null; }

            if (keyParts.Length == 1)
            {
                if (owner.Children[resolvedKey] is KeyValueItem)
                { return ((KeyValueItem)owner.Children[resolvedKey]).Value; }

                return null;
            }

            return Get((NamespaceItem)owner.Children[resolvedKey], keyParts[1]);
        }

        public Configuration GetNamespace(string key)
        {
            string[] keyParts = key.Split(ConfigurationManager.SEPARATOR_CHAR, 2);

            var resolvedKey = keyParts[0];
            //var resolvedKey = ConfigurationUtil.ResolveKey(keyParts[0]);

            if (!ItemsByKey.ContainsKey(resolvedKey))
            { return null; }

            if (ItemsByKey[resolvedKey] is KeyValueItem)
            { return null; }

            if (keyParts.Length == 1)
            {
                var namespaceItem = (NamespaceItem)ItemsByKey[resolvedKey];
                return new Configuration(namespaceItem, namespaceItem.Children.Values);
            }

            return GetNamespace((NamespaceItem)ItemsByKey[resolvedKey], keyParts[1]);
        }

        private Configuration GetNamespace(NamespaceItem owner, string key)
        {
            string[] keyParts = key.Split(ConfigurationManager.SEPARATOR_CHAR, 2);

            var resolvedKey = keyParts[0];
            //var resolvedKey = ConfigurationUtil.ResolveKey(keyParts[0]);

            if (!owner.Children.ContainsKey(resolvedKey))
            { return null; }

            if (owner.Children[resolvedKey] is KeyValueItem)
            { return null; }

            if (keyParts.Length == 1)
            {
                var namespaceItem = (NamespaceItem)owner.Children[resolvedKey];
                return new Configuration(namespaceItem, namespaceItem.Children.Values);
            }

            return GetNamespace((NamespaceItem)owner.Children[resolvedKey], keyParts[1]);
        }
    }
}
