using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace oradmin
{
    public class GrantedSysPrivilege
    {
        #region Members
        string grantee;
        ESysPrivilege privilege;
        bool admin;

        bool directGrant;
        #endregion

        #region Constructor
        public GrantedSysPrivilege(string grantee, ESysPrivilege privilege,
                                bool admin, bool directGrant)
        {
            this.grantee = grantee;
            this.privilege = privilege;
            this.admin = admin;
            this.directGrant = directGrant;
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
            get { return directGrant; }
        }
        #endregion

        #region Public interface
        public GrantedSysPrivilege CreateGrant(UserRole userRole, bool adminOption)
        {
            return new GrantedSysPrivilege(userRole.Name, privilege, adminOption, true);
        }
        #endregion
    }
}
