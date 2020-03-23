using System;
using ECode.TypeConversion;
using ECode.TypeResolution;

namespace ECode.DependencyInjection
{
    class ArgumentDefinition : DefinitionBase
    {
        public int? Index
        { get; set; }

        public string Name
        { get; set; }

        public string Type
        { get; set; }

        public DefinitionBase ValueDefinition
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

            return ValueDefinition.CanConvertTo(destinationType);
        }


        public override void Validate()
        {
            if (this.ResolvedType != null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(this.Type))
            {
                this.ResolvedType = TypeResolutionUtil.ResolveType(this.Type);
            }

            if (this.ResolvedType != null)
            {
                if (this.ValueDefinition.ResolvedType != null)
                {
                    if (!this.ResolvedType.IsAssignableFrom(this.ValueDefinition.ResolvedType))
                    {
                        throw new InvalidCastException($"Type '{this.ValueDefinition.ResolvedType.FullName}' cannot convert to target type '{this.ResolvedType.FullName}'.");
                    }
                }
                else
                {
                    if (!this.ValueDefinition.CanConvertTo(this.ResolvedType))
                    {
                        throw new InvalidCastException($"Value '{this.ValueDefinition.GetValue()}' cannot convert to target type '{this.ResolvedType.FullName}'.");
                    }
                }
            }
            else
            {
                this.ResolvedType = this.ValueDefinition.ResolvedType;
            }
        }

        public override object GetValue()
        {
            if (this.ResolvedType != null && this.ResolvedType != this.ValueDefinition.ResolvedType)
            {
                return TypeConversionUtil.ConvertValueIfNecessary(this.ResolvedType, this.ValueDefinition.GetValue());
            }

            return this.ValueDefinition.GetValue();
        }
    }
}
