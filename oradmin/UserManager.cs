using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace oradmin
{
    public class UserManager
    {
        #region Static members
        public static const string USERS_SELECT = @"
                  SELECT
                    user_id, username, default_tablespace, temporary_tablespace,
                    created, expiry_date
                  FROM
                    DBA_USERS";
        #endregion

        #region Members
        SessionManager.Session session;
        OracleConnection conn;
        CurrentUser currentUser;
        Dictionary<decimal, User> id2Users = new Dictionary<decimal, User>();
        Dictionary<string, User> name2Users = new Dictionary<string, User>();
        ObservableCollection<User> users = new ObservableCollection<User>();
        ListCollectionView defaultUserView;

        #endregion

        #region Constructor

        public UserManager(SessionManager.Session session)
        {
            if (session == null)
                throw new ArgumentNullException("Session");

            this.session = session;
            this.conn = session.Connection;
            // load current user
            if (!loadCurrentUser())
                throw new Exception("Current user not loaded");
            // load other users
            loadUsers();
            // create user view
            defaultUserView = CollectionViewSource.GetDefaultView(users) as ListCollectionView;
        }

        #endregion

        #region Properties

        protected OracleConnection Connection
        {
            get { return conn; }
        }
        public CurrentUser SessionUser
        {
            get { return currentUser; }
        }

        #endregion

        #region Implementation

        bool loadCurrentUser()
        {
            if (currentUser != null)
                throw new Exception("Current user already added");

            // nacti uzivatele
            bool loaded = true;
            OracleCommand cmd = new OracleCommand(CURRENT_USER_SELECT, conn);
            OracleDataReader odr = cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow);

            if (odr.Read())
            {
                currentUser = loadUser(odr, true);
            } else
                loaded = false;

            return loaded;
        }
        /// <summary>
        /// Loads all users from a database
        /// </summary>
        void loadUsers()
        {
            OracleCommand cmd = new OracleCommand(USERS_SELECT, conn);
            OracleDataReader odr = cmd.ExecuteReader();

            while (odr.Read())
            {
                // load user
                User user = loadUser(odr, false);
                // insert him into structures
                id2Users.Add(user.Id, user);
                name2Users.Add(user.Name, user);
                users.Add(user);
            }
        }
        User loadUser(OracleDataReader odr, bool isCurrentUser)
        {
            DateTime? created = null,
                      expiryDate = null;

            if (!odr.IsDBNull(4))
                created = (DateTime)odr.GetDateTime(odr.GetOrdinal("created"));
            if (!odr.IsDBNull(5))
                expiryDate = (DateTime)odr.GetDateTime(odr.GetOrdinal("expiry_date"));

            if (isCurrentUser)
                return new CurrentUser(
                    odr.GetDecimal(odr.GetOrdinal("user_id")),
                    odr.GetString(odr.GetOrdinal("username")),
                    odr.GetString(odr.GetOrdinal("default_tablespace")),
                    odr.GetString(odr.GetOrdinal("temporary_tablespace")),
                    created,
                    expiryDate,
                    session);
            else
                return new User(
                    odr.GetDecimal(odr.GetOrdinal("user_id")),
                    odr.GetString(odr.GetOrdinal("username")),
                    odr.GetString(odr.GetOrdinal("default_tablespace")),
                    odr.GetString(odr.GetOrdinal("temporary_tablespace")),
                    created,
                    expiryDate,
                    session);
        }

        #endregion

        
        #region User class

        public class User : UserRole
        {
            #region Members
            UserManager manager;
            
            readonly decimal id;
            object defaultTablespace, temporaryTablespace;
            DateTime? expiryDate, created;
            #endregion

            #region Constructor

            public User(decimal id, string name,
                        object defaultTablespace, object temporaryTablespace,
                        DateTime? created, DateTime? expiryDate,
                        SessionManager.Session session):
                base(name, session)
            {
                this.id = id;
                this.defaultTablespace = defaultTablespace;
                this.temporaryTablespace = temporaryTablespace;
                this.expiryDate = expiryDate;
                this.created = created;
                this.manager = session.UserManager;
                // create managers
                createManagers();
            }

            #endregion

            #region Properties
            public string Name
            {
                get { return name; }
                set { name = value; }
            }
            public decimal Id
            {
                get { return id; }
            }
            public DateTime? Created
            {
                get { return created; }
                set { created = value; }
            }
            public DateTime? Expires
            {
                get { return expiryDate; }
                set { expiryDate = value; }
            }
            public object DefaultTablespace
            {
                get { return defaultTablespace; }
                set { defaultTablespace = value; }
            }
            public object TemporaryTablespace
            {
                get { return temporaryTablespace; }
                set { temporaryTablespace = value; }
            }
            public override bool IsIndependent
            {
                get { return false; }
            }
            #endregion

            #region Helper methods
            protected virtual void createManagers()
            {
                // create local priv manager
                privManager = session.PrivManager.CreateUserPrivLocalManager(this);
                // create local role manager
                roleManager = userRoleManager.CreateUserLocalManager(this);
            }
            #endregion
        }

        #endregion

        #region CurrentUser class

        public class CurrentUser : User
        {
            #region SQL SELECTS
            public static const string CURRENT_USER_SELECT = @"
                  SELECT
                    user_id, default_tablespace, temporary_tablespace,
                    created, expiry_date
                  FROM
                    USER_USERS";
            #endregion

            #region Constructor
            public CurrentUser(
                        decimal id, string name,
                        object defaultTablespace, object temporaryTablespace,
                        DateTime? created, DateTime? expiryDate,
                        SessionManager.Session session) :
                base(id, name, defaultTablespace, temporaryTablespace, created, expiryDate, session)
            { }
            #endregion

            #region Helper methods
            protected override void createManagers()
            {
                // create priv manager for current user
                privManager = session.PrivManager.CreateCurrentUserPrivManagerLocal(this);
                // create role manager for current user
                roleManager = userRoleManager.CreateCurrentUserLocalManager(this);
            }
            #endregion
        }

        #endregion
    }
}
