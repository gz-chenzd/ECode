
namespace ECode.DependencyInjection
{
    class AliasDefinition : DefinitionBase
    {
        public string Name
        { get; set; }

        public DefinitionBase RefDefinition
        { get; set; }


        public override void Validate()
        {
            this.ResolvedType = this.RefDefinition.ResolvedType;
        }

        public override object GetValue()
        {
            return this.RefDefinition.GetValue();
        }
    }
}
