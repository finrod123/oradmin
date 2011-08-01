using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace oradmin
{
    public class GrantedColPrivilege : TableBasedPrivilegeGrant
    {
        #region Members
        GrantedColPrivilegeData data;
        #endregion

        #region Constructor
        public GrantedColPrivilege(
            string grantee,
            string owner,
            string tableName,
            string columnName,
            string grantor,
            bool grantable,
            EColPrivilege privilege)
        {
            this.data = new GrantedColPrivilegeData(grantee, owner, tableName, columnName,
                grantor, grantable, privilege);
        }
        #endregion

        #region Properties
        public string ColumnName
        {
            get { return this.data.columnName; }
        }
        public EColPrivilege Privilege
        {
            get { return this.data.privilege; }
        }
        #endregion

        #region GrantedColPrivilege class data
        public class GrantedColPrivilegeData  :TableBasedPrivilegeGrantData
        {
            #region Members
            public EColPrivilege privilege;
            public string columnName;
            #endregion

            #region Constructor
            public GrantedColPrivilegeData(
                string grantee,
                string owner,
                string tableName,
                string columnName,
                string grantor,
                bool grantable,
                EColPrivilege privilege) :
                base(grantee, owner, tableName, grantor, grantable)
            {
                this.columnName = columnName;
                this.privilege = privilege;
            }
            #endregion
        }
        #endregion
    }

    public enum EColPrivilege
    {
        Insert,
        Update,
        References
    }
}
