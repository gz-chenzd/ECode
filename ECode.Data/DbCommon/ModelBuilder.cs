using System;
using System.Collections.Generic;

namespace ECode.Data
{
    public class ModelBuilder
    {
        private Dictionary<Type, EntitySchema>  m_pSchemaMaps
            = new Dictionary<Type, EntitySchema>();


        internal EntitySchema GetSchema<TEntity>()
        {
            var entityType = typeof(TEntity);
            if (m_pSchemaMaps.ContainsKey(entityType))
            {
                return m_pSchemaMaps[entityType];
            }

            return null;
        }


        public void Entity<TEntity>(Action<SchemaBuilder<TEntity>> buildAction)
        {
            var entityType = typeof(TEntity);

            var schema = new EntitySchema();
            buildAction.Invoke(new SchemaBuilder<TEntity>(schema));

            m_pSchemaMaps[entityType] = schema;
        }
    }
}
