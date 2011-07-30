using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

//---TODO: local schema manager: to define it or not to?
//          - tasks, loading data, definition (sharing base class with session
//            level schema manager?
//          -> implies definition of schema-level object managers!!!
namespace oradmin
{
    class SchemaManager
    {
        #region Members
        SessionManager.Session session;
        TableManager tableManager;

        ColumnManager columnManager;
        ConstraintManager constraintManager;
        IndexManager indexManager;

        OracleConnection conn;
        #endregion

        #region Constructor
        public SchemaManager(SessionManager.Session session)
        {
            if (session == null)
                throw new ArgumentNullException("Session");

            this.session = session;
            this.conn = session.Connection;
            this.tableManager = new TableManager(session);
            this.columnManager = new ColumnManager();
            this.constraintManager = new ConstraintManager();
            this.indexManager = new IndexManager();
        }
        #endregion

        #region Properties
        public TableManager TableManager
        {
            get { return tableManager; }
        }
        public ColumnManager ColumnManager
        {
            get { return columnManager; }
        }
        public ConstraintManager ConstraintManager
        {
            get { return constraintManager; }
        }
        public IndexManager IndexManager
        {
            get { return indexManager; }
        }
        #endregion

        #region Local schema manager class
        public class LocalSchemaManager
        {

        }
        #endregion
    }


}
