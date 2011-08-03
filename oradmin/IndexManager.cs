using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

//---TODO: index constructor, loading index columns, loading indexes
namespace oradmin
{
    public delegate void AllIndexesRefreshedHandler();

    class SessionIndexManager
    {
        #region Members
        #region SQL SELECTS
        public static const string ALL_INDEXES_SELECT = @"
            SELECT
                owner, index_name, table_owner, table_name,
                index_type, uniqueness, compression, prefix_length,
                tablespace_name, partitioned, dropped,
                status, funcidx_status
            FROM
                ALL_INDEXES";
        public static const string ALL_INDEXES_SELECT_SCHEMA = @"
            SELECT
                owner, index_name, table_owner, table_name,
                index_type, uniqueness, compression, prefix_length,
                tablespace_name, partitioned, dropped,
                status, funcidx_status
            FROM
                ALL_INDEXES
            WHERE
                table_owner = :table_owner";
        public static const string ALL_INDEXES_SELECT_TABLE = @"
            SELECT
                owner, index_name, table_owner, table_name,
                index_type, uniqueness, compression, prefix_length,
                tablespace_name, partitioned, dropped,
                status, funcidx_status
            FROM
                ALL_INDEXES
            WHERE
                table_owner = :table_owner and
                table_name = :table_name";
        #endregion
        SessionManager.Session session;
        OracleConnection conn;

        // managers
        SessionIndexColumnManager indexColumnManager;

        List<Index> indexes = new List<Index>();
        #endregion

        #region Constructor
        public SessionIndexManager(SessionManager.Session session)
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
            OracleCommand cmd = new OracleCommand(ALL_INDEXES_SELECT, conn);
            OracleDataReader odr = cmd.ExecuteReader();

            if (!odr.HasRows)
                return;

            while (odr.Read())
            {

            }
        }
        public bool Refresh(string schema)
        {
            
        }
        public bool Refresh(SessionTableManager.Table table)
        {

        }
        #endregion

        #region Index class
        public class Index
        {
            #region Members
            string owner;
            string tableOwner;
            string tableName;
            SessionTableManager.Table tableRef;
            string indexName;
            EIndexType? indexType;
            EIndexUniqueness? indexUniqueness;
            bool? compression;
            int? prefixLength;
            string tablespaceName;
            EIndexStatus? indexStatus;
            bool? partitioned;
            bool? dropped;

            List<IndexColumn> columns = new List<IndexColumn>();
            #endregion

            #region Constructor
            public Index(
                string owner,
                string tableOwner,
                string tableName,
                SessionTableManager.Table tableRef,
                string indexName,
                EIndexType? indexType,
                EIndexUniqueness? indexUniqueness,
                bool? compression,
                int? prefixLength,
                string tablespaceName,
                EIndexStatus? indexStatus,
                bool? partitioned,
                bool? dropped)
            {

            }
            #endregion

            #region Properties
            public string Owner
            {
                get { return this.owner; }
            }
            #endregion
        }
        #endregion

        
    }

    public enum EIndexType
    {
        Normal,
        Bitmap,
        FunctionBasedNormal,
        FunctionBasedBitmap,
        Domain
    }
    public enum EIndexUniqueness
    {
        Unique,
        NonUnique
    }
    public enum EIndexStatus
    {
        Valid,
        Unusable
    }
    public enum EFuncIdxStatus
    {
        Enabled,
        Disabled
    }
}
