using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Data;

namespace oradmin
{
    #region Delegates for entity changes notification
		// definice delegatu pro notifikace manageru
    public delegate void EntityStateChangedHandler<TKey>(
        IEntityObjectWithDataKeyAndStateInfo<TKey> entity)
        where TKey : IEquatable<TKey>; 
    public delegate void EntitiesStateChangedHandler<TKey>(
        IEnumerable<IEntityObjectWithDataKeyAndStateInfo<TKey>)
        where TKey : IEquatable<TKey>;

    public delegate void EntitiesDataChangedHandler<TKey>(
        IEnumerable<IEntityObjectWithDataKey<TKey>> changed)
        where TKey : IEquatable<TKey>;

    public delegate void EntityDataChangedHandler<TKey>(IEntityObjectWithDataKey<TKey> entity)
        where TKey : IEquatable<TKey>;

    public delegate void EntityExistenceChangedHandler<TKey>(IEntityObjectWithDataKey<TKey> entity)
        where TKey : IEquatable<TKey>;

    public delegate void EntitiesExistenceChangedHandler<TKey>(
        IEnumerable<IEntityObjectWithDataKey<TKey>> entities)
        where TKey : IEquatable<TKey>;

	#endregion

    #region Interfaces for notifying about entity changes
		public interface INotifyEntityStateChanged<TKey>
        where TKey : IEquatable<TKey>
    {
        event EntityStateChangedHandler<TKey> EntityStateChanged;
        event EntitiesStateChangedHandler<TKey> EntitiesStateChanged;
    }

    public interface INotifyEntityDataChanged<TKey>
        where TKey : IEquatable<TKey>
    {
        event EntityDataChangedHandler<TKey> EntityDataChanged;
        event EntitiesDataChangedHandler<TKey> EntitiesDataChanged;
    }

    public interface INotifyEntityExistenceChanged<TKey>
        where TKey : IEquatable<TKey>
    {
        event EntityExistenceChangedHandler<TKey> EntityAdded;
        event EntityExistenceChangedHandler<TKey> EntityAttached;
        event EntityExistenceChangedHandler<TKey> EntityDetached;

        event EntitiesExistenceChangedHandler<TKey> EntitiesAdded;
        event EntitiesExistenceChangedHandler<TKey> EntitiesAttached;
        event EntitiesExistenceChangedHandler<TKey> EntitiesDetached;
    }
	#endregion

    public interface IEntityCollectionErrorIndicator<TEntity, TData, TKey>:
        IErrorIndicator
        where TEntity : IEntityObject<TData, TKey>
        where TData : IEntityDataContainer<TKey>
        where TKey : IEquatable<TKey>
    {
        IEnumerable<TEntity> EntitiesInError { get; }
    }

    public interface IEntityManagerForEntityObject<TKey>
        where TKey : IEquatable<TKey>
    {
        void Refresh(IEntityObjectWithDataKey<TKey> entity);
        void SaveChanges(IEntityObjectWithDataKeyAndStateInfo<TKey> entity);
    }

    /// <summary>
    /// Interface to serve EntityCollection
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TData"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public interface IEntityManagerBase<TEntity, TData, TKey>:
        IEntityCollectionErrorIndicator<TEntity, TData, TKey>,
        INotifyEntityExistenceChanged<TKey>,
        INotifyEntityStateChanged<TKey>,
        INotifyEntityDataChanged<TKey>
        where TEntity : IEntityObject<TData, TKey>
        where TData : IEntityDataContainer<TKey>
        where TKey : IEquatable<TKey>
    {
        bool Load(IEntityCollection<TEntity, TData, TKey> entityCollection);
        bool Refresh(IEntityCollection<TEntity, TData, TKey> entityCollection);
        void AddObject(TEntity entity);
    }

    public interface IEntityManager<TEntity, TData, TKey> :
        IEntityManagerBase<TEntity, TData, TKey>,
        IEntityManagerForEntityObject<TKey>
        where TEntity : IEntityObject<TData, TKey>
        where TData   : IEntityDataContainer<TKey>
        where TKey    : IEquatable<TKey>
    {
        bool Loaded { get; }
        void Load();

        bool Refresh();
        bool Refresh(TEntity entity);
        bool Refresh(IEnumerable<TEntity> entities);

        void SaveChanges();
        void SaveChanges(TEntity entity);
        void SaveChanges(IEnumerable<TEntity> entities);

        void AttachObject(TEntity entity);
        void DetachObject(TEntity entity);

        TEntity CreateObject();

        IEntityCollection<TEntity, TData, TKey> Entities { get;}
    }

    
    public abstract class EntityManager<TEntity, TData, TKey> :
        IEntityManager<TEntity, TData, TKey>,
        IRevertibleChangeTracking
        where TEntity : EntityObject<TData, TKey>
        where TData   : class, IEntityDataContainer<TKey>
        where TKey    : IEquatable<TKey>
    {
        #region Members
        protected IEntityStateManager<TEntity, TData, TKey> entityStateManager;
        protected Dictionary<TKey, TEntity> entitiesByKey =
            new Dictionary<TKey, TEntity>();
        protected IEntityDataAdapter<TEntity, TData, TKey> dataAdapter;
        protected IEntityCollection<TEntity, TData, TKey> entityCollection;
        #endregion

        #region Constructor
        protected EntityManager(IEntityDataAdapter<TEntity, TData, TKey> dataAdapter)
        {
            if (dataAdapter == null)
                throw new ArgumentNullException("dataAdapter");

            // store the reference to the entity data adapter
            this.dataAdapter = dataAdapter;
            // create the entity state manager
            this.createEntityStateManager();
            // create the default entity collection
            this.createEntityCollection();
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
            entitiesByKey.Add(entity.DataKey, entity);
            
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
               entitiesByKey.ContainsKey(entity.DataKey))
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
        protected abstract void createEntityCollection();
        protected abstract void createEntityStateManager();
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
    }


}