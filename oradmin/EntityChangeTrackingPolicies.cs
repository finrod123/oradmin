using System;
using System.ComponentModel;

namespace oradmin
{
    public interface IVersionedFieldInitialReadPolicyObject
    {
        void ReadData<TData>(VersionedFieldTemplatedBase<TData> field, TData data);
    }

    public interface IVersionedFieldEditPolicyObject
    {
        void EndEdit<TData>(VersionedFieldTemplatedBase<TData> field, TData data);
    }

    public interface IVersionedFieldVersionChangesPolicyObject
    {
        void AcceptChanges<TData>(VersionedFieldTemplatedBase<TData> field);
        void RejectChanges<TData>(VersionedFieldTemplatedBase<TData> field);
    }

    public interface IVersionedFieldMergePolicyObject
    {
        void Merge<TData>(VersionedFieldTemplatedBase<TData> field, TData data,
            EMergeOptions mergeOptions);
    }

    public interface IVersionedFieldQueryableAdapter<TVersion>
        where TVersion : struct
    {
        public TData GetValue<TData>(VersionedFieldTemplatedBase<TData> field, TVersion version);
    }

    public interface IVersionedFieldModifiableAdapter<TVersion>
        where TVersion : struct
    {
        public void SetValue<TData>(VersionedFieldTemplatedBase<TData> field,
            TData data, TVersion version);
    }

    public interface IVersionedFieldQueryableModifiableAdapter<TVersion> :
        IVersionedFieldQueryableAdapter<TVersion>,
        IVersionedFieldModifiableAdapter<TVersion>
        where TVersion : struct { }

    public interface IVersionedFieldDefaultVersionQueryableAdapter
    {
        public TData GetDefaultValue<TData>(VersionedFieldTemplatedBase<TData> field);
    }

    public interface IVersionedFieldDefaultVersionModifiableAdapter
    {
        public void SetDefaultValue<TData>(VersionedFieldTemplatedBase<TData> field,
            TData value);
    }

    public interface IVersionedFieldDefaultVersionQueryableModifiableAdapter :
        IVersionedFieldDefaultVersionQueryableAdapter,
        IVersionedFieldDefaultVersionModifiableAdapter
    { }

    public class VersionedFieldInitialReadPolicyObject :
        IVersionedFieldInitialReadPolicyObject
    {
        #region Members
        IEntityStateInfo info;
        #endregion

        #region Constructor
        public VersionedFieldInitialReadPolicyObject(IEntityStateInfo info)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            this.info = info;
        }
        #endregion

        #region IVersionedFieldInitialReadPolicyObject Members
        public void ReadData<TData>(VersionedFieldTemplatedBase<TData> field, TData data)
        {
            switch (this.info.EntityState)
            {
                case EEntityState.Added:
                    field.SetValue(data, EDataVersion.Current);
                    break;
                case EEntityState.Unchanged:
                    field.SetValue(data, EDataVersion.Original);
                    field.SetValue(data, EDataVersion.Current);
                    break;
                default:
                    throw new InvalidOperationException(
                        string.Format("Cannot read data in state {0}",
                        this.info.EntityState.ToString()));
                    break;
            }
        }
        #endregion
    }

    public class VersionedFieldEditPolicyObject :
        IVersionedFieldEditPolicyObject
    {
        #region IEntityChangeTrackerEditPolicyObject Members
        public void EndEdit<TData>(VersionedFieldTemplatedBase<TData> field, TData data)
        {
            field.SetValue(data, EDataVersion.Current);
        }
        #endregion
    }

    public class VersionedFieldVersionChangesPolicyObject :
        IVersionedFieldVersionChangesPolicyObject
    {
        #region Members
        IEntityStateInfo info;
        #endregion

        #region Constructor
        public VersionedFieldVersionChangesPolicyObject(
            IEntityChangeTrackerStateInfo info)
        {
            if (info == null)
                throw new ArgumentNullException("tracker");

            this.info = info;
        }
        #endregion

        #region IEntityChangeTrackerVersionChangesPolicyObject Members
        public void AcceptChanges<TData>(VersionedFieldTemplatedBase<TData> field)
        {
            switch (info.EntityState)
            {
                case EEntityState.Added:
                case EEntityState.Modified:
                    field.SetValue(field.GetValue(EDataVersion.Current), EDataVersion.Original);
                    break;
            }
        }
        public void RejectChanges<TData>(VersionedFieldTemplatedBase<TData> field)
        {
            switch (info.EntityState)
            {
                case EEntityState.Deleted:
                    // restore current value from original
                    field.SetValue(field.GetValue(EDataVersion.Original), EDataVersion.Current);
                    break;
                case EEntityState.Modified:
                    field.SetValue(field.GetValue(EDataVersion.Original), EDataVersion.Current);
                    break;
            }
        }
        #endregion
    }

    public class VersionedFieldMergePolicyObject :
        IVersionedFieldMergePolicyObject
    {
        #region Members
        IEntityStateInfo info;
        #endregion

        #region Constructor
        public VersionedFieldMergePolicyObject(IEntityStateInfo info)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            this.info = info;
        }
        #endregion

        #region IEntityChangeTrackerMergePolicyObject Members
        public void Merge<TData>(VersionedFieldTemplatedBase<TData> field, TData data,
            EMergeOptions mergeOptions)
        {
            switch (this.info.EntityState)
            {
                case EEntityState.Added:
                case EEntityState.Modified:
                case EEntityState.Deleted:
                    // rewrite original version
                    field.SetValue(data, EDataVersionWithDefault.Original);
                    break;
                case EEntityState.Unchanged:
                    // rewrite both original and current versions
                    field.SetValue(data, EDataVersionWithDefault.Original);
                    field.SetValue(data, EDataVersionWithDefault.Current);
                    break;
            }
        }
        #endregion
    }

    public class VersionedFieldQueryableModifiablePolicyObject :
        IVersionedFieldQueryableModifiableAdapter<EDataVersion>,
        IVersionedFieldDefaultVersionQueryableModifiableAdapter
    {
        #region Members
        IEntityChangeTrackerStateInfo info;
        #endregion

        #region Constructor
        public VersionedFieldQueryableModifiablePolicyObject(IEntityChangeTrackerStateInfo info)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            this.info = info;
        }
        #endregion

        #region IVersionedFieldQueryable<EDataVersion> Members
        public TData GetValue<TData>(VersionedFieldTemplatedBase<TData> field, EDataVersion version)
        {
            return field.GetValue(version);
        }
        #endregion

        #region IVersionedFieldModifiable<EDataVersion> Members
        public void SetValue<TData>(VersionedFieldTemplatedBase<TData> field, TData data, EDataVersion version)
        {
            field.SetValue(data, version);
        }
        #endregion

        #region IVersionedFieldDefaultVersionQueryable Members
        public TData GetDefaultValue<TData>(VersionedFieldTemplatedBase<TData> field)
        {
            return field.GetValue(this.getDefaultVersion());
        }
        public void SetDefaultValue<TData>(VersionedFieldTemplatedBase<TData> field, TData value)
        {
            field.SetValue(value, this.getDefaultVersion());
        }
        #endregion

        #region Helper methods
        private EDataVersion getDefaultVersion()
        {
            if (this.info.IsEditing)
                return EDataVersion.Proposed;

            switch (this.info.EntityState)
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
        #endregion
    }
}