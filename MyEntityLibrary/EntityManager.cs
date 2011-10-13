using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Data;

namespace myentitylibrary
{
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

    public class EntityStateChangedEventArgs<TEntity, TData, TKey> : EventArgs
        where TEntity : IEntityObject<TData, TKey>
        where TData : IEntityDataContainer<TKey>
        where TKey : IEquatable<TKey>
    {
        #region Members
        public EEntityState OldState { get; private set; }
        public TEntity Entity { get; private set; }
        #endregion

        #region Constructor
        public EntityStateChangedEventArgs(
            TEntity entity,
            EEntityState oldState)
        {
            this.Entity = entity;
            this.OldState = oldState;
        }
        #endregion
    }
    public delegate void EntityStateChangedHandler<TEntity, TData, TKey>(
        object sender, EntityStateChangedEventArgs<TEntity, TData, TKey> e)
        where TEntity : IEntityObject<TData, TKey>
        where TData : IEntityDataContainer<TKey>
        where TKey : IEquatable<TKey>;
    public interface INotifyEntityStateChanged<TEntity, TData, TKey>
        where TEntity : IEntityObject<TData, TKey>
        where TData : IEntityDataContainer<TKey>
        where TKey : IEquatable<TKey>
    {
        event EntityStateChangedHandler<TEntity, TData, TKey> EntityStateChanged;
    }
	#endregion
    
    #region Entities State Changed members
	public class EntitiesStateChangedEventArgs<TKey> : EventArgs
        where TKey : IEquatable<TKey>
    {
        #region Members
        public IEnumerable<KeyValuePair<IEntityObjectWithDataKeyAndStateInfo<TKey>, EEntityState>> EntitiesInfo { get; private set; }
        #endregion

        #region Constructor
        public EntitiesStateChangedEventArgs(
            IEnumerable<KeyValuePair<IEntityObjectWithDataKeyAndStateInfo<TKey>, EEntityState>> entitiesInfo)
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

    public class EntitiesStateChangedEventArgs<TEntity, TData, TKey> : EventArgs
        where TEntity : IEntityObject<TData, TKey>
        where TData : IEntityDataContainer<TKey>
        where TKey : IEquatable<TKey>
    {
        #region Members
        public IEnumerable<KeyValuePair<TEntity, EEntityState>> EntitiesInfo { get; private set; }
        #endregion

        #region Constructor
        public EntitiesStateChangedEventArgs(
            IEnumerable<KeyValuePair<TEntity, EEntityState>> entitiesInfo)
        {
            this.EntitiesInfo = entitiesInfo;
        }
        #endregion
    }
    public delegate void EntitiesStateChangedHandler<TEntity, TData, TKey>(
        object sender, EntitiesStateChangedEventArgs<TEntity, TData, TKey> e
        )
        where TEntity : IEntityObject<TData, TKey>
        where TData : IEntityDataContainer<TKey>
        where TKey : IEquatable<TKey>;

    public interface INotifyEntitiesStateChanged<TEntity, TData, TKey>
        where TEntity : IEntityObject<TData, TKey>
        where TData : IEntityDataContainer<TKey>
        where TKey : IEquatable<TKey>
    {
        event EntitiesStateChangedHandler<TEntity, TData, TKey> EntitiesStateChanged;
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

    public class EntityDataChangedEventArgs<TEntity, TData, TKey> : EventArgs
        where TEntity : IEntityObject<TData, TKey>
        where TData : IEntityDataContainer<TKey>
        where TKey : IEquatable<TKey>
    {
        #region Members
        public TEntity Entity { get; private set; }
        #endregion

        #region Constructor
        public EntityDataChangedEventArgs(TEntity entity)
        {
            this.Entity = entity;
        }
        #endregion
    }
    public delegate void EntityDataChangedHandler<TEntity, TData, TKey>(
        object sender,
        EntityDataChangedEventArgs<TEntity, TData, TKey> e)
            where TEntity : IEntityObject<TData, TKey>
            where TData : IEntityDataContainer<TKey>
            where TKey : IEquatable<TKey>;
    public interface INotifyEntityDataChanged<TEntity, TData, TKey>
        where TEntity : IEntityObject<TData, TKey>
        where TData : IEntityDataContainer<TKey>
        where TKey : IEquatable<TKey>
    {
        event EntityDataChangedHandler<TEntity, TData, TKey> EntityDataChanged;
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

    public class EntitiesDataChangedEventArgs<TEntity, TData, TKey> : EventArgs
        where TEntity : IEntityObject<TData, TKey>
        where TData : IEntityDataContainer<TKey>
        where TKey : IEquatable<TKey>
    {
        #region Members
        public IEnumerable<TEntity> Entities { get; private set; }
        #endregion

        #region Constructor
        public EntitiesDataChangedEventArgs(IEnumerable<TEntity> entities)
        {
            this.Entities = entities;
        }
        #endregion
    }
    public delegate void EntitiesDataChangedHandler<TEntity, TData, TKey>(
        object sender,
        EntitiesDataChangedEventArgs<TEntity, TData, TKey> e)
            where TEntity : IEntityObject<TData, TKey>
            where TData : IEntityDataContainer<TKey>
            where TKey : IEquatable<TKey>;
    public interface INotifyEntitiesDataChanged<TEntity, TData, TKey>
        where TEntity : IEntityObject<TData, TKey>
        where TData : IEntityDataContainer<TKey>
        where TKey : IEquatable<TKey>
    {
        event EntitiesDataChangedHandler<TEntity, TData, TKey> EntitiesDataChanged;
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
    public class EntityExistenceChangedEventArgs<TEntity, TData, TKey> : EventArgs
        where TEntity : IEntityObject<TData, TKey>
        where TData : IEntityDataContainer<TKey>
        where TKey : IEquatable<TKey>
    {
        #region Members
        public TEntity Entity { get; private set; }
        #endregion

        #region Constructor
        public EntityExistenceChangedEventArgs(TEntity entity)
        {
            this.Entity = entity;
        }
        #endregion
    }
    public delegate void EntityExistenceChangedHandler<TEntity, TData, TKey>(
        object sender,
        EntityExistenceChangedEventArgs<TEntity, TData, TKey> e)
            where TEntity : IEntityObject<TData, TKey>
            where TData : IEntityDataContainer<TKey>
            where TKey : IEquatable<TKey>;

    public interface INotifyEntityDeleted<TEntity, TData, TKey>
        where TEntity : IEntityObject<TData, TKey>
        where TData : IEntityDataContainer<TKey>
        where TKey : IEquatable<TKey>
    {
        event EntityExistenceChangedHandler<TEntity, TData, TKey> EntityDeleted;
    }

    public interface INotifyEntityAttached<TEntity, TData, TKey>
        where TEntity : IEntityObject<TData, TKey>
        where TData : IEntityDataContainer<TKey>
        where TKey : IEquatable<TKey>
    {
        event EntityExistenceChangedHandler<TEntity, TData, TKey> EntityAttached;
    }

    public interface INotifyEntityAdded<TEntity, TData, TKey>
        where TEntity : IEntityObject<TData, TKey>
        where TData : IEntityDataContainer<TKey>
        where TKey : IEquatable<TKey>
    {
        event EntityExistenceChangedHandler<TEntity, TData, TKey> EntityAdded;
    }

    public interface INotifyEntityDetached<TEntity, TData, TKey>
        where TEntity : IEntityObject<TData, TKey>
        where TData : IEntityDataContainer<TKey>
        where TKey : IEquatable<TKey>
    {
        event EntityExistenceChangedHandler<TEntity, TData, TKey> EntityDetached;
    }

    public interface INotifyEntityExistenceChanged<TEntity, TData, TKey> :
        INotifyEntityAdded<TKey>,
        INotifyEntityAttached<TKey>,
        INotifyEntityDeleted<TKey>,
        INotifyEntityDetached<TKey>
        where TEntity : IEntityObject<TData, TKey>
        where TData : IEntityDataContainer<TKey>
        where TKey : IEquatable<TKey>
    { }
    #endregion
	
    #region Entities Existence Changed members
    public class EntitiesExistenceChangedEventArgs<TKey> : EventArgs
        where TKey : IEquatable<TKey>
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

    public class EntitiesExistenceChangedEventArgs<TEntity, TData, TKey> : EventArgs
        where TEntity : IEntityObject<TData, TKey>
        where TData : IEntityDataContainer<TKey>
        where TKey : IEquatable<TKey>
    {
        #region Members
        public IEnumerable<TEntity> Entities { get; private set; }
        #endregion

        #region Constructor
        public EntitiesExistenceChangedEventArgs(IEnumerable<TEntity> entities)
        {
            this.Entities = entities;
        }
        #endregion
    }

    public delegate void EntitiesExistenceChangedHandler<TEntity, TData, TKey>(
        object sender,
        EntitiesExistenceChangedEventArgs<TEntity, TData, TKey> e)
            where TEntity : IEntityObject<TData, TKey>
            where TData : IEntityDataContainer<TKey>
            where TKey : IEquatable<TKey>;

    public interface INotifyEntitiesExistenceChanged<TEntity, TData, TKey>
        where TEntity : IEntityObject<TData, TKey>
        where TData : IEntityDataContainer<TKey>
        where TKey : IEquatable<TKey>
    {
        event EntitiesExistenceChangedHandler<TEntity, TData, TKey> EntitiesAdded;
        event EntitiesExistenceChangedHandler<TEntity, TData, TKey> EntitiesAttached;
        event EntitiesExistenceChangedHandler<TEntity, TData, TKey> EntitiesDeleted;
        event EntitiesExistenceChangedHandler<TEntity, TData, TKey> EntitiesDetached;
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
        IRevertibleChangeTrackingWithEntitiesContext<TEntity, TData, TKey>,
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
        bool Refresh(IEntityObjectWithDataKey<TKey> entity);
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
    public abstract class EntityManager<TEntityCollection, TEntity, TData, TKey> :
        IEntityManager<TEntity, TData, TKey>,
        IRevertibleChangeTracking
        where TEntityCollection : IEntityCollection<TEntity, TData, TKey>
        where TEntity : IEntityObject<TData, TKey>
        where TData : IEntityDataContainer<TKey>
        where TKey : IEquatable<TKey>
    {
        #region Members
        protected IEntityStateManager<TEntity, TData, TKey> entityStateManager;
        protected Dictionary<TKey, TEntity> entitiesByKey =
            new Dictionary<TKey, TEntity>();
        protected IEntityDataAdapter<TEntity, TData, TKey> dataAdapter;
        protected IEntityDataSaver<TEntityCollection, TEntity, TData, TKey> dataSaver;
        protected IEntityCollection<TEntity, TData, TKey> entityCollection;
        #endregion

        #region Error detection and management
        private HashSet<TEntity> entitiesInError =
            new HashSet<TEntity>();
        #endregion

        #region Constructor
        protected EntityManager(
            IEntityDataAdapter<TEntity, TData, TKey> dataAdapter,
            IEntityDataSaver<TEntityCollection, TEntity, TData, TKey> dataSaver)
        {
            if (dataAdapter == null)
                throw new ArgumentNullException("data adapter");
            if (dataSaver == null)
                throw new ArgumentNullException("data saver");

            // store the reference to the entity data adapter and data saver
            this.dataAdapter = dataAdapter;
            this.dataSaver = dataSaver;
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
            

        }

        #region IRefreshableEntityManager Members
        public abstract bool Refresh(IEnumerable<TEntity> entities);
        #endregion

        /// <summary>
        /// Adds an entity to a manager -> only if it is valid
        /// </summary>
        /// <param name="entity"></param>
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
        protected abstract void createEntityCollection();
        protected abstract void createEntityStateManager();
        #endregion

        #region Entity event handlers
        void entity_EntityDeleted(object sender, EntityExistenceChangedEventArgs<TKey> e)
        {
            this.OnEntityDeleted(e.Entity);
        }
        void entity_EntityDataChanged(object sender, EntityDataChangedEventArgs<TKey> e)
        {
            this.OnEntityDataChanged(e.Entity);
        }
        void entity_EntityStateChanged(object sender, EntityStateChangedEventArgs<TKey> e)
        {
            this.OnEntityStateChanged(e.Entity, e.OldState);
        }
        void entity_HasErrorChanged(object sender, EntityHasErrorChangedEventArgs e)
        {
            throw new NotImplementedException();
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

        #region IEntityManager<TEntity,TData,TKey> Members

        public bool Refresh(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public IEntityCollection<TEntity, TData, TKey> Entities
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IEntityManagerBase<TEntity,TData,TKey> Members

        public bool Load(IEntityCollection<TEntity, TData, TKey> entityCollection)
        {
            throw new NotImplementedException();
        }

        public bool Refresh(IEntityCollection<TEntity, TData, TKey> entityCollection)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IErrorIndicator Members

        public bool HasErrors
        {
            get { return false; }
        }

        #endregion

        #region INotifyHasErrorsChanged Members
        public event EntityHasErrorChangedHandler HasErrorChanged;
        private void OnHasErrorChanged()
        {
            EntityHasErrorChangedHandler handler = this.HasErrorChanged;

            if (handler != null)
            {
                handler(this, new EntityHasErrorChangedEventArgs(this));
            }
        }
        #endregion

        #region IEntityCollectionErrorIndicator<TEntity,TData,TKey> Members
        public IEnumerable<TEntity> EntitiesInError
        {
            get { return this.entitiesInError.AsEnumerable(); }
        }
        #endregion

        #region INotifyEntitiesHasErrorsChanged Members
        public event EntitiesHasErrorsChangedHandler EntitiesHasErrorsChanged;
        private void OnEntitiesHasErrorsChanged(IEnumerable<IErrorIndicator> errorChanges)
        {
            EntitiesHasErrorsChangedHandler handler = this.EntitiesHasErrorsChanged;

            if (handler != null)
            {
                handler(this, new EntitiesHasErrorsChangedEventArgs(errorChanges));
            }
        }
        #endregion

        #region INotifyEntityAdded<TKey> Members
        public event EntityExistenceChangedHandler<TEntity, TData, TKey> EntityAdded;
        private void OnEntityAdded(TEntity entity)
        {
            EntityExistenceChangedHandler<TEntity, TData, TKey> handler = this.EntityAdded;

            if (handler != null)
            {
                handler(this, new EntityExistenceChangedEventArgs<TEntity, TData, TKey>(entity));
            }
        }
        #endregion

        #region INotifyEntityAttached<TKey> Members
        public event EntityExistenceChangedHandler<TEntity, TData, TKey> EntityAttached;
        private void OnEntityAttached(TEntity entity)
        {
            EntityExistenceChangedHandler<TEntity, TData, TKey> handler = this.EntityAttached;

            if (handler != null)
            {
                handler(this, new EntityExistenceChangedEventArgs<TEntity, TData, TKey>(entity));
            }
        }
        #endregion

        #region INotifyEntityDeleted<TKey> Members
        public event EntityExistenceChangedHandler<TEntity, TData, TKey> EntityDeleted;
        private void OnEntityDeleted(TEntity entity)
        {
            EntityExistenceChangedHandler<TEntity, TData, TKey> handler = this.EntityDeleted;

            if (handler != null)
            {
                handler(this, new EntityExistenceChangedEventArgs<TEntity, TData, TKey>(entity));
            }
        }
        #endregion

        #region INotifyEntityDetached<TKey> Members
        public event EntityExistenceChangedHandler<TEntity, TData, TKey> EntityDetached;
        private void OnEntityDetached(TEntity entity)
        {
            EntityExistenceChangedHandler<TEntity, TData, TKey> handler = this.EntityDetached;

            if (handler != null)
            {
                handler(this, new EntityExistenceChangedEventArgs<TEntity, TData, TKey>(entity));
            }
        }
        #endregion

        #region INotifyEntitiesExistenceChanged<TKey> Members
        public event EntitiesExistenceChangedHandler<TEntity, TData, TKey> EntitiesAdded;
        private void OnEntitiesAdded(IEnumerable<TEntity> entities)
        {
            EntitiesExistenceChangedHandler<TEntity, TData, TKey> handler = this.EntitiesAdded;

            if (handler != null)
            {
                handler(this, new EntitiesExistenceChangedEventArgs<TEntity, TData, TKey>(entities));
            }
        }

        public event EntitiesExistenceChangedHandler<TEntity, TData, TKey> EntitiesAttached;
        private void OnEntitiesAttached(IEnumerable<TEntity> entities)
        {
            EntitiesExistenceChangedHandler<TEntity, TData, TKey> handler = this.EntitiesAttached;

            if (handler != null)
            {
                handler(this, new EntitiesExistenceChangedEventArgs<TEntity, TData, TKey>(entities));
            }
        }

        public event EntitiesExistenceChangedHandler<TEntity, TData, TKey> EntitiesDeleted;
        private void OnEntitiesDeleted(IEnumerable<TEntity> entities)
        {
            EntitiesExistenceChangedHandler<TEntity, TData, TKey> handler = this.EntitiesDeleted;

            if (handler != null)
            {
                handler(this, new EntitiesExistenceChangedEventArgs<TEntity, TData, TKey>(entities));
            }
        }

        public event EntitiesExistenceChangedHandler<TEntity, TData, TKey> EntitiesDetached;
        private void OnEntitiesDetached(IEnumerable<TEntity> entities)
        {
            EntitiesExistenceChangedHandler<TEntity, TData, TKey> handler = this.EntitiesDetached;

            if (handler != null)
            {
                handler(this, new EntitiesExistenceChangedEventArgs<TEntity, TData, TKey>(entities));
            }
        }
        #endregion

        #region INotifyEntityStateChanged<TKey> Members
        public event EntityStateChangedHandler<TEntity, TData, TKey> EntityStateChanged;
        private void OnEntityStateChanged(
            TEntity entity, EEntityState oldState)
        {
            EntityStateChangedHandler<TEntity, TData, TKey> handler = this.EntityStateChanged;

            if (handler != null)
            {
                handler(this, new EntityStateChangedEventArgs<TEntity, TData, TKey>(entity, oldState));
            }
        }
        #endregion

        #region INotifyEntitiesStateChanged<TKey> Members
        public event EntitiesStateChangedHandler<TEntity, TData, TKey> EntitiesStateChanged;
        private void OnEntitiesStateChanged(
            IEnumerable<KeyValuePair<TEntity, EEntityState>> entities)
        {
            EntitiesStateChangedHandler<TEntity, TData, TKey> handler = this.EntitiesStateChanged;

            if (handler != null)
            {
                handler(this, new EntitiesStateChangedEventArgs<TEntity, TData, TKey>(entities));
            }
        }
        #endregion

        #region INotifyEntityDataChanged<TKey> Members
        public event EntityDataChangedHandler<TEntity, TData, TKey> EntityDataChanged;
        private void OnEntityDataChanged(TEntity entity)
        {
            EntityDataChangedHandler<TEntity, TData, TKey> handler = this.EntityDataChanged;

            if (handler != null)
            {
                handler(this, new EntityDataChangedEventArgs<TEntity, TData, TKey>(entity));
            }
        }
        #endregion

        #region INotifyEntitiesDataChanged<TKey> Members
        public event EntitiesDataChangedHandler<TEntity, TData, TKey> EntitiesDataChanged;
        private void OnEntitiesDataChanged(IEnumerable<TEntity> entities)
        {
            EntitiesDataChangedHandler<TEntity, TData, TKey> handler = this.EntitiesDataChanged;

            if (handler != null)
            {
                handler(this, new EntitiesDataChangedEventArgs<TEntity, TData, TKey>(entities));
            }
        }
        #endregion

        #region IEntityManagerForEntityObject<TKey> Members
        public bool Refresh(IEntityObjectWithDataKey<TKey> entity)
        {
            throw new NotImplementedException();
        }
        public void SaveChanges(IEntityObjectWithDataKeyAndStateInfo<TKey> entity)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region IRefreshableObject Members

        public bool Refresh()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region IEntityManager<TEntity,TData,TKey> Members
        /// <summary>
        /// Attaches the entity to a manager (=links together with change tracker,...)
        /// </summary>
        /// <param name="entity"></param>
        public void AttachObject(TEntity entity)
        {
            // attach the entity
            if (!this.attachObjectHelper(entity, EEntityState.Unchanged))
                return;
            
            // notify others abou the new entity
            this.OnEntityAttached(entity);
        }
        private bool attachObjectHelper(TEntity entity, EEntityState initialEntityState)
        {
            // can only attach detached entity
            if (entity.EntityState != EEntityState.Detached)
                throw new InvalidOperationException("Cannot attach already attached entity");

            // if still editing the entity, call end edit
            if (entity.IsEditing)
                entity.EndEdit();

            // if the entity is not valid, do not attach it
            if (entity.HasErrors)
                return false;

            // entity has no errors -> add it to internal structures
            this.entitiesByKey.Add(entity.DataKey, entity);

            // add the tracker
            this.entityStateManager.AddTracker(entity, initialEntityState);

            return true;
        }
        #endregion

        #region IEntityManagerBase<TEntity,TData,TKey> Members
        public void AddObject(TEntity entity)
        {
            // attach the entity with the "Added" state
            if (!this.attachObjectHelper(entity, EEntityState.Added))
                return;

            // notify others about the newly added entity
            this.OnEntityAdded(entity);
        }
        #endregion

        #region IRevertibleChangeTrackingWithEntitiesContext<TEntity,TData,TKey> Members

        public void AcceptChanges(IEntityCollection<TEntity, TData, TKey> entities)
        {
            throw new NotImplementedException();
        }

        public void RejectChanges(IEntityCollection<TEntity, TData, TKey> entities)
        {
            throw new NotImplementedException();
        }

        bool IRevertibleChangeTrackingWithEntitiesContext<TEntity, TData, TKey>.IsChanged(IEntityCollection<TEntity, TData, TKey> entities)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}