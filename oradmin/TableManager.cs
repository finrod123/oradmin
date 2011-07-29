using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

namespace oradmin
{   
    // --- TODO:
    // load tables from database, decide what information to be interested in,
    // how to initialize a table from a database collected information (passing all
    // to a constructor or using property assignment?)
    class TableManager
    {
        #region Members
        SessionManager.Session session;
        SchemaManager manager;
        ColumnManager columnManager;
        ConstraintManager constraintManager;
        IndexManager indexManager;

        OracleConnection conn;
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
        }
        #endregion
    }
}
