using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

namespace oradmin
{
    class DependencyManager
    {
        #region Members
        #region SQL SELECTS
        public static const string ALL_DEPENDENCIES_SELECT = @"
            SELECT
                owner, name, type,
                referenced_owner, referenced_name, referenced_type,
                dependency_type
            FROM
                ALL_DEPENDENCIES
            WHERE
                referenced_link_name is null";
        public static const string ALL_DEPENDENCIES_SELECT_SCHEMA = @"
            SELECT
                owner, name, type,
                referenced_owner, referenced_name, referenced_type,
                dependency_type
            FROM
                ALL_DEPENDENCIES
            WHERE
                referenced_link_name is null and
                (owner = :schema or
                 referenced_owner = :schema)";
        public static const string ALL_DEPENDENCIES_SELECT_SCHEMA_TYPE = @"
            SELECT
                owner, name, type,
                referenced_owner, referenced_name, referenced_type,
                dependency_type
            FROM
                ALL_DEPENDENCIES
            WHERE
                referenced_link_name is null and
                (owner = :schema and type = :type) or
                (referenced_owner = :schema and referenced_type = :type)";
        public static const string ALL_DEPENDENCIES_SELECT_OBJECT_TYPE = @"
            SELECT
                owner, name, type,
                referenced_owner, referenced_name, referenced_type,
                dependency_type
            FROM
                ALL_DEPENDENCIES
            WHERE
                referenced_link_name is null and
                (owner = :owner and name = :name and type = :type) or
                (referenced_owner = :owner and referenced_name = :name and referenced_type = :type)";
        #endregion
        SessionManager.Session session;
        OracleConnection conn;
        #endregion

        #region Constructor
        public DependencyManager(SessionManager.Session session)
        {
            if (session == null)
                throw new ArgumentNullException("Session");

            this.session = session;
            this.conn = this.session.Connection;
        }
        #endregion
    }
}
