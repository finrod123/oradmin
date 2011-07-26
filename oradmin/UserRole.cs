using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

namespace oradmin
{
    public abstract class UserRole
    {
        #region Members
        // session
        protected SessionManager.Session session;
        // spojeni
        protected OracleConnection conn;
        // lokalni managery
        protected PrivManager.PrivManagerLocal privManager;
        protected RoleManager.RoleManagerLocal roleManager;
        
        // data z db
        protected string name;

        #endregion

        #region Constructor

        public UserRole(string name, SessionManager.Session session)
        {
            if (session == null)
                throw new ArgumentNullException("Session");

            this.session = session;
            this.name = name;
            this.conn = session.Connection;
        }

        #endregion

        #region Properties

        public string Name
        {
            get { return name; }
        }

        #endregion

        #region Helper methods

        protected abstract void createManagers();

        #endregion
    }
}
