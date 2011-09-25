using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace oradmin
{
    public interface IEntityStateManager
    {
        IEnumerable<IEntityChangeTracker> GetEntityEntries(EEntityState state);
        IEnumerable<IEntityChangeTracker> GetChanged();
        IEnumerable<IEntityChangeTracker> GetAdded();
        IEnumerable<IEntityChangeTracker> GetDeleted();
    }

    public abstract class EntityStateManager : IEntityStateManager
    {

        #region IEntityStateManager Members

        public IEnumerable<IEntityChangeTracker> GetEntityEntries(EEntityState state)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IEntityChangeTracker> GetChanged()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IEntityChangeTracker> GetAdded()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IEntityChangeTracker> GetDeleted()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}