using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace oradmin
{
    public interface IEntityStateManager<TEntity, TKey>
        where TEntity : EntityObject<TKey>
        where TKey    : IEquatable<TKey>
    {
        void AddTracker(TEntity entity);
        void RemoveTracker(TEntity entity);

        IEnumerable<IEntityChangeTracker> GetEntityEntries(EEntityState state);
        IEnumerable<IEntityChangeTracker> GetChanged();
        IEnumerable<IEntityChangeTracker> GetAdded();
        IEnumerable<IEntityChangeTracker> GetDeleted();
    }

    public class EntityStateManager<TEntity, TKey>: IEntityStateManager<TEntity, TKey>
        where TEntity : EntityObject<TKey>
        where TKey    : IEquatable<TKey>
    {
        #region Members
        protected Dictionary<EntityKey, EntityChangeTracker<TEntity>> trackerByEntityKey
            = new Dictionary<EntityKey,EntityChangeTracker<TEntity>>();
        #endregion

        #region IEntityStateManager Members
        public IEnumerable<IEntityChangeTracker> GetEntityEntries(EEntityState state)
        {
            return
                from tracker in this.trackerByEntityKey.Values
                where tracker.EntityState == state
                select tracker as IEntityChangeTracker;
        }
        public IEnumerable<IEntityChangeTracker> GetChanged()
        {
            return GetEntityEntries(EEntityState.Modified);
        }
        public IEnumerable<IEntityChangeTracker> GetAdded()
        {
            return GetEntityEntries(EEntityState.Added);
        }
        public IEnumerable<IEntityChangeTracker> GetDeleted()
        {
            return GetEntityEntries(EEntityState.Deleted);
        }
        #endregion

        #region Helper methods
        protected abstract IEntityChangeTracker CreateTracker(TEntity entity,
            EEntityState state);
        #endregion
        
        #region IEntityStateManager Members
        public void AddTracker(TEntity entity, EEntityState state)
        {
            if (!this.trackerByEntityKey.ContainsKey(entity.EntityKey))
            {
                // create a tracker and set it to an entity
                IEntityChangeTracker tracker = CreateTracker(entity, state);
                entity.Tracker = tracker;
            }
        }
        public void RemoveTracker(TEntity entity)
        {
            EntityChangeTracker<TEntity> tracker;

            if (this.trackerByEntityKey.TryGetValue(entity.EntityKey, out tracker))
            {
                // remove tracker from the dictionary
                this.trackerByEntityKey.Remove(entity.EntityKey);
                // unset tracker for entity -> detached entity
                entity.Tracker = null;
            }
        }
        #endregion
    }
}