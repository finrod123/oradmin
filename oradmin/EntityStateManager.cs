﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace oradmin
{
    public interface IEntityStateManager<TEntity, TData, TKey>
        where TEntity : EntityObjectBase<TData, TKey>
        where TData   : IEntityDataContainer<TKey>
        where TKey    : IEquatable<TKey>
    {
        void AddTracker(TEntity entity);
        void RemoveTracker(TEntity entity);
    }

    public abstract class EntityStateManager<TEntity, TData, TKey>:
        IEntityStateManager<TEntity, TData, TKey>
        where TEntity : EntityObjectBase<TData, TKey>
        where TData   : IEntityDataContainer<TKey>
        where TKey    : IEquatable<TKey>
    {
        #region Helper methods
        protected abstract IEntityChangeTracker CreateTracker(TEntity entity,
            EEntityState state);
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