using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

namespace oradmin
{
    class SessionPlSqlUnitManager
    {
        #region Members
        #region SQL SELECTS
        public const string ALL_PROCEDURES_SELECT = @"
            SELECT
                owner, object_name, procedure_name,
                aggregate, pipelined, parallel
            FROM
                ALL_PROCEDURES";
        public const string ALL_PROCEDURES_PROCEDURES_SELECT = @"
            SELECT
                owner, object_name, procedure_name,
                parallel
            FROM
                ALL_PROCEDURES";
        public const string ALL_PROCEDURES_PROCEDURE_SELECT = @"
            SELECT
                owner, object_name, procedure_name,
                parallel
            FROM
                ALL_PROCEDURES
            WHERE
                owner = :owner and
                object_name = :object_name";
        public const string ALL_PROCEDURES_FUNCTION_SELECT = @"
            SELECT
                owner, object_name, procedure_name,
                aggregate, pipelined, parallel
            FROM
                ALL_PROCEDURES
            WHERE
                owner = :owner and
                object_name = :object_name";
        #endregion
        SessionManager.Session session;
        OracleConnection conn;
        #endregion

        #region Constructor
        public SessionPlSqlUnitManager(SessionManager.Session session)
        {
            if (session == null)
                throw new ArgumentNullException("Session");

            this.session =session;
            this.conn  = this.session.Connection;
        }
        #endregion
    }
}
