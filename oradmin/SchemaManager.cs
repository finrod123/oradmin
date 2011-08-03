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
    class SessionSchemaManager
    {
        #region Members
        SessionManager.Session session;
        OracleConnection conn;
        // managers
        SessionTableManager tableManager;
        SessionColumnManager columnManager;
        SessionConstraintManager constraintManager;
        SessionIndexManager indexManager;
        SessionTriggerManager triggerManager;

        SessionViewManager viewManager;
        //---TODO: add PL/SQL managers
        SessionSynonymManager synonymManager;
        SessionSequenceManager sequenceManager;
        
        #endregion

        #region Constructor
        public SessionSchemaManager(SessionManager.Session session)
        {
            if (session == null)
                throw new ArgumentNullException("Session");

            this.session = session;
            this.conn = session.Connection;

            this.tableManager = new SessionTableManager(this.session);
            this.columnManager = new SessionColumnManager(this.session);
            this.constraintManager = new SessionConstraintManager(this.session);
            this.indexManager = new SessionIndexManager(this.session);
            this.triggerManager = new SessionTriggerManager();

            this.viewManager = new SessionViewManager(this.session);
            this.sequenceManager = new SessionSequenceManager();
            this.synonymManager = new SessionSynonymManager();
        }
        #endregion

        #region Properties
        public SessionTableManager TableManager
        {
            get { return tableManager; }
        }
        public SessionColumnManager ColumnManager
        {
            get { return columnManager; }
        }
        public SessionConstraintManager ConstraintManager
        {
            get { return constraintManager; }
        }
        public SessionIndexManager IndexManager
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
