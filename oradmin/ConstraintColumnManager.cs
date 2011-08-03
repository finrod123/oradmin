using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

namespace oradmin
{
    public delegate void AllConstraintColumnsRefreshedHandler();

    class SessionConstraintColumnManager
    {
        #region Members
        #region SQL SELECTS
        public static const string ALL_CONS_COLUMNS_SELECT = @"
            SELECT
                owner, constraint_name, table_name,
                column_name, position
            FROM
                ALL_CONS_COLUMNS";
        public static const string ALL_CONS_COLUMNS_SELECT_SCHEMA = @"
            SELECT
                owner, constraint_name, table_name,
                column_name, position
            FROM
                ALL_CONS_COLUMNS
            WHERE
                owner = :owner";
        public static const string ALL_CONS_COLUMNS_SELECT_TABLE = @"
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

        List<ConstraintColumn> columns = new List<ConstraintColumn>();
        #endregion

        #region Constructor
        public SessionConstraintColumnManager(SessionManager.Session session)
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
            OracleCommand cmd = new OracleCommand(ALL_CONS_COLUMNS_SELECT, conn);
            OracleDataReader odr = cmd.ExecuteReader();

            if (!odr.HasRows)
                return;

            // purge old data
            columns.Clear();

            while (odr.Read())
            {
                ConstraintColumn column = LoadColumn(odr);
                columns.Add(column);
                
            }
        }
        #endregion

        #region Public static interface
        public static ConstraintColumn LoadColumn(OracleDataReader odr)
        {
            string owner = odr.GetString(odr.GetOrdinal("owner"));
            string constraintName = odr.GetString(odr.GetOrdinal("constraint_name"));
            string tableName = odr.GetString(odr.GetOrdinal("table_name"));
            string columnName = null;
            int? position = null;

            if (!odr.IsDBNull(odr.GetOrdinal("column_name")))
                columnName = odr.GetString(odr.GetOrdinal("column_name"));

            if (!odr.IsDBNull(odr.GetOrdinal("position")))
                position = odr.GetInt32(odr.GetOrdinal("position"));

            return new ConstraintColumn(
                owner, constraintName, tableName, columnName, position);
        }
        #endregion

        #region Constraint column class
        public class ConstraintColumn
        {
            #region Members
            string owner;
            string constraintName;
            string tableName;
            string columnName;
            SessionConstraintManager.ColumnBasedConstraint constraintRef;
            SessionColumnManager.TableColumn columnRef;
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
                this.columnRef = null;
                this.position = position;
            }
            #endregion

            #region Properties
            public string Owner
            {
                get
                {
                    if (constraintRef != null)
                        return constraintRef.Owner;

                    return owner;
                }
            }
            public string ConstraintName
            {
                get
                {
                    if (constraintRef != null)
                        return constraintRef.ConstraintName;

                    return constraintName;
                }
            }
            public SessionConstraintManager.ColumnBasedConstraint Constraint
            {
                get { return this.constraintRef; }
                set { this.constraintRef = value; }
            }
            public string TableName
            {
                get
                {
                    if (constraintRef != null)
                        return constraintRef.TableName;

                    return tableName;
                }
            }
            public string ColumnName
            {
                get
                {
                    if (this.columnRef != null)
                        return columnRef.ColumnName;

                    return columnName;
                }
            }
            public SessionColumnManager.TableColumn Column
            {
                get { return this.columnRef; }
                set { this.columnRef = value; }
            }
            public int? Position
            {
                get { return this.position; }
            }
            #endregion
        }
        #endregion
    }
}
