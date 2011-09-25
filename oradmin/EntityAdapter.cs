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
            // refresh all like with load and then force manager
            // to merge new data in

        }
        public abstract bool GetChanges(EntityObject entity, out IEntityDataContainer data);
        public abstract bool GetChanges(IEnumerable<EntityObject> entities, out IEnumerable<IEntityDataContainer> data);
        public abstract bool GetChanges(out IEnumerable<IEntityDataContainer> data);
        public abstract void Update(IEntityManager entityManager);
        public abstract void Update(IEnumerable<EntityObject> entities);
        public abstract void Update(EntityObject entity);
        #endregion
    }

    public interface IEquatableObject : IEquatable<IEquatableObject>
    {
        #region IEquatable<EquatableObject> Members
        bool Equals(IEquatableObject other);
        #endregion
    }
}
