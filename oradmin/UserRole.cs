using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

namespace oradmin
{
    public abstract class UserRole : IDisposable
    {
        #region Members
        // session
        protected SessionManager.Session session;
        // spojeni
        protected OracleConnection conn;        
        // data z db
        protected UserRoleData data;
        #endregion

        #region Constructor
        public UserRole(UserRoleData data, SessionManager.Session session)
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

        #region UserRole data class
        public abstract class UserRoleData
        {
            #region Members
            public string name;
            #endregion

            #region Constructor
            public UserRoleData(string name)
            {
                this.name = name;
            }
            #endregion
        }
        #endregion

        #region IDisposable Members
        public abstract void Dispose();
        #endregion
    }
}
