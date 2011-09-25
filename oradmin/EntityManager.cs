using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace oradmin
{
    // definice delegatu pro notifikace manageru
    public delegate void EntitiesChangedHandler(IEnumerable<EntityObject> added);

    public interface IEntityManager
    {
        IEntityManager ParentManager { get; }

        event EntitiesChangedHandler EntitiesAdded;
        event EntitiesChangedHandler EntitiesModified;
        event EntitiesChangedHandler EntitiesDeleted;

        bool Loaded { get; }
        void Load();

        void AddObject(EntityObject entity);
        void DeleteObject(EntityObject entity);
        
        EntityObject CreateObject();

        void MergeData(IEnumerable<IEntityDataContainer> data);

        Predicate<EntityObject> BelongsTo { get; }
    }

    public interface IRefreshableEntityManager
    {
        void Refresh();
        void Refresh(IEnumerable<EntityObject> entities);
        void Refresh(EntityObject entity);
    }

    public interface IUpdatableEntityManager
    {
        void SaveChanges();
        void SaveChanges(IEnumerable<EntityObject> entities);
        void SaveChanges(EntityObject entity);
    }

    public interface IEntityManagerWithErrorReporting
    {
        bool HasErrors { get; }
        IEnumerable<EntityObject> EntitiesInError { get; }
    }

    public abstract class EntityManager : IEntityManager,
        IRefreshableEntityManager, IUpdatableEntityManager,
        IRevertibleChangeTracking, IEntityManagerWithErrorReporting
    {
        #region Members
        protected IEntityManager parentManager;
        protected EntityStateManager entityStateManager;
        protected Predicate<EntityObject> belongsToPredicate;
        protected Dictionary<EntityKey, EntityObject> entityByEntityKey =
            new Dictionary<EntityKey, EntityObject>();
        protected Dictionary<IEquatableObject, EntityObject> entityByDataKey =
            new Dictionary<IEquatableObject, EntityObject>();
        protected EntityDataAdapter dataAdapter;
        #endregion

        #region Constructor
        protected EntityManager(
            IEntityManager parentManager,
            Predicate<EntityObject> belongsToPredicate)
        {
            if (belongsToPredicate == null)
                this.belongsToPredicate = entity => true;
            else
                this.belongsToPredicate = belongsToPredicate;

            ParentManager = parentManager;
            // factory methods to create data adapter and state manager

            // register for update notifications

        }
        #endregion

        #region IEntityManager Members
        public bool Loaded { get; private set; }

        public void Load()
        {
            // fill with data adapter
        }

        /// <summary>
        /// Adds an entity to a manager -> only if it is valid
        /// </summary>
        /// <param name="entity"></param>
        public void AddObject(EntityObject entity)
        {
            // must belong to an entity manager context
            if (!this.belongsToPredicate(entity))
                return;

            // if it is being edited, stop it
            if (entity.IsEditing)
                entity.EndEdit();

            // test for entity errors
            if (!entity.HasErrors &&
                !entityByEntityKey.ContainsKey(entity.EntityKey) &&
                !entityByDataKey.ContainsKey(entity.DataKey))
            {
                attachEntityObject(entity, EEntityState.Added);
            }
        }
        public void DeleteObject(EntityObject entity)
        {
            // mark object for deletion;
            // will be deleted when saveChanges() overload is called
            IEntityChangeTracker tracker = entity.Tracker;
            tracker.EntityState = EEntityState.Deleted;
        }
        public abstract EntityObject CreateObject();
        public void MergeData(IEnumerable<IEntityDataContainer> data)
        {
            // let entitys object merge themselves with their data

        }

        #endregion

        #region Helper methods
        private void attachEntityObject(EntityObject entity, EEntityState state)
        {
            // add into dictionaries
            entityByEntityKey.Add(entity.EntityKey, entity);
            entityByDataKey.Add(entity.DataKey, entity);
            // begin tracking
            addEntityTracking(entity, state);
            // set the entity manager for an entity object
            entity.EntityManager = this;
        }
        private void addEntityTracking(EntityObject entity, EEntityState state)
        {
            this.entityStateManager.AddTracker(entity, state);
        }
        #endregion

        #region IRefreshableEntityManager Members
        public void Refresh()
        {
            IEnumerable<IEntityDataContainer> refreshedEntityData;
            // call data adapter.GetChanges
            if (!dataAdapter.GetChanges(out refreshedEntityData))
                return;

            // merge new data

        }
        public void Refresh(IEnumerable<EntityObject> entities)
        {
            // call data adapter.GetChanges and merge it
        }
        public void Refresh(EntityObject entity)
        {
            // call data adapter.GetChanges and merge it
        }
        #endregion

        #region IUpdatableEntityManager Members
        public void SaveChanges()
        {
            throw new NotImplementedException();
        }

        public void SaveChanges(IEnumerable<EntityObject> entities)
        {
            throw new NotImplementedException();
        }
        public void SaveChanges(EntityObject entity)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region IRevertibleChangeTracking Members
        public void RejectChanges()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region IChangeTracking Members
        public void AcceptChanges()
        {
            throw new NotImplementedException();
        }
        public bool IsChanged
        {
            get { throw new NotImplementedException(); }
        }
        #endregion

        #region IEntityManagerWithErrorReporting Members
        public bool HasErrors { get; private set; }
        public IEnumerable<EntityObject> EntitiesInError
        {
            get { throw new NotImplementedException(); }
        }
        #endregion

        #region IEntityManager Members
        public IEntityManager ParentManager
        {
            get { return this.parentManager; }
            private set
            {
                if (value != null &&
                    value.GetType() == GetType())
                {
                    this.parentManager = value;
                }
            }
        }
        #endregion

        #region IEntityManager Members
        public Predicate<EntityObject> BelongsTo
        {
            get { return this.belongsToPredicate; }
        }
        #endregion

        #region IEntityManager Members
        public event EntitiesAddedHandler EntitiesAdded;
        public event EntitiesModifiedHandler EntitiesModified;
        public event EntitiesDeletedHandler EntitiesDeleted;
        #endregion

        #region Helper methods
        private void OnEntitiesAdded(IEnumerable<EntityObject> added)
        {
            EntitiesChangedHandler handler = this.EntitiesAdded;

            if (handler != null)
            {
                handler(added);
            }
        }
        private void OnEntitiesModified(IEnumerable<EntityObject> modified)
        {
            EntitiesChangedHandler handler = this.EntitiesModified;

            if (handler != null)
            {
                handler(modified);
            }
        }
        private void OnEntitiesDeleted(IEnumerable<EntityObject> deleted)
        {
            EntitiesChangedHandler handler = this.EntitiesDeleted;

            if (handler != null)
            {
                handler(deleted);
            }
        }
        #endregion
    }


}