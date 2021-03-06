﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

namespace oradmin
{
    class RoleDependencyManager
    {
        #region Members
        OracleConnection conn;
        #endregion

        #region Constructor

        public RoleDependencyManager(OracleConnection conn)
        {
            if (conn == null)
                throw new ArgumentNullException("Oracle connection");

            this.conn = conn;
        }

        #endregion
    }
}
