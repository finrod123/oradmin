using System;
using System.ComponentModel;
using System.Data;

namespace oradmin
{
    public interface IInitialReadToVersionedFieldAdapter
    {
        void ReadData<TData>(VersionedFieldTemplatedBase<TData> field, TData data);
    }

    public interface IEditVersionedFieldAdapter
    {
        // saves proposed data as current and returns whether the current data
        // had changed; (returns which data versions had changed?)
        bool EndEdit<TData>(VersionedFieldTemplatedBase<TData> field, TData data)
            where TData : IEquatable<TData>;
    }

    public interface IVersionChangesForVersionedFieldAdapter
    {
        void AcceptChanges<TData>(VersionedFieldTemplatedBase<TData> field)
            where TData : IEquatable<TData>;
        void RejectChanges<TData>(VersionedFieldTemplatedBase<TData> field)
            where TData : IEquatable<TData>;
        bool HasChanges<TData>(VersionedFieldTemplatedBase<TData> field)
            where TData : IEquatable<TData>;
    }

    public interface IMergeForVersionedFieldAdapter
    {
        // merges data into the field and returns whether current data was changed
        // (returns the flag combination of changed data versions???)
        bool Merge<TData>(VersionedFieldTemplatedBase<TData> field, TData data,
            EMergeOptions mergeOptions)
            where TData : IEquatable<TData>;
    }

    public interface IValueGetterForVersionedFieldAdapter<TVersion>
        where TVersion : struct
    {
        public TData GetValue<TData>(VersionedFieldTemplatedBase<TData> field, TVersion version);
    }

    public interface IValueGetterForVersionedFieldEDataVersionAdapter:
        IValueGetterForVersionedFieldAdapter<EDataVersion>
    { }

    public interface IValueSetterForVersionedFieldAdapter<TVersion>
        where TVersion : struct
    {
        // returns whether data was changed (which version?)
        public bool SetValue<TData>(VersionedFieldTemplatedBase<TData> field,
            TData data, TVersion version)
            where TData : IEquatable<TData>;
    }

    public interface IValueSetterForVersionedFieldEDataVersionAdapter :
        IValueSetterForVersionedFieldAdapter<EDataVersion>
    { }

    public interface IValueGetterSetterForVersionedFieldAdapter<TVersion> :
        IValueGetterForVersionedFieldAdapter<TVersion>,
        IValueSetterForVersionedFieldAdapter<TVersion>
        where TVersion : struct { }

    public interface IValueGetterSetterForVersionedFieldEDataVersionAdapter :
        IValueGetterForVersionedFieldEDataVersionAdapter,
        IValueSetterForVersionedFieldEDataVersionAdapter
    { }

    public interface IDefaultVersionGetterForVersionedFieldAdapter<TVersion>
        where TVersion : struct
    {
        TVersion GetDefaultVersion();
    }

    public interface IDefaultVersionGetterForVersionedFieldEDataVersionAdapter :
        IDefaultVersionGetterForVersionedFieldAdapter<EDataVersion>
    { }

    public interface IDefaultVersionValueGetterForVersionedFieldAdapter
    {
        public TData GetDefaultValue<TData>(VersionedFieldTemplatedBase<TData> field);
    }

    public interface IDefaultVersionValueSetterForVersionedFieldAdapter
    {
        // sets the default value of the field and returns whether it has changed
        // (returns which data versions were changed?)
        public bool SetDefaultValue<TData>(VersionedFieldTemplatedBase<TData> field,
            TData value)
            where TData : IEquatable<TData>;
    }

    public interface IDefaultVersionValueGetterSetterForVersionedFieldAdapter :
        IDefaultVersionValueGetterForVersionedFieldAdapter,
        IDefaultVersionValueSetterForVersionedFieldAdapter
    { }

    public class InitialReadToVersionedFieldAdapter :
        IInitialReadToVersionedFieldAdapter
    {
        #region Members
        IEntityStateInfo info;
        IValueSetterForVersionedFieldEDataVersionAdapter fieldSetter;
        #endregion

        #region Constructor
        public InitialReadToVersionedFieldAdapter(IEntityStateInfo info,
            IValueSetterForVersionedFieldEDataVersionAdapter fieldSetter)
        {
            if (info == null)
                throw new ArgumentNullException("info");
            if (fieldSetter == null)
                throw new ArgumentNullException("field setter");

            this.info = info;
            this.fieldSetter = fieldSetter;
        }
        #endregion

        #region IVersionedFieldInitialReadPolicyObject Members
        public void ReadData<TData>(VersionedFieldTemplatedBase<TData> field, TData data)
        {
            switch (this.info.EntityState)
            {
                case EEntityState.Added:
                    fieldSetter.SetValue<TData>(field, data, EDataVersion.Current);
                    break;
                case EEntityState.Unchanged:
                    fieldSetter.SetValue<TData>(field, data, EDataVersion.Original);
                    fieldSetter.SetValue<TData>(field, data, EDataVersion.Current);
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

    public class EditVersionedFieldAdapter :
        IEditVersionedFieldAdapter
    {
        #region Members
        IValueSetterForVersionedFieldEDataVersionAdapter fieldSetter;
        #endregion

        #region Constructor
        public EditVersionedFieldAdapter(IValueSetterForVersionedFieldEDataVersionAdapter fieldSetter)
        {
            if (fieldSetter == null)
                throw new ArgumentException("field setter");

            this.fieldSetter = fieldSetter;
        }
        #endregion

        #region IEntityChangeTrackerEditPolicyObject Members
        public bool EndEdit<TData>(VersionedFieldTemplatedBase<TData> field, TData data)
            where TData : IEquatable<TData>
        {
            return fieldSetter.SetValue<TData>(field, data, EDataVersion.Current);
        }
        #endregion
    }

    public class VersionChangesForVersionedFieldAdapter :
        IVersionChangesForVersionedFieldAdapter
    {
        #region Members
        IEntityStateInfo info;
        IValueGetterSetterForVersionedFieldEDataVersionAdapter fieldGetterSetter;
        #endregion

        #region Constructor
        public VersionChangesForVersionedFieldAdapter(
            IEntityChangeTrackerStateInfo info,
            IValueGetterSetterForVersionedFieldEDataVersionAdapter fieldGetterSetter
            )
        {
            if (info == null)
                throw new ArgumentNullException("tracker");
            if (fieldGetterSetter == null)
                throw new ArgumentNullException("field getter and setter");

            this.info = info;
            this.fieldGetterSetter = fieldGetterSetter;
        }
        #endregion

        #region IEntityChangeTrackerVersionChangesPolicyObject Members
        public void AcceptChanges<TData>(VersionedFieldTemplatedBase<TData> field)
            where TData : IEquatable<TData>
        {
            switch (info.EntityState)
            {
                case EEntityState.Added:
                case EEntityState.Modified:
                    fieldGetterSetter.SetValue<TData>(
                        field,
                        fieldGetterSetter.GetValue<TData>(field, EDataVersion.Current),
                        EDataVersion.Original);
                    break;
            }
        }
        public void RejectChanges<TData>(VersionedFieldTemplatedBase<TData> field)
            where TData : IEquatable<TData>
        {
            switch (info.EntityState)
            {
                case EEntityState.Deleted:
                    // restore current value from original
                    fieldGetterSetter.SetValue<TData>(
                        field,
                        fieldGetterSetter.GetValue<TData>(field, EDataVersion.Original),
                        EDataVersion.Current);
                    break;
                case EEntityState.Modified:
                    fieldGetterSetter.SetValue<TData>(
                        field,
                        fieldGetterSetter.GetValue<TData>(field, EDataVersion.Original),
                        EDataVersion.Current);
                    break;
            }
        }
        public bool HasChanges<TData>(VersionedFieldTemplatedBase<TData> field)
            where TData : IEquatable<TData>
        {
            return !this.fieldGetterSetter.GetValue<TData>(field, EDataVersion.Current)
                   .Equals(
                   this.fieldGetterSetter.GetValue<TData>(field, EDataVersion.Original));
        }
        #endregion
    }

    public class MergeForVersionedFieldAdapter :
        IMergeForVersionedFieldAdapter
    {
        #region Members
        IEntityStateInfo info;
        IValueSetterForVersionedFieldEDataVersionAdapter fieldSetter;
        #endregion

        #region Constructor
        public MergeForVersionedFieldAdapter(
            IEntityStateInfo info,
            IValueSetterForVersionedFieldEDataVersionAdapter fieldSetter)
        {
            if (info == null)
                throw new ArgumentNullException("info");
            if (fieldSetter == null)
                throw new ArgumentNullException("field setter");

            this.info = info;
            this.fieldSetter = fieldSetter;
        }
        #endregion

        #region IEntityChangeTrackerMergePolicyObject Members
        public bool Merge<TData>(VersionedFieldTemplatedBase<TData> field, TData data,
            EMergeOptions mergeOptions)
            where TData : IEquatable<TData>
        {
            bool changed = false;

            switch (this.info.EntityState)
            {
                case EEntityState.Added:
                case EEntityState.Modified:
                case EEntityState.Deleted:
                    // rewrite original version
                    changed = fieldSetter.SetValue<TData>(
                              field,
                              data,
                              EDataVersion.Original);
                    break;
                case EEntityState.Unchanged:
                    // rewrite both original and current versions
                    changed = fieldSetter.SetValue<TData>(
                              field,
                              data,
                              EDataVersion.Original) |

                              fieldSetter.SetValue<TData>(
                              field,
                              data,
                              EDataVersion.Current);
                    break;
            }

            return changed;
        }
        #endregion
    }

    public class ValueGetterSetterWithDefaultVersionForVersionedFieldEDataVersionAdapter :
        IValueGetterSetterForVersionedFieldEDataVersionAdapter,
        IDefaultVersionValueGetterSetterForVersionedFieldAdapter,
        IDefaultVersionGetterForVersionedFieldEDataVersionAdapter,
    {
        #region Members
        IEntityChangeTrackerStateInfo info;
        #endregion

        #region Constructor
        public ValueGetterSetterWithDefaultVersionForVersionedFieldEDataVersionAdapter(
            IEntityChangeTrackerStateInfo info)
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
        public bool SetValue<TData>(VersionedFieldTemplatedBase<TData> field, TData data, EDataVersion version)
            where TData : IEquatable<TData>
        {
            TData oldData = field.GetValue(version);
            field.SetValue(data, version);

            return !oldData.Equals(data);
        }
        #endregion

        #region IVersionedFieldDefaultVersionQueryable Members
        public TData GetDefaultValue<TData>(VersionedFieldTemplatedBase<TData> field)
        {
            return field.GetValue(this.GetDefaultVersion());
        }
        public bool SetDefaultValue<TData>(VersionedFieldTemplatedBase<TData> field, TData value)
            where TData : IEquatable<TData>
        {
            TData oldData = this.GetDefaultValue(field);
            field.SetValue(value, this.GetDefaultVersion());

            return !oldData.Equals(value);
        }
        #endregion

        #region IDefaultVersionGetterForVersionedFieldAdapter<EDataVersion> Members
        public EDataVersion GetDefaultVersion()
        {
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
                default:
                    throw new VersionNotFoundException(
                        "This entity version has no default data version");
            }
        }
        #endregion
    }
}