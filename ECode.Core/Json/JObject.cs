using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECode.Json
{
    public sealed class JObject : JToken, IEnumerable<KeyValuePair<string, JToken>>
    {
        private IDictionary<string, JToken>     Fields      = null;


        public JToken this[string name]
        {
            get { return this.Fields[name]; }
        }

        public string[] Keys
        {
            get { return this.Fields.Keys.ToArray(); }
        }


        internal JObject(IDictionary<string, JToken> fields)
            : base(JValueKind.Object)
        {
            this.Fields = fields ?? new Dictionary<string, JToken>();
        }


        private string EscapeKey(string key)
        {
            return key.Replace("\"", "\\\"");
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("{ ");

            bool firstItem = true;
            foreach (var item in this.Fields)
            {
                if (!firstItem)
                { sb.Append(", "); }

                firstItem = false;
                sb.Append($"\"{EscapeKey(item.Key)}\": {item.Value.ToString()}");
            }

            sb.Append(" }");
            return sb.ToString();
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.Fields.GetEnumerator();
        }

        public IEnumerator<KeyValuePair<string, JToken>> GetEnumerator()
        {
            return this.Fields.GetEnumerator();
        }
    }
}
