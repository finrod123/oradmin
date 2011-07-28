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
        bool active;

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
        public RoleGrant(RoleGrant grant, string grantee, bool adminOption)
        {
            if(grant == null)
                throw new ArgumentNullException("Grant");

            this.grantee = grantee;
            this.grantedRole = grant.GrantedRole;
            this.adminOption = adminOption;
            this.weakReference = false;
            this.roleReference = grant.Role;
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
        public bool Active
        {
            get { return active; }
            set { active = value; }
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
