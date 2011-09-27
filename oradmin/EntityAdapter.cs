using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace oradmin
{
    public interface IEntityDataAdapter<TEntity, TData, TKey>
        where TEntity : EntityObject<TKey>
        where TData   : IEntityDataContainer<TKey>
        where TKey    : IEquatable<TKey>
    {
        void Fill(IEntityManager<TEntity, TData, TKey> entityManager);

        bool GetChanges(TEntity entity, out TData data);
        bool GetChanges(IEnumerable<TEntity> entities,
                        out IEnumerable<TData> data);
        bool GetChanges(out IEnumerable<TData> data);
    }

    public interface IEntityDataSaver<TEntity, TKey>
        where TEntity : EntityObject<TKey>
        where TKey    : IEquatable<TKey>
    {
        void Update(IEntityManager<TKey> entityManager);
        void Update(IEnumerable<TEntity> entities);
        void Update(TEntity entity);
    }

    public abstract class EntityDataAdapter<TEntity, TData, TKey> :
        IEntityDataAdapter<TEntity, TData, TKey>
        where TEntity : EntityObject<TKey>
        where TData : IEntityDataContainer<TKey>
        where TKey : IEquatable<TKey>
    {
        #region IEntityDataAdapter<TKey> Members
        public virtual void Fill(IEntityManager<TEntity, TData, TKey> entityManager)
        {
            IEnumerable<TData> loadedEntityContainers;

            // if I can get the changes, merge them into the entityManager
            if (GetChanges(out loadedEntityContainers))
            {
                entityManager.MergeData(loadedEntityContainers);
            }
        }
        #endregion

        #region IEntityDataAdapter<TEntity,TData,TKey> Members
        public abstract bool GetChanges(TEntity entity, out TData data);
        public abstract bool GetChanges(IEnumerable<TEntity> entities, out IEnumerable<TData> data);
        public abstract bool GetChanges(out IEnumerable<TData> data);
        #endregion
    }
}
