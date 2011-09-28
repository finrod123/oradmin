using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Data;

namespace oradmin
{
    public interface IVersionedFieldBase<TVersion>
        where TVersion : IEquatable<TVersion>
    {
        bool HasVersion(TVersion version);
    }

    public interface IVersionedField<TVersion, TData> :
        IVersionedFieldBase<TVersion>
        where TVersion : IEquatable<TVersion>
    {
        TData GetValue(TVersion version);
        void SetValue(TData data, TVersion version);
    }

    public interface IVersionedFieldWithDefaultValue<TVersion, TData> :
        IVersionedField<TVersion, TData>
    {
        TData DefaultValue { get; }
    }

    public abstract class VersionedFieldBase : IVersionedFieldBase<EDataVersion>,
        IEditableObject, IRevertibleChangeTracking
    {
        #region Properties
        public bool IsEditing { get; private set; }
        #endregion
        
        #region IVersionedFieldBase<EDataVersion> Members
        public abstract bool HasVersion(EDataVersion version);
        #endregion

        #region IEditableObject Members
        public abstract void BeginEdit();
        public abstract void CancelEdit();
        public abstract void EndEdit();
        #endregion

        #region IRevertibleChangeTracking Members
        public void RejectChanges()
        {
            if (!IsEditing)
            {

            }
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

    public abstract class VersionedFieldTemplatedBase<TData> :
        VersionedFieldBase
    {
        public override bool HasVersion(EDataVersion version)
        {
            return GetValue(version) != null;
        }
        
        #region IVersionedField<EDataVersion,TData> Members
        public abstract TData GetValue(EDataVersion version);
        public abstract void SetValue(TData data, EDataVersion version);
        #endregion

        #region IRevertibleChangeTracking Members
        public abstract void RejectChanges();
        #endregion

        #region IChangeTracking Members
        public abstract void AcceptChanges();
        public abstract bool IsChanged { get; }
        #endregion
    }

    public abstract class VersionedFieldReferenceType<TData> : VersionedFieldTemplatedBase<TData>
        where TData : class
    {
        #region Members
        protected TData original,
                        current,
                        proposed;
        #endregion
        
        public override TData GetValue(EDataVersion version)
        {
            switch (version)
            {
                case EDataVersion.Original:
                    return this.original;
                case EDataVersion.Current:
                    return this.current;
                case EDataVersion.Proposed:
                    return this.proposed;
                case EDataVersion.Default:
                    return null;
            }
        }
        public override void SetValue(TData data, EDataVersion version)
        {
            switch (version)
            {
                case EDataVersion.Original:
                    this.original = data;
                case EDataVersion.Current:
                    this.current = data;
                case EDataVersion.Proposed:
                    this.proposed = data;
                case EDataVersion.Default:
                    throw new VersionNotFoundException("Default version not found!");
            }
        }

        public override void RejectChanges()
        {
            throw new NotImplementedException();
        }

        public override void AcceptChanges()
        {
            throw new NotImplementedException();
        }

        public override bool IsChanged
        {
            get { throw new NotImplementedException(); }
        }
    }

    public class VersionedFieldClonable<TData> : VersionedFieldReferenceType<TData>
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

            this.proposed = null;
        }
        #endregion
    }

    public class VersionedFieldNullableValueType<TData> :
        VersionedFieldReferenceType<Nullable<TData>>
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

            this.proposed = null;
        }
        #endregion
    }

    public class VersionedFieldValueType<TData> : VersionedFieldTemplatedBase<TData>
        where TData : struct
    {
        #region Members
        TData? original,
               current,
               proposed;
        #endregion

        #region Constructor
        public VersionedFieldValueType(TData data)
        {
            this.original = new TData?(data);
            this.current = new TData?(data);
            this.proposed = null;
        }
        #endregion

        public override TData GetValue(EDataVersion version)
        {
            TData? tryReturn;

            switch (version)
            {
                case EDataVersion.Original:
                    tryReturn = this.original;
                    break;
                case EDataVersion.Current:
                    tryReturn = this.current;
                    break;
                case EDataVersion.Proposed:
                    tryReturn = this.proposed;
                    break;
                case EDataVersion.Default:
                    tryReturn = null;
                    break;
            }

            if (!tryReturn.HasValue)
                throw new VersionNotFoundException("Version requested not found!");

            return tryReturn.Value;
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
                case EDataVersion.Proposed:
                    this.proposed = data;
                    break;
                case EDataVersion.Default:
                    throw new NotSupportedException("Default version not supported");
                    break;
            }
        }



        public override void RejectChanges()
        {
            throw new NotImplementedException();
        }
        public override void AcceptChanges()
        {
            throw new NotImplementedException();
        }
        public override bool IsChanged
        {
            get { throw new NotImplementedException(); }
        }
    }

    public enum EDataVersion
    {
        Original,
        Current,
        Proposed,
        Default
    }
}
