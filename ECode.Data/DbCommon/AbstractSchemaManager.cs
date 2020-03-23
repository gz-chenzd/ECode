
namespace ECode.Data
{
    public abstract class AbstractSchemaManager : ISchemaManager
    {
        private bool                m_Initialized       = false;
        private ModelBuilder        m_pModelBuilder     = null;


        public AbstractSchemaManager()
        {
            m_pModelBuilder = new ModelBuilder();
        }


        public EntitySchema GetSchema<TEntity>()
        {
            if (!m_Initialized)
            {
                m_Initialized = true;
                OnModelCreating(m_pModelBuilder);
            }

            return m_pModelBuilder.GetSchema<TEntity>();
        }


        protected abstract void OnModelCreating(ModelBuilder modelBuilder);
    }
}