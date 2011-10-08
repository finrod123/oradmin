using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Data;

namespace oradmin
{
    public interface IEntityCollection<TEntity, TData, TKey>
        where TEntity : IEntityObject<TData, TKey>
        where TData   : IEntityDataContainer<TKey>
        where TKey    : IEquatable<TKey>
    {
        void Load();
        bool Loaded { get; }
        bool Refresh();

        void AddObject(TEntity entity);
        
        bool BelongsTo(TEntity entity);

        ICollectionView EntityView { get; }
    }

    public abstract class EntityCollection<TEntity, TData, TKey> :
        IEntityCollection<TEntity, TData, TKey>
        where TEntity : IEntityObject<TData, TKey>
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
            this.setUpExistenceChangeEventHandling();
            //this.setUpStateChangeEventHandling();
            //this.setUpDataChangeEventHandling();
            // set collectionviewsource source collection
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
        public abstract bool BelongsTo(TEntity entity);
        public ICollectionView EntityView
        {
            get { return this.viewSource.View; }
        }
        #endregion

        #region Helper methods
        void attachObject(TEntity entity)
        {
            this.entitiesByKey.Add(entity.DataKey, entity);
            this.entities.Add(entity);
        }
        void detachObject(TEntity entity)
        {
            this.entitiesByKey.Remove(entity.DataKey);
            this.entities.Remove(entity);
        }
        void setUpStateChangeEventHandling()
        {

        }


        void setUpExistenceChangeEventHandling()
        {
            
        }

        void setUpDataChangeEventHandling()
        {

        }
        #endregion

        #region Event handlers
        
        #endregion
    }
}