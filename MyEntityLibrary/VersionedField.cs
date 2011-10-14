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

    public interface IVersionedFieldQueryable<TVersion>
        where TVersion : struct
    {
        object GetValue(TVersion version);
    }

    public interface IVersionedFieldModifiable<TVersion>
        where TVersion : struct
    {
        void SetValue(object data, TVersion version);
    }

    public interface IVersionedFieldQueryableModifiable<TVersion> :
        IVersionedFieldQueryable<TVersion>,
        IVersionedFieldModifiable<TVersion>
        where TVersion : struct
    { }


    public interface IVersionedField<TVersion> :
        IVersionedFieldVersionQueryable<TVersion>,
        IVersionedFieldQueryableModifiable<TVersion>
        where TVersion : struct
    {
        string Name { get; }
    }

    public interface IVersionedFieldBase :
        IVersionedField<EDataVersion>
    { }

    public abstract class VersionedFieldBase :
        IVersionedFieldBase
    {
        #region Members
        protected object original,
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

        #region IVersionedField<EDataVersion> Members
        public string Name { get; private set; }
        #endregion

        #region IVersionedFieldVersionQueryable<EDataVersion> Members
        public bool HasVersion(EDataVersion version)
        {
            bool hasVersions = true;

            if ((version & EDataVersion.Original) == EDataVersion.Original)
                hasVersions = hasVersions &&
                              this.original != null;

            if ((version & EDataVersion.Current) == EDataVersion.Current)
                hasVersions = hasVersions &&
                              this.current != null;

            return hasVersions;
        }
        #endregion

        #region IVersionedFieldQueryable<EDataVersion> Members
        public object GetValue(EDataVersion version)
        {
            if (!this.HasVersion(version))
                throw new VersionNotFoundException(
                    string.Format("Version not found: {0}", version.ToString()));

            switch (version)
            {
                case EDataVersion.Original:
                    return this.original;
                    break;
                case EDataVersion.Current:
                    return this.current;
                    break;
                default:
                    throw new ArgumentException(
                        string.Format("Cannot find version: {0}", version.ToString()),
                        "version");
            }
        }
        #endregion

        #region IVersionedFieldModifiable<EDataVersion> Members
        public virtual void SetValue(object data, EDataVersion version)
        {
            // check the input data type
            if (!this.isValidInputType(data))
                throw new ArgumentException(
                    string.Format("Input data type not valid: {0}",
                                   data.GetType().ToString()),
                    "data");

            // check the version
            if (!this.HasVersion(version))
                throw new VersionNotFoundException(
                    string.Format("Version not found: {0}", version.ToString()));

            // set data
            if ((version & EDataVersion.Original) == EDataVersion.Original)
                this.original = this.createDataForSet(data);

            if ((version & EDataVersion.Current) == EDataVersion.Current)
                this.current = this.createDataForSet(data);
        }
        #endregion

        protected abstract bool isValidInputType(object input);
        protected abstract object createDataForSet(object input);
    }

    public class VersionedFieldClonable : VersionedFieldBase
    {
        #region Constructor
        public VersionedFieldClonable(string name) :
            base(name)
        { }
        #endregion

        protected override object createDataForSet(object input)
        {
            ICloneable data = input as ICloneable;

            return data.Clone();
        }
        protected override bool isValidInputType(object input)
        {
            Type inputType = input.GetType();

            return inputType is ICloneable;
        }
    }

    public abstract class VersionedFieldValueTypeBase : VersionedFieldBase
    {
        #region Constructor
        public VersionedFieldValueTypeBase(string name) :
            base(name)
        { }
        #endregion

        protected override object createDataForSet(object input)
        {
            return input;
        }
        protected virtual bool isValidInputType(object input)
        {
            Type type = input.GetType();

            return type.IsValueType;
        }
    }

    public class VersionFieldValueType : VersionedFieldValueTypeBase
    {
        #region Constructor
        public VersionFieldValueType(string name) :
            base(name)
        { }
    }

    public class VersionedFieldNullableValueType : VersionedFieldValueTypeBase
    {
        #region Constructor
        public VersionedFieldNullableValueType(string name) :
            base(name)
        { }
        #endregion

        protected override bool isValidInputType(object input)
        {
            Type type = input.GetType();

            if (!type.IsGenericType)
                return false;

            return type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
    }
}
