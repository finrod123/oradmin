using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace myentitylibrary
{
    public interface IEntityDataAdapter<TEntity, TData, TKey>
        where TEntity : IEntityObject<TData, TKey>
        where TData   : IEntityDataContainer<TKey>
        where TKey    : IEquatable<TKey>
    {
        bool GetChanges(TEntity entity, out TData data);
        bool GetChanges(IEnumerable<TEntity> entities,
                        out IEnumerable<TData> data);
        bool GetChanges(out IEnumerable<TData> data);
    }

    public interface IEntityDataSaver<TEntityCollection, TEntity, TData, TKey>
        where TEntityCollection : IEntityCollection<TEntity, TData, TKey>
        where TEntity  : IEntityObject<TData, TKey>
        where TData    : IEntityDataContainer<TKey>
        where TKey     : IEquatable<TKey>
    {
        void SaveChanges(TEntityCollection entities);
        void SaveChanges(IEnumerable<TEntity> entities);
        void SaveChanges(TEntity entity);
    }

    public abstract class EntityDataAdapter<TEntity, TData, TKey> :
        IEntityDataAdapter<TEntity, TData, TKey>
        where TEntity : EntityObject<TData, TKey>
        where TData : IEntityDataContainer<TKey>
        where TKey : IEquatable<TKey>
    {
        
        #region IEntityDataAdapter<TEntity,TData,TKey> Members
        public abstract bool GetChanges(TEntity entity, out TData data);
        public abstract bool GetChanges(IEnumerable<TEntity> entities, out IEnumerable<TData> data);
        public abstract bool GetChanges(out IEnumerable<TData> data);
        #endregion
    }
}
