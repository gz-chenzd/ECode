using ECode.TypeResolution;

namespace ECode.DependencyInjection
{
    class TypeDefinition : DefinitionBase
    {
        public string Type
        { get; set; }


        public override void Validate()
        {
            if (this.ResolvedType != null)
            {
                return;
            }

            this.ResolvedType = TypeResolutionUtil.ResolveType(this.Type);
        }

        public override object GetValue()
        {
            return this.ResolvedType;
        }
    }
}
