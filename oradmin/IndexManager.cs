using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

//---TODO: index constructor, loading index columns, loading indexes
namespace oradmin
{
    class IndexManager
    {
        #region Members
        SessionManager.Session session;
        OracleConnection conn;

        List<Index> indexes = new List<Index>();
        #endregion

        #region Constructor
        public IndexManager(SessionManager.Session session)
        {
            if (session == null)
                throw new ArgumentNullException("Session");

            this.session = session;
            this.conn = this.session.Connection;
        }
        #endregion

        #region Index class
        public class Index
        {
            #region Members
            string owner;
            string tableOwner;
            string tableName;
            TableManager.Table tableRef;
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
                TableManager.Table tableRef,
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
