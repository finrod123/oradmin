using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace oradmin
{
    public interface IEntityStateManager
    {
        void AddTracker(EntityObject entity);
        void RemoveTracker(EntityObject entity);

        IEnumerable<IEntityChangeTracker> GetEntityEntries(EEntityState state);
        IEnumerable<IEntityChangeTracker> GetChanged();
        IEnumerable<IEntityChangeTracker> GetAdded();
        IEnumerable<IEntityChangeTracker> GetDeleted();
    }

    public abstract class EntityStateManager: IEntityStateManager
    {
        #region Members
        protected Dictionary<EntityKey, IEntityChangeTracker> trackerByEntityKey
            = new Dictionary<EntityKey,IEntityChangeTracker>();
        #endregion

        #region IEntityStateManager Members
        public IEnumerable<IEntityChangeTracker> GetEntityEntries(EEntityState state)
        {
            return
                from tracker in this.trackerByEntityKey.Values
                where tracker.EntityState == state
                select tracker;
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
        public abstract IEntityChangeTracker CreateTracker(EntityObject entity,
            EEntityState state);
        #endregion
        
        #region IEntityStateManager Members
        public void AddTracker(EntityObject entity, EEntityState state)
        {
            if (!this.trackerByEntityKey.ContainsKey(entity.EntityKey))
            {
                // create a tracker and set it to an entity
                IEntityChangeTracker tracker = CreateTracker(entity, state);
                entity.Tracker = tracker;
            }
        }
        public void RemoveTracker(EntityObject entity)
        {
            IEntityChangeTracker tracker;

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