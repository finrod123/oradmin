using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Data;

namespace oradmin
{
    using EntitiesStateChangeInfo = IEnumerable<KeyValuePair<IEntityObjectWithDataKeyAndStateInfo<TKey>, EEntityState>>;

    #region Delegates for entity changes notification
	
    #region Entity State Changed members
		// definice delegatu pro notifikace manageru
    public class EntityStateChangedEventArgs<TKey> : EventArgs
        where TKey : IEquatable<TKey>
    {
        #region Members
        public EEntityState OldState { get; private set; }
        public IEntityObjectWithDataKeyAndStateInfo<TKey> Entity { get; private set; }
        #endregion

        #region Constructor
        public EntityStateChangedEventArgs(
            IEntityObjectWithDataKeyAndStateInfo<TKey> entity,
            EEntityState oldState)
        {
            this.Entity = entity;
            this.OldState = oldState;
        }
        #endregion
    }
    public delegate void EntityStateChangedHandler<TKey>(
        object sender, EntityStateChangedEventArgs<TKey> e)
        where TKey : IEquatable<TKey>;
    public interface INotifyEntityStateChanged<TKey>
        where TKey : IEquatable<TKey>
    {
        event EntityStateChangedHandler<TKey> EntityStateChanged;
    }

	#endregion
    
    #region Entities State Changed members
	public class EntitiesStateChangedEventArgs<TKey> : EventArgs
        where TKey : IEquatable<TKey>
    {
        #region Members
        public EntitiesStateChangeInfo EntitiesInfo { get; private set; }
        #endregion

        #region Constructor
        public EntitiesStateChangedEventArgs(EntitiesStateChangeInfo entitiesInfo)
        {
            this.EntitiesInfo = entitiesInfo;
        }
        #endregion
    }
    public delegate void EntitiesStateChangedHandler<TKey>(
        object sender, EntitiesStateChangedEventArgs<TKey> e
        )
        where TKey : IEquatable<TKey>;
    public interface INotifyEntitiesStateChanged<TKey>
        where TKey : IEquatable<TKey>
    {
        event EntitiesStateChangedHandler<TKey> EntitiesStateChanged;
    }

	#endregion
    
    #region Entity Data Changed members
		public class EntityDataChangedEventArgs<TKey> : EventArgs
        where TKey : IEquatable<TKey>
    {
        #region Members
        public IEntityObjectWithDataKey<TKey> Entity { get; private set; }
	    #endregion

        #region Constructor
		public EntityDataChangedEventArgs(IEntityObjectWithDataKey<TKey> entity)
        {
            this.Entity = entity;
        }
	    #endregion
    }
    public delegate void EntityDataChangedHandler<TKey>(
        object sender,
        EntityDataChangedEventArgs<TKey> e)
            where TKey : IEquatable<TKey>;
    public interface INotifyEntityDataChanged<TKey>
        where TKey : IEquatable<TKey>
    {
        event EntityDataChangedHandler<TKey> EntityDataChanged;
    }

	#endregion
    
    #region Entities Data Changed members
	public class EntitiesDataChangedEventArgs<TKey> : EventArgs
        where TKey : IEquatable<TKey>
    {
        #region Members
        public IEnumerable<IEntityObjectWithDataKey<TKey>> Entities { get; private set; }
	    #endregion

        #region Constructor
        public EntitiesDataChangedEventArgs(IEnumerable<IEntityObjectWithDataKey<TKey>> entities)
        {
            this.Entities = entities;
        }
	    #endregion
    }
    public delegate void EntitiesDataChangedHandler<TKey>(
        object sender,
        EntitiesDataChangedEventArgs<TKey> e)
            where TKey : IEquatable<TKey>;
    public interface INotifyEntitiesDataChanged<TKey>
        where TKey : IEquatable<TKey>
    {
        event EntitiesDataChangedHandler<TKey> EntitiesDataChanged;
    }
	#endregion

    #region Entity Existence Changed members
	public class EntityExistenceChangedEventArgs<TKey> : EventArgs
        where TKey : IEquatable<TKey>
    {
        #region Members
        public IEntityObjectWithDataKey<TKey> Entity { get; private set; }
	    #endregion

        #region Constructor
        public EntityExistenceChangedEventArgs(IEntityObjectWithDataKey<TKey> entity)
        {
            this.Entity = entity;
        }
	    #endregion
    }
    public delegate void EntityExistenceChangedHandler<TKey>(
        object sender,
        EntityExistenceChangedEventArgs<TKey> e)
            where TKey : IEquatable<TKey>;
    
    public interface INotifyEntityDeleted<TKey>
        where TKey : IEquatable<TKey>
    {
        event EntityExistenceChangedHandler<TKey> EntityDeleted;
    }

    public interface INotifyEntityAttached<TKey>
        where TKey : IEquatable<TKey>
    {
        event EntityExistenceChangedHandler<TKey> EntityAttached;
    }

    public interface INotifyEntityAdded<TKey>
        where TKey : IEquatable<TKey>
    {
        event EntityExistenceChangedHandler<TKey> EntityAdded;
    }

    public interface INotifyEntityDetached<TKey>
        where TKey : IEquatable<TKey>
    {
        event EntityExistenceChangedHandler<TKey> EntityDetached;
    }

    public interface INotifyEntityExistenceChanged<TKey> :
        INotifyEntityAdded<TKey>,
        INotifyEntityAttached<TKey>,
        INotifyEntityDeleted<TKey>,
        INotifyEntityDetached<TKey>
        where TKey : IEquatable<TKey>
    { }
    #endregion
	
    #region Entities Existence Changed members
    public class EntitiesExistenceChangedEventArgs<TKey> : EventArgs
    {
        #region Members
		public IEnumerable<IEntityObjectWithDataKey<TKey>> Entities { get; private set; }
	    #endregion

        #region Constructor
        public EntitiesExistenceChangedEventArgs(
            IEnumerable<IEntityObjectWithDataKey<TKey>> entities)
        {
            this.Entities = entities;
        }
	    #endregion
    }
	
    public delegate void EntitiesExistenceChangedHandler<TKey>(
        object sender,
        EntitiesExistenceChangedEventArgs<TKey> e)
            where TKey : IEquatable<TKey>;

    public interface INotifyEntitiesExistenceChanged<TKey>
        where TKey : IEquatable<TKey>
    {
        event EntitiesExistenceChangedHandler<TKey> EntitiesAdded;
        event EntitiesExistenceChangedHandler<TKey> EntitiesAttached;
        event EntitiesExistenceChangedHandler<TKey> EntitiesDeleted;
        event EntitiesExistenceChangedHandler<TKey> EntitiesDetached;
    }
	#endregion

	#endregion

    public interface IEntityCollectionErrorIndicator<TEntity, TData, TKey>
        where TEntity : IEntityObject<TData, TKey>
        where TData : IEntityDataContainer<TKey>
        where TKey : IEquatable<TKey>
    {
        IEnumerable<TEntity> EntitiesInError { get; }
    }

    /// <summary>
    /// Interface to serve EntityCollection
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <typeparam name="TData">Entity data container type</typeparam>
    /// <typeparam name="TKey">Data key type</typeparam>
    public interface IEntityManagerBase<TEntity, TData, TKey>:
        IErrorIndicator,
        INotifyHasErrorsChanged,
        IEntityCollectionErrorIndicator<TEntity, TData, TKey>,
        INotifyEntitiesHasErrorsChanged,
        INotifyEntityExistenceChanged<TKey>,
        INotifyEntitiesExistenceChanged<TKey>,
        INotifyEntityStateChanged<TKey>,
        INotifyEntitiesStateChanged<TKey>,
        INotifyEntityDataChanged<TKey>,
        INotifyEntitiesDataChanged<TKey>
        where TEntity : IEntityObject<TData, TKey>
        where TData : IEntityDataContainer<TKey>
        where TKey : IEquatable<TKey>
    {
        bool Load(IEntityCollection<TEntity, TData, TKey> entityCollection);
        bool Refresh(IEntityCollection<TEntity, TData, TKey> entityCollection);
        void AddObject(TEntity entity);
    }

    /// <summary>
    /// Interface which servers an entity object
    /// </summary>
    /// <typeparam name="TKey">Data key type</typeparam>
    public interface IEntityManagerForEntityObject<TKey>
        where TKey : IEquatable<TKey>
    {
        void Refresh(IEntityObjectWithDataKey<TKey> entity);
        void SaveChanges(IEntityObjectWithDataKeyAndStateInfo<TKey> entity);
    }

    
    /// <summary>
    /// Complex entity manager interface
    /// </summary>
    /// <typeparam name="TEntity">Type of the entity managed</typeparam>
    /// <typeparam name="TData">Entity data container type</typeparam>
    /// <typeparam name="TKey">Data key type</typeparam>
    public interface IEntityManager<TEntity, TData, TKey> :
        IEntityManagerBase<TEntity, TData, TKey>,
        IEntityManagerForEntityObject<TKey>,
        IRefreshableObject, IUpdatableObject
        where TEntity : IEntityObject<TData, TKey>
        where TData   : IEntityDataContainer<TKey>
        where TKey    : IEquatable<TKey>
    {
        bool Loaded { get; }
        void Load();

        bool Refresh(TEntity entity);
        bool Refresh(IEnumerable<TEntity> entities);

        void SaveChanges(TEntity entity);
        void SaveChanges(IEnumerable<TEntity> entities);

        void AttachObject(TEntity entity);
        void DetachObject(TEntity entity);

        TEntity CreateObject();

        IEntityCollection<TEntity, TData, TKey> Entities { get;}
    }

    
    /// <summary>
    /// Abstract base class for entity manager implementation
    /// </summary>
    /// <typeparam name="TEntity">Type of the entity managed</typeparam>
    /// <typeparam name="TData">Entity data container type</typeparam>
    /// <typeparam name="TKey">Data key type</typeparam>
    public abstract class EntityManager<TEntity, TData, TKey> :
        IEntityManager<TEntity, TData, TKey>,
        IRevertibleChangeTracking
        where TEntity : EntityObjectBase<TData, TKey>
        where TData : class, IEntityDataContainer<TKey>
        where TKey : IEquatable<TKey>
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
            if (canBeAttached(entity))
            {
                attachEntityObject(entity, EEntityState.Added);
            }
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
            if (entityByEntityKey.ContainsKey(entity.EntityKey) ||
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
    }
}