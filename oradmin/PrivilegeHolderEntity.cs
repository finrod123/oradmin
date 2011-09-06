using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

namespace oradmin
{
    /// <summary>
    /// Abstract class to represent an entity that can hold privilege and role grants,
    /// that can be granted privileges to or revoked privileges from
    /// </summary>
    public abstract class PrivilegeHolderEntity : IDisposable, INotifyPropertyChanged,
        IValidatableObject<EPrivilegeHolderEntityError>
    {
        #region Members
        // session
        protected SessionManager.Session session;
        // spojeni
        protected OracleConnection conn;        
        // data z db
        protected PrivilegeHolderEntityData data;

        // validation data
        protected bool hasErrors = false;
        protected Dictionary<EPrivilegeHolderEntityError,
                             ObjectError<EPrivilegeHolderEntityError>> errors =
                             new Dictionary<EPrivilegeHolderEntityError, ObjectError<EPrivilegeHolderEntityError>>();
        
        // system privilege grants manager
        PrivilegeHolderEntitySystemPrivilegeManager sysPrivManager;
        // role manager
        PrivilegeHolderEntityRoleManager roleManager;
        // table privilege grants manager
        PrivilegeHolderEntityTablePrivilegeManager tabPrivManager;
        // column privilege grants manager
        PrivilegeHolderEntityColumnPrivilegeManager colPrivManager;
        #endregion

        #region Constructor
        public PrivilegeHolderEntity(PrivilegeHolderEntityData data, SessionManager.Session session)
        {
            if (session == null)
                throw new ArgumentNullException("Session");

            this.session = session;
            this.conn = session.Connection;
            this.data = data;
        }
        #endregion

        #region Properties
        public string Name
        {
            get { return this.data.name; }
        }
        #endregion

        #region PrivilegeHolderEntity data class
        public abstract class PrivilegeHolderEntityData
        {
            #region Members
            public string name;
            #endregion

            #region Constructor
            public PrivilegeHolderEntityData(string name)
            {
                this.name = name;
            }
            #endregion
        }
        #endregion

        #region IDisposable Members
        public abstract void Dispose();
        #endregion

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region IValidatableObject<EPrivilegeHolderEntityError> Members

        public bool HasErrors
        {
            get { throw new NotImplementedException(); }
        }

        public ReadOnlyCollection<ObjectError<EPrivilegeHolderEntityError>> Errors
        {
            get { throw new NotImplementedException(); }
        }

        public bool Validate(out List<ObjectError<EPrivilegeHolderEntityError>> errors)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public enum EPrivilegeHolderEntityError
    {
        EmptyName,
        None
    }
}
