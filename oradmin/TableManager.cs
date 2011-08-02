using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

namespace oradmin
{   
    // --- TODO: refresh - nelze jednoduse purge, je nutny diff merge (add, modify, delete
    //     nonexistent
    //                   - UpdateTable, nacteni dalsich souvisejicich udaju (cols, indexes,...

    using TableLookupKey = Tuple<string, string>;
    using TableLookupDictionary = Dictionary<Tuple<string, string>, SessionTableManager.Table>;
    using LoadedTablesLookupDictionary = Dictionary<Tuple<string, string>, SessionTableManager.Table.TableData>;
    using TableList = List<SessionTableManager.Table>;
    using TableLookupKeyList = List<Tuple<string, string>>;

    public delegate void AllTablesRefreshedHandler();
    
    
    class SessionTableManager
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
        SchemaManagerSession manager;
        ColumnManagerSession columnManager;
        ConstraintManagerSession constraintManager;
        IndexManagerSession indexManager;
        OracleConnection conn;
        // structures to hold tables
        ObservableCollection<Table> tables = new ObservableCollection<Table>();
        ListCollectionView view;
        TableLookupDictionary tablesDict = new TableLookupDictionary();
        #endregion

        #region Constructor
        public SessionTableManager(SessionManager.Session session)
        {
            if (session == null)
                throw new ArgumentNullException("Session");

            this.session = session;
            this.manager = session.SchemaManager;
            this.conn = session.Connection;
            this.columnManager = manager.ColumnManager;
            this.constraintManager = manager.ConstraintManager;
            this.indexManager = manager.IndexManager;

            view = new ListCollectionView(tables);
        }
        #endregion

        #region Public interface
        public void Refresh()
        {
            OracleCommand cmd = new OracleCommand(ALL_TABLES_SELECT, conn);
            OracleDataReader odr = cmd.ExecuteReader();

            if (!odr.HasRows)
                return;

            TableList newTablesList = new List<Table>();
            TableLookupKeyList existingTablesKeys = new List<Tuple<string, string>>();

            while (odr.Read())
            {
                // load a table data struct
                Table.TableData tableData = LoadTableData(odr);
                // get its key
                TableLookupKey key = tableData.Key;
                // try to find it
                Table table;
                if (!tablesDict.TryGetValue(key, out table))
                {
                    newTablesList.Add(new Table(tableData, this.session, this));
                } else
                {
                    // update the table
                    table.Update(tableData);
                    existingTablesKeys.Add(key);
                }
            }

            // remove nonexistent tables
            removeTables(existingTablesKeys);

            // add new tables
            addNewTables(newTablesList);

            // refresh colu
        }

        #endregion

        #region Events
        #endregion

        #region Helper methods
        private void addNewTables(TableList tableDataList)
        {
            tables.AddRange(tableDataList);
        }
        private void removeTables(TableLookupKeyList toDeleteList)
        {
            // get deleted tables keys
            HashSet<TableLookupKey> deletedKeys =
                new HashSet<TableLookupKey>(
                    tablesDict.Keys.Except(toDeleteList));

            // walk tables and clean the associated objects (columns, constraints...)

            // delete old tables
            tables.RemoveAll((table) => (deletedKeys.Contains(table.Key)));
        }
        #endregion

        #region Public static interface
        public static Table.TableData LoadTableData(OracleDataReader odr)
        {
            string owner;
            string tableName;
            string tablespaceName = null;
            bool? compression = null;
            bool? dropped = null;

            owner = odr.GetString(odr.GetOrdinal("owner"));
            tableName = odr.GetString(odr.GetOrdinal("table_name"));

            if (!odr.IsDBNull(odr.GetOrdinal("tablespace_name")))
                tablespaceName = odr.GetString(odr.GetOrdinal("tablespace_name"));

            //---TODO: enum converter!!!
            //if (!odr.IsDBNull(odr.GetOrdinal("compression")))

            StringToBoolConverter strToBoolConverter = new StringToBoolConverter();
            if (!odr.IsDBNull(odr.GetOrdinal("dropped")))
                dropped = (bool)strToBoolConverter.Convert(
                    odr.GetString(odr.GetOrdinal("dropped")),
                    typeof(string), EStringToBoolConverterOption.YesNo, null);

            return new Table.TableData(owner, tableName, tablespaceName, compression, dropped);
        }
        #endregion

        #region Properties
        public ListCollectionView TablesView
        {
            get { return view; }
        }
        #endregion











        #region Table class
        public class Table
        {
            #region Members
            SessionManager.Session session;
            OracleConnection conn;
            SessionTableManager manager;
            DataManager.LocalDataManager dataManager;

            TableData data, copyData;
            #endregion

            #region Constructor
            public Table(
                TableData data,
                SessionManager.Session session,
                SessionTableManager manager)
            {
                if (session == null)
                    throw new ArgumentNullException("Session");

                this.session = session;
                this.conn = this.session.Connection;
                this.manager = manager;
                this.data = data;

                
            }
            #endregion

            #region Properties
            public string Name
            {
                get { return data.tableName; }
            }
            public string Owner
            {
                get { return data.owner; }
            }
            public string TablespaceName
            {
                get { return data.tablespaceName; }
            }
            public bool? Compression
            {
                get { return data.compression; }
            }
            public bool? Dropped
            {
                get { return data.dropped; }
            }
            public TableLookupKey Key
            {
                get { return data.Key; }
            }
            #endregion

            #region Public interface
            public void Update(TableData data)
            {
                this.data = data;
            }
            #endregion

            #region Struct for flat table data
            public struct TableData
            {
                #region Members
                public string owner;
                public string tableName;
                public string tablespaceName;
                public bool? compression;
                public bool? dropped;
                #endregion

                #region Constructor
                public TableData(
                    string owner,
                    string tableName,
                    string tablespaceName,
                    bool? compression,
                    bool? dropped)
                {
                    this.owner = owner;
                    this.tableName = tableName;
                    this.tablespaceName = tablespaceName;
                    this.compression = compression;
                    this.dropped = dropped;
                }
                #endregion

                #region Public interface
                public TableLookupKey Key
                {
                    get { return new TableLookupKey(owner, tableName); }
                }
                #endregion
            }
            #endregion
        }
        #endregion
    }
}
