using System;

namespace ECode.DependencyInjection
{
    abstract class DefinitionBase
    {
        public virtual Type ResolvedType
        { get; protected set; }


        public virtual bool CanConvertTo(Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException(nameof(destinationType));
            }

            if (this.ResolvedType == null)
            {
                throw new InvalidOperationException("Cannot resolve the definition type.");
            }

            return destinationType.IsAssignableFrom(this.ResolvedType);
        }


        public abstract void Validate();

        public abstract object GetValue();
    }
}
