using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

namespace oradmin
{
    class SessionTriggerManager
    {
        #region Members
        #region SQL SELECTS
        public static const string ALL_TRIGGERS_SELECT = @"
            SELECT
                owner, trigger_name,
                trigger_type, triggering_event, table_owner,
                base_object_type, table_name,
                referencing_names, when_clause, status,
                description, action_type, trigger_body
            FROM
                ALL_TRIGGERS
            WHERE
                column_name is null";
        public static const string ALL_TRIGGERS_SELECT_SCHEMA = @"
            SELECT
                owner, trigger_name,
                trigger_type, triggering_event, table_owner,
                base_object_type, table_name,
                referencing_names, when_clause, status,
                description, action_type, trigger_body
            FROM
                ALL_TRIGGERS
            WHERE
                column_name is null and
                (owner = :owner or
                    (
                      (base_object_type = 'TABLE' or base_object_type = 'VIEW') and
                       table_owner = :owner)
                    )
                 )";
                 
        #endregion
        SessionManager.Session session;
        OracleConnection conn;
        #endregion
    }
}
