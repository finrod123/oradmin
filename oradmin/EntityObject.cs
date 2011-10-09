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
    public delegate void PropertyChangedPassingValueHandler(object sender, PropertyChangedPassingValueEventArgs e);
    public delegate void HasErrorChangedHandler(IErrorIndicator sender);

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

    public interface IEntityDataContainer<TKey>
        where TKey : IEquatable<TKey>
    {
        TKey DataKey { get; }
    }

    public interface IEntityObjectWithDataKey<TKey> :
        IEntityDataContainer<TKey>,
        IEquatable<IEntityObjectWithDataKey<TKey>>
        where TKey : IEquatable<TKey> { }

    public interface IEntityObjectWithDataKeyAndStateInfo<TKey> :
        IEntityObjectWithDataKey<TKey>, IEntityStateInfo
        where TKey : IEquatable<TKey>
    { }

    public interface IErrorIndicator
    {
        bool HasErrors { get; }
        event HasErrorChangedHandler HasErrorChanged;
    }

    public interface IRefreshableObject
    {
        void Refresh();
    }

    public interface IUpdatableObject
    {
        void SaveChanges();
    }

    public interface IEntityWithDeletableChangeTracker<TData, TKey>
        where TData : IEntityDataContainer<TKey>
        where TKey : IEquatable<TKey>
    {
        bool HasTracker { get; }
        void SetChangeTracker(IDeletableMergeableChangeTracker<TData, TKey> tracker);
    }

    
    public enum EEntityState
    {
        Unchanged,
        Added,
        Modified,
        Deleted,
        Detached
    }

    public interface IEntityObject<TData, TKey> :
        IEntityObjectWithDataKeyAndStateInfo<TKey>, IEntityWithDeletableChangeTracker<TData, TKey>,
        IEntityDataContainer<TKey>, IMergeableWithEntityDataContainer<TData, TKey>,
        IEditableObject, IDeletableObject, IRefreshableObject, IUpdatableObject,
        IEditableObjectInfo, IRevertibleChangeTracking,
        INotifyPropertyChanging, INotifyPropertyChanged, INotifyPropertyChangedPassingValue,
        IDataErrorInfo, IErrorIndicator
        where TData : IEntityDataContainer<TKey>
        where TKey : IEquatable<TKey>
    {
        IEntityManagerForEntityObject<TKey> Manager { get; }
    }

    public abstract class EntityObject<TData, TKey> : IEntityObject<TData, TKey>
            where TData : class, IEntityDataContainer<TKey>
            where TKey  : IEquatable<TKey>
    {
        #region Members
        protected IEntityValidator validator;
        protected IDeletableMergeableChangeTracker<TData, TKey> changeTracker;
        private bool hasErrors;
        #endregion

        #region Constructor
        protected EntityObject(IEntityManagerForEntityObject<TKey> manager)
        {
            if (manager == null)
                throw new ArgumentNullException("manager");

            this.Manager = manager;
        }
        #endregion
        
        #region IEditableObject Members
        public virtual void BeginEdit()
        {
            if (IsEditing)
                return;

            IsEditing = true;
            // call begin edit on a change tracker
            
        }
        public virtual void CancelEdit()
        {

        }
        public virtual void EndEdit()
        {

        }
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
        protected void OnHasErrorChanged()
        {
            HasErrorChangedHandler handler = this.HasErrorChanged;

            if (handler != null)
            {
                handler(this);
            }
        }
        protected void reportMemberChanging(string member)
        {
            if (!IsEditing)
                this.changeTracker.EntityMemberChanging(member);

            OnPropertyChanging(member);
        }
        protected void reportMemberChanged<TMember>(string member, TMember value)
        {
            if (!IsEditing)
                this.changeTracker.EntityMemberChanged(member, value);

            OnPropertyChanged(member);
            OnPropertyChangedPassingValue(member, value);
        }
        protected void propertySetter<TProperty>(
            ref TProperty property, TProperty value, string propertyName)
        {
            if (IsEditing)
            {
                this.reportMemberChanging(propertyName);
                // TODO: custom notifications here?
                this.validator.ValidateProperty(propertyName, value);
                property = value;
                this.reportMemberChanged<TProperty>(propertyName, value);
                // TODO: custom notification here?
            } else
                throw new InvalidOperationException("Cannot edit when not in editing mode.");
        }
        protected abstract void readCurrentData(TData data);
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
        public bool Merge(TData data,
            EMergeOptions mergeOptions)
        {
            // check valid entity state for the operation
            if (this.EntityState == EEntityState.Detached)
                throw new InvalidOperationException("Cannot merge data into a detached entity");

            // entity is not detached -> it has a change tracker
            bool currentDataChanged = this.changeTracker.Merge(data, mergeOptions);
            // when not editing, read current data into the entity cache, if not deleted
            if (this.EntityState != EEntityState.Deleted)
                this.readCurrentData(data);

            return currentDataChanged;
        }
        public void Delete()
        {
            if (HasTracker)
            {
                
            }
        }
        #endregion

        #region IEntityWithChangeTracker Members
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

        #region IRefreshableObject Members
        public void Refresh()
        {
            this.Manager.Refresh(this);
        }
        #endregion
        #region IUpdatableObject Members
        public void SaveChanges()
        {
            this.Manager.SaveChanges(this);
        }
        #endregion
        
        #region IErrorIndicator Members
        public bool HasErrors
        {
            get { return this.hasErrors; }
            private set
            {
                if (this.hasErrors != value)
                {
                    this.hasErrors = value;
                    OnHasErrorChanged();
                }
            }
        }
        public event HasErrorChangedHandler HasErrorChanged;
        #endregion

        #region IEntityWithDeletableChangeTracker<TKey> Members
        public void SetChangeTracker(IDeletableMergeableChangeTracker<TData, TKey> tracker)
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
        #endregion

        #region IEntityObject<TData,TKey> Members
        public IEntityManagerForEntityObject<TKey> Manager { get; private set; }
        #endregion

        #region IEquatable<IEntityObjectWithDataKey<TKey>> Members
        public bool Equals(IEntityObjectWithDataKey<TKey> other)
        {
            return this.DataKey.Equals(other.DataKey);
        }
        #endregion
    }

    public enum EMergeOptions
    {
        KeepCurrentValues,
        OverrideCurrentValues
    }
}
