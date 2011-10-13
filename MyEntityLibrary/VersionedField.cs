using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Data;

namespace myentitylibrary
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

    public interface IVersionedFieldBase :
        IVersionedFieldVersionQueryable<EDataVersion>,
        IEditableObjectInfo
    {
        #region Properties
        string Name { get; }
        bool IsEditing { get; private set; }
        #endregion

        #region IVersionedFieldBase<EDataVersion> Members
        bool HasVersion(EDataVersion version);
        #endregion
    }

    public interface IVersionedFieldTemplatedBase<TData> :
        IVersionedFieldBase,
        IVersionedField<EDataVersion, TData>
    {
        #region IVersionedField<EDataVersion,TData> Members
        TData GetValue(EDataVersion version);
        void SetValue(TData data, EDataVersion version);
        #endregion
    }

    public abstract class VersionedFieldBase<TData> :
        IVersionedFieldTemplatedBase<TData>
    {
        #region Members
        protected string name;
        protected TData original,
                        current;
        #endregion

        #region Constructor
        public VersionedFieldBase(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            this.Name = name;
        }
        #endregion

        public abstract bool HasVersion(EDataVersion version);
        public abstract TData GetValue(EDataVersion version);
        public void SetValue(TData data, EDataVersion version)
        {
            if (!this.HasVersion(version))
                throw new VersionNotFoundException(
                    string.Format("This version or version combination does not exist: {0}",
                                  version.ToString()));

            if ((version & EDataVersion.Original) == EDataVersion.Original)
                this.original = this.getDataToSet(data);

            if ((version & EDataVersion.Current) == EDataVersion.Current)
                this.current = this.getDataToSet(data);

        }
        
        public string Name
        {
            get { return this.name; }
            private set { this.name = value; }
        }
        public bool IsEditing { get; private set; }
    }

    public abstract class VersionedFieldReferenceType<TData> :
        VersionedFieldBase<TData>
        where TData : class
    {
        #region Constructor
        public VersionedFieldReferenceType(string fieldName) :
            base(fieldName)
        { }
        #endregion

        public virtual TData GetValue(EDataVersion version)
        {
            if (!this.HasVersion(version))
                throw new VersionNotFoundException(
                    string.Format("Version {0} not found!", version.ToString()));

            switch (version)
            {
                case EDataVersion.Original:
                    return this.getDataToGet(this.original);
                case EDataVersion.Current:
                    return this.getDataToGet(this.current);
                default:
                    throw new VersionNotFoundException(
                        string.Format("Cannot get version {0}", version.ToString()));
            }
        }
        public override bool  HasVersion(EDataVersion version)
        {
            bool hasVersion = true;

            if ((version & EDataVersion.Original) == EDataVersion.Original)
                hasVersion = hasVersion &&
                             this.hasFieldValue(this.original);

            if ((version & EDataVersion.Current) == EDataVersion.Current)
                hasVersion = hasVersion &&
                             this.hasFieldValue(this.current);

            return hasVersion;
        }

        protected abstract bool hasOriginalValue();
        protected abstract bool hasCurrentValue();
        protected abstract TData getDataToSet(TData input);
        protected TData getDataToGet(TData input)
        {
            return input;
        }
    }

    public class VersionedFieldClonable<TData> : VersionedFieldReferenceType<TData>
        where TData : class, ICloneable
    {
        #region Constructor
        public VersionedFieldClonable(string name, TData data):
            base(name)
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

        protected override TData getDataToSet(TData input)
        {
            return input.Clone() as TData;
        }
    }

    public class VersionedFieldNullableValueType<TData> :
        VersionedFieldReferenceType<Nullable<TData>>
        where TData : struct
    {
        #region Constructor
        public VersionedFieldNullableValueType(string name, TData? data):
            base(name)
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

        protected override TData? getDataToSet(TData? input)
        {
            return input.Value;
        }
    }

    public class VersionedFieldValueType<TData> : VersionedFieldBase<TData>
        where TData : struct
    {
        #region Members
        TData? original,
               current;
        #endregion

        #region Constructor
        public VersionedFieldValueType(string name, TData data):
            base(name)
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
        
        protected override TData getDataToSet(TData input)
        {
            return input;
        }
    }
}
