using ECode.TypeConversion;

namespace ECode.Utility
{
    public static class ConvertUtil
    {
        public static T ConvertTo<T>(string value)
        {
            return (T)TypeConversionUtil.ConvertValueIfNecessary(typeof(T), value);
        }

        public static T ConvertTo<T>(string value, T defaultValue)
        {
            try
            {
                return (T)TypeConversionUtil.ConvertValueIfNecessary(typeof(T), value);
            }
            catch
            { return defaultValue; }
        }
    }
}
