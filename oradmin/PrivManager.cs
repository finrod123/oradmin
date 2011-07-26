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
        const string CURRENT_USER_PRIVS_SELECT =
            @"SELECT
                grantee, privilege, admin_option
              FROM
                USER_SYS_PRIVS";

        const string USERS_PRIVS_SELECT = @"
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
        List<GrantedPrivilege> currentUserGrants = new List<GrantedPrivilege>();
        List<GrantedPrivilege> grants = new List<GrantedPrivilege>();
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
        #region Events
        public event PrivilegeGrantsRefreshedHandler PrivilegeGrantsRefreshed;
        #endregion

        #region Properties
        
        #endregion

        #region Public interface

        /// <summary>
        /// Refreshes information about all privilege grants
        /// </summary>
        public void RefreshCurrentUserData()
        {
            currentUserGrants.Clear();

            OracleCommand cmd = new OracleCommand(CURRENT_USER_PRIVS_SELECT, conn);
            OracleDataReader odr = cmd.ExecuteReader();

            while (odr.Read())
            {
                GrantedPrivilege grant = loadPrivilege(odr);
                // insert him into list
                currentUserGrants.Add(grant);
            }
        }
        public void RefreshUsersData()
        {
            grants.Clear();

            OracleCommand cmd = new OracleCommand(USERS_PRIVS_SELECT, conn);
            OracleDataReader odr = cmd.ExecuteReader();

            while (odr.Read())
            {
                GrantedPrivilege grant = loadPrivilege(odr);
                grants.Add(grant);
            }

            // notify users and roles
            OnPrivilegeGrantsRefreshed();
        }

        #endregion

        #region Protected interface

        IEnumerable<GrantedPrivilege> downloadPrivileges(UserManager.User user)
        {
            return
                 from grant in grants
                    where grant.Grantee == user.Name
                    select grant;
        }
        IEnumerable<GrantedPrivilege> downloadCurrentUserPrivileges()
        {
            return
                 from grant in currentUserGrants
                 where grant.Grantee == session.CurrentUser.Name
                 select grant;
        }
        #endregion

        #region Helper methods
        GrantedPrivilege loadPrivilege(OracleDataReader odr)
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
            return new GrantedPrivilege(
                odr.GetString(odr.GetOrdinal("grantee")),
                (EPrivilege)privConverter.ConvertBack(odr.GetString(odr.GetOrdinal("privilege")), typeof(EPrivilege), null, System.Globalization.CultureInfo.CurrentCulture),
                adminOption,
                true);
        }
        void OnPrivilegeGrantsRefreshed()
        {
            if (PrivilegeGrantsRefreshed != null)
            {
                PrivilegeGrantsRefreshed();
            }
        }
        #endregion

        #region PrivManager inner class

        public class PrivManagerLocal
        {
            #region Members
            SessionManager.Session session;
            PrivManager manager;
            UserManager.User user;
            // list of privileges
            ObservableCollection<GrantedPrivilege> privileges = new ObservableCollection<GrantedPrivilege>();
            // default view of privileges
            ListCollectionView defaultView;
            #endregion

            #region Constructor
            public PrivManagerLocal(SessionManager.Session session, UserManager.User user)
            {
                if (session == null || user == null)
                    throw new ArgumentNullException("Session or User");

                this.manager = session.PrivManager;
                this.user = user;
                // download data from PrivManager
                if (user == session.CurrentUser)
                    // download data
                    privileges.AddRange(this.manager.downloadCurrentUserPrivileges());
                else
                {
                    // register for notifications about new grants
                    this.manager.PrivilegeGrantsRefreshed += new PrivilegeGrantsRefreshedHandler(manager_PrivilegeGrantsRefreshed);
                    // download data
                    privileges.AddRange(manager.downloadPrivileges(user));
                }
            }

            void manager_PrivilegeGrantsRefreshed()
            {
                throw new NotImplementedException();
            }
            #endregion

            #region Properties

            ListCollectionView DefaultView
            {
                get { return defaultView; }
            }

            #endregion
        }

        #endregion
    }

    public class GrantedPrivilege
    {
        #region Members

        string grantee;
        EPrivilege privilege;
        bool admin;

        string grantor;
        UserManager.User grantorRef;
        bool directGrant;

        #endregion

        #region Constructor

        public GrantedPrivilege(string grantee, EPrivilege privilege,
                                bool admin, bool directGrant):
            this(grantee, privilege, admin, directGrant, string.Empty, null)
        { }

        public GrantedPrivilege(string grantee, EPrivilege privilege,
                                string grantor, UserManager.User grantorRef):
            this(grantee, privilege, false, false, grantor, grantorRef)
        { }

        private GrantedPrivilege(string grantee, EPrivilege privilege,
                                bool admin, bool directGrant,
                                string grantor, UserManager.User grantorRef)
        {
            this.grantee = grantee;
            this.privilege = privilege;
            this.admin = admin;
            this.directGrant = directGrant;
            this.grantor = grantor;
            this.grantorRef = grantorRef;
        }
        #endregion

        #region Properties

        public string Grantee
        {
            get { return grantee; }
        }
        public string Grantor
        {
            get { return grantor; }
        }
        public EPrivilege Privilege
        {
            get { return privilege; }
        }
        public bool Admin
        {
            get { return admin; }
        }
        public bool DirectGrant
        {
            get { return directGrant; }
        }

        #endregion
    }

    public enum EPrivilege
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
