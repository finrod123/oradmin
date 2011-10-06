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

    public interface IEntityChangeTracker: IDisposable
    {
        EEntityState EntityState { get; }
        void EntityMemberChanging(string member);
        void EntityMemberChanged<TData>(string member, TData value);
    }

    public interface IMergeableWithEntityDataContainer<TData, TKey>
        where TData : IEntityDataContainer<TKey>
        where TKey : IEquatable<TKey>
    {
        void Merge(TData data);
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
        IEntityChangeTracker,
        IDeletableObject,
        IMergeableWithEntityDataContainer<TData, TKey>,
        IEditableObject,
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
        #endregion

        #region Constuctor
        protected EntityChangeTracker(TEntity entity)
        {
            this.entity = entity;
            // initialize versioned properties
            this.createVersionedFields();
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
        
        private EDataVersion getDefaultVersion(EEntityState state, bool isEditing)
        {
            if (isEditing)
                return EDataVersion.Original;

            switch (state)
            {
                case EEntityState.Added:
                    return EDataVersion.Current;
                case EEntityState.Modified:
                    return EDataVersion.Current;
                case EEntityState.Unchanged:
                    return EDataVersion.Original;
                case EEntityState.Deleted:
                    return EDataVersion.Original;
                case EEntityState.Detached:
                    return EDataVersion.Proposed;
            }
        }
        private bool tryGetField(string fieldName, out VersionedFieldBase field)
        {
            if (!versionedFields.TryGetValue(fieldName, out field))
            {
                throw new KeyNotFoundException("Field with this key does not exist");
            }

            return true;
        }
        #endregion

        #region Public interface
        public TData GetFieldValue<TData>(string fieldName, EDataVersion version)
            where TData : class
        {
            VersionedFieldBase field;
            tryGetField(fieldName, out field);
            
            return (field as VersionedFieldTemplatedBase<TData>).GetValue(version);
        }
        public TData GetDefaultFieldValue<TData>(string fieldName)
            where TData : class
        {
            return GetFieldValue<TData>(fieldName,
                                        getDefaultVersion(this.entityState,
                                                          this.entity.IsEditing));
        }
        public TData GetOriginalFieldValue<TData>(string fieldName)
            where TData : class
        {
            return GetFieldValue<TData>(fieldName, EDataVersion.Original);
        }
        public TData GetCurrentFieldValue<TData>(string fieldName)
            where TData : class
        {
            return GetFieldValue<TData>(fieldName, EDataVersion.Current);
        }
        public TData GetProposedValue<TData>(string fieldName)
            where TData : class
        {
            return GetFieldValue<TData>(fieldName, EDataVersion.Proposed);
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
            if(this.entityState == EEntityState.Deleted)
                throw new InvalidOperationException("Cannot edit deleted object!");

            VersionedFieldBase field;
            tryGetField(member, out field);

            (field as VersionedFieldTemplatedBase<TData>).
                SetValue(value, getDefaultVersion(this.entityState, this.entity.IsEditing));
                                                                                
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
            if (IsEditing)
                return;

            switch (this.entityState)
            {
                case EEntityState.Deleted:
                    throw new InvalidOperationException("Cannot accept changes on deleted object!");
                case EEntityState.Added:
                case EEntityState.Modified:
                    ;
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
        public abstract void Merge(TData data);
        #endregion
    }
}