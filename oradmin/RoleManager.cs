using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;
using oradmin;


namespace oradmin
{
    class SessionRoleManager
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
        SessionSysPrivManager privManager;
        OracleConnection conn;

        ObservableCollection<Role> roles = new ObservableCollection<Role>();
        Dictionary<string, Role> name2Role = new Dictionary<string, Role>();
        List<RoleGrant> rolesGrants = new List<RoleGrant>();

        #endregion

        #region Constructor

        public SessionRoleManager(SessionManager.Session session)
        {
            if (session == null)
                throw new ArgumentNullException("Session");

            this.session = session;
            this.conn = session.Connection;
            privManager = session.SysPrivManager;
        }

        #endregion

        #region Public interface
        public RoleRoleManagerLocal CreateRoleLocalManager(Role role)
        {
            return new RoleRoleManagerLocal(session, role);
        }
        public UserRoleManagerLocal CreateUserLocalManager(UserManager.User user)
        {
            return new UserRoleManagerLocal(session, user);
        }
        public CurrentUserRoleManagerLocal CreateCurrentUserLocalManager(UserManager.CurrentUser currentUser)
        {
            return new CurrentUserRoleManagerLocal(session, currentUser);
        }
        #endregion

        #region Public events
        public event RoleGrantsOfAllRolesRefreshedHandler RoleGrantsOfAllRolesRefreshed;
        public event RoleGrantsRefreshedHandler RoleGrantsRefreshed;
        #endregion

        #region Protected interface

        protected IEnumerable<RoleGrant> downloadCurrentUserGrants()
        {
            return
                from grant in currentUserGrants
                select grant;
        }
        protected IEnumerable<RoleGrant> downloadRoleGrants(UserRole userRole)
        {
            return
                from grant in rolesGrants
                where grant.Grantee == userRole.Name
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
        /// ---TODO: add new and remove nonexistent roles---
        public void LoadRoles()
        {
            bool currentUserRolesLoaded = false;
            bool allRolesLoaded = false;
            bool currentUserDirectRolesLoaded = false;
            // load privileges:
            // 1) system
            privManager.RefreshRolesData();
            // 2), 3) TODO tab, col privs
            // ---TODO---

            // try to load role grants from DBA_ROLE_PRIVS
            if (!loadAllRoleGrants())
            {
                // load user accessible roles
                if (!loadCurrentUserRoleGrants())
                {
                    if (loadCurrentUserDirectRoleGrants())
                        currentUserDirectRolesLoaded = true;
                } else
                    currentUserRolesLoaded = true;
            } else
                allRolesLoaded = true;
            
            // try to load role information from DBA_ROLES
            if (!loadDbaRoles())
            {
                if (allRolesLoaded || currentUserRolesLoaded)
                    //create them from role
                    // grant information stored in "rolesGrants"
                    createRolesFromGrants();
                else if (currentUserDirectRolesLoaded)
                    createRolesFromCurrentUserDirectRoleGrants();
            }

            // update users and roles
            updateChanges();
        }
        /// <summary>
        /// Creates user accessible role objects from role grants info (ROLE_ROLE_PRIVS)
        /// </summary>
        void createRolesFromGrants()
        {
            // get distinct role names
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
        /// Creates user accessible role objects from role grants info (USER_ROLE_PRIVS)
        /// </summary>
        void createRolesFromCurrentUserDirectRoleGrants()
        {
            // get distinct role names
            IEnumerable<string> roleNames =
                (from role in rolesGrants
                 select role.GrantedRole).Distinct();

            // walk them and create roles
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

            rolesGrants.Clear();

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

            rolesGrants.Clear();

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
        bool loadCurrentUserDirectRoleGrants()
        {
            OracleCommand cmd = new OracleCommand(CURRENT_USER_DIRECT_ROLE_GRANTS_SELECT, conn);
            OracleDataReader odr = cmd.ExecuteReader();

            if (!odr.HasRows)
                return false;

            // flush current user direct role grants
            rolesGrants.Clear();

            while(odr.Read())
            {
                RoleGrant grant = loadCurrentUserDirectRoleGrant(odr);
                rolesGrants.Add(grant);
            }

            return true;
        }

        #region Helper grant loading methods
        public static RoleGrant loadCurrentUserDirectRoleGrant(OracleDataReader odr)
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
        public static RoleGrant loadCurrentUserAccessibleRoleGrant(OracleDataReader odr)
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
        public static RoleGrant loadDbaRoleGrant(OracleDataReader odr)
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

        private void OnRoleGrantsOfAllRolesRefreshed()
        {
            if (RoleGrantsOfAllRolesRefreshed != null)
            {
                RoleGrantsOfAllRolesRefreshed();
            }
        }
        private void OnRoleGrantsRefreshed(ReadOnlyCollection<UserRole> affected)
        {
            if (RoleGrantsRefreshed != null)
            {
                RoleGrantsRefreshed(affected);
            }
        }
        #endregion

        #region Methods for distribution of changes

        /// <summary>
        /// updateChanges method takes the affected roles (roles, whose dependencies
        /// changed = were reloaded from a database), notifies them about
        /// changes, which makes them download the changes, finds the independent ones
        /// and initiates a BFS from them down the USERROLE tree to distribute
        /// changes
        /// </summary>
        void updateChanges()
        {
            // notify roles
            OnRoleGrantsOfAllRolesRefreshed();
            // find the independent roles
            ReadOnlyCollection<Role> independentRoles = findIndependentRoles();
            // start the BFS and distribution of grants
            distributeChanges(independentRoles);
        }
        void updateChanges(ReadOnlyCollection<UserRole> affected)
        {
            // notify roles
            OnRoleGrantsRefreshed(affected);
            // find the independent roles
            ReadOnlyCollection<Role> independentRoles = findIndependentRoles(affected);
            // start the change distribution
            distributeChanges(independentRoles);
        }

        /// <summary>
        /// Finds the independent roles to start BFS and distribution of role and sys grants
        /// in the USERROLE tree
        /// </summary>
        /// <returns>ReadOnly collection of independent roles</returns>
        ReadOnlyCollection<Role> findIndependentRoles()
        {
            return
                (from role in roles
                 where role.IsIndependent
                 select role).ToList<Role>().AsReadOnly();
        }

        /// <summary>
        /// Finds the independent roles to start BFS and distribution of role and sys grants
        /// in the USERROLE tree using the collection of the affected roles to start the search with
        /// </summary>
        /// <param name="affected">ReadOnly collection of affected roles</param>
        /// <returns>ReadOnly collection of independent roles</returns>
        ReadOnlyCollection<Role> findIndependentRoles(ReadOnlyCollection<UserRole> affected)
        {
            return
                (from userRole in affected
                 where userRole.IsIndependent
                 select (userRole as Role)).ToList<Role>().AsReadOnly();
        }

        /// <summary>
        /// Distributes changes using BFS down the USERROLE truee
        /// </summary>
        /// <param name="initialQueue">Changed roles to start with</param>
        void distributeChanges(ReadOnlyCollection<Role> initialQueue)
        {
            Queue<UserRole> queue = new Queue<UserRole>(initialQueue as IEnumerable<UserRole>);
            // start BFS
            while (queue.Count > 0)
            {
                UserRole userRole = queue.Dequeue();
                // refresh changes in a USERROLE
                if (!userRole.IsIndependent)
                    userRole.RefreshChanges();
                // get the role's local role manager
                RoleManagerLocal roleManager = userRole.RoleManager;
                // if a USERROLE has any dependants...
                if (roleManager.HasDependants)
                {
                    //... get the list of them
                    ReadOnlyCollection<UserRole> dependants = roleManager.Dependants;
                    // insert them into a queue
                    foreach (UserRole ur in dependants)
                    {
                        queue.Enqueue(ur);
                    }
                }
            }
        }
        #endregion










        #region Role class

        public class Role : UserRole
        {
            #region Members
            bool passwordRequired;
            #endregion

            #region Constructor

            public Role(string name, bool passwordRequired, SessionManager.Session session) :
                base(name, session)
            {
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
                // create local priv manager
                privManager = session.SysPrivManager.CreateRolePrivLocalManager(this);
                // create local role manager
                roleManager = userRoleManager.CreateRoleLocalManager(this);
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

        #region Local RoleManager class
        public abstract class RoleManagerLocal
        {
            #region Members
            protected int directGrantCount = 0;
            protected SessionManager.Session session;
            protected SessionRoleManager manager;
            protected OracleConnection conn;
            protected UserRole userRole;

            protected ObservableCollection<RoleGrant> roleGrants =
                new ObservableCollection<RoleGrant>();
            protected List<RoleGrant> directRoleGrants =
                new List<RoleGrant>();
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
            }
            #endregion

            #region Helper methods
            private void unregisterAsDependant(IEnumerable<RoleGrant> ancestors)
            {
                foreach (RoleGrant roleGrant in ancestors)
                {
                    (roleGrant.Role.RoleManager as RoleRoleManagerLocal).RemoveDependant(userRole);
                }
            }
            private void registerAsDependant(IEnumerable<RoleGrant> grants)
            {
                foreach (RoleGrant grant in grants)
                {
                    Role ancestor;
                    if (manager.getRoleByName(grant.GrantedRole, out ancestor))
                    {
                        // increase direct grant count if needed
                        if (grant.DirectGrant)
                            ++directGrantCount;
                        // set reference to a role
                        grant.Role = ancestor;
                        // register within its local role manager
                        RoleRoleManagerLocal roleManager = ancestor.RoleManager as RoleRoleManagerLocal;
                        roleManager.AddDependant(userRole);
                    }
                }
            }
            /// <summary>
            /// Downloads direct role grant info from session-level role manager
            /// </summary>
            public virtual void DownloadData()
            {
                // download them from manager
                IEnumerable<RoleGrant> newlyLoadedGrants = downloadRoleGrantsFromManager();
                // get non existent role grants
                IEnumerable<RoleGrant> deletedGrants =
                    from grant in roleGrants
                    where !newlyLoadedGrants.Contains(grant)
                    select grant;
                // select new grants
                IEnumerable<RoleGrant> newGrants =
                    from grant in newlyLoadedGrants
                    where !roleGrants.Contains(grant)
                    select grant;

                // unregister deleted ones
                unregisterAsDependant(deletedGrants);
                // register new ones
                registerAsDependant(newGrants);
                // replace old grants
                roleGrants.Clear();
                roleGrants.AddRange(newlyLoadedGrants);
                // add direct role grants == all newly downloaded role grants
                directRoleGrants.AddRange(newlyLoadedGrants);
            }
            /// <summary>
            /// Downloads role grant info from all ancestor roles
            /// </summary>
            public void RefreshGrants()
            {
                // walk direct role grants and download role grants
                foreach (RoleGrant grant in directRoleGrants)
                {
                    // get associated role
                    Role ancestor = grant.Role;
                    // download its role grants
                    ReadOnlyCollection<RoleGrant> grants =
                        ancestor.RoleManager.RoleGrants;
                    // add them
                    addInheritedGrants(grants);
                }
            }
            private void addInheritedGrants(ReadOnlyCollection<RoleGrant> grants)
            {
                foreach (RoleGrant grant in grants)
                {
                    // create an inherited grant
                    RoleGrant iGrant =
                        new RoleGrant(grant, userRole.Name, false);
                    // add it
                    roleGrants.Add(iGrant);
                }
            }
            protected virtual IEnumerable<RoleGrant> downloadRoleGrantsFromManager()
            {
                return this.manager.downloadRoleGrants(userRole);
            }
            #endregion

            #region Properties
            public bool IsIndependent
            {
                get
                {
                    return directGrantCount == 0;
                }
            }
            public bool HasDependants
            {
                get { return dependants.Count > 0;}
            }
            public ReadOnlyCollection<UserRole> Dependants
            {
                get { return dependants.Values.ToList<UserRole>().AsReadOnly(); }
            }
            public ReadOnlyCollection<RoleGrant> RoleGrants
            {
                get { return roleGrants.ToList<RoleGrant>().AsReadOnly(); }
            }
            public ReadOnlyCollection<RoleGrant> DirectRoleGrants
            {
                get { return directRoleGrants.AsReadOnly(); }
            }
            #endregion
        }
        #endregion
        #region Role RoleManager class
        public class RoleRoleManagerLocal : RoleManagerLocal
        {
            #region Members

            #endregion

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
            public void RemoveDependant(UserRole userRole)
            {
                dependants.Remove(userRole.Name);

            }
            #endregion
        }
        #endregion
        #region User local role manager
        public class UserRoleManagerLocal : RoleManagerLocal
        {
            #region Members
            Role defaultRole;
            #endregion
            #region Constructor
            public UserRoleManagerLocal(SessionManager.Session session, UserManager.User user) :
                base(session, user)
            { }
            #endregion
            #region Helper methods
            public override void DownloadData()
            {
                // download data
                base.DownloadData();
                // find default role
                IEnumerable<Role> defaultRoleQuery =
                     from grant in roleGrants
                     where grant.DefaultRole
                     select grant.Role;

                if (defaultRoleQuery.Count() > 1)
                    throw new Exception("Too many default roles");
                // assign default role
                if (defaultRoleQuery.Count() != 0)
                    defaultRole = defaultRoleQuery.First();
            }
            #endregion
        }
        #endregion
        #region Current user local role manager
        public class CurrentUserRoleManagerLocal : UserRoleManagerLocal
        {
            #region Members
            #region SQL SELECTS
            const string ENABLED_ROLES_SELECT = @"
                SELECT
                    role
                FROM
                    SESSION_ROLES";
            #endregion
            #endregion
            #region Constructor
            public CurrentUserRoleManagerLocal(SessionManager.Session session,
                                               UserManager.CurrentUser currentUser) :
                base(session, currentUser)
            { }
            #endregion

            #region Helper methods
            public override void DownloadData()
            {
                // download role grants
                base.DownloadData();
                // determine enabled roles
                loadEnabledRoles();
            }
            protected override IEnumerable<RoleGrant> downloadRoleGrantsFromManager()
            {
                return manager.downloadCurrentUserGrants();
            }
            private void loadEnabledRoles()
            {
                OracleCommand cmd = new OracleCommand(ENABLED_ROLES_SELECT, conn);
                OracleDataReader odr = cmd.ExecuteReader();
                StringCollection enabledRoles = new StringCollection();

                while (odr.Read())
                {
                    string enabledRoleName = odr.GetString(odr.GetOrdinal("role"));
                    enabledRoles.Add(enabledRoleName);
                }

                resetActiveRoles();
                enableRoles(enabledRoles);
            }
            private void resetActiveRoles()
            {
                IEnumerable<RoleGrant> activeRoles =
                    from grant in roleGrants
                    where grant.Active
                    select grant;

                foreach (RoleGrant grant in activeRoles)
                {
                    grant.Active = false;
                }
            }
            private void enableRoles(StringCollection enabledRoles)
            {
                IEnumerable<RoleGrant> enabledRolesToBe =
                    from grant in roleGrants
                    where enabledRoles.Contains(grant.GrantedRole)
                    select grant;

                foreach (RoleGrant grant in enabledRolesToBe)
                {
                    grant.Active = true;
                }
            }
            #endregion
        }
        #endregion

    }



    




    public delegate void RoleGrantsOfAllRolesRefreshedHandler();
    public delegate void RoleGrantsRefreshedHandler(ReadOnlyCollection<UserRole> affected);
}
