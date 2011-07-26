using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace oradmin
{
    class RoleGrant
    {
        #region Members

        string grantee;
        string grantedRole;
        RoleManager.Role roleReference;
        bool weakReference;
        bool adminOption;
        bool directGrant;
        bool defaultRole;

        #endregion

        #region Constructor

        public RoleGrant(string grantee, string grantedRole, bool adminOption, bool defaultRole,
                         bool directGrant)
        {
            this.grantee = grantee;
            this.grantedRole = grantedRole;
            this.adminOption = adminOption;
            this.weakReference = true;
            this.roleReference = null;
            this.directGrant = directGrant;
            this.defaultRole = defaultRole;
        }
        public RoleGrant(RoleGrant grant, RoleManager.Role role)
        {
            if(grant == null || role == null)
                throw new ArgumentNullException("Grant or role");

            this.grantee = grant.Grantee;
            this.grantedRole = grant.GrantedRole;
            this.adminOption = false;
            this.weakReference = false;
            this.roleReference = role;
            this.directGrant = false;
            this.defaultRole = false;
        }

        #endregion

        #region Properties

        public string Grantee
        {
            get { return this.grantee; }
        }
        public string GrantedRole
        {
            get { return this.grantedRole; }
        }
        public RoleManager.Role Role
        {
            get { return this.roleReference; }
            set
            {
                this.roleReference = value;
                this.weakReference = value == null;
            }
        }
        public bool WeakReference
        {
            get { return this.weakReference; }
        }
        public bool DirectGrant
        {
            get { return this.directGrant; }
        }
        public bool AdminOption
        {
            get { return this.adminOption; }
        }
        public bool DefaultRole
        {
            get { return defaultRole; }
        }
        #endregion

        #region Public static interface

        public static bool ParseAdminOption(object value)
        {
            if (value == DBNull.Value)
                return false;

            StringToBoolConverter converter = new StringToBoolConverter();
            return (bool)converter.Convert(value, typeof(bool),
                EStringToBoolConverterOption.YesNo, null);
        }
        public static bool ParseDefaultRole(object value)
        {
            if (value == DBNull.Value)
                return false;

            StringToBoolConverter converter = new StringToBoolConverter();
            return (bool)converter.Convert(value, typeof(bool),
                EStringToBoolConverterOption.YesNo, null);
        }

        #endregion
    }
}
