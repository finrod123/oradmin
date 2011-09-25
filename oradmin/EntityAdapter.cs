using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace oradmin
{
    public interface IEntityDataAdapter
    {
        void Fill(IEntityManager entityManager);

        bool GetChanges(EntityObject entity, out IEntityDataContainer data);
        bool GetChanges(IEnumerable<EntityObject> entities,
                        out IEnumerable<IEntityDataContainer> data);
        bool GetChanges(out IEnumerable<IEntityDataContainer> data);

        void Update(IEntityManager entityManager);
        void Update(IEnumerable<EntityObject> entities);
        void Update(EntityObject entity);
    }

    public abstract class EntityDataAdapter : IEntityDataAdapter
    {
        #region IEntityAdapter Members
        public void Fill(IEntityManager entityManager)
        {
            throw new NotImplementedException();
        }
        public bool GetChanges(EntityObject entity, out IEntityDataContainer data)
        {
            throw new NotImplementedException();
        }
        public bool GetChanges(IEnumerable<EntityObject> entities, out IEnumerable<IEntityDataContainer> data)
        {
            throw new NotImplementedException();
        }
        public bool GetChanges(out IEnumerable<IEntityDataContainer> data)
        {
            throw new NotImplementedException();
        }
        public void Update(IEntityManager entityManager)
        {
            throw new NotImplementedException();
        }
        public void Update(IEnumerable<EntityObject> entities)
        {
            throw new NotImplementedException();
        }
        public void Update(EntityObject entity)
        {
            throw new NotImplementedException();
        }
        #endregion
    }

    public interface IEquatableObject : IEquatable<IEquatableObject>
    {
        #region IEquatable<EquatableObject> Members
        bool Equals(IEquatableObject other);
        #endregion
    }
}
