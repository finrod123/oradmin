using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

namespace oradmin
{   
    // --- TODO: refresh - nelze jednoduse purge, je nutny diff merge (add, modify, delete
    //     nonexistent
    //                   - LoadTable

    public delegate void AllTablesRefreshedHandler();

    class TableManager
    {
        #region Members
        #region SQL SELECTS
        public static const string ALL_TABLES_SELECT = @"
            SELECT
                owner, table_name, tablespace_name, compression, dropped
            FROM
                ALL_TABLES";
        public static const string ALL_TABLES_SELECT_SCHEMA = @"
            SELECT
                owner, table_name, tablespace_name, compression, dropped
            FROM
                ALL_TABLES
            WHERE
                owner = :owner";
        public static const string ALL_TABLES_SELECT_TABLE = @"
            SELECT
                owner, table_name, tablespace_name, compression, dropped
            FROM
                ALL_TABLES
            WHERE
                owner = :owner and
                table_name = :table_name";
        #endregion
        SessionManager.Session session;
        SchemaManager manager;
        ColumnManager columnManager;
        ConstraintManager constraintManager;
        IndexManager indexManager;

        OracleConnection conn;

        ObservableCollection<Table> tables = new ObservableCollection<Table>();
        #endregion

        #region Constructor
        public TableManager(SessionManager.Session session)
        {
            if (session == null)
                throw new ArgumentNullException("Session");

            this.session = session;
            this.manager = session.SchemaManager;
            this.conn = session.Connection;
            this.columnManager = manager.ColumnManager;
            this.constraintManager = manager.ConstraintManager;
            this.indexManager = manager.IndexManager;
        }
        #endregion

        #region Public interface
        public void Refresh()
        {
            OracleCommand cmd = new OracleCommand(ALL_TABLES_SELECT, conn);
            OracleDataReader odr = cmd.ExecuteReader();

            if (!odr.HasRows)
                return;

            // purge old data
            tables.Clear();

            while (odr.Read())
            {
                Table table = LoadTable(odr);
                tables.Add(table);
            }

            // notify
            OnAllTablesRefreshed();
        }
        public bool Refresh(string schema)
        {
            OracleCommand cmd = new OracleCommand(ALL_TABLES_SELECT_SCHEMA, conn);
            cmd.BindByName = true;
            // set up parameters
            OracleParameter schemaParam = cmd.CreateParameter();
            schemaParam.ParameterName = "owner";
            schemaParam.OracleDbType = OracleDbType.Char;
            schemaParam.Direction = System.Data.ParameterDirection.Input;
            schemaParam.Value = schema;
            cmd.Parameters.Add(schemaParam);
            // execute
            OracleDataReader odr = cmd.ExecuteReader();

            if (!odr.HasRows)
                return false;

            // purge old data
            purgeOldData(schema);

            while (odr.Read())
            {
                Table table = LoadTable(odr);
                addTable(table);
            }

            return true;
        }
        public void Refresh(Table table)
        {
            OracleCommand cmd = new OracleCommand(ALL_TABLES_SELECT_TABLE, conn);
            cmd.BindByName = true;
            // set up parameters
            // schema parameter
            OracleParameter schemaParam = cmd.CreateParameter();
            schemaParam.ParameterName = "owner";
            schemaParam.OracleDbType = OracleDbType.Char;
            schemaParam.Direction = System.Data.ParameterDirection.Input;
            schemaParam.Value = table.Owner;
            cmd.Parameters.Add(schemaParam);
            // table_name parameter
            OracleParameter tableParam = cmd.CreateParameter();
            tableParam.ParameterName = "table_name";
            tableParam.OracleDbType = OracleDbType.Char;
            tableParam.Direction = System.Data.ParameterDirection.Input;
            tableParam.Value = table.Name;
            cmd.Parameters.Add(tableParam);
            // execute
            OracleDataReader odr = cmd.ExecuteReader();

            if (!odr.HasRows)
                return false;

            // purge old data
            purgeOldData(table);

            while (odr.Read())
            {
                Table table = LoadTable(odr);
            }
        }
        #endregion

        #region Events
        public event AllTablesRefreshedHandler AllTablesRefreshed;
        #endregion

        #region Helper methods
        private void OnAllTablesRefreshed()
        {
            if (AllTablesRefreshed != null)
            {
                AllTablesRefreshed();
            }
        }
        private void purgeOldData(string schema)
        {
            
        }
        private void purgeOldData(Table table)
        {
            tables.Remove(table);
        }
        private void addTable(Table table)
        {
            tables.Add(table);
        }
        #endregion

        #region Public static interface
        public static Table LoadTable(OracleDataReader odr)
        {

        }
        #endregion

        #region Table class
        public class Table
        {
            #region Members
            string owner; // ---TODO: pridat i referenci na uzivatele?
            string tableName;
            string tablespaceName;
            bool compression;
            bool dropped;
            #endregion

            #region Constructor

            #endregion

            #region Properties
            public string Name
            {
                get { return tableName; }
            }
            public string Owner
            {
                get { return owner; }
            }
            #endregion
        }
        #endregion
    }
}
