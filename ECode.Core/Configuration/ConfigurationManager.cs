using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ECode.Logging;
using ECode.Utility;

namespace ECode.Configuration
{
    public static class ConfigurationManager
    {
        public const char   SEPARATOR_CHAR   = '/';


        static IList<IConfigProvider>               Providers       = new List<IConfigProvider>();
        static IDictionary<string, string>          KeyValues       = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        static IDictionary<string, ConfigItem>      RootItems       = new Dictionary<string, ConfigItem>(StringComparer.InvariantCultureIgnoreCase);


        static ConfigurationManager()
        {
            AddProvider(new EnvironmentVariables());
        }


        public static event ChangedHandler Changed;

        static void OnChanged(ChangedEventArgs e)
        {
            if (Changed != null)
            {
                try
                {
                    Changed(e);
                }
                catch (Exception ex)
                { string dummy = ex.Message; }
            }
        }


        public static void AddProvider(IConfigProvider provider)
        {
            AssertUtil.ArgumentNotNull(provider, nameof(provider));

            Providers.Add(provider);
            provider.Changed += OnProviderChanged;

            Resolve();
        }

        public static void RemoveProvider(IConfigProvider provider)
        {
            AssertUtil.ArgumentNotNull(provider, nameof(provider));

            if (Providers.Remove(provider))
            {
                provider.Changed -= OnProviderChanged;

                Resolve();
            }
        }

        public static void ClearProviders()
        {
            foreach (var provider in Providers.ToArray())
            {
                Providers.Remove(provider);
                provider.Changed -= OnProviderChanged;
            }

            Resolve();
        }

        static void OnProviderChanged(object sender, EventArgs e)
        {
            try
            {
                Resolve();
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("Configuration").Error("Configuration reload error: " + ex.Message, ex);

                throw ex;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        static void Resolve()
        {
            var newRootItems = new SortedDictionary<string, ConfigItem>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var provider in Providers.ToArray())
            {
                foreach (var keyValuePair in new Configuration(null, provider.GetConfigItems()))
                {
                    string[] keyParts = keyValuePair.Key.Split(SEPARATOR_CHAR, 2);
                    var resolvedKey = ConfigurationUtil.ResolveKey(keyParts[0]);

                    if (keyParts.Length == 1)
                    {
                        newRootItems[resolvedKey] = new KeyValueItem(resolvedKey, keyValuePair.Value);
                        continue;
                    }

                    if (!newRootItems.ContainsKey(resolvedKey))
                    {
                        newRootItems[resolvedKey] = new NamespaceItem(resolvedKey);
                    }
                    else if (newRootItems[resolvedKey] is KeyValueItem)
                    {
                        newRootItems[resolvedKey] = new NamespaceItem(resolvedKey);
                    }

                    ResolveChild((NamespaceItem)newRootItems[resolvedKey], keyParts[1], keyValuePair.Value);
                }
            }

            var newKeyValues = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var keyValuePair in new Configuration(null, newRootItems.Values))
            {
                newKeyValues[keyValuePair.Key] = keyValuePair.Value;
            }


            var oldKeyValues = KeyValues;
            KeyValues = newKeyValues;
            RootItems = newRootItems;

            foreach (var keyValuePair in oldKeyValues)
            {
                if (!newKeyValues.ContainsKey(keyValuePair.Key))
                {
                    OnChanged(new ChangedEventArgs(keyValuePair.Key, keyValuePair.Value, ChangedStatus.Delete));
                }
            }

            foreach (var keyValuePair in newKeyValues)
            {
                if (!oldKeyValues.ContainsKey(keyValuePair.Key))
                {
                    OnChanged(new ChangedEventArgs(keyValuePair.Key, keyValuePair.Value, ChangedStatus.Set));
                }
                else if (keyValuePair.Value != oldKeyValues[keyValuePair.Key])
                {
                    OnChanged(new ChangedEventArgs(keyValuePair.Key, keyValuePair.Value, ChangedStatus.Set));
                }
            }
        }

        static void ResolveChild(NamespaceItem owner, string key, string value)
        {
            string[] keyParts = key.Split(SEPARATOR_CHAR, 2);
            var resolvedKey = ConfigurationUtil.ResolveKey(keyParts[0]);

            if (keyParts.Length == 1)
            {
                owner.Children[resolvedKey] = new KeyValueItem(resolvedKey, value, owner);
                return;
            }

            if (!owner.Children.ContainsKey(resolvedKey))
            {
                owner.Children[resolvedKey] = new NamespaceItem(resolvedKey, owner);
            }
            else if (owner.Children[resolvedKey] is KeyValueItem)
            {
                owner.Children[resolvedKey] = new NamespaceItem(resolvedKey, owner);
            }

            ResolveChild((NamespaceItem)owner.Children[resolvedKey], keyParts[1], value);
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        public static string Get(string key)
        {
            if (KeyValues.ContainsKey(key))
            {
                return KeyValues[key];
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static Configuration GetNamespace(string key)
        {
            string[] keyParts = key.Split(SEPARATOR_CHAR, 2);

            var resolvedKey = keyParts[0];
            //var resolvedKey = ConfigurationUtil.ResolveKey(keyParts[0]);

            if (!RootItems.ContainsKey(resolvedKey))
            { return null; }

            if (RootItems[resolvedKey] is KeyValueItem)
            { return null; }

            if (keyParts.Length == 1)
            {
                var namespaceItem = (NamespaceItem)RootItems[resolvedKey];
                return new Configuration(namespaceItem, namespaceItem.Children.Values);
            }

            return GetNamespace((NamespaceItem)RootItems[resolvedKey], keyParts[1]);
        }

        static Configuration GetNamespace(NamespaceItem owner, string key)
        {
            string[] keyParts = key.Split(SEPARATOR_CHAR, 2);

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
