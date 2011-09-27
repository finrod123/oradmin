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

    public interface IEntityWithChangeTracker
    {
        IEntityChangeTracker Tracker { get; set; }
    }

    public class EntityKey : IEquatable<EntityKey>
    {
        #region Constructor
        public EntityKey(IEntityObjectBase entity)
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
    public interface IEntityObjectBase
    {
        EntityKey EntityKey { get; }
        IEntityManager Manager { get; }
    }

    public interface IEntityObject<TKey> : IEntityObjectBase, IEditableObject, IRevertibleChangeTracking,
        INotifyPropertyChanging, INotifyPropertyChanged, INotifyPropertyChangedPassingValue,
        IDataErrorInfo, IEntityWithChangeTracker, IEntityWithErrorReporting,
        IEntityDataContainer<TKey>
        where TKey : IEquatable<TKey>
    {
        EEntityState EntityState { get; }
    }

    public abstract class EntityObject<TKey> : IEntityObject<TKey>
            //where TData : IEntityDataContainer<TKey>
            where TKey  : IEquatable<TKey>
    {
        #region Members
        protected EntityKey entityKey;
        protected EEntityState entityState;
        protected IEntityValidator validator;
        protected IEntityChangeTracker changeTracker;
        protected IEntityManager manager;

        /// <summary>
        /// attribute dictionary
        /// </summary>
        protected static EntityAttributesList entityAttributes;
        protected static PropertyAttributesLists propertyAttributes;
        protected static Type entityType;
        #endregion

        #region Static interface
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
        protected EntityObject(IEntityManager manager)
        {
            if (manager == null)
                throw new ArgumentNullException("manager");

            this.manager = manager;
            // assign an entity key
            this.entityKey = new EntityKey(this);
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

        #region Porperties
        public EntityKey EntityKey
        {
            get { return this.entityKey; }
            private set
            {
                this.entityKey = value;
            }
        }
        public EEntityState EntityState
        {
            get
            {
                if (this.changeTracker == null)
                    return EEntityState.Detached;

                return this.changeTracker.EntityState;
            }
        }
        public EntityManager EntityManager
        {
            get
            {
                return this.manager;
            }
            set
            {
                EntityManager manager = value as EntityManager;

                if (manager != null &&
                    manager.BelongsTo(this))
                {
                    this.manager = manager;
                }
            }
        }
        #endregion

        #region Public methods
        public abstract void Merge(IEntityDataContainer<TKey> data,
            EMergeOptions mergeOptions);
        #endregion

        #region IEntityWithErrorReporting Members

        public abstract bool HasErrors { get; }

        #endregion

        #region IEntityWithChangeTracker Members
        public IEntityChangeTracker Tracker
        {
            get
            {
                return this.changeTracker;
            }
            set
            {
                if (value != null)
                {
                    this.changeTracker = value;
                }
            }
        }
        #endregion

        #region IEntityObject<TKey> Members


        public IEntityManager Manager
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }

    public abstract class DataSourceEnabledEntityObject : EntityObject,
        IRefreshableEntityObject, IUpdatableEntityObject
    {

        public override void BeginEdit()
        {
            throw new NotImplementedException();
        }

        public override void CancelEdit()
        {
            throw new NotImplementedException();
        }

        public override void EndEdit()
        {
            throw new NotImplementedException();
        }

        public override void RejectChanges()
        {
            throw new NotImplementedException();
        }

        public override void AcceptChanges()
        {
            throw new NotImplementedException();
        }

        public override void Merge(IEntityDataContainer data)
        {
            throw new NotImplementedException();
        }

        #region IRefreshableEntityObject Members

        public void Refresh()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IUpdatableEntityObject Members

        public void SaveChanges()
        {
            throw new NotImplementedException();
        }

        #endregion

        public abstract bool HasErrors { get; }
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
