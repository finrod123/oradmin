using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace oradmin
{
    public interface IVersionedField<TVersion, TData>
        where TVersion : IEquatable<TVersion>
    {
        TData GetValue(TVersion version);
        void SetValue(TData data, TVersion version);
        bool HasVersion(TVersion version);
    }

    public class VersionedField<TData> : IVersionedField<EDataVersion, TData>
        where TData : class
    {
        TData original,
               current,
               proposed;

        public VersionedField(TData data)
        {
            
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
