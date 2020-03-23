using ECode.Core;

namespace ECode.Configuration
{
    static class ConfigurationUtil
    {
        public static string ResolveKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            { throw new ConfigurationException($"Key cannot be empty"); }

            if (key.IndexOf(ConfigurationManager.SEPARATOR_CHAR) >= 0)
            { throw new ConfigurationException($"Key contains invalid char '{ConfigurationManager.SEPARATOR_CHAR}'"); }

            return key.Trim();
        }
    }
}
