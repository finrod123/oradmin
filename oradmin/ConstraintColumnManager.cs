using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

namespace oradmin
{
    class ConstraintColumnManager
    {
        #region Members
        #region SQL SELECTS
        public static const string ALL_CONS_COLUMNS_SELECT = @"
            SELECT
                owner, constraint_name, table_name,
                column_name, position
            FROM
                ALL_CONS_COLUMNS";
        public static const string ALL_CONS_COLUMNS_SCHEMA_SELECT = @"
            SELECT
                owner, constraint_name, table_name,
                column_name, position
            FROM
                ALL_CONS_COLUMNS
            WHERE
                owner = :owner";
        public static const string ALL_CONS_COLUMNS_TABLE_SELECT = @"
            SELECT
                owner, constraint_name, table_name,
                column_name, position
            FROM
                ALL_CONS_COLUMNS
            WHERE
                owner = :owner and
                table_name = :table_name";
        #endregion
        SessionManager.Session session;
        OracleConnection conn;
        #endregion

        #region Constructor
        public ConstraintColumnManager(SessionManager.Session session)
        {
            if (session == null)
                throw new ArgumentNullException("Session");

            this.session = session;
            this.conn = this.session.Connection;
        }
        #endregion

        #region Public interface
        public void Refresh()
        {

        }
        #endregion

        #region Constraint column class
        public struct ConstraintColumn
        {
            #region Members
            string owner;
            string constraintName;
            string tableName;
            string columnName;
            ColumnBasedConstraint constraintRef;
            int? position;
            #endregion

            #region Constructor
            public ConstraintColumn(
                string owner,
                string constraintName,
                string tableName,
                string columnName,
                int? position)
            {
                this.owner = owner;
                this.constraintName = constraintName;
                this.constraintRef = null;
                this.tableName = tableName;
                this.columnName = columnName;
                this.position = position;
            }
            #endregion

            #region Properties

            #endregion
        }
        #endregion
    }
}
