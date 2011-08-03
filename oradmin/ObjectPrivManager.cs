using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

namespace oradmin
{
    class SessionTabPrivManager
    {
        #region Members
        #region SQL SELECTS
        public static const string DBA_TAB_PRIVS_SELECT = @"
            SELECT
                grantee, owner, table_name,
                grantor, privilege, grantable
            FROM
                DBA_TAB_PRIVS";
        public static const string DBA_TAB_PRIVS_USERROLE_SELECT = @"
            SELECT
                grantee, owner, table_name,
                grantor, privilege, grantable
            FROM
                DBA_TAB_PRIVS
            WHERE
                grantee = :grantee";
        public static const string DBA_TAB_PRIVS_USERS_SELECT = @"
            SELECT
                grantee, owner, table_name,
                grantor, privilege, grantable
            FROM
                DBA_TAB_PRIVS dtp
                    INNER JOIN
                DBA_USERS du
                    ON(du.username = dtp.grantee)";
        public static const string DBA_TAB_PRIVS_ROLES_SELECT = @"
            SELECT
                grantee, owner, table_name,
                grantor, privilege, grantable
            FROM
                DBA_TAB_PRIVS dtp
                    INNER JOIN
                DBA_ROLES dr
                    ON(dr.role = dtp.grantee)";

        public static const string ALL_TAB_PRIVS_MADE_SELECT = @"
            SELECT
                grantee, owner, table_name,
                grantor, privilege, grantable
            FROM
                ALL_TAB_PRIVS_MADE";
        public static const string ALL_TAB_PRIVS_MADE_USERS_SELECT = @"
            SELECT
                grantee, owner, table_name,
                grantor, privilege, grantable
            FROM
                ALL_TAB_PRIVS_MADE atp
                    INNER JOIN
                ALL_USERS au
                    ON(au.username = atp.grantee)";
        public static const string ALL_TAB_PRIVS_MADE_ROLES_SELECT = @"
            SELECT
                grantee, owner, table_name,
                grantor, privilege, grantable
            FROM
                ALL_TAB_PRIVS_MADE
            WHERE
                grantee not in
                    (SELECT
                        username
                     FROM
                        ALL_USERS)";
        public static const string ALL_TAB_PRIVS_MADE_USERROLE_SELECT = @"
            SELECT
                grantee, owner, table_name,
                grantor, privilege, grantable
            FROM
                ALL_TAB_PRIVS_MADE
            WHERE
                grantee = :grantee";
        public static const string ROLE_TAB_PRIVS_SELECT = @"
            SELECT
                role, owner, table_name,
                privilege, grantable
            FROM
                ROLE_TAB_PRIVS
            WHERE
                column_name is null";
        public static const string ROLE_TAB_PRIVS_ROLE_SELECT = @"
            SELECT
                role, owner, table_name,
                privilege, grantable
            FROM
                ROLE_TAB_PRIVS
            WHERE
                column_name is null and
                role = :role";
        public static const string PUBLIC_TAB_PRIVS_SELECT = @"
            SELECT
                role, owner, table_name,
                privilege, grantable
            FROM
                ALL_TAB_PRIVS_RECD
            WHERE
                grantee = 'PUBLIC'";
        public static const string USER_TAB_PRIVS_RECD_SELECT = @"
            SELECT
                owner, table_name,
                grantor, privilege, grantable
            FROM
                USER_TAB_PRIVS_RECD";
        #endregion
        SessionManager.Session session;
        OracleConnection conn;
        List<GrantedTabPrivilege> grants = new List<GrantedTabPrivilege>();
        #endregion

        #region Constructor
        public SessionTabPrivManager(SessionManager.Session session)
        {
            if (session == null)
                throw new ArgumentNullException("Session");

            this.session = session;
            this.conn = session.Connection;
        }
        #endregion

        #region Public interface
        public void Refresh()
        {

        }
        #endregion

        #region Object privilege class
        
        #endregion
    }
}
