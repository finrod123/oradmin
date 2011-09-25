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
    using PropertyAttributesPair = KeyValuePair<string, IEnumerable<Attribute>>;
    using PropertyAttributesLists = IEnumerable<KeyValuePair<string, IEnumerable<Attribute>>>;
    using EntityAttributesList = IEnumerable<Attribute>;

    public interface IEntityWithErrorState
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

    public interface IEntityChangeTracker
    {
        EEntityState EntityState { get; }
        void EntityMemberChanging(string member);
        void EntityMemberChanged(string member, object value);
    }

    public interface IEntityWithChangeTracker
    {
        void SetChangeTracker(IEntityChangeTracker tracker);
    }

    public class EntityKey : IEquatable<EntityKey>
    {
        #region Constructor
        public EntityKey(EntityObject entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            Entity = entity;
        }
        #endregion
        
        #region Properties
        public EntityObject Entity { get; private set; }
        #endregion

        #region IEquatable<EntityKey> Members
        public bool Equals(EntityKey other)
        {
            return object.ReferenceEquals(this, other);
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

    public abstract class EntityObject : IEditableObject, IRevertibleChangeTracking,
        INotifyPropertyChanging, INotifyPropertyChanged, IDataErrorInfo, IEntityWithChangeTracker,
        IUpdatableEntityObject, IRefreshableEntityObject, IEntityWithErrorState
    {
        #region Members
        protected EntityKey key;
        protected EEntityState state;
        protected IEntityValidator validator;
        protected IEntityChangeTracker changeTracker;

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
        public abstract bool IsChanged;
        #endregion

        #region INotifyPropertyChanging Members
        public event PropertyChangingEventHandler PropertyChanging;
        #endregion

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion


        #region Helper methods
        private void OnPropertyChanging(string property)
        {
            PropertyChangingEventHandler handler = this.PropertyChanging;

            if (handler != null)
            {
                handler(this, new PropertyChangingEventArgs(property));
            }
        }
        private void OnPropertyChanged(string property)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(property));
            }
        }
        #endregion

        #region IDataErrorInfo Members
        public string Error
        {
            get { return validator.Error; }
        }
        public string this[string columnName]
        {
            get { return validator[columnName]; }
        }
        #endregion
    }

    public class EntityObjectChangeTracker : IEntityChangeTracker, IEditableObject,
        IRevertibleChangeTracking
    {

        #region IEntityChangeTracker Members

        public EEntityState EntityState
        {
            get { throw new NotImplementedException(); }
        }

        public void EntityMemberChanging(string member)
        {
            throw new NotImplementedException();
        }

        public void EntityMemberChanged(string member, object value)
        {
            throw new NotImplementedException();
        }

        #endregion
        #region IEditableObject Members

        public void BeginEdit()
        {
            throw new NotImplementedException();
        }

        public void CancelEdit()
        {
            throw new NotImplementedException();
        }

        public void EndEdit()
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

    public interface IEntityDataContainer
    {
        IEquatableObject Key { get; }
    }
}
