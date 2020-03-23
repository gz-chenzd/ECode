using System;
using ECode.TypeConversion;
using ECode.TypeResolution;

namespace ECode.DependencyInjection
{
    class ValueDefinition : DefinitionBase
    {
        public static readonly ValueDefinition NULL = new ValueDefinition();


        public string Type
        { get; set; }

        public string Value
        { get; set; }


        public override bool CanConvertTo(Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException(nameof(destinationType));
            }

            if (this.ResolvedType != null)
            {
                return destinationType.IsAssignableFrom(this.ResolvedType);
            }

            TypeConversionUtil.ConvertValueIfNecessary(destinationType, this.Value);
            return true;
        }


        public override void Validate()
        {
            if (this.ResolvedType != null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(this.Type))
            {
                this.ResolvedType = TypeResolutionUtil.ResolveType(this.Type);
            }
        }

        public override object GetValue()
        {
            if (this.ResolvedType == null)
            {
                return this.Value;
            }

            return TypeConversionUtil.ConvertValueIfNecessary(this.ResolvedType, this.Value);
        }
    }
}
