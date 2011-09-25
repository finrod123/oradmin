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
    public interface IEntityChangeTracker
    {
        EEntityState EntityState { get; set; }
        void EntityMemberChanging(string member);
        void EntityMemberChanged<TData>(string member, TData value);
    }
    public class EntityObjectChangeTracker : IEntityChangeTracker, IEditableObject,
        IRevertibleChangeTracking
    {
        #region Members
        protected EntityObject entity;
        protected EEntityState entityState;
        protected Dictionary<string, VersionedFieldBase> versionedProperties;
        #endregion

        #region Static members
        /// <summary>
        /// List of properties to track (loaded with "loadTrackingProperties")
        /// </summary>
        List<string> propertyNames;
        /// <summary>
        /// Intended to run from static constructor of a subclass
        /// </summary>
        protected void Initialize()
        {
            // get properties to track from TEntity type (marked with Trackable attribute)
            loadTrackingProperties();
        }
        private void loadTrackingProperties()
        {

        }
        #endregion

        #region Constuctor
        public EntityObjectChangeTracker(TEntity entity)
        {
            this.entity = entity;
            // initialize versioned properties
            initializeVersionedProperties();
        }
        #endregion

        #region Helper methods
        private bool tryGetVersionedField<TData>(string fieldName, out VersionedField<TData> field)
            where TData : class
        {
            VersionedFieldBase fieldBase;

            if (!versionedProperties.TryGetValue(fieldName, out fieldBase))
            {
                field = default(VersionedField<TData>);
                return false;
            }

            field = fieldBase as VersionedField<TData>;

            if (field == null)
            {
                return false;
            }

            return true;
        }
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
        #endregion

        #region Public interface
        public TData GetFieldValue<TData>(string fieldName, EDataVersion version)
            where TData : class
        {
            VersionedField<TData> field;

            if (!tryGetVersionedField<TData>(fieldName, out field))
                return default(TData);

            return field.GetValue(version);
        }
        public TData GetDefaultFieldValue<TData>(string fieldName)
            where TData : class
        {
            VersionedField<TData> field;

            if (!tryGetVersionedField(fieldName, out field))
                return default(TData);

            return field.DefaultValue;
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
            set
            {
                this.entityState = value;
                // do some actions with data?

            }
        }
        public void EntityMemberChanging(string member)
        {
            // cache a current value
        }

        public void EntityMemberChanged(string member, object value)
        {
            // set new current value
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
}