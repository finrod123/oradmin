using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace oradmin
{
    public class GrantedSysPrivilege  : PrivilegeBase
    {
        #region Members
        GrantedSysPrivilegeData data;
        #endregion

        #region Constructor
        public GrantedSysPrivilege(string grantee,
                                   ESysPrivilege privilege,
                                   bool adminOption)
        {
            this.data = new GrantedSysPrivilegeData(grantee, privilege, adminOption);
        }
        #endregion

        #region Properties
        public ESysPrivilege Privilege
        {
            get { return this.data.privilege; }
        }
        public bool AdminOption
        {
            get { return this.data.adminOption; }
            set { this.data.adminOption = value; }
        }
        #endregion

        #region Public interface
        public GrantedSysPrivilege CreateGrant(PrivilegeHolderEntity userRole, bool adminOption)
        {
            return new GrantedSysPrivilege(userRole.Name, privilege, true, adminOption);
        }
        public bool IsStrongerThan(GrantedSysPrivilege grant)
        {
            if (this.Privilege != grant.Privilege)
                throw new Exception("Incomparable privileges");

            return
                this.AdminOption &&
                !grant.AdminOption;
        }
        #endregion

        #region GrantedSysPrivilege data class
        public class GrantedSysPrivilegeData : PrivilegeBaseData
        {
            #region Members
            public ESysPrivilege privilege;
            public bool adminOption;
            #endregion

            #region Constructor
            public GrantedSysPrivilegeData(string grantee, ESysPrivilege privilege,
                bool adminOption) :
                base(grantee)
            {
                this.privilege = privilege;
                this.adminOption = adminOption;
            }
            #endregion
        }
        #endregion
    }
}
