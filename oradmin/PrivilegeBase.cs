using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace oradmin
{
    public abstract class PrivilegeBase
    {
        #region Members
        protected string rootGrantee;
        #endregion

        #region Constructor
        public PrivilegeBase(string rootGrantee)
        {
            this.rootGrantee = rootGrantee;
        }
        #endregion

        #region Properties
        public string RootGrantee
        {
            get { return rootGrantee; }
        }
        #endregion
    }
}
