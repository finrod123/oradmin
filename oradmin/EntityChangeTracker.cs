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

    public interface IDeletableEntityChangeTracker : IEntityChangeTracker, IDeletableObject
    { }

    public class EntityChangeTracker<TEntity, TData, TKey> :
        IDeletableEntityChangeTracker,
        IEditableObject,
        IRevertibleChangeTracking
        where TEntity : EntityObject<TData, TKey>
        where TData   : IEntityDataContainer<TKey>
        where TKey    : IEquatable<TKey>
    {
        #region Members
        protected TEntity entity;
        protected EEntityState entityState;
        protected Dictionary<string, VersionedFieldBase> versionedProperties;
        #endregion

        #region Static members
        /// <summary>
        /// List of properties to track (loaded with "loadTrackingProperties")
        /// </summary>
        protected static List<string> propertyNames;
        /// <summary>
        /// Intended to run from static constructor of a subclass
        /// </summary>
        protected static void Initialize()
        {
            // get properties to track from TEntity type (marked with Trackable attribute)
            loadTrackingProperties();
        }
        private void loadTrackingProperties()
        {
            Type entityType = typeof(TEntity);

            propertyNames =
                (from property in entityType.GetProperties()
                 where Attribute.IsDefined(property, typeof(TrackedAttribute))
                 select property.Name).ToList();
        }
        #endregion

        #region Constuctor
        public EntityChangeTracker(TEntity entity)
        {
            this.entity = entity;
            // initialize versioned properties
            initializeVersionedProperties();
            // read data from the entity
            readEntityData();
        }
        #endregion

        #region Helper methods
        /// <summary>
        /// Override in subclass to actually assign the instances of VersionedFields
        /// to propertyName keys in "versionedFields" dictionary
        /// </summary>
        protected virtual void initializeVersionedProperties()
        {
            versionedProperties = new Dictionary<string, VersionedFieldBase>();

            foreach (string propertyName in propertyNames)
            {
                versionedProperties.Add(propertyName, null);
            }
        }
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
            if (!versionedProperties.TryGetValue(fieldName, out field))
            {
                throw new KeyNotFoundException("Field with this key does not exist");
            }

            return true;
        }
        private void updateOriginalValuesToCurrent()
        {
            foreach (VersionedFieldBase field in versionedProperties.Values)
            {
                
            }
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
        public void Delete()
        {
            this.entityState = EEntityState.Deleted;
            // a dalsi akce
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
            if (this.entity.IsEditing)
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

        #region IEntityChangeTracker<TData,TKey> Members
        public EntityObject<TData, TKey> Entity
        {
            get { return this.entity; }
        }
        #endregion
    }

    public class TrackedAttribute : Attribute
    {
        #region Constructor
        protected TrackedAttribute(bool track)
        {
            Track = track;
        }
        public TrackedAttribute() : this(false) { }
        #endregion

        #region Properties
        public bool Track { get; set; }
        #endregion
    }
}