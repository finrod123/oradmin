using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows;

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

            UserManager userManager;
            RoleManager roleManager;
            RoleDependencyManager roleDependencyManager;
            PrivManager privManager;
            #endregion

            #region Constructor

            public Session(OracleConnection conn)
            {
                if(conn == null)
                    throw new ArgumentNullException("Connection");

                this.conn = conn;

                try
                {
                    // create managers
                    userManager = new UserManager(conn);
                    roleManager = new RoleManager(conn);
                    roleDependencyManager = new RoleDependencyManager(conn);
                    privManager = new PrivManager(this);
                    // load current user data

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
            public UserManager.User CurrentUser
            {
                get { return userManager.CurrentUser; }
            }
            public PrivManager PrivManager
            {
                get { return privManager; }
            }
            public UserManager UserManager
            {
                get { return userManager; }
            }

            #endregion
        }
    }
}
