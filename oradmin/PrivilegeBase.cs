using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace oradmin
{
    public abstract class PrivilegeBase
    {
        #region Members
        protected string grantee;
        protected UserRole granteeRef;
        #endregion

        #region Constructor
        public PrivilegeBase(string grantee)
        {
            this.grantee = grantee;
        }
        #endregion

        #region Properties
        public string Grantee
        {
            get { return grantee; }
        }
        #endregion
    }
}
