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
    
    #region PropertyChangedPassingValue members\
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
    #endregion

    #region Delegates, EventArgs and interfaces for error change notification
    public class EntityHasErrorChangedEventArgs : EventArgs
    {
        #region Members
        IErrorIndicator Entity { get; private set; }
        #endregion

        public EntityHasErrorChangedEventArgs(IErrorIndicator entity)
        {
            this.Entity = entity;
        }
    }
    public delegate void EntityHasErrorChangedHandler(object sender, EntityHasErrorChangedEventArgs e);
    public interface INotifyHasErrorsChanged
    {
        event EntityHasErrorChangedHandler HasErrorChanged;
    }

    public class EntitiesHasErrorsChangedEventArgs : EventArgs
    {
        #region Members
        IEnumerable<IErrorIndicator> Entities { get; private set; }
        #endregion

        #region Constructor
        public EntitiesHasErrorsChangedEventArgs(IEnumerable<IErrorIndicator> entities)
        {
            this.Entities = entities;
        }
        #endregion
    }
    public delegate void EntitiesHasErrorsChangedHandler(object sender, EntitiesHasErrorsChangedEventArgs e);
    public interface INotifyEntitiesHasErrorsChanged
    {
        event EntitiesHasErrorsChangedHandler EntitiesHasErrorsChanged;
    }
    #endregion

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
    }

    
    public interface IEntityWithDeletableChangeTracker<TData, TKey>
        where TData : IEntityDataContainer<TKey>
        where TKey : IEquatable<TKey>
    {
        bool HasTracker { get; }
        void SetChangeTracker(IChangeTrackerForEntityObject<TData, TKey> tracker);
    }

    public interface IEntityObjectWithManager<TKey>
        where TKey : IEquatable<TKey>
    {
        IEntityManagerForEntityObject<TKey> Manager { get;}
    }


    public interface IEntityObjectBase<TData, TKey> :
        IEntityObjectWithDataKeyAndStateInfo<TKey>, IEntityWithDeletableChangeTracker<TData, TKey>,
        IEntityDataContainer<TKey>, IMergeableWithEntityDataContainer<TData, TKey>,
        IEditableObject, IEditableObjectInfo, IRevertibleChangeTracking,
        IDeletableObject, IUndeletableObject,
        INotifyPropertyChanging, INotifyPropertyChanged, INotifyPropertyChangedPassingValue,
        INotifyEntityDeleted<TKey>,
        INotifyEntityStateChanged<TKey>,
        INotifyEntityDataChanged<TKey>,
        IDataErrorInfo, IErrorIndicator, INotifyHasErrorsChanged,
        IEntityWithDeletableChangeTracker<TData, TKey>
        where TData : IEntityDataContainer<TKey>
        where TKey : IEquatable<TKey>
    { }


    public interface IEntityObject<TData, TKey> :
        IEntityObjectBase<TData, TKey>,
        IEntityObjectWithManager<TKey>,
        IRefreshableObject, IUpdatableObject
        where TData : IEntityDataContainer<TKey>
        where TKey : IEquatable<TKey>
    { }

    public abstract class EntityObjectBase<TData, TKey> : IEntityObjectBase<TData, TKey>
            where TData : IEntityDataContainer<TKey>
            where TKey  : IEquatable<TKey>
    {
        #region Members
        protected IEntityValidator validator;
        protected IChangeTrackerForEntityObject<TData, TKey> changeTracker;
        private bool hasErrors;
        #endregion

        #region Constructor
        protected EntityObjectBase()
        {

        }
        #endregion
        
        #region IEditableObject Members
        public virtual void BeginEdit()
        {
            // check entity state
            if (this.EntityState == EEntityState.Deleted)
                throw new InvalidOperationException("Cannot edit a deleted object!");
            
            // if already editing, return
            if (IsEditing)
                return;

            // start editing
            IsEditing = true;
            
            // call tracker to start editing
            this.changeTracker.BeginEdit();
        }
        public virtual void CancelEdit()
        {
            // if already not editing, return
            if (!IsEditing)
                return;

            // stop editing by calling a tracker
            this.changeTracker.CancelEdit();

            // set the editing flag
            this.IsEditing = false;
        }
        public virtual void EndEdit()
        {
            // perform entity state check
            if (this.EntityState == EEntityState.Deleted)
                throw new InvalidOperationException(
                    "Cannot end editing operation on a deleted object!");

            // if not editing, return
            if (!IsEditing)
                return;

            // validate the entity -> can result in events with error change notification
            // being triggered and caught by manager and entity collections
            this.validator.ValidateEntity();

            // if the entity does not have any errors, save the changes in the tracker
            // -> can result in events with data and state change notification being
            // triggered and caught by manager and entity collections
            this.changeTracker.EndEdit();

            // mark the entity as non-editing
            this.IsEditing = false;
        }
        #endregion

        #region IRevertibleChangeTracking Members
        public abstract void RejectChanges();
        #endregion

        #region IChangeTracking Members
        public virtual void AcceptChanges()
        {
            // preform the entity state check
            EEntityState currentState = this.EntityState;

            // cannot accept changes on a detached entity
            if (currentState == EEntityState.Detached)
                throw new InvalidOperationException(
                    "Cannot accept changes on a detached entity");

            // accepting changes on an unchaned entity does not have any sense
            if (currentState == EEntityState.Unchanged)
                return;

            // if the object is marked as deleted, detach him
            if (currentState == EEntityState.Deleted)
            {
                
            }
        }
        public bool IsChanged
        {
            get
            {
                if (!this.HasTracker)
                    return false;

                return this.changeTracker.IsChanged;
            }
        }
        #endregion

        #region INotifyPropertyChanging Members
        public event PropertyChangingEventHandler PropertyChanging;
        private void OnPropertyChanging(string property)
        {
            PropertyChangingEventHandler handler = this.PropertyChanging;

            if (handler != null)
            {
                handler(this, new PropertyChangingEventArgs(property));
            }
        }
        #endregion

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string property)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(property));
            }
        }
        #endregion

        #region Helper methods
        protected void reportMemberChanging(string member)
        {
            if (!IsEditing)
                this.changeTracker.EntityMemberChanging(member);

            this.OnPropertyChanging(member);
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
                // store old HasErrors
                bool oldHasErrors = this.HasErrors;
                // report member changing
                this.reportMemberChanging(propertyName);
                // TODO: custom notifications here?
                this.validator.ValidateProperty(propertyName, value);
                property = value;
                // check for HasErrors changes
                if (this.HasErrors != oldHasErrors)
                {
                    this.OnHasErrorChanged();
                }
                // report member change
                this.reportMemberChanged<TProperty>(propertyName, value);
                // TODO: custom notification here?
            } else
                throw new InvalidOperationException("Cannot edit when not in editing mode.");
        }
        protected abstract void readCurrentData(TData data);
        private void attachToNewChangeTracker(IChangeTrackerForEntityObject<TData, TKey> tracker)
        {
            // set the new change tracker
            this.changeTracker = tracker;
            // bind to its events
            this.changeTracker.TrackerStateChanged += new TrackerStateChangedHandler(changeTracker_TrackerStateChanged);
            this.changeTracker.TrackerDataChanged += new TrackerDataChangedHandler(changeTracker_TrackerDataChanged);
        }
        private void detachFromOldChageTracker()
        {
            // dispose the old change tracker
            this.changeTracker.Dispose();
            // unbind from its events
            this.changeTracker.TrackerStateChanged -= changeTracker_TrackerStateChanged;
            this.changeTracker.TrackerDataChanged -= changeTracker_TrackerDataChanged;
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
        private void OnPropertyChangedPassingValue(string propertyName, object value)
        {
            PropertyChangedPassingValueHandler handler = this.PropertyChangedPassingValue;

            if (handler != null)
            {
                handler(this, new PropertyChangedPassingValueEventArgs(propertyName, value));
            }
        }
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
            // if editing, cancel editing
            if (this.IsEditing)
                this.CancelEdit();

            // if an entity does not have a change tracker, it cannot be deleted
            if (!HasTracker)
                return;

            // call delete on the change tracker
            this.changeTracker.Delete();

            // trigger the deleted event
            this.OnEntityDeleted();
        }
        public void UnDelete()
        {

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

        #region IEntityWithDeletableChangeTracker<TKey> Members
        public void SetChangeTracker(IChangeTrackerForEntityObject<TData, TKey> tracker)
        {
            if (this.changeTracker != null)
            {
                this.detachFromOldChageTracker();
            }

            // if proposed tracker is null, set it to null
            if (tracker == null)
            {
                this.changeTracker = null;
            }
            // if not, then set the new change tracker and bind to its events
            else if (tracker.Entity == this)
            {
                this.attachToNewChangeTracker(tracker);
            }
        }
        #endregion

        #region IEquatable<IEntityObjectWithDataKey<TKey>> Members
        public bool Equals(IEntityObjectWithDataKey<TKey> other)
        {
            return this.DataKey.Equals(other.DataKey);
        }
        #endregion

        #region INotifyEntityDeleted<TKey> Members
        public event EntityExistenceChangedHandler<TKey> EntityDeleted;
        private void OnEntityDeleted()
        {
            EntityExistenceChangedHandler<TKey> handler = this.EntityDeleted;

            if (handler != null)
            {
                handler(this, new EntityExistenceChangedEventArgs<TKey>(this));
            }
        }
        #endregion

        #region INotifyEntityStateChanged<TKey> Members
        public event EntityStateChangedHandler<TKey> EntityStateChanged;
        private void OnEntityStateChanged(EEntityState oldState)
        {
            EntityStateChangedHandler<TKey> handler = this.EntityStateChanged;

            if (handler != null)
            {
                handler(this, new EntityStateChangedEventArgs<TKey>(this, oldState));
            }
        }
        void changeTracker_TrackerStateChanged(object sender, TrackerStateChangedEventArgs e)
        {
            this.OnEntityStateChanged(e.OldState);
        }
        #endregion

        #region INotifyEntityDataChanged<TKey> Members
        public event EntityDataChangedHandler<TKey> EntityDataChanged;
        private void OnEntityDataChanged()
        {
            EntityDataChangedHandler<TKey> handler = this.EntityDataChanged;

            if (handler != null)
            {
                handler(this, new EntityDataChangedEventArgs<TKey>(this));
            }
        }
        void changeTracker_TrackerDataChanged(object sender, TrackerDataChangedEventArgs e)
        {
            this.OnEntityDataChanged();
        }
        #endregion
    }

    public abstract class EntityObject<TData, TKey> : EntityObjectBase<TData, TKey>,
        IEntityObject<TData, TKey>
        where TData : IEntityDataContainer<TKey>
        where TKey : IEquatable<TKey>
    {
        #region Constructor
        protected EntityObject(IEntityManagerForEntityObject<TKey> manager)
        {
            if (manager == null)
                throw new ArgumentNullException("manager");

            this.Manager = manager;
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

        #region IEntityObject<TData,TKey> Members
        public IEntityManagerForEntityObject<TKey> Manager { get; private set; }
        #endregion
    }

    public enum EMergeOptions
    {
        KeepCurrentValues,
        OverrideCurrentValues
    }
}
