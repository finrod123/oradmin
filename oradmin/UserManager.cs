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
    class UserManager
    {
        #region Static members

        public const string CURRENT_USER_SELECT = @"SELECT
                    user_id, username,
                    default_tablespace, temporary_tablespace,
                    created, expiry_date
                  FROM
                    USER_USERS";

        #endregion

        #region Members
        OracleConnection conn;
        User currentUser;
        Dictionary<int, User> id2Users = new Dictionary<int, User>();
        Dictionary<string, User> name2Users = new Dictionary<string, User>();
        ObservableCollection<User> users = new ObservableCollection<User>();
        ListCollectionView defaultUserView;

        #endregion

        #region Constructor

        public UserManager(OracleConnection conn)
        {
            if (conn == null)
                throw new ArgumentNullException("Oracle connection");

            this.conn = conn;
            // load current user
            loadCurrentUser();
            // load other users

            // create user view
            defaultUserView = CollectionViewSource.GetDefaultView(users) as ListCollectionView;
        }

        #endregion

        #region Properties

        protected OracleConnection Connection
        {
            get { return conn; }
        }
        public User CurrentUser
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
                DateTime? created = null,
                          expiryDate = null;

                if (!odr.IsDBNull(4))
                    created = (DateTime)odr.GetDateTime(4);
                if (!odr.IsDBNull(5))
                    expiryDate = (DateTime)odr.GetDateTime(5);

                currentUser = new User(
                    odr.GetDecimal(0),
                    odr.GetString(1),
                    odr.GetString(2),
                    odr.GetString(3),
                    created,
                    expiryDate,
                    this,
                    true);
            } else
                loaded = false;

            return loaded;
        }
        /// <summary>
        /// Loads all users from a database
        /// </summary>
        void loadUsers()
        {

        }

        #endregion

        #region User class

        public class User
        {
            #region Members
            UserManager manager;
            PrivManager.PrivManagerLocal privManager;

            OracleConnection conn;
            bool local;

            readonly decimal id;
            string name;
            object defaultTablespace, temporaryTablespace;
            DateTime? created, expiryDate;

            #endregion

            #region Constructor

            public User(decimal id, string name,
                        object defaultTablespace, object temporaryTablespace,
                        DateTime? created, DateTime? expiryDate,
                        UserManager manager, bool local)
            {
                if (manager == null)
                    throw new ArgumentNullException("User manager");

                this.id = id;
                this.name = name;
                this.defaultTablespace = defaultTablespace;
                this.temporaryTablespace = temporaryTablespace;
                this.created = created;
                this.expiryDate = expiryDate;
                this.manager = manager;
                this.local = local;
                this.conn = this.manager.Connection;
                // nahraj si privilegia
                privManager = new PrivManager.PrivManagerLocal(
                // nahraj si role

                // nahraj si kvoty

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

            #endregion
        }

        #endregion
    }
}
