using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Types;
using Oracle.DataAccess.Client;
using System.Windows.Data;
using System.Collections.ObjectModel;

//---TODO: load sys privs of the current user
//         load privs of one user or role or of a collection of users mixed with roles
namespace oradmin
{
    #region Typedefs
    using GrantedSysPrivilegeKey = Tuple<string, ESysPrivilege>;
    #endregion


    public delegate void AllUsersSysPrivilegesRefreshedHandler();
    public delegate void AllRolesSysPrivilegesRefreshedHandler();
    public delegate void UsersSysPrivilegesRefreshedHandler(ReadOnlyCollection<UserManager.User> affected);
    public delegate void RolesSysPrivilegesRefreshedHandler(ReadOnlyCollection<SessionRoleManager.Role> affected);
    public delegate void UsersRolesSysPrivilegesRefreshedHandler(ReadOnlyCollection<PrivilegeHolderEntity> affected);
    public delegate void UsersRolesSysPrivilegesRefreshedHandler(ReadOnlyCollection<PrivilegeHolderEntity> affected);

    public delegate void PrivilegeGrantedHandler(SessionRoleManager.Role sender, ESysPrivilege privilege);
    public delegate void PrivilegesGrantedHandler(SessionRoleManager.Role sender, ReadOnlyCollection<ESysPrivilege> privileges);

    public class SessionSysPrivManager
    {
        #region SQL SELECTS
        public static const string DBA_SYS_PRIVS_SELECT = @"
            SELECT
                grantee, privilege, admin_option
            FROM
                DBA_SYS_PRIVS";
        public static const string DBA_SYS_PRIVS_USERS_SELECT = @"
            SELECT
                grantee, privilege, admin_option
            FROM
                DBA_SYS_PRIVS dsp
                    INNER JOIN
                DBA_USERS du
                    ON(dsp.grantee = du.username)";
        public static const string DBA_SYS_PRIVS_ROLES_SELECT = @"
            SELECT
                grantee, privilege, admin_option
            FROM
                DBA_SYS_PRIVS dsp
                    INNER JOIN
                DBA_ROLES dr
                    ON(dsp.grantee = dr.role)";
        public static const string DBA_SYS_PRIVS_USERROLE_SELECT = @"
            SELECT
                grantee, privilege, admin_option
            FROM
                DBA_SYS_PRIVS
            WHERE
                grantee = :grantee";
        public static const string ROLE_SYS_PRIVS_SELECT = @"
            SELECT
                role, privilege, admin_option
            FROM
                ROLE_SYS_PRIVS";
        public static const string ROLE_SYS_PRIVS_ROLE_SELECT = @"
            SELECT
                role, privilege, admin_option
            FROM
                ROLE_SYS_PRIVS
            WHERE
                role = :role";
        #endregion
        #region Members
        // session
        SessionManager.Session session;
        // connection handle
        OracleConnection conn;
        // list of privilege grants
        ObservableCollection<GrantedSysPrivilege> grants =
            new ObservableCollection<GrantedSysPrivilege>();
        #endregion

        #region Constructor
        public SessionSysPrivManager(SessionManager.Session session)
        {
            if (session == null)
                throw new ArgumentNullException("Session");

            this.session = session;
            this.conn = session.Connection;
        }
        #endregion

        #region Public interface
        /// <summary>
        /// Refreshes information about all privilege grants
        /// </summary>
        public void Refresh()
        {
            OracleCommand cmd = new OracleCommand(DBA_SYS_PRIVS_USERS_SELECT, conn);
            OracleDataReader odr = cmd.ExecuteReader();

            if (!odr.HasRows)
                return;

            grants.Clear();

            while (odr.Read())
            {
                GrantedSysPrivilege grant = LoadPrivilege(odr);
                grants.Add(grant);
            }

            // notify them about changes
            OnAllUsersSysPrivilegesRefreshed();
        }
        public void RefreshUsersData(ReadOnlyCollection<UserManager.User> users)
        {
            StringCollection userNames = (from user in users select user.Name) as StringCollection;

            if (refreshUsersRolesData(userNames))
                // notify about changes
                OnUsersSysPrivilegesRefreshed(users);
        }
        public bool RefreshUserData(UserManager.User user)
        {
            return refreshUserRoleData(user);
        }
        public void RefreshRolesData()
        {
            OracleCommand cmd = new OracleCommand(DBA_SYS_PRIVS_ROLES_SELECT, conn);
            OracleDataReader odr = cmd.ExecuteReader();
            bool hasRows = true;

            if (!odr.HasRows)
            {
                cmd.CommandText = ROLE_SYS_PRIVS_SELECT;
                odr = cmd.ExecuteReader();
                hasRows = odr.HasRows;
            }

            if (hasRows)
            {
                grants.Clear();

                while (odr.Read())
                {
                    GrantedSysPrivilege grant = LoadPrivilege(odr);
                    grants.Add(grant);
                }

                // notify roles
                OnAllRolesSysPrivilegesRefreshed();
            }
        }
        public void RefreshRolesData(ReadOnlyCollection<SessionRoleManager.Role> roles)
        {
            StringCollection roleNames = (from userRole in roles select userRole.Name) as StringCollection;

            if (refreshUsersRolesData(roleNames))
                OnRolesSysPrivilegesRefreshed(roles);
            else
            {
                // try to load it from ROLE_SYS_PRIVS
                OracleCommand cmd = new OracleCommand(
                    string.Format("{0}\r\n{1}",
                                  ROLE_SYS_PRIVS_SELECT,
                                  createSysPrivsWhereClause(roleNames, "role")),
                                  conn);

                OracleDataReader odr = cmd.ExecuteReader();

                if (!odr.HasRows)
                    return;

                // purge old data
                purgeOldUserRoleSysPrivs(roleNames);

                while (odr.Read())
                {
                    GrantedSysPrivilege grant = LoadPrivilege(odr);
                    grants.Add(grant);
                }

                // notify
                OnRolesSysPrivilegesRefreshed(roles);
            }
        }
        public bool RefreshRoleData(SessionRoleManager.Role role)
        {
            if (refreshUserRoleData(role))
                return true;
            else
            {
                OracleCommand cmd = new OracleCommand(ROLE_SYS_PRIVS_ROLE_SELECT, conn);
                // set up parameters
                OracleParameter roleParam = cmd.CreateParameter();
                roleParam.ParameterName = "role";
                roleParam.OracleDbType = OracleDbType.Char;
                roleParam.Direction = System.Data.ParameterDirection.Input;
                roleParam.Value = role.Name;
                // execute
                OracleDataReader odr = cmd.ExecuteReader();

                if (!odr.HasRows)
                    return false;

                // purge old data
                purgeOldUserRoleSysPrivs(role.Name);

                while (odr.Read())
                {
                    GrantedSysPrivilege grant = LoadPrivilege(odr);
                    grants.Add(grant);
                }

                return true;
            }
        }
        public void RefreshUsersRolesData(ReadOnlyCollection<PrivilegeHolderEntity> usersRoles)
        {
            StringCollection userRoleNames = (from userRole in usersRoles select userRole.Name) as StringCollection;
            if (refreshUsersRolesData(userRoleNames))
                OnUsersRolesSysPrivilegesRefreshed(usersRoles);
            else
            {
                RefreshRolesData(
                    new ReadOnlyCollection<SessionRoleManager.Role>
                        ((from userRole in usersRoles
                          where userRole is SessionRoleManager.Role
                          select userRole as SessionRoleManager.Role).ToList<SessionRoleManager.Role>())); 
            }
        }

        public UserPrivManagerLocal CreateUserPrivLocalManager(UserManager.User user)
        {
            UserPrivManagerLocal privManager =new UserPrivManagerLocal(session, user);
            bindToLocalManagerEvents(privManager);

            return privManager;
        }
        public RolePrivManagerLocal CreateRolePrivLocalManager(SessionRoleManager.Role role)
        {
            return new RolePrivManagerLocal(session, role);
        }
        public CurrentUserPrivManagerLocal CreateCurrentUserPrivManagerLocal(UserManager.CurrentUser user)
        {
            return new CurrentUserPrivManagerLocal(session, user);
        }
        #endregion

        #region Helper methods
        private string createSysPrivsWhereClause(StringCollection userRoleNames, string granteeAlias)
        {
            
            string[] userClauses =
                    (from userName in userRoleNames as IEnumerable<string>
                     select string.Format("{0} = {1}", granteeAlias, userName)) as string[];

            return string.Format("WHERE {1}",
                                 string.Join(" or \r\n", userClauses));
        }
        private void purgeOldUserRoleSysPrivs(StringCollection userRoleNames)
        {
            grants.RemoveAll((grant) => (userRoleNames.Contains(grant.Grantee)));
        }
        private void purgeOldUserRoleSysPrivs(string userRoleName)
        {
            grants.RemoveAll((grant) => (userRoleName.Equals(grant.Grantee)));
        }
        private void OnAllUsersSysPrivilegesRefreshed()
        {
            if (AllUsersSysPrivilegesRefreshed != null)
            {
                AllUsersSysPrivilegesRefreshed();
            }
        }
        private void OnAllRolesSysPrivilegesRefreshed()
        {
            if (AllRolesSysPrivilegesRefreshed != null)
            {
                AllRolesSysPrivilegesRefreshed();
            }
        }
        private void OnUsersSysPrivilegesRefreshed(ReadOnlyCollection<UserManager.User> affected)
        {
            if (UsersSysPrivilegesRefreshed != null)
            {
                UsersSysPrivilegesRefreshed(affected);
            }
        }
        private void OnRolesSysPrivilegesRefreshed(ReadOnlyCollection<SessionRoleManager.Role> affected)
        {
            if (RolesSysPrivilegesRefreshed != null)
            {
                RolesSysPrivilegesRefreshed(affected);
            }
        }
        private bool refreshUsersRolesData(StringCollection userRoleNames)
        {
            OracleCommand cmd = new OracleCommand(
                string.Format("{0}\r\n{1}",
                              DBA_SYS_PRIVS_SELECT, 
                              createSysPrivsWhereClause(userRoleNames, "grantee")),
                              conn);

            OracleDataReader odr = cmd.ExecuteReader();

            if (!odr.HasRows)
                return false;

            // purge old data
            purgeOldUserRoleSysPrivs(userRoleNames);

            while (odr.Read())
            {
                GrantedSysPrivilege grant = LoadPrivilege(odr);
                grants.Add(grant);
            }

            return true;
        }
        private void bindToLocalManagerEvents(PrivManagerLocal privManager)
        {
            privManager.PrivilegeGranted += new PrivilegeGrantedHandler(privManager_PrivilegeGranted);
            privManager.PrivilegesGranted += new PrivilegesGrantedHandler(privManager_PrivilegesGranted);
        }
        private bool refreshUserRoleData(PrivilegeHolderEntity userRole)
        {
            OracleCommand cmd = new OracleCommand(DBA_SYS_PRIVS_USERROLE_SELECT, conn);
            // set up parameters
            OracleParameter granteeParam = cmd.CreateParameter();
            granteeParam.ParameterName = "grantee";
            granteeParam.OracleDbType = OracleDbType.Char;
            granteeParam.Direction = System.Data.ParameterDirection.Input;
            granteeParam.Value = userRole.Name;
            // execute
            OracleDataReader odr = cmd.ExecuteReader();

            if (!odr.HasRows)
                return false;

            // purge user's data
            purgeOldUserRoleSysPrivs(userRole.Name);

            while (odr.Read())
            {
                GrantedSysPrivilege grant = LoadPrivilege(odr);
                grants.Add(grant);
            }

            return false;
        }
        private void OnUsersRolesSysPrivilegesRefreshed(ReadOnlyCollection<PrivilegeHolderEntity> affected)
        {
            if (UsersRolesSysPrivilegesRefreshed != null)
            {
                UsersRolesSysPrivilegesRefreshed(affected);
            }
        }
        private void privManager_PrivilegeGranted(SessionRoleManager.Role sender, ESysPrivilege privilege)
        {
            // create a queue
            var queue = CreateDistributionQueue(
                new {
                    Source = default(SessionRoleManager.Role),
                    Destination = default(PrivilegeHolderEntity),
                    Privilege = default(ESysPrivilege)
                });

            // fill a queue
            foreach (PrivilegeHolderEntity userRole in sender.RoleManager.Dependants)
            {
                queue.Enqueue(new { Source = sender, Destination = userRole, Privilege = privilege });
            }
            
            // start BFS
            while (queue.Count > 0)
            {
                // get a distribution vector
                var vector = queue.Dequeue();
                // get a user or role, which is going to download the privilege info
                PrivilegeHolderEntity destination = vector.Destination;
                
                if (destination.PrivManager.DownloadPrivilegeChange(privilege, vector.Source))
                {
                    // enqueue dependants
                    foreach (PrivilegeHolderEntity dependant in destination.RoleManager.Dependants)
                    {
                        queue.Enqueue(new { Source = destination as SessionRoleManager.Role, Destination = dependant, Privilege = privilege });
                    }
                }
            }
        }
        private void privManager_PrivilegesGranted(SessionRoleManager.Role sender,
                                                   ReadOnlyCollection<ESysPrivilege> privileges)
        {
            // create a queue
            var queue = CreateDistributionQueue(
                new {
                    Source = default(SessionRoleManager.Role),
                    Destination = default(PrivilegeHolderEntity),
                    Privileges = default(ReadOnlyCollection<ESysPrivilege>)
                });

            // fill a queue
            foreach (PrivilegeHolderEntity userRole in sender.RoleManager.Dependants)
            {
                queue.Enqueue(new { Source = sender, Destination = userRole, Privileges = privileges });
            }

            // start BFS
            while (queue.Count > 0)
            {
                // get a distribution vector
                var vector = queue.Dequeue();
                // get a user or role, which is going to download the privilege info
                PrivilegeHolderEntity destination = vector.Destination;

                if (destination.PrivManager.DownloadPrivilegeChange(privilege, vector.Source))
                {
                    // enqueue dependants
                    foreach (PrivilegeHolderEntity dependant in destination.RoleManager.Dependants)
                    {
                        queue.Enqueue(new { Source = destination as SessionRoleManager.Role, Destination = dependant, Privilege = privilege });
                    }
                }
            }
        }

        #endregion

        #region Protected interface
        protected IEnumerable<GrantedSysPrivilege> downloadUserRolePrivileges(PrivilegeHolderEntity userRole)
        {
            return
                 from grant in grants
                    where grant.Grantee == userRole.Name
                    select grant;
        }
        /// <summary>
        /// ---TODO: return error code and string message (e.g. OracleException message?)
        /// </summary>
        /// <param name="grant"></param>
        /// <returns></returns>
        protected bool GrantSysPrivilege(GrantedSysPrivilege grant, out string errorMsg)
        {
            // check whether the grant can be performed etc.

            // perform a grant
            OracleCommand cmd = new OracleCommand(
                                    prepareGrantStatement(grant),
                                    conn);
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (OracleException e)
            {
                errorMsg = string.Format("Error occured:\r\n{0}", e.Message);
                return false;
            }

            errorMsg = string.Empty;
            return true;
        }
        
        #endregion

        #region Public static interface
        public static string prepareGrantStatement(GrantedSysPrivilege grant)
        {
            ESysPrivilegeEnumConverter converter = new ESysPrivilegeEnumConverter();
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("GRANT {0} TO {1}",
                          converter.Convert(grant.Privilege, typeof(string), null, null),
                          grant.Grantee);

            if (grant.AdminOption)
                sb.Append(" WITH ADMIN OPTION");

            return sb.ToString();
        }
        public static GrantedSysPrivilege LoadPrivilege(OracleDataReader odr)
        {
            bool adminOption;
            ESysPrivilegeEnumConverter privConverter = new ESysPrivilegeEnumConverter();

            // load admin option
            if (odr.IsDBNull(odr.GetOrdinal("admin_option")))
                adminOption = false;
            else
            {
                string boolStr = odr.GetString(odr.GetOrdinal("admin_option"));
                if (boolStr.ToLowerInvariant().Equals("yes"))
                    adminOption = true;
                else if (boolStr.ToLowerInvariant().Equals("no"))
                    adminOption = false;
                else
                    adminOption = false;
            }

            // create new grant
            return new GrantedSysPrivilege(
                odr.GetString(odr.GetOrdinal("grantee")),
                (ESysPrivilege)privConverter.ConvertBack(odr.GetString(odr.GetOrdinal("privilege")), typeof(ESysPrivilege), null, System.Globalization.CultureInfo.CurrentCulture),
                true,
                adminOption);
        }
        private static Queue<T> CreateDistributionQueue<T>(T templateValue)
        {
            return new Queue<T>();
        }
        #endregion

        #region Events
        public event AllUsersSysPrivilegesRefreshedHandler AllUsersSysPrivilegesRefreshed;
        public event AllRolesSysPrivilegesRefreshedHandler AllRolesSysPrivilegesRefreshed;
        public event UsersSysPrivilegesRefreshedHandler UsersSysPrivilegesRefreshed;
        public event RolesSysPrivilegesRefreshedHandler RolesSysPrivilegesRefreshed;
        public event UsersRolesSysPrivilegesRefreshedHandler UsersRolesSysPrivilegesRefreshed;
        #endregion

        /*
        abstract class SysPrivGrantDistributionVectorBase
        {
            #region Members
            protected UserRole downloadToUserRole;
            protected RoleManager.Role downloadFromRole;
            #endregion

            #region Constructor
            public SysPrivGrantDistributionVector(RoleManager.Role from,
                                               UserRole to)
            {
                this.downloadFromRole = from;
                this.downloadToUserRole = to;
            }
            #endregion

            #region Properties
            public UserRole DownloadDestination
            {
                get { return downloadToUserRole; }
            }
            public RoleManager.Role DownloadSource
            {
                get { return downloadFromRole; }
            }
            #endregion
        }

        public class SysPrivGrantDistributionVector : SysPrivGrantDistributionVectorBase
        {
            #region Members
            ESysPrivilege privilege;
	        #endregion
        }
        */

































        #region PrivManager inner class

        /// <summary>
        /// Abstract local privilege manager for both users and roles
        /// </summary>
        public abstract class PrivManagerLocal
        {
            #region Members
            protected SessionManager.Session session;
            protected OracleConnection conn;
            protected SessionSysPrivManager manager;
            protected PrivilegeHolderEntity userRole;
            protected SessionRoleManager.RoleManagerLocal localRoleManager;
            // list of privileges
            protected ObservableCollection<GrantedSysPrivilege> privileges = new ObservableCollection<GrantedSysPrivilege>();
            // default view of privileges
            protected ListCollectionView defaultView;
            #endregion

            #region Constructor
            public PrivManagerLocal(SessionManager.Session session, PrivilegeHolderEntity userRole)
            {
                if (session == null || userRole == null)
                    throw new ArgumentNullException("Session or User");

                this.manager = session.SysPrivManager;
                this.conn = session.Connection;
                this.userRole = userRole;
                this.localRoleManager = userRole.RoleManager;
                // create privilege view
                defaultView = CollectionViewSource.GetDefaultView(privileges) as ListCollectionView;
            }

            #endregion
            
            #region Helper methods
            /// <summary>
            /// Downloads privileges from session priv manager
            /// </summary>
            public virtual void DownloadPrivileges()
            {
                privileges.Clear();
                privileges.AddRange(manager.downloadUserRolePrivileges(userRole));
            }
            // Downloads privileges from directly granted roles
            public void RefreshPrivileges()
            {
                // get direct role grants from associated local role manager
                ReadOnlyCollection<RoleGrant> directRoleGrants =
                    localRoleManager.DirectRoleGrants;
                // walk them and download role grants
                foreach (RoleGrant grant in directRoleGrants)
                {
                    // get role reference
                    SessionRoleManager.Role grantedRole = grant.Role;
                    // get all of its privilege grants
                    ReadOnlyCollection<GrantedSysPrivilege> privs =
                        grantedRole.PrivManager.PrivilegeGrants;
                    // find those which are applicable for you and add them
                    // to your privilege list
                    addInheritedGrants(privs);
                }
            }
            // adds inherited grants (removes duplicities?)
            private ReadOnlyCollection<GrantedSysPrivilege> addInheritedGrants(
                ReadOnlyCollection<GrantedSysPrivilege> grants)
            {
                
            }
            private GrantedSysPrivilege createInheritedGrant(GrantedSysPrivilege grant)
            {
                return new GrantedSysPrivilege(userRole.Name, grant.Privilege, false, false);
            }

            public bool DownloadPrivilegeChange(ESysPrivilege privilege, SessionRoleManager.Role from)
            {
                GrantedSysPrivilege downloaded;
                RolePrivManagerLocal rolePrivManager = from.PrivManager as RolePrivManagerLocal;

                if (rolePrivManager.GetPrivilegeGrantInfo(privilege, out downloaded))
                {
                    if (mergePrivilege(createInheritedGrant(downloaded)))
                        return true;
                    else
                        return false;
                }
                return false;
            }
            public ReadOnlyCollection<GrantedSysPrivilege> DownloadPrivilegesChange(
                ReadOnlyCollection<ESysPrivilege> privs, SessionRoleManager.Role from)
            {
                RolePrivManagerLocal rolePrivManager = from.PrivManager as RolePrivManagerLocal;
                ReadOnlyCollection<GrantedSysPrivilege> grants;

                if (rolePrivManager.GetPrivilegesGrantsInfo(privs, out grants))
                {

                }
                
            }
            public virtual bool GrantSysPrivilege(GrantedSysPrivilege grant, bool adminOption,
                                                  out string errorMsg)
            {
                //---TODO: kontrola, zda jiz neni prideleno???---

                // create a grant for user or role
                GrantedSysPrivilege newGrant = grant.CreateGrant(userRole, adminOption);
                // try to perform it via session-level priv manager
                if (!manager.GrantSysPrivilege(newGrant, out errorMsg))
                {
                    return false;
                }
                
                // grant succeeded, add it to the collection and notify RoleManager
                // about a change to perform change distribution
                if (mergePrivilege(newGrant))
                    OnPrivilegeGranted(grant.Privilege);

                errorMsg = string.Empty;
                return true;
            }
            private bool mergePrivilege(GrantedSysPrivilege newGrant)
            {
                bool privilegeStrengthRaised;

                IEnumerable<GrantedSysPrivilege> existingGrants =
                    from grant in privileges
                    where grant.Privilege == newGrant.Privilege
                    select grant;

                if (existingGrants.Count() > 0)
                {
                    GrantedSysPrivilege existingGrant = existingGrants.First();

                    if (newGrant.IsStrongerThan(existingGrant))
                    {
                        privileges.Remove(existingGrant);
                        privilegeStrengthRaised = true;
                    } else
                        privilegeStrengthRaised = false;
                } else
                    privilegeStrengthRaised = true;

                if (privilegeStrengthRaised)
                    privileges.Add(newGrant);

                return privilegeStrengthRaised;
            }
            private void OnPrivilegeGranted(ESysPrivilege privilege)
            {
                if (PrivilegeGranted != null)
                {
                    PrivilegeGranted(this.userRole as SessionRoleManager.Role, privilege);
                }
            }
            private void OnPrivilegesGranted(IList<ESysPrivilege> privileges)
            {
                if (PrivilegesGranted != null)
                {
                    PrivilegesGranted(this.userRole as SessionRoleManager.Role, new ReadOnlyCollection<ESysPrivilege>(privileges));
                }
            }
            #endregion

            #region Events
            public event PrivilegeGrantedHandler PrivilegeGranted;
            public event PrivilegesGrantedHandler PrivilegesGranted;
            #endregion

            #region Properties
            ListCollectionView Privileges
            {
                get { return defaultView; }
            }
            ReadOnlyCollection<GrantedSysPrivilege> PrivilegeGrants
            {
                get { return privileges.ToList<GrantedSysPrivilege>().AsReadOnly(); }
            }
            #endregion
        }
        /// <summary>
        /// Local privilege manager for users
        /// </summary>
        public class UserPrivManagerLocal : PrivManagerLocal
        {
            #region Constructor

            public UserPrivManagerLocal(SessionManager.Session session, UserManager.User user) :
                base(session, user)
            { }

            #endregion
        }
        /// <summary>
        /// Local privilege manager for roles
        /// </summary>
        public class RolePrivManagerLocal : PrivManagerLocal
        {
            #region Constructor
            public RolePrivManagerLocal(SessionManager.Session session, SessionRoleManager.Role role) :
                base(session, role)
            { }
            #endregion
            
            #region Public interface
            public bool GetPrivilegeGrantInfo(ESysPrivilege privilege, out GrantedSysPrivilege grant)
            {
                IEnumerable<GrantedSysPrivilege> search =
                    from privGrant in privileges
                    where privGrant.Privilege == privilege
                    select privGrant;

                if (search.Count() > 0)
                {
                    grant = search.First();
                    return true;
                } else
                {
                    grant = null;
                    return false;
                }
            }
            public bool GetPrivilegesGrantsInfo(ReadOnlyCollection<ESysPrivilege> privs,
                out ReadOnlyCollection<GrantedSysPrivilege> grants)
            {
                IEnumerable<GrantedSysPrivilege> search =
                    from grant in privileges
                    where privs.Contains(grant.Privilege)
                    select grant;

                if (search.Count() > 0)
                {
                    grants = search.ToList().AsReadOnly();
                    return true;
                } else
                {
                    grants = null;
                    return false;
                }
            }
            #endregion
        }

        public class CurrentUserPrivManagerLocal : UserPrivManagerLocal
        {
            #region SQL SELECTS
            const string CURRENT_USER_PRIVS_SELECT = @"
              SELECT
                grantee, privilege, admin_option
              FROM
                USER_SYS_PRIVS";
            #endregion

            #region Constructor
            public CurrentUserPrivManagerLocal(SessionManager.Session session,
                                               UserManager.CurrentUser user) :
                base(session, user)
            { }
            #endregion

            public override void DownloadPrivileges()
            {
                IEnumerable<GrantedSysPrivilege> newPrivs =
                    manager.downloadUserRolePrivileges(userRole);

                if (newPrivs.Count() > 0)
                {
                    privileges.Clear();
                    privileges.AddRange(newPrivs);
                } else
                {
                    OracleCommand cmd = new OracleCommand(CURRENT_USER_PRIVS_SELECT, conn);
                    OracleDataReader odr = cmd.ExecuteReader();

                    if (odr.HasRows)
                        privileges.Clear();

                    while (odr.Read())
                    {
                        GrantedSysPrivilege grant = SessionSysPrivManager.LoadPrivilege(odr);
                        // add it
                        privileges.Add(grant);
                    }
                }
            }
        }
        #endregion
    }

    public enum ESysPrivilege
    {
        AdministerAnySqlTuningSet,
        AdministerDatabaseTrigger,
        AdministerResourceManager,
        AdministerSqlTuningSet,
        Advisor,
        AlterAnyCluster,
        AlterAnyDimension,
        AlterAnyEvaluationContext,
        AlterAnyIndex,
        AlterAnyIndextype,
        AlterAnyLibrary,
        AlterAnyMaterializedView,
        AlterAnyOperator,
        AlterAnyOutline,
        AlterAnyProcedure,
        AlterAnyRole,
        AlterAnyRule,
        AlterAnyRuleSet,
        AlterAnySequence,
        AlterAnySqlProfile,
        AlterAnyTable,
        AlterAnyTrigger,
        AlterAnyType,
        AlterDatabase,
        AlterProfile,
        AlterResourceCost,
        AlterRollbackSegment,
        AlterSession,
        AlterSystem,
        AlterTablespace,
        AlterUser,
        AnalyzeAny,
        AnalyzeAnyDictionary,
        AuditAny,
        AuditSystem,
        BackupAnyTable,
        BecomeUser,
        ChangeNotification,
        CommentAnyTable,
        CreateAnyCluster,
        CreateAnyContext,
        CreateAnyDimension,
        CreateAnyDirectory,
        CreateAnyEvaluationContext,
        CreateAnyIndex,
        CreateAnyIndextype,
        CreateAnyJob,
        CreateAnyLibrary,
        CreateAnyMaterializedView,
        CreateAnyOperator,
        CreateAnyOutline,
        CreateAnyProcedure,
        CreateAnyRule,
        CreateAnyRuleSet,
        CreateAnySequence,
        CreateAnySqlProfile,
        CreateAnySynonym,
        CreateAnyTable,
        CreateAnyTrigger,
        CreateAnyType,
        CreateAnyView,
        CreateCluster,
        CreateDatabaseLink,
        CreateDimension,
        CreateEvaluationContext,
        CreateExternalJob,
        CreateIndextype,
        CreateJob,
        CreateLibrary,
        CreateMaterializedView,
        CreateOperator,
        CreateProcedure,
        CreateProfile,
        CreatePublicDatabaseLink,
        CreatePublicSynonym,
        CreateRole,
        CreateRollbackSegment,
        CreateRule,
        CreateRuleSet,
        CreateSequence,
        CreateSession,
        CreateSynonym,
        CreateTable,
        CreateTablespace,
        CreateTrigger,
        CreateType,
        CreateUser,
        CreateView,
        DebugAnyProcedure,
        DebugConnectSession,
        DeleteAnyTable,
        DequeueAnyQueue,
        DropAnyCluster,
        DropAnyContext,
        DropAnyDimension,
        DropAnyDirectory,
        DropAnyEvaluationContext,
        DropAnyIndex,
        DropAnyIndextype,
        DropAnyLibrary,
        DropAnyMaterializedView,
        DropAnyOperator,
        DropAnyOutline,
        DropAnyProcedure,
        DropAnyRole,
        DropAnyRule,
        DropAnyRuleSet,
        DropAnySequence,
        DropAnySqlProfile,
        DropAnySynonym,
        DropAnyTable,
        DropAnyTrigger,
        DropAnyType,
        DropAnyView,
        DropProfile,
        DropPublicDatabaseLink,
        DropPublicSynonym,
        DropRollbackSegment,
        DropTablespace,
        DropUser,
        EnqueueAnyQueue,
        ExecuteAnyClass,
        ExecuteAnyEvaluationContext,
        ExecuteAnyIndextype,
        ExecuteAnyLibrary,
        ExecuteAnyOperator,
        ExecuteAnyProcedure,
        ExecuteAnyProgram,
        ExecuteAnyRule,
        ExecuteAnyRuleSet,
        ExecuteAnyType,
        ExemptAccessPolicy,
        ExemptIdentityPolicy,
        ExportFullDatabase,
        FlashbackAnyTable,
        ForceAnyTransaction,
        ForceTransaction,
        GlobalQueryRewrite,
        GrantAnyObjectPrivilege,
        GrantAnyPrivilege,
        GrantAnyRole,
        ImportFullDatabase,
        InsertAnyTable,
        LockAnyTable,
        ManageAnyFileGroup,
        ManageAnyQueue,
        ManageFileGroup,
        ManageScheduler,
        ManageTablespace,
        MergeAnyView,
        OnCommitRefresh,
        QueryRewrite,
        ReadAnyFileGroup,
        RestrictedSession,
        Resumable,
        SelectAnyDictionary,
        SelectAnySequence,
        SelectAnyTable,
        SelectAnyTransaction,
        Sysdba,
        Sysoper,
        UnderAnyTable,
        UnderAnyType,
        UnderAnyView,
        UnlimitedTablespace,
        UpdateAnyTable,
        Unknown
    }
}
