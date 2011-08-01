using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

// ---TODO: 
//    opravit refresh pattern
//    enum converters!!!
//    column properties
//    local column manager: download data
namespace oradmin
{
    public delegate void AllColumnsRefreshedHandler();

    class ColumnManagerSession
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

        List<ColumnManagerSession.TableColumn> columns = new List<TableColumn>();
        #endregion

        #region Constructor
        public ColumnManagerSession(SessionManager.Session session)
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
        public bool Refresh(string schema)
        {
            OracleCommand cmd = new OracleCommand(ALL_TAB_COLUMNS_SCHEMA_SELECT, conn);
            // set up parameters
            OracleParameter ownerParam = cmd.CreateParameter();
            ownerParam.ParameterName = "owner";
            ownerParam.OracleDbType = OracleDbType.Char;
            ownerParam.Direction = System.Data.ParameterDirection.Input;
            ownerParam.Value = schema;
            cmd.Parameters.Add(ownerParam);
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
        public bool Refresh(SessionTableManager.Table table)
        {
            // set up command and parameters
            OracleCommand cmd = new OracleCommand(ALL_TAB_COLUMNS_TABLE_SELECT, conn);
            OracleParameter ownerParam = cmd.CreateParameter();
            ownerParam.ParameterName = "owner";
            ownerParam.OracleDbType = OracleDbType.Char;
            ownerParam.Direction = System.Data.ParameterDirection.Input;
            ownerParam.Value = table.Name;
            cmd.Parameters.Add(ownerParam);
            OracleParameter tableParam = cmd.CreateParameter();
            tableParam.ParameterName = "table_name";
            tableParam.OracleDbType = OracleDbType.Char;
            tableParam.Direction = System.Data.ParameterDirection.Input;
            tableParam.Value = table.Owner;
            cmd.Parameters.Add(tableParam);
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
        private void purgeTableColumnsData(SessionTableManager.Table table)
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
            string tableOwner;
            string tableName;
            string columnName;
            OracleDbType? dataType = null;
            int dataLength;
            int? dataPrecision = null, dataScale = null;
            bool nullable;
            int? defaultLength = null;
            object dataDefault = null;
            int? charLength = null;
            bool? charUsed = null;

            tableOwner = odr.GetString(odr.GetOrdinal("owner"));
            tableName = odr.GetString(odr.GetOrdinal("table_name"));
            columnName = odr.GetString(odr.GetOrdinal("column_name"));

            
            //if(!odr.IsDBNull(odr.GetOrdinal("data_type")))
            //    =odr.GetString(odr.GetOrdinal(""));
            dataLength = odr.GetInt32(odr.GetOrdinal("data_length"));

            if (!odr.IsDBNull(odr.GetOrdinal("data_precision")))
                dataPrecision = odr.GetInt32(odr.GetOrdinal("data_precision"));

            if(!odr.IsDBNull(odr.GetOrdinal("data_scale")))
                dataScale = odr.GetInt32(odr.GetOrdinal("scale"));

            //// nullable
            //if(!odr.IsDBNull(odr.GetOrdinal("data_type")))
            //    =odr.GetString(odr.GetOrdinal(""));

            if(!odr.IsDBNull(odr.GetOrdinal("default_length")))
                defaultLength = odr.GetInt32(odr.GetOrdinal("default_length"));

            if(!odr.IsDBNull(odr.GetOrdinal("data_default")))
                dataDefault = odr.GetValue(odr.GetOrdinal("data_default"));

            if(!odr.IsDBNull(odr.GetOrdinal("char_length")))
                charLength = odr.GetInt32(odr.GetOrdinal("char_length"));

            //// char used
            //if(!odr.IsDBNull(odr.GetOrdinal("char_used")))
            //    charUsed=odr.GetString(odr.GetOrdinal("char_used"));

            return new TableColumn(tableOwner, tableName, columnName,
                dataType, dataLength, dataPrecision, dataScale,
                nullable, defaultLength, dataDefault, charLength, charUsed);
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
            ColumnData data;
            SessionTableManager.Table tableRef;
            #endregion

            #region Constructor
            public TableColumn(
                string tableOwner,
                string tableName,
                string columnName,
                OracleDbType? dataType,
                int dataLength,
                int? dataPrecision, int? dataScale,
                bool nullable,
                int? defaultLength,
                object dataDefault,
                int? charLength,
                bool? charUsed)
            {
                data = new ColumnData(tableOwner, tableName, columnName, dataType, dataLength,
                    dataPrecision, dataScale, nullable, defaultLength, dataDefault,
                    charLength, charUsed);

                this.tableRef = null;
            }
            #endregion

            #region Properties
            public SessionTableManager.Table Table
            {
                get { return tableRef; }
                set { tableRef = value; }
            }
            public string TableOwner
            {
                get { return data.tableOwner; }
                set { data.tableOwner = value; }
            }
            public string ColumnName
            {
                get { return data.columnName; }
                set { data.columnName = value; }
            }
            public OracleDbType? DataType
            {
                get { return data.dataType; }
                set { data.dataType = value; }
            }
            public int DataLength
            {
                get { return data.dataLength; }
                set { data.dataLength = value; }
            }
            public int? DataPrecision
            {
                get { return data.dataPrecision; }
                set { data.dataPrecision = value; }
            }
            public int? DataScale
            {
                get { return data.dataScale; }
                set { data.dataScale = value; }
            }
            public bool Nullable
            {
                get { return data.nullable; }
                set { data.nullable = value; }
            }
            public int? DefaultLength
            {
                get { return data.defaultLength; }
                set { data.defaultLength = value; }
            }
            public object DataDefault
            {
                get { return data.dataDefault; }
                set { data.dataDefault = value; }
            }
            public int? CharLength
            {
                get { return data.charLength; }
                set { data.charLength = value; }
            }
            public bool? CharUsed
            {
                get { return data.charUsed; }
                set { data.charUsed = value; }
            }
            #endregion

            #region Column data struct
            public struct ColumnData
            {
                #region Members
                public string tableOwner;
                public string tableName;
                public string columnName;
                public OracleDbType? dataType;
                public int dataLength;
                public int? dataPrecision, dataScale;
                public bool nullable;
                public int? defaultLength;
                public object dataDefault;
                public int? charLength;
                public bool? charUsed;
                #endregion

                #region Constructor
                public ColumnData(
                    string tableOwner,
                    string tableName,
                    string columnName,
                    OracleDbType? dataType,
                    int dataLength,
                    int? dataPrecision, int? dataScale,
                    bool nullable,
                    int? defaultLength,
                    object dataDefault,
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

            ColumnManagerSession manager;
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
