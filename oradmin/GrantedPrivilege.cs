using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace oradmin
{
    public class GrantedSysPrivilege : PrivilegeBase
    {
        #region Members
        string grantee;
        ESysPrivilege privilege;
        bool admin;
        #endregion

        #region Constructor
        public GrantedSysPrivilege(string grantee, string rootGrantee, ESysPrivilege privilege,
                                bool admin) :
            base(rootGrantee)
        {
            this.grantee = grantee;
            this.privilege = privilege;
            this.admin = admin;
        }
        #endregion

        #region Properties
        public string Grantee
        {
            get { return grantee; }
        }
        public ESysPrivilege Privilege
        {
            get { return privilege; }
        }
        public bool Admin
        {
            get { return admin; }
        }
        public bool DirectGrant
        {
            get { return grantee == rootGrantee; }
        }
        #endregion

        #region Public interface
        public GrantedSysPrivilege CreateGrant(UserRole userRole, bool adminOption)
        {
            return new GrantedSysPrivilege(userRole.Name, rootGrantee, privilege, adminOption);
        }
        #endregion
    }
}
