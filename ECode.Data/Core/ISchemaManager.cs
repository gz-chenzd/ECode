
namespace ECode.Data
{
    public interface ISchemaManager
    {
        EntitySchema GetSchema<TEntity>();
    }
}
