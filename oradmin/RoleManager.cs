using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

namespace oradmin
{
    class RoleManager
    {
        #region Members
        OracleConnection conn;
        #endregion

        #region Constructor

        public RoleManager(OracleConnection conn)
        {
            if (conn == null)
                throw new ArgumentNullException("Oracle connection");

            this.conn = conn;
        }

        #endregion
    }
}
