using System;
using System.Collections.Generic;

namespace ECode.Configuration
{
    public class EnvironmentVariables : IConfigProvider
    {
        public event EventHandler Changed;

        private void OnChanged()
        {
            if (this.Changed != null)
            {
                try
                { this.Changed(this, EventArgs.Empty); }
                catch (Exception ex)
                { string dummy = ex.Message; }
            }
        }


        public ICollection<ConfigItem> GetConfigItems()
        {
            var itemsByKey = new SortedDictionary<string, ConfigItem>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var key in Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Machine).Keys)
            {
                var keyValueItem = new KeyValueItem((string)key, null);
                keyValueItem.Value = Environment.GetEnvironmentVariable((string)key, EnvironmentVariableTarget.Machine);

                itemsByKey[keyValueItem.Key] = keyValueItem;
            }

            foreach (var key in Environment.GetEnvironmentVariables(EnvironmentVariableTarget.User).Keys)
            {
                var keyValueItem = new KeyValueItem((string)key, null);
                keyValueItem.Value = Environment.GetEnvironmentVariable((string)key, EnvironmentVariableTarget.User);

                itemsByKey[keyValueItem.Key] = keyValueItem;
            }

            foreach (var key in Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Process).Keys)
            {
                var keyValueItem = new KeyValueItem((string)key, null);
                keyValueItem.Value = Environment.GetEnvironmentVariable((string)key, EnvironmentVariableTarget.Process);

                itemsByKey[keyValueItem.Key] = keyValueItem;
            }

            return itemsByKey.Values;
        }
    }
}