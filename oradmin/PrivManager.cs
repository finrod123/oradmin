using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Types;
using Oracle.DataAccess.Client;
using System.Windows.Data;
using System.Collections.ObjectModel;

namespace oradmin
{
    public delegate void PrivilegeGrantsRefreshedHandler();

    public class PrivManager
    {
        #region Constants
        public static const string USERSROLES_PRIVS_SELECT = @"
            SELECT
                grantee, privilege, admin_option
            FROM
                DBA_SYS_PRIVS";
      
        #endregion

        #region Members
        // session
        SessionManager.Session session;
        // connection handle
        OracleConnection conn;
        // list of privilege grants
        List<GrantedSysPrivilege> grants = new List<GrantedSysPrivilege>();
        #endregion

        #region Constructor
        public PrivManager(SessionManager.Session session)
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
        public void RefreshUsersData()
        {
            refreshUserRoleData();
        }
        private void refreshUserRoleData()
        {
            OracleCommand cmd = new OracleCommand(USERSROLES_PRIVS_SELECT, conn);
            OracleDataReader odr = cmd.ExecuteReader();

            if (odr.HasRows)
                grants.Clear();

            while (odr.Read())
            {
                GrantedSysPrivilege grant = loadPrivilege(odr);
                grants.Add(grant);
            }
        }

        /// <summary>
        /// Queries for privileges of all roles
        /// </summary>
        public void RefreshRolesData()
        {
            refreshUserRoleData();
        }
        public UserPrivManagerLocal CreateUserPrivLocalManager(UserManager.User user)
        {
            return new UserPrivManagerLocal(session, user);
        }
        public RolePrivManagerLocal CreateRolePrivLocalManager(RoleManager.Role role)
        {
            return new RolePrivManagerLocal(session, role);
        }
        public CurrentUserPrivManagerLocal CreateCurrentUserPrivManagerLocal(UserManager.CurrentUser user)
        {
            return new CurrentUserPrivManagerLocal(session, user);
        }
        #endregion

        #region Protected interface
        protected IEnumerable<GrantedSysPrivilege> downloadUserRolePrivileges(UserRole userRole)
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
        protected bool TryGrant(GrantedSysPrivilege grant)
        {

        }
        #endregion

        #region Public static interface
        public static GrantedSysPrivilege loadPrivilege(OracleDataReader odr)
        {
            bool adminOption;
            EPrivilegeEnumConverter privConverter = new EPrivilegeEnumConverter();

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
                odr.GetString(odr.GetOrdinal("grantee")),
                (ESysPrivilege)privConverter.ConvertBack(odr.GetString(odr.GetOrdinal("privilege")), typeof(ESysPrivilege), null, System.Globalization.CultureInfo.CurrentCulture),
                adminOption);
        }
        #endregion










        #region PrivManager inner class

        /// <summary>
        /// Abstract local privilege manager for both users and roles
        /// </summary>
        public abstract class PrivManagerLocal
        {
            #region Members
            protected SessionManager.Session session;
            protected OracleConnection conn;
            protected PrivManager manager;
            protected UserRole userRole;
            protected RoleManager.RoleManagerLocal localRoleManager;
            // list of privileges
            protected ObservableCollection<GrantedSysPrivilege> privileges = new ObservableCollection<GrantedSysPrivilege>();
            // default view of privileges
            protected ListCollectionView defaultView;
            #endregion

            #region Constructor
            public PrivManagerLocal(SessionManager.Session session, UserRole userRole)
            {
                if (session == null || userRole == null)
                    throw new ArgumentNullException("Session or User");

                this.manager = session.PrivManager;
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
                    RoleManager.Role grantedRole = grant.Role;
                    // get all of its privilege grants
                    ReadOnlyCollection<GrantedSysPrivilege> privs =
                        grantedRole.PrivManager.PrivilegeGrants;
                    // find those which are applicable for you and add them
                    // to your privilege list
                    addInheritedGrants(privs);
                }
            }
            // adds inherited grants (removes duplicities?)
            private void addInheritedGrants(ReadOnlyCollection<GrantedSysPrivilege> privs)
            {
                foreach (GrantedSysPrivilege grant in privs)
                {
                    // create own downloaded grant info
                    GrantedSysPrivilege downloaded =
                        new GrantedSysPrivilege(userRole.Name, grant.Privilege, false, false);
                    // insert it
                    privileges.Add(downloaded);
                }
            }
            public bool Grant(GrantedSysPrivilege grant, bool adminOption)
            {
                //---TODO: kontrola, zda jiz neni prideleno???---

                // create a grant for user or role
                GrantedSysPrivilege newGrant = grant.CreateGrant(userRole, adminOption);
                // try to perform it via session-level priv manager
                if (!manager.TryGrant(newGrant))
                {
                    return false;
                }
                
                // grant succeeded, add it to the collection and notify RoleManager
                // about a change to perform change distribution
                privileges.Add(newGrant);

            }
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
            public RolePrivManagerLocal(SessionManager.Session session, RoleManager.Role role) :
                base(session, role)
            { }
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
                        GrantedSysPrivilege grant = PrivManager.loadPrivilege(odr);
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
