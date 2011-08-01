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
    class SchemaManagerSession
    {
        #region Members
        SessionManager.Session session;
        SessionTableManager tableManager;

        ColumnManagerSession columnManager;
        ConstraintManagerSession constraintManager;
        IndexManagerSession indexManager;

        OracleConnection conn;
        #endregion

        #region Constructor
        public SchemaManagerSession(SessionManager.Session session)
        {
            if (session == null)
                throw new ArgumentNullException("Session");

            this.session = session;
            this.conn = session.Connection;
            this.tableManager = new SessionTableManager(session);
            this.columnManager = new ColumnManagerSession();
            this.constraintManager = new ConstraintManagerSession();
            this.indexManager = new IndexManagerSession();
        }
        #endregion

        #region Properties
        public SessionTableManager TableManager
        {
            get { return tableManager; }
        }
        public ColumnManagerSession ColumnManager
        {
            get { return columnManager; }
        }
        public ConstraintManagerSession ConstraintManager
        {
            get { return constraintManager; }
        }
        public IndexManagerSession IndexManager
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
