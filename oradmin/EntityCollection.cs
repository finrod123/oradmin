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
        void DeleteObject(TEntity entity);

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
            // set collectionviewsource source collection
            this.viewSource.Source = this.entities;
            // set up event handling
            this.manager.EntitiesAdded += new EntitiesChangedHandler<TEntity, TData, TKey>(manager_EntitiesAdded);
            this.manager.EntitiesModified += new EntitiesChangedHandler<TEntity, TData, TKey>(manager_EntitiesModified);
            this.manager.EntitiesDeleted += new EntitiesChangedHandler<TEntity, TData, TKey>(manager_EntitiesDeleted);
            this.manager.EntitiesAttached += new EntitiesChangedHandler<TEntity, TData, TKey>(manager_EntitiesAttached);
            this.manager.EntitiesDetached += new EntitiesChangedHandler<TEntity, TData, TKey>(manager_EntitiesDetached);
        }
        #endregion

        #region IEntityCollection<TEntity,TData,TKey> Members
        /// <summary>
        /// Loads the contents of the entity collection via entity manager
        /// </summary>
        public void Load()
        {
            IEnumerable<TEntity> loadedEntities;

            if (!this.manager.Load(out loadedEntities))
            {
                this.Loaded = false;
                return;
            }

            // do some merging
        }
        public bool Loaded { get; private set; }
        public bool Refresh()
        {
            IEnumerable<TEntity> refreshedEntities;

            if (!this.manager.Refresh(out refreshedEntities))
            {
                return false;
            }

            // do some merging

            return true;
        }
        public void AddObject(TEntity entity)
        {
            this.manager.AddObject(entity);
        }
        public void DeleteObject(TEntity entity)
        {
            this.manager.DeleteObject(entity);
        }
        public abstract bool BelongsTo(TEntity entity);
        public ICollectionView EntityView
        {
            get { return this.viewSource.View; }
        }
        #endregion

        #region Helper methods
        void manager_EntitiesDeleted(IEnumerable<TEntity> changed)
        {
            throw new NotImplementedException();
        }
        void manager_EntitiesModified(IEnumerable<TEntity> changed)
        {
            throw new NotImplementedException();
        }
        void manager_EntitiesAdded(IEnumerable<TEntity> changed)
        {
            
        }
        void manager_EntitiesDetached(IEnumerable<TEntity> changed)
        {
            throw new NotImplementedException();
        }
        void manager_EntitiesAttached(IEnumerable<TEntity> changed)
        {
            foreach (TEntity entity in changed)
                if (this.BelongsTo(entity))
                    this.attachObject(entity);
        }
        void attachObject(TEntity entity)
        {
            this.entitiesByKey.Add(entity.DataKey, entity);
            
        }
        void detachObject(TEntity entity)
        {

        }
        #endregion
    }
}