using System;

namespace ECode.DependencyInjection
{
    class PropertyDefinition : DefinitionBase
    {
        public string Name
        { get; set; }

        public DefinitionBase ValueDefinition
        { get; set; }


        public override bool CanConvertTo(Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException(nameof(destinationType));
            }

            if (this.ResolvedType != null && destinationType.IsAssignableFrom(this.ResolvedType))
            {
                return true;
            }

            return this.ValueDefinition.CanConvertTo(destinationType);
        }


        public override void Validate()
        {
            if (this.ResolvedType != null)
            {
                return;
            }

            this.ResolvedType = this.ValueDefinition.ResolvedType;
        }

        public override object GetValue()
        {
            return this.ValueDefinition.GetValue();
        }
    }
}
