﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

namespace oradmin
{
    public abstract class PrivilegeHolderEntityTablePrivilegeManager
    {
        #region Members
        // kolekce grantu
        ObservableCollection<GrantedSysPrivilege> grants =
            new ObservableCollection<GrantedSysPrivilege>();
        #endregion

        #region Constructor
        public PrivilegeHolderEntityTablePrivilegeManager()
        {

        }
        #endregion
    } 
}