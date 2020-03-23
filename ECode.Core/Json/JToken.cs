using ECode.Core;
using ECode.TypeConversion;

namespace ECode.Json
{
    public abstract class JToken
    {
        public JValueKind ValueKind
        { get; }


        public JToken(JValueKind valueKind)
        {
            this.ValueKind = valueKind;
        }


        public virtual T ToValue<T>()
        {
            if (this.ValueKind == JValueKind.Null)
            {
                if (typeof(T).IsValueType)
                {
                    throw new JsonException($"Null cannot be converted to type {typeof(T)}");
                }

                return default(T);
            }

            if (this.ValueKind == JValueKind.Array
                || this.ValueKind == JValueKind.Object)
            {
                return JsonUtil.Deserialize<T>(this.ToString());
            }
            else
            {
                return (T)TypeConversionUtil.ConvertValueIfNecessary(typeof(T), this.ToString());
            }
        }
    }
}
