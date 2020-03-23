using ECode.Core;
using ECode.TypeConversion;

namespace ECode.Json
{
    public sealed class JValue : JToken
    {
        internal static readonly JValue     NULL    = new JValue("null", JValueKind.Null);
        internal static readonly JValue     TRUE    = new JValue("true", JValueKind.Bool);
        internal static readonly JValue     FALSE   = new JValue("false", JValueKind.Bool);


        public string Value
        { get; private set; }

        private string RawValue
        { get; set; }


        internal JValue(string value, JValueKind valueKind)
            : this(value, value, valueKind)
        {

        }

        internal JValue(string value, string rawValue, JValueKind valueKind)
            : base(valueKind)
        {
            this.Value = value;
            this.RawValue = rawValue;
        }


        public void TrimValue()
        {
            if (this.ValueKind == JValueKind.String)
            {
                this.Value = this.Value.Trim();
                this.RawValue = this.RawValue.Trim();
            }
        }


        public override string ToString()
        {
            if (this.ValueKind == JValueKind.String)
            { return $"\"{this.RawValue}\""; }

            return this.RawValue;
        }


        public override T ToValue<T>()
        {
            if (this.ValueKind == JValueKind.Null)
            {
                if (typeof(T).IsValueType)
                { throw new JsonException($"Null cannot be converted to type {typeof(T)}"); }

                return default(T);
            }
            else
            { return (T)TypeConversionUtil.ConvertValueIfNecessary(typeof(T), this.Value); }
        }
    }
}
