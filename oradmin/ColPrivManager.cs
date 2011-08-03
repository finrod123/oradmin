using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace oradmin
{
    class SessionColPrivManager
    {
        #region Members
        #region SQL SELECTS
        public static const string DBA_COL_PRIVS_SELECT = @"
            SELECT
                grantee, owner, table_name, column_name,
                grantor, privilege, grantable
            FROM
                DBA_COL_PRIVS";
        public static const string DBA_COL_PRIVS_USERROLE_SELECT = @"
            SELECT
                grantee, owner, table_name, column_name,
                grantor, privilege, grantable
            FROM
                DBA_COL_PRIVS
            WHERE
                grantee = :grantee";
        public static const string DBA_COL_PRIVS_USERS_SELECT = @"
            SELECT
                grantee, owner, table_name, column_name,
                grantor, privilege, grantable
            FROM
                DBA_COL_PRIVS dcp
                    INNER JOIN
                DBA_USERS du
                    ON(du.username = dcp.grantee)";
        public static const string DBA_COL_PRIVS_ROLES_SELECT = @"
            SELECT
                grantee, owner, table_name, column_name,
                grantor, privilege, grantable
            FROM
                DBA_COL_PRIVS dcp
                    INNER JOIN
                DBA_ROLES dr
                    ON(acpacp.role = dcp.grantee)";

        public static const string ALL_COL_PRIVS_MADE_SELECT = @"
            SELECT
                grantee, owner, table_name, column_name,
                grantor, privilege, grantable
            FROM
                ALL_COL_PRIVS_MADE";
        public static const string ALL_COL_PRIVS_MADE_USERROLE_SELECT = @"
            SELECT
                grantee, owner, table_name, column_name,
                grantor, privilege, grantable
            FROM
                ALL_COL_PRIVS_MADE
            WHERE
                grantee = :grantee";
        public static const string ALL_COL_PRIVS_MADE_USERS_SELECT = @"
            SELECT
                grantee, owner, table_name, column_name,
                grantor, privilege, grantable
            FROM
                ALL_COL_PRIVS_MADE acp
                    INNER JOIN
                ALL_USERS au
                    ON(au.username = acp.grantee)";
        public static const string ALL_COL_PRIVS_MADE_ROLES_SELECT = @"
            SELECT
                grantee, owner, table_name, column_name,
                grantor, privilege, grantable
            FROM
                ALL_COL_PRIVS
            WHERE
                grantee not in
                    (SELECT
                        username
                     FROM
                        ALL_USERS
                     )";
        public static const string ROLE_COL_PRIVS_SELECT = @"
            SELECT
                role, owner, table_name, column_name,
                privilege, grantable
            FROM
                ROLE_TAB_PRIVS
            WHERE
                column_name is not null";
        public static const string ROLE_COL_PRIVS_ROLE_SELECT = @"
            SELECT
                role, owner, table_name, column_name,
                privilege, grantable
            FROM
                ROLE_TAB_PRIVS
            WHERE
                column_name is not null and
                role = :role";
        public static const string PUBLIC_COL_PRIVS_SELECT = @"
            SELECT
                role, owner, table_name, column_name,
                privilege, grantable
            FROM
                ALL_COL_PRIVS_RECD
            WHERE
                grantee = 'PUBLIC'";
        public static const string USER_COL_PRIVS_RECD_SELECT = @"
            SELECT
                owner, table_name, column_name,
                grantor, privilege, grantable
            FROM
                USER_COL_PRIVS_RECD";
        #endregion
        #endregion

        #region Constructor
        public SessionColPrivManager(SessionManager.Session session)
        {

        }
        #endregion
    }
}
