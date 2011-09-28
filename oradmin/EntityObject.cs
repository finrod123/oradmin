using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Collections.Specialized;

namespace oradmin
{
    using PropertysPair = KeyValuePair<string, IEnumerable<Attribute>>;
    using PropertyAttributesLists = IEnumerable<KeyValuePair<string, IEnumerable<Attribute>>>;
    using EntityAttributesList = IEnumerable<Attribute>;

    public delegate void PropertyChangedPassingValueHandler(object sender, PropertyChangedPassingValueEventArgs e);
    public class PropertyChangedPassingValueEventArgs : PropertyChangedEventArgs
    {
        #region Constructor
		public PropertyChangedPassingValueEventArgs(string propertyName, object value) :
            base(propertyName)
        {
            Value = value;
        } 
	    #endregion

        #region Properties
        public object Value { get; private set; } 
        #endregion
    }

    public interface INotifyPropertyChangedPassingValue
    {
        event PropertyChangedPassingValueHandler PropertyChangedPassingValue;
    }

    public interface IEntityWithErrorReporting
    {
        bool HasErrors { get; }
    }

    public interface IRefreshableEntityObject
    {
        void Refresh();
    }

    public interface IUpdatableEntityObject
    {
        void SaveChanges();
    }

    public interface IEntityChangeTrackerBase
    {
        bool HasTracker { get; }
    }

    public interface IEntityWithChangeTracker : IEntityChangeTrackerBase
    {
        void SetChangeTracker(IEntityChangeTracker tracker);
    }

    public interface IEntityWithDeletableChangeTracker : IEntityChangeTrackerBase
    {
        void SetChangeTracker(IDeletableEntityChangeTracker tracker);
    }

    public class EntityKey<TData, TKey> : IEquatable<EntityKey<TData, TKey>>
        where TData : IEntityDataContainer<TKey>
        where TKey  : IEquatable<TKey>
    {
        #region Constructor
        public EntityKey(IEntityObjectBase<TData, TKey> entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            if (entity.EntityKey != null)
                throw new ApplicationException("Entity already associated!");

            Entity = entity;
        }
        #endregion

        #region Properties
        public IEntityObjectBase Entity { get; private set; }
        #endregion

        #region IEquatable<EntityKey> Members
        public bool Equals(EntityKey other)
        {
            return object.ReferenceEquals(this.Entity, other.Entity);
        }
        #endregion

        #region Object override
        public override bool Equals(object obj)
        {
            EntityKey other = obj as EntityKey;

            if (other != null)
                return this.Equals(other);

            return base.Equals(obj);
        }
        #endregion
    }

    public enum EEntityState
    {
        Unchanged,
        Added,
        Modified,
        Deleted,
        Detached
    }

    /// <summary>
    /// Serves EntityKey class
    /// </summary>
    public interface IEntityObjectBase<TData, TKey>
        where TData : IEntityDataContainer<TKey>
        where TKey  : IEquatable<TKey>
    {
        EntityKey<TData, TKey> EntityKey { get; }
        IEntityManager<TData, TKey> Manager { get; }
    }

    public interface IEntityObject<TData, TKey> : IEntityObjectBase<TData, TKey>,
        IEditableObject, IRevertibleChangeTracking,
        INotifyPropertyChanging, INotifyPropertyChanged, INotifyPropertyChangedPassingValue,
        IDataErrorInfo, IEntityWithDeletableChangeTracker, IEntityWithErrorReporting,
        IEntityDataContainer<TKey>
        where TData : IEntityDataContainer<TKey>
        where TKey  : IEquatable<TKey>
    {
        EEntityState EntityState { get; }
    }

    public abstract class EntityObject<TData, TKey> : IEntityObject<TData, TKey>
            where TData : class, IEntityDataContainer<TKey>
            where TKey  : IEquatable<TKey>
    {
        #region Members
        protected EntityKey<TData, TKey> entityKey;
        protected EEntityState entityState;
        protected IEntityValidator validator;
        protected IDeletableEntityChangeTracker changeTracker;
        protected IEntityManager<TData, TKey> manager;
        #endregion

        #region Static members
        protected static EntityAttributesList entityAttributes;
        protected static PropertyAttributesLists propertyAttributes;
        protected static Type entityType;

        public static EntityAttributesList GetEntityAttributes()
        {
            return entityAttributes.AsEnumerable();
        }
        public static PropertyAttributesLists GetPropertyAttributes()
        {
            return propertyAttributes.AsEnumerable();
        }
        protected static void Initialize(Type type)
        {
            if (!SetEntityType(type))
                return;

            LoadTypeInfo();
        }
        private static bool SetEntityType(Type type)
        {
            if (type == null ||
               entityType != null ||
               !type.IsSubclassOf(typeof(EntityObject)))
            {
                return false;
            }

            entityType = type;

            return true;
        }
        private static void LoadTypeInfo()
        {
            LoadEntityAttributes();
            LoadPropertyAttributes();
        }
        private static void LoadEntityAttributes()
        {
            entityAttributes = entityType.GetCustomAttributes(true) as EntityAttributesList;
        }
        private static void LoadPropertyAttributes()
        {
            propertyAttributes = new List<PropertyAttributesPair>();

            foreach (PropertyInfo p in entityType.GetProperties())
            {
                IEnumerable<Attribute> atts = Attribute.GetCustomAttributes(p);

                if (atts.Count() > 0)
                {
                    (propertyAttributes as List<PropertyAttributesPair>).Add(
                        new KeyValuePair<string, IEnumerable<Attribute>>(
                            p.Name, atts));
                }
            }
        }
        #endregion

        #region Constructor
        protected EntityObject(IEntityManager<TData, TKey> manager)
        {
            if (manager == null)
                throw new ArgumentNullException("manager");

            this.manager = manager;
            // assign an entity key
            this.entityKey = new EntityKey<TData, TKey>(this);
            // assign a validator
            createValidator();
        }
        #endregion

        #region Helper methods
        protected abstract void createValidator();
        #endregion

        #region IEditableObject Members
        public abstract void BeginEdit();
        public abstract void CancelEdit();
        public abstract void EndEdit();
        #endregion

        #region IRevertibleChangeTracking Members
        public abstract void RejectChanges();
        #endregion

        #region IChangeTracking Members
        public abstract void AcceptChanges();
        public abstract bool IsChanged { get; }
        #endregion

        #region INotifyPropertyChanging Members
        public event PropertyChangingEventHandler PropertyChanging;
        #endregion

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Helper methods
        protected void OnPropertyChanging(string property)
        {
            PropertyChangingEventHandler handler = this.PropertyChanging;

            if (handler != null)
            {
                handler(this, new PropertyChangingEventArgs(property));
            }
        }
        protected void OnPropertyChanged(string property)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(property));
            }
        }
        protected void OnPropertyChangedPassingValue(string propertyName, object value)
        {
            PropertyChangedPassingValueHandler handler = this.PropertyChangedPassingValue;

            if (handler != null)
            {
                handler(this, new PropertyChangedPassingValueEventArgs(propertyName, value));
            }
        }
        protected void reportMemberChanging(string member)
        {
            this.changeTracker.EntityMemberChanging(member);
            OnPropertyChanging(member);
        }
        protected void reportMemberChanged<TMember>(string member, TMember value)
        {
            this.changeTracker.EntityMemberChanged(member, value);
            OnPropertyChanged(member);
        }

        #endregion

        #region IDataErrorInfo Members
        public string Error
        {
            get { return (validator as IDataErrorInfo).Error; }
        }
        public string this[string columnName]
        {
            get { return (validator as IDataErrorInfo)[columnName]; }
        }
        #endregion

        #region Properties
        public bool IsEditing { get; private set; }
        #endregion

        #region INotifyPropertyChangedPassingValue Members
        public event PropertyChangedPassingValueHandler PropertyChangedPassingValue;
        #endregion

        #region IEntityDataContainer Members
        public abstract TKey DataKey { get; private set; }
        #endregion

        #region Public methods
        public abstract void Merge(TData data,
            EMergeOptions mergeOptions);
        public void Delete()
        {
            if (HasTracker)
            {
                this.changeTracker.
            }
        }
        #endregion

        #region IEntityWithErrorReporting Members
        public abstract bool HasErrors { get; }
        #endregion

        #region IEntityObject<TKey> Members
        public IEntityManager<TData, TKey> Manager
        {
            get { return this.manager; }
        }
        #endregion

        #region IEntityWithChangeTracker Members
        public void SetChangeTracker(IDeletableEntityChangeTracker tracker)
        {
            if (this.changeTracker != null)
            {
                this.changeTracker.Dispose();
            }

            if (tracker == null ||
                    (tracker != null &&
                     tracker.Entity == this)
                )
            {
                this.changeTracker = tracker;
            }
        }
        public bool HasTracker
        {
            get { return this.changeTracker != null; }
        }
        #endregion

        #region IEntityObject<TData,TKey> Members
        public EEntityState EntityState
        {
            get
            {
                if (this.changeTracker != null)
                {
                    return this.changeTracker.EntityState;
                }

                return EEntityState.Detached;
            }
        }
        #endregion

        #region IEntityObjectBase<TData,TKey> Members
        public EntityKey<TData, TKey> EntityKey
        {
            get { return this.entityKey; }
        }
        #endregion
    }

    public interface IEntityDataContainer<TKey>
        where TKey : IEquatable<TKey>
    {
        TKey DataKey { get; }
    }

    public enum EMergeOptions
    {
        KeepCurrentValues,
        OverrideCurrentValues
    }
}
