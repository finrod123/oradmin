using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows;

//---TODO: navrhni znovu zakladni tridu pro user a role a jejich lokalni manazery
namespace oradmin
{
    class SessionManager
    {
        #region Members

        SequenceGenerator sessIdGenerator = new SequenceGenerator(0, int.MaxValue, 1, false);
        Dictionary<int, Session> id2Sessions = new Dictionary<int, Session>();
        ObservableCollection<Session> sessions = new ObservableCollection<Session>();
        ListCollectionView sessionsDefaultView;

        #endregion

        #region Constructor

        public SessionManager()
        {
            sessionsDefaultView = CollectionViewSource.GetDefaultView(sessions) as ListCollectionView;
        }

        #endregion

        #region Public interface

        public void AddSession(OracleConnection conn)
        {
            try
            {
                // create session
                Session session = new Session(conn);

                // generate session id
                int sessionId = sessIdGenerator.Next;

                // add session to internal structures
                sessions.Add(session);
                id2Sessions.Add(sessionId, session);
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("Exception caught:\n{0}", e.Message));
            }
        }

        #endregion

        #region Properties

        public ListCollectionView DefaultSessionsView
        {
            get { return sessionsDefaultView; }
        }

        #endregion

        public class Session
        {
            #region Members
            OracleConnection conn;

            // session user manager
            UserManager userManager;
            // session role manager (role objects and role to role grants)
            SessionRoleManager roleManager;
            // session system privileges manager (grants to users and roles)
            SessionSysPrivManager sysPrivManager;
            // session table privileges manager (table grants for users and role)
            SessionTabPrivManager tabPrivManager;
            // session column privileges manager (column grants fro users and roles)
            SessionColPrivManager colPrivManager;
            // session schema manager (contains session object managers, e.g. table manager)
            SessionSchemaManager schemaManager;
            // session dependencies manager (dependencies between database objects)
            SessionDependencyManager dependencyManager;
            // session tablespace quotas manager
            SessionQuotasManager quotasManager;
            // session resource limits manager (kernel and password resource limits for users)
            SessionResourceLimitManager resourceLimitsManager;
            // session profile manager (groups resource limits into profiles)
            SessionProfileManager profileManager;

            // current user pointer
            UserManager.CurrentUser currentUser;
            #endregion

            #region Constructor

            public Session(OracleConnection conn)
            {
                if(conn == null)
                    throw new ArgumentNullException("Connection");

                this.conn = conn;

                try
                {
                    // privilege holder entities
                    roleManager = new SessionRoleManager(this);
                    userManager = new UserManager(this);
                    // privilege managers
                    sysPrivManager = new SessionSysPrivManager(this);
                    tabPrivManager = new SessionTabPrivManager(this);
                    colPrivManager = new SessionColPrivManager(this);
                    // dependency manager
                    dependencyManager = new SessionDependencyManager(this);
                    // quotas manager
                    quotasManager = new SessionQuotasManager(this);
                    // resource limits manager
                    resourceLimitsManager = new SessionResourceLimitManager(this);
                    // profile manager
                    profileManager = new SessionProfileManager(this);
                    // schema manager
                    schemaManager = new SessionSchemaManager(this);
                    // store current user reference
                    currentUser = null;
                }
                catch (Exception e)
                {
                    MessageBox.Show(string.Format("Exception caught:\n{0}", e.Message));
                }
            }

            #endregion

            #region Properties
            public OracleConnection Connection
            {
                get { return conn; }
            }
            
            public UserManager UserManager
            {
                get { return userManager; }
            }
            public SessionRoleManager RoleManager
            {
                get { return roleManager; }
            }
            public UserManager.User CurrentUser
            {
                get { return currentUser; }
            }

            public SessionSysPrivManager SysPrivManager
            {
                get { return sysPrivManager; }
            }
            public SessionTabPrivManager TabPrivManager
            {
                get { return this.tabPrivManager; }
            }
            public SessionColPrivManager ColPrivManager
            {
                get { return this.colPrivManager; }
            }

            public SessionDependencyManager DependencyManager
            {
                get { return this.dependencyManager; }
            }
            public SessionQuotasManager QuotasManager
            {
                get { return this.quotasManager; }
            }
            public SessionResourceLimitManager ResourceLimitsManager
            {
                get { return this.resourceLimitsManager; }
            }
            public SessionProfileManager ProfileManager
            {
                get { return this.profileManager; }
            }
            public SessionSchemaManager SchemaManager
            {
                get { return schemaManager; }
            }
            #endregion
        }
    }
}
