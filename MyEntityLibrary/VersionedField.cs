using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Data;

namespace oradmin
{
    public interface IVersionedFieldVersionQueryable<TVersion>
        where TVersion : struct
    {
        bool HasVersion(TVersion version);
    }

    public interface IVersionedFieldQueryable<TVersion, TData>
        where TVersion : struct
    {
        TData GetValue(TVersion version);
    }

    public interface IVersionedFieldModifiable<TVersion, TData>
        where TVersion : struct
    {
        void SetValue(TData data, TVersion version);
    }

    public interface IVersionedFieldQueryableModifiable<TVersion, TData> :
        IVersionedFieldQueryable<TVersion, TData>,
        IVersionedFieldModifiable<TVersion, TData>
        where TVersion : struct
    { }

    
    public interface IVersionedField<TVersion, TData> :
        IVersionedFieldVersionQueryable<TVersion>,
        IVersionedFieldQueryableModifiable<TVersion, TData>
        where TVersion : struct
    { }

    public abstract class VersionedFieldBase :
        IVersionedFieldVersionQueryable<EDataVersion>,
        IEditableObjectInfo
    {
        #region Properties
        public bool IsEditing { get; private set; }
        #endregion

        #region IVersionedFieldBase<EDataVersion> Members
        public abstract bool HasVersion(EDataVersion version);
        #endregion
    }

    public abstract class VersionedFieldTemplatedBase<TData> :
        VersionedFieldBase,
        IVersionedField<EDataVersion, TData>
    {
        #region IVersionedField<EDataVersion,TData> Members
        public abstract TData GetValue(EDataVersion version);
        public abstract void SetValue(TData data, EDataVersion version);
        #endregion
    }

    public abstract class VersionedFieldGetBase<TData> :
        VersionedFieldTemplatedBase<TData>
    {
        #region Members
        protected TData original,
                        current;
        #endregion

        public override TData GetValue(EDataVersion version)
        {
            if (!this.HasVersion(version))
                throw new VersionNotFoundException(
                    string.Format("Version {0} not found!", version.ToString()));

            switch (version)
            {
                case EDataVersion.Original:
                    return this.original;
                case EDataVersion.Current:
                    return this.current;
                default:
                    return default(TData);
            }
        }
        public override bool HasVersion(EDataVersion version)
        {
            switch (version)
            {
                case EDataVersion.Original:
                    return this.original != null;
                case EDataVersion.Current:
                    return this.current != null;
            }

            return false;
        }
    }

    public class VersionedFieldClonable<TData> : VersionedFieldGetBase<TData>
        where TData : class, ICloneable
    {
        #region Constructor
        public VersionedFieldClonable(TData data)
        {
            if (data != null)
            {
                this.original = data.Clone() as TData;
                this.current = data.Clone() as TData;

            } else
            {
                this.original = null;
                this.current = null;
            }
        }
        #endregion

        public override void SetValue(TData data, EDataVersion version)
        {
            TData member;

            switch (version)
            {
                case EDataVersion.Original:
                    member = this.original;
                    break;
                case EDataVersion.Current:
                    member = this.current;
                    break;
            }

            member = data.Clone() as TData;
        }
    }

    public class VersionedFieldNullableValueType<TData> :
        VersionedFieldGetBase<Nullable<TData>>
        where TData : struct
    {
        #region Constructor
        public VersionedFieldNullableValueType(TData? data)
        {
            if (data.HasValue)
            {
                this.original = new TData?(data.Value);
                this.current = new TData?(data.Value);
            } else
            {
                this.original = null;
                this.current = null;
            }
        }
        #endregion

        public override void SetValue(TData? data, EDataVersion version)
        {
            TData? member;

            switch (version)
            {
                case EDataVersion.Original:
                    member = this.original;
                    break;
                case EDataVersion.Current:
                    member = this.current;
                    break;
            }

            member = data.Value;
        }
    }

    public class VersionedFieldValueType<TData> : VersionedFieldTemplatedBase<TData>
        where TData : struct
    {
        #region Members
        TData? original,
               current;
        #endregion

        #region Constructor
        public VersionedFieldValueType(TData data)
        {
            this.original = new TData?(data);
            this.current = new TData?(data);
        }
        #endregion

        public override TData GetValue(EDataVersion version)
        {
            if (!this.HasVersion(version))
                throw new VersionNotFoundException(
                    string.Format("Version {0} not found!", version.ToString()));

            switch (version)
            {
                case EDataVersion.Original:
                    return this.original.Value;
                case EDataVersion.Current:
                    return this.current.Value;
                default:
                    return default(TData);
            }
        }
        public override void SetValue(TData data, EDataVersion version)
        {
            switch (version)
            {
                case EDataVersion.Original:
                    this.original = data;
                    break;
                case EDataVersion.Current:
                    this.current = data;
                    break;
            }
        }
        public override bool HasVersion(EDataVersion version)
        {
            switch (version)
            {
                case EDataVersion.Original:
                    return this.original.HasValue;
                case EDataVersion.Current:
                    return this.current.HasValue;
            }

            return false;
        }
    }
}
