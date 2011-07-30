using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

// ---TODO: LoadColumn, Refresh schema and table, notify
namespace oradmin
{
    class IndexColumnManager
    {
        #region Members
        #region SQL SELECTS
        public static const string ALL_IND_COLUMNS = @"
            SELECT
                index_owner, index_name,
                table_owner, table_name, column_name,
                column_position, descend
            FROM
                ALL_IND_COLUMNS";
        public static const string ALL_IND_COLUMNS_SCHEMA = @"
            SELECT
                index_owner, index_name,
                table_owner, table_name, column_name,
                column_position, descend
            FROM
                ALL_IND_COLUMNS
            WHERE
                table_owner = :table_owner";
        public static const string ALL_IND_COLUMNS_TABLE = @"
            SELECT
                index_owner, index_name,
                table_owner, table_name, column_name,
                column_position, descend
            FROM
                ALL_IND_COLUMNS
            WHERE
                table_owner = :table_owner and
                table_name = :table_name";
        #endregion
        SessionManager.Session session;
        OracleConnection conn;

        List<IndexColumn> columns = new List<IndexColumn>();
        #endregion


        #region Constructor
        public IndexColumnManager(SessionManager.Session session)
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
            OracleCommand cmd = new OracleCommand(ALL_IND_COLUMNS, conn);
            OracleDataReader odr = cmd.ExecuteReader();

            if (!odr.HasRows)
                return;

            // purge old data
            columns.Clear();

            while (odr.Read())
            {
                IndexColumn column = LoadColumn(odr);
                columns.Add(column);
            }

            // notify
        }
        public bool Refresh(string schema)
        {

        }
        public bool Refresh(TableManager.Table table)
        {

        }
        #endregion

        #region Public static interface
        public static IndexColumn LoadColumn(OracleDataReader odr)
        {

        }
        #endregion

        #region IndexColumn struct
        public struct IndexColumn
        {
            #region Members
            string indexOwner;
            string indexName;
            string tableOwner;
            string tableName;
            string columnName;
            int columnPosition;
            bool? descend;
            #endregion

            #region Constructor
            public IndexColumn(
                string indexOwner,
                string indexName,
                string tableOwner,
                string tableName,
                string columnName,
                int columnPosition,
                bool? descend)
            {
                this.indexOwner = indexOwner;
                this.indexName = indexName;
                this.tableOwner = tableOwner;
                this.tableName = tableName;
                this.columnName = columnName;
                this.columnPosition = columnPosition;
                this.descend = descend;
            }
            #endregion
        }
        #endregion
    }
}
