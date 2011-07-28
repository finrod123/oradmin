using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        protected RoleManager userRoleManager;
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
            this.userRoleManager = session.RoleManager;
            this.name = name;
            this.conn = session.Connection;
            // register with events of the role manager
            userRoleManager.RoleGrantsOfAllRolesRefreshed +=
                new RoleGrantsOfAllRolesRefreshedHandler(manager_RoleGrantsOfAllRolesRefreshed);
            userRoleManager.RoleGrantsRefreshed +=
                new RoleGrantsRefreshedHandler(manager_RoleGrantsRefreshed);
        }

        #endregion

        #region Public interface
        /// <summary>
        /// Refreshes the list of privilege and role grants
        /// </summary>
        public override void RefreshChanges()
        {
            // let privilege manager download the privilege data from the role
            privManager.RefreshPrivileges();
            // let role manager refresh itself
            roleManager.RefreshGrants();
        }
        #endregion

        #region Properties
        public string Name
        {
            get { return name; }
        }
        public abstract bool IsIndependent { get; }
        public RoleManager.RoleManagerLocal RoleManager
        {
            get { return roleManager; }
        }
        public PrivManager.PrivManagerLocal PrivManager
        {
            get { return privManager; }
        }
        #endregion

        #region Helper methods
        protected abstract void createManagers();
        void manager_RoleGrantsRefreshed(ReadOnlyCollection<UserRole> affected)
        {
            if (affected.Contains(this))
            {
                privManager.DownloadPrivileges();
                roleManager.DownloadData();
            }
        }
        void manager_RoleGrantsOfAllRolesRefreshed()
        {
            privManager.DownloadPrivileges();
            roleManager.DownloadData();
        }
        #endregion
    }
}
