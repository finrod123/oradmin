using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;


namespace oradmin
{
    class RoleManager
    {
        #region Constants

        public static const string DBA_ROLES_SELECT = @"
            SELECT
                role, password_required
            FROM
                DBA_ROLES";

        public static const string CURRENT_USER_DIRECT_ROLE_GRANTS_SELECT = @"
            SELECT
                username, granted_role, admin_option, default_role
            FROM
                USER_ROLE_PRIVS";

        public static const string CURRENT_USER_ROLE_ROLE_GRANTS_SELECT = @"
            SELECT
                role, granted_role, admin_option
            FROM
                ROLE_ROLE_PRIVS";

        public static const string DBA_ROLE_PRIVS_SELECT = @"
            SELECT
                grantee, granted_role, admin_option, default_role
            FROM
                DBA_ROLE_PRIVS";

        #endregion
        #region Members
        SessionManager.Session session;
        OracleConnection conn;

        ObservableCollection<Role> roles = new ObservableCollection<Role>();
        Dictionary<string, Role> name2Role = new Dictionary<string, Role>();
        List<RoleGrant> currentUserGrants = new List<RoleGrant>();
        List<RoleGrant> rolesGrants = new List<RoleGrant>();

        #endregion

        #region Constructor

        public RoleManager(SessionManager.Session session)
        {
            if (session == null)
                throw new ArgumentNullException("Session");

            this.session = session;
            this.conn = session.Connection;
            // load information about roles in a database
            loadRoles();
        }

        #endregion

        #region Public interface

        public RoleRoleManagerLocal CreateRoleLocalManager(Role role)
        {
            return new RoleRoleManagerLocal(session, role);
        }
        #endregion

        #region Public events
        public event RoleGrantsRefreshedHandler RoleGrantsRefreshed;
        #endregion

        #region Protected interface

        protected IEnumerable<RoleGrant> downloadCurrentUserGrants()
        {
            return
                from grant in currentUserGrants
                select grant;
        }
        protected IEnumerable<RoleGrant> downloadUserGrants(UserManager.User user)
        {
            return
                from grant in rolesGrants
                where grant.Grantee == user.Name
                select grant;
        }
        protected IEnumerable<RoleGrant> downloadRoleGrants(RoleManager.Role role)
        {
            return
                from grant in rolesGrants
                where grant.Grantee == role.Name
                select grant;
        }
        protected bool getRoleByName(string name, out Role role)
        {
            return name2Role.TryGetValue(name, out role);
        }
        

        #endregion

        #region Helper methods
        /// <summary>
        /// Loads roles and their grants on the beginning of the session
        /// </summary>
        void loadRoles()
        {
            bool currentUserRolesLoaded = false;
            // flush role to role grants information
            rolesGrants.Clear();

            // try to load role grants from DBA_ROLE_PRIVS
            if (!loadAllRoleGrants())
                // load user accessible roles
                currentUserRolesLoaded = loadCurrentUserRoleGrants();
            
            // try to load role information from DBA_ROLES
            if (!loadDbaRoles() &&
                currentUserRolesLoaded)
            {
                //create them from role
                // grant information stored in "rolesGrants"
                createRolesFromGrants();
            }

            // load role grants of those roles currently directly granted to the current user
            loadCurrentUserDirectRoleGrants();

            // notify roles about new role grant information
            OnRoleGrantsRefreshed();

            // update effective privileges and role grants of users and roles

        }

        /// <summary>
        /// Creates user accessible role objects from role grants info (ROLE_ROLE_PRIVS)
        /// </summary>
        void createRolesFromGrants()
        {
            // get gistinct role names
            IEnumerable<string> roleNames =
                ((from role in rolesGrants
                  select role.Grantee)
                    .Union<string>
                (from role in rolesGrants
                 select role.GrantedRole))
                    .Distinct<string>();

            // walk them one by one and create roles
            foreach (string roleName in roleNames)
            {
                Role role = new Role(roleName, false, session);
                // add role to structures
                roles.Add(role);
                name2Role.Add(roleName, role);
            }
        }

        /// <summary>
        /// Tries to load inforation about all roles in a database from DBA_ROLES
        /// and creates those roles
        /// </summary>
        bool loadDbaRoles()
        {
            OracleCommand cmd = new OracleCommand(DBA_ROLES_SELECT, conn);
            OracleDataReader odr = cmd.ExecuteReader();

            if (!odr.HasRows)
                return false;

            while (odr.Read())
            {
                string roleName;
                bool passwordRequired;

                if (odr.IsDBNull(odr.GetOrdinal("role")))
                    continue;

                roleName = odr.GetString(odr.GetOrdinal("role"));
                passwordRequired = Role.ParsePasswordRequired(odr.GetValue(odr.GetOrdinal("password_required")));

                Role role;

                if (!name2Role.TryGetValue(roleName, out role))
                {
                    role = new Role(roleName, passwordRequired, session);
                    // add role to structures
                    name2Role.Add(roleName, role);
                    roles.Add(role);
                } else
                {
                    role.PasswordRequired = passwordRequired;
                }
            }

            return true;
        }
        /// <summary>
        /// Loads role grants from DBA_ROLE_PRIVS
        /// </summary>
        bool loadAllRoleGrants()
        {
            OracleCommand cmd = new OracleCommand(DBA_ROLE_PRIVS_SELECT, conn);
            OracleDataReader odr = cmd.ExecuteReader();

            if (!odr.HasRows)
                return false;

            while (odr.Read())
            {
                RoleGrant grant = loadDbaRoleGrant(odr);
                rolesGrants.Add(grant);
            }

            return true;
        }
        /// <summary>
        /// Loads role grants from ROLE_ROLE_PRIVS (current user accessible roles)
        /// </summary>
        bool loadCurrentUserRoleGrants()
        {
            OracleCommand cmd = new OracleCommand(CURRENT_USER_ROLE_ROLE_GRANTS_SELECT, conn);
            OracleDataReader odr = cmd.ExecuteReader();

            if (!odr.HasRows)
                return false;

            while (odr.Read())
            {
                RoleGrant grant = loadCurrentUserAccessibleRoleGrant(odr);
                rolesGrants.Add(grant);
            }

            return true;
        }
        /// <summary>
        /// Loads role grants from USER_ROLE_PRIVS (roles directly granted to current user)
        /// </summary>
        void loadCurrentUserDirectRoleGrants()
        {
            OracleCommand cmd = new OracleCommand(CURRENT_USER_DIRECT_ROLE_GRANTS_SELECT, conn);
            OracleDataReader odr = cmd.ExecuteReader();

            // flush current user direct role grants
            currentUserGrants.Clear();

            while(odr.Read())
            {
                RoleGrant grant = loadCurrentUserRoleGrant(odr);
                currentUserGrants.Add(grant);
            }
        }

        #region Helper grant loading methods
        RoleGrant loadCurrentUserRoleGrant(OracleDataReader odr)
        {
            string username;
            string grantedRole;
            bool adminOption;
            bool defaultRole;
            bool directGrant;

            if (odr.IsDBNull(odr.GetOrdinal("username")) ||
               odr.IsDBNull(odr.GetOrdinal("granted_role")))
                return null;

            username = odr.GetString(odr.GetOrdinal("username"));
            if (username.Equals("PUBLIC"))
                directGrant = false;
            else
                directGrant = true;

            grantedRole = odr.GetString(odr.GetOrdinal("granted_role"));

            adminOption = RoleGrant.ParseAdminOption(odr.GetValue(odr.GetOrdinal("admin_option")));
            defaultRole = RoleGrant.ParseDefaultRole(odr.GetValue(odr.GetOrdinal("default_role")));

            return new RoleGrant(username, grantedRole, adminOption, defaultRole, directGrant);
        }
        RoleGrant loadCurrentUserAccessibleRoleGrant(OracleDataReader odr)
        {
            string role;
            string grantedRole;
            bool adminOption;
            bool directGrant = true;

            if (odr.IsDBNull(odr.GetOrdinal("role")) ||
               odr.IsDBNull(odr.GetOrdinal("granted_role")))
                return null;

            role = odr.GetString(odr.GetOrdinal("role"));
            grantedRole = odr.GetString(odr.GetOrdinal("grantedRole"));

            adminOption = RoleGrant.ParseAdminOption(odr.GetValue(odr.GetOrdinal("admin_option")));

            return new RoleGrant(role, grantedRole, adminOption, false, directGrant);
        }
        RoleGrant loadDbaRoleGrant(OracleDataReader odr)
        {
            string grantee;
            string grantedRole;
            bool adminOption;
            bool defaultRole;

            if (odr.IsDBNull(odr.GetOrdinal("grantee")) ||
               odr.IsDBNull(odr.GetOrdinal("granted_role")))
                return null;

            grantee = odr.GetString(odr.GetOrdinal("grantee"));
            grantedRole = odr.GetString(odr.GetOrdinal("grantedRole"));
            adminOption = RoleGrant.ParseAdminOption(odr.GetValue(odr.GetOrdinal("admin_option")));
            defaultRole = RoleGrant.ParseDefaultRole(odr.GetValue(odr.GetOrdinal("default_role")));

            return new RoleGrant(grantee, grantedRole, adminOption, defaultRole, true);
        }
        #endregion

        #endregion

        #region Helper methods
        private void OnRoleGrantsRefreshed()
        {
            if (RoleGrantsRefreshed != null)
            {
                RoleGrantsRefreshed();
            }
        }
        #endregion









        #region Local RoleManager class

        public abstract class RoleManagerLocal
        {
            #region Members
            protected SessionManager.Session session;
            protected RoleManager manager;
            protected OracleConnection conn;
            protected UserRole userRole;

            protected ObservableCollection<RoleGrant> roleGrants =
                new ObservableCollection<RoleGrant>();
            protected Dictionary<string, UserRole> dependants =
                new Dictionary<string, UserRole>();
            #endregion

            #region Constructor

            public RoleManagerLocal(SessionManager.Session session, UserRole userRole)
            {
                if (session == null)
                    throw new ArgumentNullException("Session");

                this.session = session;
                manager = session.RoleManager;
                this.userRole = userRole;
                this.conn = session.Connection;
                // register with events of a role manager
                this.manager.RoleGrantsRefreshed += new RoleGrantsRefreshedHandler(roleManager_RoleGrantsRefreshed);
            }

            #endregion

            #region Helper methods

            protected abstract void downloadData();
            protected abstract void roleManager_RoleGrantsRefreshed();

            #endregion
        }

        #endregion

        #region Role RoleManager class

        public class RoleRoleManagerLocal : RoleManagerLocal
        {
            #region Constructor
            public RoleRoleManagerLocal(SessionManager.Session session, Role role) :
                base(session, role)
            { }
            #endregion

            #region Public interface
            public void AddDependant(UserRole userRole)
            {
                if (!dependants.ContainsKey(userRole.Name))
                {
                    dependants.Add(userRole.Name, userRole);
                }
            }
            #endregion

            #region Helper methods
            protected override void downloadData()
            {
                // clear grant data
                roleGrants.Clear();
                // download them from manager
                IEnumerable<RoleGrant> newGrants = this.manager.downloadRoleGrants(userRole as Role);
                roleGrants.AddRange(newGrants);
                // walk them and register them
                registerAsDependant(newGrants);
            }
            protected override void roleManager_RoleGrantsRefreshed()
            {
                downloadData();
            }
            #endregion

            #region Helper methods
            private void registerAsDependant(IEnumerable<RoleGrant> grants)
            {
                foreach (RoleGrant grant in grants)
                {
                    Role ancestor;
                    if (manager.getRoleByName(grant.GrantedRole, out ancestor))
                    {
                        
                    }
                }
            }
            #endregion
        }

        #endregion










        #region Role class

        public class Role : UserRole
        {
            #region Members
            bool passwordRequired;
            RoleManager manager;
            #endregion

            #region Constructor

            public Role(string name, bool passwordRequired, SessionManager.Session session) :
                base(name, session)
            {
                this.manager = session.RoleManager;
                this.passwordRequired = passwordRequired;
                // create managers
                createManagers();
            }

            #endregion

            #region Properties
            public bool PasswordRequired
            {
                get { return passwordRequired; }
                set { passwordRequired = value; }
            }
            #endregion


            #region Helper methods
            protected override void createManagers()
            {
                // create role priv grant manager
                privManager = session.PrivManager.CreateRolePrivLocalManager(this);
                // create role role grant manager
                roleManager = manager.CreateRoleLocalManager(this);
            }
            #endregion

            #region Static public interface
            public static bool ParsePasswordRequired(object value)
            {
                if (value == DBNull.Value)
                    return false;

                StringToBoolConverter converter = new StringToBoolConverter();

                return (bool)converter.Convert(value, typeof(bool),
                    EStringToBoolConverterOption.RequiredNull, null);
            }
            #endregion
        }

        #endregion
    }

    public delegate void RoleGrantsRefreshedHandler();
}
