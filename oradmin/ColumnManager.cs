using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

// ---TODO: loadColumn, column properties
//    local column manager: download data
namespace oradmin
{
    public delegate void AllColumnsRefreshedHandler();

    class ColumnManager
    {
        #region Members
        #region SQL SELECTS
        public static const string ALL_TAB_COLUMNS_SELECT = @"
            SELECT
                owner, table_name, column_name,
                data_type, data_length, data_precision, data_scale,
                nullable, default_length, data_default,
                char_length, char_used
            FROM
                ALL_TAB_COLUMNS";
        public static const string ALL_TAB_COLUMNS_TABLE_SELECT = @"
            SELECT
                owner, table_name, column_name,
                data_type, data_length, data_precision, data_scale,
                nullable, default_length, data_default,
                char_length, char_used
            FROM
                ALL_TAB_COLUMNS
            WHERE
                owner = :owner and
                table_name = :table_name";
        public static const string ALL_TAB_COLUMNS_SCHEMA_SELECT = @"
            SELECT
                owner, table_name, column_name,
                data_type, data_length, data_precision, data_scale,
                nullable, default_length, data_default,
                char_length, char_used
            FROM
                ALL_TAB_COLUMNS
            WHERE
                owner = :owner";
        #endregion
        SessionManager.Session session;
        OracleConnection conn;

        List<ColumnManager.TableColumn> columns = new List<TableColumn>();
        #endregion

        #region Constructor
        public ColumnManager(SessionManager.Session session)
        {
            if (session == null)
                throw new ArgumentNullException("Session");

            this.session = session;
            this.conn = this.session.Connection;
        }
        #endregion

        #region Public interface
        public void RefreshColumns()
        {
            OracleCommand cmd = new OracleCommand(ALL_TAB_COLUMNS_SELECT, conn);
            OracleDataReader odr = cmd.ExecuteReader();

            if (!odr.HasRows)
                return;

            columns.Clear();

            while (odr.Read())
            {
                TableColumn column = LoadColumn(odr);
                columns.Add(column);
            }

            // notify about refresh
            OnAllColumnsRefreshed();
        }
        public bool RefreshColumns(string schema)
        {
            OracleCommand cmd = new OracleCommand(ALL_TAB_COLUMNS_SCHEMA_SELECT, conn);
            // set up parameters
            OracleParameter ownerParam = cmd.CreateParameter();
            ownerParam.ParameterName = "owner";
            ownerParam.OracleDbType = OracleDbType.Char;
            ownerParam.Direction = System.Data.ParameterDirection.Input;
            ownerParam.Value = schema;
            // execute command
            OracleDataReader odr = cmd.ExecuteReader();

            if (!odr.HasRows)
                return false;

            // purge old data
            purgeTableColumnsData(schema);

            while (odr.Read())
            {
                TableColumn column = LoadColumn(odr);
                columns.Add(column);
            }

            return true;
        }
        public bool RefreshColumns(TableManager.Table table)
        {
            // set up command and parameters
            OracleCommand cmd = new OracleCommand(ALL_TAB_COLUMNS_TABLE_SELECT, conn);
            OracleParameter ownerParam = cmd.CreateParameter();
            ownerParam.ParameterName = "owner";
            ownerParam.OracleDbType = OracleDbType.Char;
            ownerParam.Direction = System.Data.ParameterDirection.Input;
            ownerParam.Value = table.Name;
            OracleParameter tableParam = cmd.CreateParameter();
            tableParam.ParameterName = "table_name";
            tableParam.OracleDbType = OracleDbType.Char;
            tableParam.Direction = System.Data.ParameterDirection.Input;
            tableParam.Value = table.Owner;
            // prepare data reader
            OracleDataReader odr = cmd.ExecuteReader();

            if (!odr.HasRows)
                return false;

            // purge table columns data
            purgeTableColumnsData(table);

            while (odr.Read())
            {
                TableColumn column = LoadColumn(odr);
                columns.Add(column);
            }

            return true;
        }
        
        #endregion

        #region Helper methods
        private void purgeTableColumnsData(TableManager.Table table)
        {
            columns.RemoveAll((column) => (column.Table == table));
        }
        private void purgeTableColumnsData(string schema)
        {
            columns.RemoveAll((column) => (schema.Equals(column.TableOwner)));
        }
        private void OnAllColumnsRefreshed()
        {
            if (AllColumnsRefreshed != null)
            {
                AllColumnsRefreshed();
            }
        }
        #endregion

        #region Public static interface
        public static TableColumn LoadColumn(OracleDataReader odr)
        {

        }
        #endregion

        #region Events
        public event AllColumnsRefreshedHandler AllColumnsRefreshed;
        #endregion


        // ---TODO: loadColumn, column properties
        #region Column class
        public class TableColumn
        {
            #region Members
            string tableOwner;
            string tableName;
            TableManager.Table table;
            string columnName;
            Type? dataType;
            int dataLength;
            int? dataPrecision, dataScale;
            bool nullable;
            int? defaultLength;
            object? dataDefault;
            int? charLength;
            bool? charUsed;
            #endregion

            #region Constructor
            public TableColumn(
                string tableOwner,
                string tableName,
                string columnName,
                Type? dataType,
                int dataLength,
                int? dataPrecision, int? dataScale,
                bool nullable,
                int? defaultLength,
                object? dataDefault,
                int? charLength,
                bool? charUsed)
            {
                this.tableOwner = tableOwner;
                this.tableName = tableName;
                this.table = null;
                this.columnName = columnName;
                this.dataType = dataType;
                this.dataLength = dataLength;
                this.dataPrecision = dataPrecision;
                this.dataScale = dataScale;
                this.nullable = nullable;
                this.defaultLength = defaultLength;
                this.dataDefault = dataDefault;
                this.charLength = charLength;
                this.charUsed = charUsed;
            }
            #endregion

            #region Properties
            public TableManager.Table Table
            {
                get { return table; }
            }
            public string TableOwner
            {
                get { return tableOwner; }
            }
            #endregion
        }
        #endregion

        #region Local column manager class
        public class LocalColumnManager
        {
            #region Members
            SessionManager.Session session;
            OracleConnection conn;

            ColumnManager manager;
            ObservableCollection<TableColumn> columns = new ObservableCollection<TableColumn>();
            #endregion

            #region Constructor
            public LocalColumnManager(SessionManager.Session session)
            {
                if (session == null)
                    throw new ArgumentNullException("Session");

                this.session = session;
                this.conn = this.session.Connection;
                manager = this.session.SchemaManager.ColumnManager;
                // set up event handlers
                this.manager.AllColumnsRefreshed += new AllColumnsRefreshedHandler(manager_AllColumnsRefreshed);
            }
            #endregion

            #region Helper methods
            void manager_AllColumnsRefreshed()
            {
                downloadData();
            }
            void downloadData()
            {

            }
            #endregion
        }
        #endregion
    }
}
