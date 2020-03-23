using System.Collections.Specialized;

namespace ECode.DependencyInjection
{
    class NameValuesDefinition : DefinitionBase
    {
        public NameValueCollection NameValues
        { get; set; } = new NameValueCollection();


        public override void Validate()
        {
            if (this.ResolvedType != null)
            {
                return;
            }

            this.ResolvedType = typeof(NameValueCollection);
        }

        public override object GetValue()
        {
            var dict = new NameValueCollection();
            foreach (string key in this.NameValues.Keys)
            {
                dict[key] = this.NameValues[key];
            }

            return dict;
        }
    }
}
