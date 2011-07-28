using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

namespace oradmin
{
    class TabPrivManager
    {
        #region Members
        #region SQL SELECTS
        public static const string DBA_TAB_PRIVS_SELECT = @"
            SELECT
                grantee, owner, table_name,
                grantor, privilege, grantable
            FROM
                DBA_TAB_PRIVS";
                
        #endregion
        SessionManager.Session session;
        OracleConnection conn;
        List<GrantedTabPrivilege> grants = new List<GrantedTabPrivilege>();
        #endregion

        #region Constructor
        public TabPrivManager(SessionManager.Session session)
        {
            if (session == null)
                throw new ArgumentNullException("Session");

            this.session = session;
            this.conn = session.Connection;
        }
        #endregion

        #region Properties
        public OracleConnection Connection
        {
            get { return conn; }
        }
        #endregion

        #region Public interface
        public void RefreshPrivileges()
        {

        }
        #endregion
    }
}
