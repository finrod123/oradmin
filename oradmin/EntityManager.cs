using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Data;

namespace oradmin
{
    // definice delegatu pro notifikace manageru
    public delegate void EntitiesChangedHandler<TEntity, TData, TKey>(IEnumerable<TEntity> changed)
        where TEntity : IEntityObject<TData, TKey>
        where TData : IEntityDataContainer<TKey>
        where TKey : IEquatable<TKey>;

    public interface IEntityManagerBase<TEntity, TData, TKey>
        where TEntity : IEntityObject<TData, TKey>
        where TData : IEntityDataContainer<TKey>
        where TKey : IEquatable<TKey>
    {
        bool Load(out IEnumerable<TEntity> entities);
        bool Refresh(out IEnumerable<TEntity> entities);
        void AddObject(TEntity entity);
        void DeleteObject(TEntity entity);

        event EntitiesChangedHandler<TEntity, TData, TKey> EntitiesAdded;
        event EntitiesChangedHandler<TEntity, TData, TKey> EntitiesModified;
        event EntitiesChangedHandler<TEntity, TData, TKey> EntitiesDeleted;
        event EntitiesChangedHandler<TEntity, TData, TKey> EntitiesAttached;
        event EntitiesChangedHandler<TEntity, TData, TKey> EntitiesDetached;
    }


    

    public interface IEntityManager<TData, TKey>
        where TData : IEntityDataContainer<TKey>
        where TKey : IEquatable<TKey>
    {
        bool BelongsTo(TData keyedData);
    }



    public interface IEntityManager<TEntity, TData, TKey> : IEntityManager<TData, TKey>
        where TEntity : IEntityObject<TData, TKey>
        where TData   : IEntityDataContainer<TKey>
        where TKey    : IEquatable<TKey>
    {
        IEntityManager<TEntity, TData, TKey> ParentManager { get; }

        event EntitiesChangedHandler<TEntity, TKey> EntitiesAdded;
        event EntitiesChangedHandler<TEntity, TKey> EntitiesModified;
        event EntitiesChangedHandler<TEntity, TKey> EntitiesDeleted;

        bool Loaded { get; }
        void Load();

        void AddObject(TEntity entity);
        void AttachObject(TEntity entity);
        void DeleteObject(TEntity entity);
        void DetachObject(TEntity entity);

        ListCollectionView View { get; }
        TEntity CreateObject();
        void MergeData(IEnumerable<TData> data);
    }

    public interface IRefreshableEntityManager<TEntity, TData, TKey>
        where TEntity : EntityObject<TData, TKey>
        where TData   : IEntityDataContainer<TKey>
        where TKey    : IEquatable<TKey>
    {
        bool Refresh();
        bool Refresh(IEnumerable<TEntity> entities);
    }

    public interface IUpdatableEntityManager<TEntity, TData, TKey>
        where TEntity : EntityObject<TData, TKey>
        where TData : IEntityDataContainer<TKey>
        where TKey : IEquatable<TKey>
    {
        void SaveChanges();
        void SaveChanges(IEnumerable<TEntity> entities);
    }

    public interface IEntityManagerWithErrorReporting<TEntity, TData, TKey>
        where TEntity : EntityObject<TKey>
        where TKey : IEquatable<TKey>
    {
        bool HasErrors { get; }
        IEnumerable<TEntity> EntitiesInError { get; }
    }

    public abstract class EntityManager<TEntity, TData, TKey> :
        IEntityManager,
        IEntityManager<TEntity, TData, TKey>,
        IRefreshableEntityManager<TEntity, TKey>,
        IUpdatableEntityManager<TEntity, TKey>,
        IRevertibleChangeTracking, IEntityManagerWithErrorReporting<TEntity, TKey>
        where TEntity : EntityObject<TData, TKey>
        where TData   : class, IEntityDataContainer<TKey>
        where TKey    : IEquatable<TKey>
    {
        #region Members
        protected IEntityManager parentManager;
        protected EntityStateManager<TEntity, TKey> entityStateManager;
        protected Dictionary<EntityKey<TData, TKey>, TEntity> entityByEntityKey =
            new Dictionary<EntityKey<TData, TKey>, TEntity>();
        protected Dictionary<TKey, TEntity> entityByDataKey =
            new Dictionary<TKey, TEntity>();
        protected EntityDataAdapter<TEntity, TData, TKey> dataAdapter;
        // observable collection of entities and collection view\
        ObservableCollection<EntityObject<TKey>> entities =
            new ObservableCollection<EntityObject<TData, TKey>>();
        CollectionViewSource view;
        #endregion

        #region Constructor
        protected EntityManager(IEntityManager parentManager,
            EntityDataAdapter<TEntity, TData, TKey> dataAdapter)
        {
            if (parentManager == null)
                throw new ArgumentNullException("parentManager");
            if (dataAdapter == null)
                throw new ArgumentNullException("dataAdapter");

            this.parentManager = parentManager;
            this.dataAdapter = dataAdapter;
            this.entityStateManager = new EntityStateManager<TEntity, TData, TKey>();
            this.view = new CollectionViewSource();
            this.view.Source = this.entities;
        }
        #endregion

        #region IEntityManager Members
        public bool Loaded { get; private set; }
        public void Load()
        {
            // use refresh
            if (Refresh())
            {
                Loaded = true;
            }
        }
        
        #region IRefreshableEntityManager Members
        public abstract bool Refresh();
        public abstract bool Refresh(IEnumerable<TEntity> entities);
        #endregion
        
        public void AttachObject(TEntity entity)
        {
            if (canBeAttached(entity))
            {
                attachEntityObject(entity, EEntityState.Unchanged);
            }
        }
        /// <summary>
        /// Adds an entity to a manager -> only if it is valid
        /// </summary>
        /// <param name="entity"></param>
        public void AddObject(TEntity entity)
        {
            if(canBeAttached(entity))
            {
                attachEntityObject(entity, EEntityState.Added);
            }
        }
        public void DeleteObject(TEntity entity)
        {
            // mark object for deletion;
            // will be deleted when saveChanges() overload is called
            //IEntityChangeTracker tracker = entity.Tracker;
            
        }
        public void DetachObject(TEntity entity)
        {
            throw new NotImplementedException();
        }
        public abstract bool BelongsTo(TData keyedData);

        public abstract TEntity CreateObject();
        public void MergeData(IEnumerable<TData> data)
        {
            // let entities object merge themselves with their data

        }
        #endregion

        #region Helper methods
        private void attachEntityObject(TEntity entity, EEntityState state)
        {
            // add to the data structures
            // 1) add into observable collection
            this.entities.Add(entity);
            // 2) add into dictionaries
            entityByEntityKey.Add(entity.EntityKey, entity);
            entityByDataKey.Add(entity.DataKey, entity);
            
            // begin tracking
            addEntityTracking(entity, state);
        }
        private void addEntityTracking(TEntity entity, EEntityState state)
        {
            this.entityStateManager.AddTracker(entity, state);
        }
        private bool canBeAttached(TEntity entity)
        {
            // only entities that belong here can be attached
            if (!BelongsTo(entity as TData))
                return false;

            // only detached (= new from data source or newly create by createObject)
            if (entity.EntityState != EEntityState.Detached)
                return false;

            // already attached entities cannot be attached
            if(entityByEntityKey.ContainsKey(entity.EntityKey) ||
               entityByDataKey.ContainsKey(entity.DataKey))
            {
                return false;
            }

            // if it is being edited, stop it
            if (entity.IsEditing)
                entity.EndEdit();

            // entities with errors cannot be attached
            if (entity.HasErrors)
            {
                return false;
            }

            return true;
        }
        #endregion

        #region IUpdatableEntityManager Members
        public void SaveChanges()
        {
            throw new NotImplementedException();
        }
        public void SaveChanges(IEnumerable<TEntity> entities)
        {
            throw new NotImplementedException();
        }
        public void SaveChanges(TEntity entity)
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
        public IEnumerable<TEntity> EntitiesInError
        {
            get { throw new NotImplementedException(); }
        }
        #endregion

        #region IEntityManager Members
        public IEntityManager<TEntity, TData, TKey> ParentManager
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
        #endregion

        #region IEntityManager Members
        public event EntitiesChangedHandler<TEntity, TKey> EntitiesAdded;
        public event EntitiesChangedHandler<TEntity, TKey> EntitiesModified;
        public event EntitiesChangedHandler<TEntity, TKey> EntitiesDeleted;
        #endregion

        #region Helper methods
        private void OnEntitiesAdded(IEnumerable<TEntity> added)
        {
            EntitiesChangedHandler<TEntity, TKey> handler = this.EntitiesAdded;

            if (handler != null)
            {
                handler(added);
            }
        }
        private void OnEntitiesModified(IEnumerable<TEntity> modified)
        {
            EntitiesChangedHandler<TEntity, TKey> handler = this.EntitiesModified;

            if (handler != null)
            {
                handler(modified);
            }
        }
        private void OnEntitiesDeleted(IEnumerable<TEntity> deleted)
        {
            EntitiesChangedHandler<TEntity, TKey> handler = this.EntitiesDeleted;

            if (handler != null)
            {
                handler(deleted);
            }
        }
        #endregion

        #region IEntityManager<TEntity,TData,TKey> Members
        public ListCollectionView View
        {
            get { return this.view.View as ListCollectionView; }
        }
        #endregion
    }


}