﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/* TODO: upravit dedici tridy, aby pouzivaly VZDY zdedeny datovy kontejner "data"
 * 
 * */
namespace oradmin
{
    public abstract class PrivilegeBase
    {
        #region Members
        protected PrivilegeBaseData data;
        protected PrivilegeHolderEntity granteeRef;
        #endregion

        #region Constructor
        public PrivilegeBase() { }
        #endregion

        #region Properties
        public string Grantee
        {
            get { return data.grantee; }
        }
        public PrivilegeHolderEntity GranteeRef
        {
            get { return this.granteeRef; }
            set { this.granteeRef = value; }
        }
        #endregion

        #region PrivilegeBase data class
        public abstract class PrivilegeBaseData
        {
            #region Members
            public string grantee;
            #endregion

            #region MyRegion
            public PrivilegeBaseData(string grantee)
            {
                this.grantee = grantee;
            }
            #endregion
        }
        #endregion
    }
}
