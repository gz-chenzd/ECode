using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ECode.Json
{
    public sealed class JArray : JToken, IEnumerable<JToken>
    {
        private IList<JToken>   Items   = null;


        internal JArray(IList<JToken> items)
            : base(JValueKind.Array)
        {
            this.Items = items ?? new List<JToken>();
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.Items.GetEnumerator();
        }

        public IEnumerator<JToken> GetEnumerator()
        {
            return this.Items.GetEnumerator();
        }


        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("[ ");

            bool firstItem = true;
            foreach (var item in this.Items)
            {
                if (!firstItem)
                { sb.Append(", "); }

                firstItem = false;
                sb.Append(item.ToString());
            }

            sb.Append(" ]");
            return sb.ToString();
        }
    }
}
