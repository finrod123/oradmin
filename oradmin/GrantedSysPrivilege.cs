using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace oradmin
{
    public class GrantedSysPrivilege  : PrivilegeBase
    {
        #region Members
        string grantee;
        UserRole granteeRef;
        ESysPrivilege privilege;
        bool adminOption;
        bool directGrant;
        #endregion

        #region Constructor
        public GrantedSysPrivilege(string grantee,
                                   ESysPrivilege privilege,
                                   bool directGrant,
                                   bool adminOption) :
            base(grantee)
        {
            this.grantee = grantee;
            this.privilege = privilege;
            this.adminOption = adminOption;
            this.directGrant = directGrant;
        }
        #endregion

        #region Properties
        public ESysPrivilege Privilege
        {
            get { return this.privilege; }
        }
        public bool AdminOption
        {
            get { return this.adminOption; }
        }
        public bool DirectGrant
        {
            get { return directGrant; }
            set { directGrant = value; }
        }
        #endregion

        #region Public interface
        public GrantedSysPrivilege CreateGrant(UserRole userRole, bool adminOption)
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
    }
}
