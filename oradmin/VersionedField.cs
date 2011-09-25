using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

    public abstract class VersionedFieldBase : IVersionedFieldBase<EDataVersion>
    {
        #region IVersionedFieldBase<EDataVersion> Members
        public abstract bool HasVersion(EDataVersion version);
        #endregion
    }

    public class VersionedField<TData> :
        VersionedFieldBase, IVersionedFieldWithDefaultValue<EDataVersion, TData>
        where TData : class
    {
        #region Members
        TData original,
              current,
              proposed; 
        #endregion

        #region Constructor
        #endregion

        public override bool HasVersion(EDataVersion version)
        {
            throw new NotImplementedException();
        }
        
        #region IVersionedFieldWithDefaultValue<EDataVersion,TData> Members
        public TData DefaultValue
        {
            get { throw new NotImplementedException(); }
        }
        #endregion
        #region IVersionedField<EDataVersion,TData> Members
        public TData GetValue(EDataVersion version)
        {
            throw new NotImplementedException();
        }
        public void SetValue(TData data, EDataVersion version)
        {
            throw new NotImplementedException();
        }
        #endregion
        
        #region Properties
        public EEntityState EntityState { get; private set; }
        #endregion
    }

    public enum EDataVersion
    {
        Original,
        Current,
        Proposed,
        Default
    }
}
