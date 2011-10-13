using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Data;

namespace myentitylibrary
{
    public interface IRevertibleChangeTrackingWithEntitiesContext<TEntity, TData, TKey>
        where TEntity : IEntityObject<TData,TKey>
        where TData : IEntityDataContainer<TKey>
        where TKey : IEquatable<TKey>
    {
        void AcceptChanges(IEntityCollection<TEntity, TData, TKey> entities);
        void RejectChanges(IEntityCollection<TEntity, TData, TKey> entities);
        bool IsChanged(IEntityCollection<TEntity, TData, TKey> entities);
    }

    public interface IEntityCollection<TEntity, TData, TKey>
        where TEntity : class, IEntityObject<TData, TKey>
        where TData   : IEntityDataContainer<TKey>
        where TKey    : IEquatable<TKey>
    {
        void Load();
        bool Loaded { get; }
        bool Refresh();

        void AddObject(TEntity entity);
        
        ICollectionView EntityView { get; }
    }

    public abstract class EntityCollection<TEntity, TData, TKey> :
        IEntityCollection<TEntity, TData, TKey>,
        IRevertibleChangeTracking
        where TEntity : class, IEntityObject<TData, TKey>
        where TData : IEntityDataContainer<TKey>
        where TKey : IEquatable<TKey>
    {
        #region Members
        /// <summary>
        /// Entity manager
        /// </summary>
        IEntityManagerBase<TEntity, TData, TKey> manager;
        /// <summary>
        /// Entities organized by data key
        /// </summary>
        Dictionary<TKey, TEntity> entitiesByKey =
            new Dictionary<TKey, TEntity>();
        /// <summary>
        /// Entities in observable collection
        /// </summary>
        ObservableCollection<TEntity> entities =
            new ObservableCollection<TEntity>();
        /// <summary>
        /// Collection view source to encapsulate "entities" observable collection
        /// </summary>
        CollectionViewSource viewSource;
        #endregion

        #region Constructor
        public EntityCollection(IEntityManagerBase<TEntity, TData, TKey> manager)
        {
            if (manager == null)
                throw new ArgumentNullException("manager");

            this.manager = manager;
            // set up event handling
            
            this.viewSource = new CollectionViewSource();
            this.viewSource.Source = this.entities;
        }
        #endregion

        #region IEntityCollection<TEntity,TData,TKey> Members
        /// <summary>
        /// Loads the contents of the entity collection via entity manager
        /// </summary>
        public void Load()
        {
            this.Loaded = this.manager.Load(this);
        }
        public bool Loaded { get; private set; }
        public bool Refresh()
        {
            return this.manager.Refresh(this);
        }
        public void AddObject(TEntity entity)
        {
            this.manager.AddObject(entity);
        }
        protected abstract bool BelongsTo(TEntity entity);
        public ICollectionView EntityView
        {
            get { return this.viewSource.View; }
        }
        #endregion

        #region Helper methods
        private void setUpEventHandling()
        {
            this.manager.EntityAdded += new EntityExistenceChangedHandler<TKey>(manager_EntityAdded);
        }

        void manager_EntityAdded(object sender, EntityExistenceChangedEventArgs<TKey> e)
        {
            TEntity added = e.Entity as TEntity;
        }
        #endregion

        #region Event handlers helpers
        private void addEntity(IEntityObjectWithDataKey<TKey> entity)
        {
            // if the entity does not belong to the collection, ignore it
            if (!this.BelongsTo(entity))
                return;

            // add it to the internal data structures
            this.entitiesByKey.Add(entity.DataKey, ent
        }
        #endregion

        #region Event handlers
        
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
    }
}