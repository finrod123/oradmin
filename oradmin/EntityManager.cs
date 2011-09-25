using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace oradmin
{
    public interface IEntityManager
    {
        bool Loaded();
        void Load();

        void AddObject(EntityObject entity);
        void DeleteObject(EntityObject entity);
        
        EntityObject CreateObject();

        void MergeData(IEnumerable<IEntityDataContainer> data);
    }

    public interface IRefreshableEntityManager
    {
        void Refresh();
        void Refresh(IEnumerable<EntityObject> entities);
        void Refresh(EntityObject entity);
    }

    public interface IUpdatableEntityManager
    {
        void SaveChanges();
        void SaveChanges(IEnumerable<EntityObject> entities);
        void SaveChanges(EntityObject entity);
    }

    public abstract class EntityManager : IEntityManager,
        IRefreshableEntityManager, IUpdatableEntityManager,
        IRevertibleChangeTracking
    {

        #region IEntityManager Members
        public bool Loaded()
        {
            throw new NotImplementedException();
        }

        public void Load()
        {
            throw new NotImplementedException();
        }

        public void AddObject(EntityObject entity)
        {
            throw new NotImplementedException();
        }

        public void DeleteObject(EntityObject entity)
        {
            throw new NotImplementedException();
        }

        public EntityObject CreateObject()
        {
            throw new NotImplementedException();
        }

        public void MergeData(IEnumerable<IEntityDataContainer> data)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IRefreshableEntityManager Members

        public void Refresh()
        {
            throw new NotImplementedException();
        }

        public void Refresh(IEnumerable<EntityObject> entities)
        {
            throw new NotImplementedException();
        }

        public void Refresh(EntityObject entity)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IUpdatableEntityManager Members

        public void SaveChanges()
        {
            throw new NotImplementedException();
        }

        public void SaveChanges(IEnumerable<EntityObject> entities)
        {
            throw new NotImplementedException();
        }

        public void SaveChanges(EntityObject entity)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IRevertibleChangeTracking Members

        public void RejectChanges()
        {
            throw new NotImplementedException();
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


}