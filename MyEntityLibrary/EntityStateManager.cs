using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace myentitylibrary
{
    public interface IEntityStateManager<TEntity, TData, TKey>
        where TEntity : IEntityObject<TData, TKey>
        where TData   : IEntityDataContainer<TKey>
        where TKey    : IEquatable<TKey>
    {
        void AddTracker(TEntity entity, EEntityState initialEntityState);
        void RemoveTracker(TEntity entity);
    }

    public abstract class EntityStateManager<TEntity, TData, TKey>:
        IEntityStateManager<TEntity, TData, TKey>
        where TEntity : IEntityObject<TData, TKey>
        where TData   : IEntityDataContainer<TKey>
        where TKey    : IEquatable<TKey>
    {
        #region Helper methods
        #endregion

        #region IEntityStateManager<TEntity,TData,TKey> Members
        public void AddTracker(TEntity entity)
        {
            throw new NotImplementedException();
        }
        public void RemoveTracker(TEntity entity)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}