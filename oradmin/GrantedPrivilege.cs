using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace oradmin
{
    public class GrantedPrivilege
    {
        #region Members

        string grantee;
        EPrivilege privilege;
        bool admin;

        string grantor;
        UserManager.User grantorRef;
        bool directGrant;

        #endregion

        #region Constructor

        public GrantedPrivilege(string grantee, EPrivilege privilege,
                                bool admin, bool directGrant) :
            this(grantee, privilege, admin, directGrant, string.Empty, null)
        { }

        public GrantedPrivilege(string grantee, EPrivilege privilege,
                                string grantor, UserManager.User grantorRef) :
            this(grantee, privilege, false, false, grantor, grantorRef)
        { }

        private GrantedPrivilege(string grantee, EPrivilege privilege,
                                bool admin, bool directGrant,
                                string grantor, UserManager.User grantorRef)
        {
            this.grantee = grantee;
            this.privilege = privilege;
            this.admin = admin;
            this.directGrant = directGrant;
            this.grantor = grantor;
            this.grantorRef = grantorRef;
        }
        #endregion

        #region Properties

        public string Grantee
        {
            get { return grantee; }
        }
        public string Grantor
        {
            get { return grantor; }
        }
        public EPrivilege Privilege
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
    }
}
