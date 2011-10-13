using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Collections.Specialized;

namespace myentitylibrary
{
    #region Delegates, Event Args and Interfaces for DataChanged and StateChanged tracker events
    public class TrackerDataChangedEventArgs : EventArgs
    { }
    public delegate void TrackerDataChangedHandler(object sender, TrackerDataChangedEventArgs e);
    /// <summary>
    /// Interface for data changes notification (caught by entity and forwarded)
    /// </summary>
    public interface INotifyTrackerDataChanged
    {
        event TrackerDataChangedHandler TrackerDataChanged;
    }

    public class TrackerStateChangedEventArgs : EventArgs
    {
        #region Members
        public EEntityState OldState { get; private set; }
        #endregion

        #region Constructor
        public TrackerStateChangedEventArgs(EEntityState oldState)
        {
            this.OldState = oldState;
        }
        #endregion
    }
    public delegate void TrackerStateChangedHandler(object sender, TrackerStateChangedEventArgs e);
    /// <summary>
    /// Interface for state changes notification (caught by entity and forwarded)
    /// </summary>
    public interface INotifyTrackerStateChanged
    {
        event TrackerStateChangedHandler TrackerStateChanged;
    }
    #endregion

    public interface IEntityStateInfo
    {
        EEntityState EntityState { get; }
    }

    public interface IEntityChangeTrackerStateInfo :
        IEntityStateInfo, IEditableObjectInfo { }

    public interface IEntityChangeTracker<TKey>:
        IEntityStateInfo, IDisposable
        where TKey : IEquatable<TKey>
    {
        IEntityObjectWithDataKey<TKey> Entity { get; }
        void EntityMemberChanging(string member);
        void EntityMemberChanged<T>(string member, T value)
            where T : IEquatable<T>;
    }
    
    public interface IChangeTrackerForEntityObject<TData, TKey> :
        IEntityChangeTracker<TKey>,
        IEditableObject, IEditableObjectInfo, IDeletableObject, IUndeletableObject,
        IMergeableWithEntityDataContainer<TData, TKey>,
        INotifyTrackerDataChanged, INotifyTrackerStateChanged,
        IRevertibleChangeTracking
        where TData : IEntityDataContainer<TKey>
        where TKey : IEquatable<TKey>
    { }

    
    public interface IMergeableWithEntityDataContainer<TData, TKey>
        where TData : IEntityDataContainer<TKey>
        where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// Method called to merge the entity with a change tracler
        /// </summary>
        /// <param name="data">Data container</param>
        /// <param name="mergeOptions">Additional options</param>
        /// <returns>Value indicating the change of current version of data</returns>
        bool Merge(TData data, EMergeOptions mergeOptions);
    }

    public interface IGetterEDataVersionByFieldName
    {
        TData GetOriginalFieldValue<TData>(string fieldName);
        TData GetCurrentFieldValue<TData>(string fieldName);
    }

    public interface IDefaultVersionGetterByFieldName
    {
        TData GetDefaultValue<TData>(string fieldName);
    }
    
    /// <summary>
    /// The class that provides a basic tracking functionality for entities,
    /// it is able to merge itself with new data, perform *edit and *changes
    /// operations, recording changes to appropriate data versions, handling
    /// data versions with respect to the entity state and entity editing state.
    /// The class is intended to be subclassed with complete template specialization
    /// for the use with various entity types. 
    /// </summary>
    /// <typeparam name="TEntity">The type of entity</typeparam>
    /// <typeparam name="TData">The type of entity data container</typeparam>
    /// <typeparam name="TKey">The type of entity data key</typeparam>
    public abstract class EntityChangeTracker<TEntity, TData, TKey> :
        IChangeTrackerForEntityObject<TData, TKey>,
        IEntityChangeTrackerStateInfo,
        IGetterEDataVersionByFieldName,
        IDefaultVersionGetterByFieldName,
        IMergeableWithEntityDataContainer<TData, TKey>,
        IEditableObject,
        IEditableObjectInfo,
        IDeletableObject,
        IRevertibleChangeTracking
        where TEntity : IEntityObject<TData, TKey>
        where TData : IEntityDataContainer<TKey>
        where TKey : IEquatable<TKey>
    {
        #region Members
        /// <summary>
        /// The entity instance to record changes for.
        /// </summary>
        protected TEntity entity;
        /// <summary>
        /// The entity state to be used from the entity.
        /// </summary>
        protected EEntityState entityState;
        /// <summary>
        /// The dictionary of versioned fields.
        /// </summary>
        protected Dictionary<string, IVersionedFieldBase> versionedFields;

        #region Field adapters
        /// <summary>
        /// Set of policy objects for modifying, editing, version changing
        /// and merging versioned fields' values
        /// </summary>
        private InitialReadToVersionedFieldAdapter fieldInitialReader;
        private ValueGetterSetterWithDefaultVersionForVersionedFieldEDataVersionAdapter
            fieldGetterSetter;
        protected EditVersionedFieldAdapter fieldEditor;
        private VersionChangesForVersionedFieldAdapter fieldVersionChanger;
        protected MergeForVersionedFieldAdapter fieldMerger;
        #endregion

        #region Members for change detection
        private bool hasChanges;
        private int changedFieldsCount;
        #endregion

        #endregion

        #region Constuctor
        protected EntityChangeTracker(TEntity entity)
        {
            this.entity = entity;
            // initialize versioned properties
            this.createVersionedFields();
            // create policy objects for versioned fields manipulation
            this.createVersionedFieldAdapters();
            // read data from the entity
            this.readInitialData();
        }
        #endregion

        #region Helper methods
        /// <summary>
        /// Override in subclass to actually assign the instances of VersionedFields
        /// to propertyName keys in "versionedFields" dictionary
        /// </summary>
        protected abstract void createVersionedFields();
        protected abstract void readInitialData();
        protected abstract bool readChanges();
        private void createVersionedFieldAdapters()
        {
            this.fieldGetterSetter =
                new ValueGetterSetterWithDefaultVersionForVersionedFieldEDataVersionAdapter(this);

            this.fieldInitialReader = new InitialReadToVersionedFieldAdapter(
                this, this.fieldGetterSetter);

            this.fieldEditor = new EditVersionedFieldAdapter(this.fieldGetterSetter);

            this.fieldVersionChanger = new VersionChangesForVersionedFieldAdapter(
                this, this.fieldGetterSetter);

            this.fieldMerger = new MergeForVersionedFieldAdapter(
                this, this.fieldGetterSetter);
        }
        private T getFieldValue<T>(string fieldName, EDataVersion version)
        {
            return this.fieldGetterSetter.GetValue(
                this.versionedFields[fieldName] as IVersionedFieldTemplatedBase<T>,
                version);
        }
        private bool setFieldValue<T>(string fieldName, T data, EDataVersion version)
            where T : IEquatable<T>
        {
            // load a field
            IVersionedFieldTemplatedBase<T> field =
                this.versionedFields[fieldName] as IVersionedFieldTemplatedBase<T>;
            // load current changes state
            bool oldHasChanges = this.fieldVersionChanger.HasChanges<T>(field);
            // set the new value
            bool fieldCurrentDataChanged = this.fieldGetterSetter.SetValue<T>(
                            field,
                            data,
                            version);

            // detect the HasChanges change
            if (oldHasChanges != this.fieldVersionChanger.HasChanges<T>(field))
            {
                // based on whether the changes emerged or ceased to exist,
                // increase, or decrease the changed field count
                if (oldHasChanges)
                    --this.changedFieldsCount;
                else
                    ++this.changedFieldsCount;

                // set the new value of "hasChanged", if there are any fields with changes
                this.hasChanges = this.changedFieldsCount > 0;

                // set the entity state accordingly
                if (!this.hasChanges)
                {
                    if (this.entityState == EEntityState.Modified)
                        this.entityState = EEntityState.Unchanged;
                } else
                {
                    if (this.entityState == EEntityState.Unchanged)
                        this.entityState = EEntityState.Modified;
                }
            }

            return fieldCurrentDataChanged;
        }
        private bool setFieldDefaultValue<T>(string fieldName, T data)
            where T : IEquatable<T>
        {
            return this.setFieldValue<T>(
                fieldName,
                data,
                this.fieldGetterSetter.GetDefaultVersion());
        }
        #endregion

        #region Public interface
        public T GetDefaultValue<T>(string fieldName)
        {
            return this.fieldGetterSetter.GetDefaultValue(
                this.versionedFields[fieldName] as IVersionedFieldTemplatedBase<T>);
        }
        public T GetOriginalFieldValue<T>(string fieldName)
        {
            return this.getFieldValue<T>(fieldName, EDataVersion.Original);
        }
        public T GetCurrentFieldValue<T>(string fieldName)
        {
            return this.getFieldValue<T>(fieldName, EDataVersion.Current);
        }
        #endregion

        #region IEntityChangeTracker Members
        public EEntityState EntityState
        {
            get
            {
                return this.entityState;
            }
            private set
            {
                this.entityState = value;
            }
        }
        public void EntityMemberChanging(string member) { }
        public void EntityMemberChanged<T>(string member, T value)
            where T : IEquatable<T>
        {
            // remember the old state
            EEntityState oldState = this.entityState;
            // set the default value;
            // if changes occur, report them to the entity
            if (this.setFieldDefaultValue(member, value))
            {
                this.OnTrackerDataChanged();
            }

            // if state change occured, report it to the entity
            if (oldState != this.EntityState)
            {
                this.OnTrackerStateChanged(oldState);
            }
        }
        #endregion

        #region IEditableObject Members
        public void BeginEdit()
        {
            // if already editing, return
            if (IsEditing)
                return;

            // set the editation flag
            IsEditing = true;
        }
        public void CancelEdit()
        {
            // if not editing, return
            if (!IsEditing)
                return;

            // set the editation flag
            IsEditing = false;
        }
        public void EndEdit()
        {
            // if not editing, return
            if (!IsEditing)
                return;

            // store the old entity state
            EEntityState oldState = this.EntityState;
            
            // push changes from the associated entity to the change tracker;
            // if a data change occured, report it to the entity
            if (this.readChanges())
            {
                this.OnTrackerDataChanged();
            }

            // check for a state change;
            // if a state change occured, report it to the entity
            if (oldState != this.EntityState)
            {
                this.OnTrackerStateChanged(oldState);
            }

            // set the editation flag
            IsEditing = false;
        }
        #endregion

        #region IRevertibleChangeTracking Members
        /// <summary>
        /// Base method which changes the entity state
        /// </summary>
        public abstract void RejectChanges();
        #endregion

        #region IChangeTracking Members
        /// <summary>
        /// Base method which changes the entity state
        /// </summary>
        public abstract void AcceptChanges();
        public bool IsChanged
        {
            get { return this.hasChanges; }
        }
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Properties
        public bool IsEditing { get; private set; }
        #endregion

        #region IDeletableObject Members
        public void Delete()
        {
            // mark as deleted -> collapse data versions ???
            this.entityState = EEntityState.Deleted;
            // collapse data?
        }
        public void UnDelete()
        {
            this.entityState = EEntityState.Unchanged;
            // renew data?
        }
        #endregion

        #region IMergeableWithEntityDataContainer<TData,TKey> Members
        public abstract bool Merge(TData data, EMergeOptions mergeOptions);
        #endregion

        #region IEntityChangeTracker<TKey> Members
        public IEntityObjectWithDataKey<TKey> Entity
        {
            get { return this.entity; }
        }
        #endregion


        #region INotifyTrackerDataChanged Members
        public event TrackerDataChangedHandler TrackerDataChanged;
        private void OnTrackerDataChanged()
        {
            TrackerDataChangedHandler handler = this.TrackerDataChanged;

            if (handler != null)
            {
                handler(this, new TrackerDataChangedEventArgs());
            }
        }
        #endregion

        #region INotifyTrackerStateChanged Members
        public event TrackerStateChangedHandler TrackerStateChanged;
        private void OnTrackerStateChanged(EEntityState oldState)
        {
            TrackerStateChangedHandler handler = this.TrackerStateChanged;

            if (handler != null)
            {
                handler(this, new TrackerStateChangedEventArgs(oldState));
            }
        }
        #endregion
    }

    
}