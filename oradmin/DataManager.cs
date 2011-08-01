using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

namespace oradmin
{
    class DataManager
    {
        #region Members
        SessionManager.Session session;
        OracleConnection conn;

        DataSet ds = new DataSet();
        #endregion

        #region Constructor
        public DataManager(SessionManager.Session session)
        {
            if (session == null)
                throw new ArgumentNullException("Session");

            this.session = session;
            this.conn = this.session.Connection;
        }
        #endregion

        #region Public interface
        public LocalDataManager CreateManager()
        {
            return new LocalDataManager(this.session, this);
        }
        #endregion

        #region Local data manager class
        public class LocalDataManager
        {
            #region Members
            SessionManager.Session session;
            OracleConnection conn;
            DataManager manager;
            SessionTableManager.Table table;

            DataTable dataTable;
            OracleDataAdapter dataAdapter;
            #endregion

            #region Constructor
            public LocalDataManager(SessionManager.Session session, DataManager manager,
                SessionTableManager.Table table)
            {
                if (session == null || manager == null)
                    throw new ArgumentNullException("Session or manager");

                this.session = session;
                this.manager = manager;
                this.conn = this.session.Connection;
                this.table = table;
            }
            #endregion
        }
        #endregion
    }
}
