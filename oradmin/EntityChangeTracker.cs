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
    public interface IDeletableObject
    {
        void Delete();
    }

    public interface IEditableObjectInfo
    {
        bool IsEditing { get; }
    }

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
        void EntityMemberChanged<TData>(string member, TData value);
    }

    public interface IDeletableChangeTracker<TKey> :
        IEntityChangeTracker<TKey>, IDeletableObject
        where TKey : IEquatable<TKey>
    { }

    
    public interface IMergeableWithEntityDataContainer<TData, TKey>
        where TData : IEntityDataContainer<TKey>
        where TKey : IEquatable<TKey>
    {
        void Merge(TData data, EMergeOptions mergeOptions);
    }

    public interface IEDataVersionQueryable
    {
        public TData GetOriginalFieldValue<TData>(string fieldName);
        public TData GetCurrentFieldValue<TData>(string fieldName);
    }

    public interface IDefaultVersionQueryableByFieldName
    {
        public TData GetDefaultValue<TData>(string fieldName);
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
        IDeletableChangeTracker,
        IEntityChangeTrackerStateInfo,
        IEDataVersionQueryable,
        IDefaultVersionQueryableByFieldName,
        IMergeableWithEntityDataContainer<TData, TKey>,
        IEditableObject,
        IEditableObjectInfo,
        IDeletableObject,
        IRevertibleChangeTracking
        where TEntity : IEntityObject<TData, TKey>
        where TData   : IEntityDataContainer<TKey>
        where TKey    : IEquatable<TKey>
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
        protected Dictionary<string, VersionedFieldBase> versionedFields;

        /// <summary>
        /// Set of policy objects for modifying, editing, version changing
        /// and merging versioned fields' values
        /// </summary>
        private VersionedFieldQueryableModifiablePolicyObject getSetPolicyObject;
        protected VersionedFieldEditPolicyObject editPolicyObject;
        private VersionedFieldVersionChangesPolicyObject versionChangesPolicyObject;
        protected VersionedFieldMergePolicyObject mergePolicyObject;
        #endregion

        #region Constuctor
        protected EntityChangeTracker(TEntity entity)
        {
            this.entity = entity;
            // initialize versioned properties
            this.createVersionedFields();
            // create policy objects for versioned fields manipulation
            this.createVersionedFieldPolicyObjects();
            // read data from the entity
            this.readEntityData();
        }
        #endregion

        #region Helper methods
        /// <summary>
        /// Override in subclass to actually assign the instances of VersionedFields
        /// to propertyName keys in "versionedFields" dictionary
        /// </summary>
        protected abstract void createVersionedFields();
        protected abstract void readEntityData();
        protected abstract void detectChanges();
        private void createVersionedFieldPolicyObjects()
        {
            this.getSetPolicyObject = new VersionedFieldQueryableModifiablePolicyObject(this);
            this.editPolicyObject = new VersionedFieldEditPolicyObject();
            this.versionChangesPolicyObject = new VersionedFieldVersionChangesPolicyObject(this);
            this.mergePolicyObject = new VersionedFieldMergePolicyObject(this);
        }
        private TData getFieldValue<TData>(string fieldName, EDataVersion version)
        {
            return this.getSetPolicyObject.GetValue(
                this.versionedFields[fieldName] as VersionedFieldTemplatedBase<TData>,
                version);
        }
        private void setFieldValue<TData>(string fieldName, TData data, EDataVersion version)
        {
            this.getSetPolicyObject.SetValue<TData>(
                this.versionedFields[fieldName] as VersionedFieldTemplatedBase<TData>,
                data,
                version);
        }
        #endregion

        #region Public interface
        public TData GetDefaultValue<TData>(string fieldName)
            where TData : class
        {
            return this.getSetPolicyObject.GetDefaultValue(
                this.versionedFields[fieldName] as VersionedFieldTemplatedBase<TData>);
        }
        public TData GetOriginalFieldValue<TData>(string fieldName)
            where TData : class
        {
            return this.getFieldValue<TData>(fieldName, EDataVersion.Original);
        }
        public TData GetCurrentFieldValue<TData>(string fieldName)
            where TData : class
        {
            return this.getFieldValue<TData>(fieldName, EDataVersion.Current);
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
        public void EntityMemberChanged<TData>(string member, TData value)
        {
            // store the value in a default version
            this.getSetPolicyObject.SetDefaultValue<TData>(
                this.versionedFields[member] as VersionedFieldTemplatedBase<TData>,
                value);                      
        }
        #endregion
        
        #region IEditableObject Members
        public void BeginEdit()
        {
            if (IsEditing)
                return;

            IsEditing = true;
        }
        public void CancelEdit()
        {
            if (!IsEditing)
                return;

            IsEditing = false;
        }
        public void EndEdit()
        {
            if (!IsEditing)
                return;

            // push changes from the associated entity to the change tracker
            this.detectChanges();
            IsEditing = false;
        }
        #endregion

        #region IRevertibleChangeTracking Members
        /// <summary>
        /// Base method which changes the entity state
        /// </summary>
        public virtual void RejectChanges()
        {
            switch (this.entityState)
            {
                case EEntityState.Deleted:
                case EEntityState.Modified:
                    this.entityState = EEntityState.Unchanged;
                    break;
            }
        }
        #endregion

        #region IChangeTracking Members
        /// <summary>
        /// Base method which changes the entity state
        /// </summary>
        public virtual void AcceptChanges()
        {
            switch (this.entityState)
            {
                case EEntityState.Added:
                case EEntityState.Modified:
                    this.entityState = EEntityState.Unchanged;
                    break;
            }
        }
        public bool IsChanged
        {
            get { throw new NotImplementedException(); }
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
            throw new NotImplementedException();
        }
        #endregion

        #region IMergeableWithEntityDataContainer<TData,TKey> Members
        public abstract void Merge(TData data, EMergeOptions mergeOptions);
        #endregion
    }

    
}