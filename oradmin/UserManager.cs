using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
        public static const string DBA_USERS_SELECT = @"
            SELECT
                user_id, username, default_tablespace, temporary_tablespace,
                created, expiry_date
            FROM
                DBA_USERS";
        public static const string ALL_USERS_SELECT = @"
            SELECT
                user_id, username, created
            FROM
                ALL_USERS";
        #endregion

        #region Members
        SessionManager.Session session;
        OracleConnection conn;
        CurrentUser currentUser;
        
        Dictionary<string, User> usersDict = new Dictionary<string, User>();
        ObservableCollection<User> users = new ObservableCollection<User>();
        ListCollectionView view;
        #endregion

        #region Constructor

        public UserManager(SessionManager.Session session)
        {
            if (session == null)
                throw new ArgumentNullException("Session");

            this.session = session;
            this.conn = session.Connection;
            // create the user view
            view = new ListCollectionView(users);
            // set up handlers
            users.CollectionChanged += new NotifyCollectionChangedEventHandler(users_CollectionChanged);
        }

        void users_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            bool add = false,
                 remove = false;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    add = true;
                    break;
                case NotifyCollectionChangedAction.Remove:
                    remove = true;
                    break;
                case NotifyCollectionChangedAction.Reset:
                    add = remove = true;
                    break;
            }

            if (add && e.NewItems != null)
            {
                foreach (User user in e.NewItems)
                {
                    usersDict.Add(user.Name, user);
                }
            }

            if (remove && e.OldItems != null)
            {
                foreach (User user in e.OldItems)
                {
                    usersDict.Remove(user.Name);
                }
            }
        }
        #endregion

        #region Public interface

        /// <summary>
        /// Loads all users from a database
        /// </summary>
        public void Refresh()
        {
            OracleCommand cmd = new OracleCommand(DBA_USERS_SELECT, conn);
            OracleDataReader odr = cmd.ExecuteReader();

            // if cannot access DBA view, return
            bool hasRows = odr.HasRows;

            if (!hasRows)
            {
                // try ALL view
                cmd.CommandText = ALL_USERS_SELECT;
                odr = cmd.ExecuteReader();
                // has rows?
                hasRows = odr.HasRows;
            }

            if (!hasRows)
                return;

            // perform refresh
            List<User> newUsers = new List<User>();
            List<string> existingUserNames = new List<string>();

            while (odr.Read())
            {
                User.UserData userData = LoadUserData(odr);
                string userKey = userData.name;
                
                // skip current user
                if (userKey == currentUser.Name)
                    continue;

                User user;
                if (!usersDict.TryGetValue(userKey, out user))
                {
                    newUsers.Add(new User(userData, this.session));
                } else
                {
                    user.Update(userData);
                    existingUserNames.Add(userKey);
                }
            }

            // remove nonexistent users
            removeNonExistentUsers(usersDict.Keys.Except(existingUserNames));

            // add new users
            addNewUsers(newUsers);

            // load privileges, roles, quotas, resource limits etc.

        }
        public bool RefreshCurrentUser()
        {
            OracleCommand cmd = new OracleCommand(CurrentUser.CURRENT_USER_SELECT, conn);
            OracleDataReader odr = cmd.ExecuteReader();

            if (!odr.HasRows)
                return false;

            CurrentUser.CurrentUserData data = LoadCurrentUserData(odr);

            if (currentUser == null)
            {
                currentUser = new CurrentUser(data, this.session);
            } else
            {
                currentUser.Update(data);
            }

            // load system privileges and roles


            return true;
        }
        #endregion

        #region Helper methods
        User.UserData LoadUserData(OracleDataReader odr)
        {
            decimal id = odr.GetDecimal(odr.GetOrdinal("user_id"));
            string name = odr.GetString(odr.GetOrdinal("username"));
            object defaultTablespace = odr.GetValue(odr.GetOrdinal("default_tablespace"));
            object temporaryTablespace = odr.GetValue(odr.GetOrdinal("temporary_tablespace"));
            DateTime? created = odr.GetDateTime(odr.GetOrdinal("created"));
            DateTime? expiryDate = null;
            
            if (!odr.IsDBNull(odr.GetOrdinal("expiry_date")))
                expiryDate = odr.GetDateTime(odr.GetOrdinal("expiry_date"));

            return new User.UserData(id, name, defaultTablespace, temporaryTablespace,
                expiryDate, created);
        }
        CurrentUser.CurrentUserData LoadCurrentUserData(OracleDataReader odr)
        {
            decimal id = odr.GetDecimal(odr.GetOrdinal("user_id"));
            string name = odr.GetString(odr.GetOrdinal("username"));
            object defaultTablespace = odr.GetValue(odr.GetOrdinal("default_tablespace"));
            object temporaryTablespace = odr.GetValue(odr.GetOrdinal("temporary_tablespace"));
            DateTime? created = odr.GetDateTime(odr.GetOrdinal("created"));
            DateTime? expiryDate = null;

            if (!odr.IsDBNull(odr.GetOrdinal("expiry_date")))
                expiryDate = odr.GetDateTime(odr.GetOrdinal("expiry_date"));

            return new CurrentUser.CurrentUserData(id, name, defaultTablespace, temporaryTablespace,
                expiryDate, created);
        }
        void removeNonExistentUsers(IEnumerable<string> userNames)
        {
            HashSet<string> hashNames = new HashSet<string>(userNames);
            // TODO: maybe unload associated privileges, roles, ...?
            foreach (string userName in userNames)
            {
                User user = usersDict[userName];
                // dispose user
                user.Dispose();
            }
            // delete users from the list
            users.RemoveAll((user) => (hashNames.Contains(user.Name)));
        }
        void addNewUsers(List<User> newUsers)
        {
            users.AddRange(newUsers);
        }
        #endregion

        #region Properties
        public ListCollectionView View
        {
            get { return view; }
        }
        #endregion







        #region User class

        public class User : UserRole
        {
            #region Members
            SessionManager.Session session;
            OracleConnection conn;
            UserManager manager;
            #endregion

            #region Constructor

            public User(UserData data, SessionManager.Session session):
                base(data, session)
            {
                this.manager = this.session.UserManager;
            }
            #endregion

            #region Public interface
            public void Update(UserData data)
            {
                
                
            }
            #endregion

            #region Properties
            public decimal Id
            {
                get { return (this.data as UserData).id; }
            }
            public DateTime? Created
            {
                get { return (this.data as UserData).created; }
                set { (this.data as UserData).created = value; }
            }
            public DateTime? Expires
            {
                get { return (this.data as UserData).expiryDate; }
                set { (this.data as UserData).expiryDate = value; }
            }
            public object DefaultTablespace
            {
                get { return (this.data as UserData).defaultTablespace; }
                set { (this.data as UserData).defaultTablespace = value; }
            }
            public object TemporaryTablespace
            {
                get { return (this.data as UserData).defaultTablespace; }
                set { (this.data as UserData).defaultTablespace = value; }
            }
            #endregion


            #region User data class
            public class UserData : UserRoleData
            {
                #region Members
                public readonly decimal id;
                public string name;
                public object defaultTablespace, temporaryTablespace;
                public DateTime? expiryDate, created;
                #endregion

                #region Constructor
                public UserData(
                    decimal id, string name,
                    object defaultTablespace, object temporaryTablespace,
                    DateTime? expiryDate, DateTime? created):
                    base(name)
                {
                    this.id = id;
                    this.name = name;
                    this.defaultTablespace = defaultTablespace;
                    this.temporaryTablespace = temporaryTablespace;
                    this.expiryDate = expiryDate;
                    this.created = created;
                }
                #endregion
            }
            #endregion

            public override void Dispose()
            {
                // uvolni zdroje (privs, roles, ...)
            }
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
            public CurrentUser(CurrentUserData data, SessionManager.Session session) :
                base(data, session)
            { }
            #endregion

            #region Current user class data
            public class CurrentUserData : UserData
            {
                #region Constructor
                public CurrentUserData(
                    decimal id, string name,
                    object defaultTablespace, object temporaryTablespace,
                    DateTime? expiryDate, DateTime? created) :
                    base(id, name, defaultTablespace, temporaryTablespace,
                          expiryDate, created)
                { }
                #endregion
            }
            #endregion
        }

        #endregion
    }
}
