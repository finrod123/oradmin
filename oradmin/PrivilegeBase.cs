using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/* TODO: doplnit *data tridu pro dedeni v ramci trid privilegii
 * 
 * */
namespace oradmin
{
    public abstract class PrivilegeBase
    {
        #region Members
        protected PrivilegeBaseData data;
        protected UserRole granteeRef;
        #endregion

        #region Constructor
        public PrivilegeBase() { }
        #endregion

        #region Properties
        public string Grantee
        {
            get { return data.grantee; }
        }
        public UserRole GranteeRef
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
