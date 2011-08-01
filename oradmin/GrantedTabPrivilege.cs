using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace oradmin
{
    //---TODO: enum converters
    public class GrantedTabPrivilege : TableBasedPrivilegeGrant
    {
        #region Members
        GrantedTabPrivilegeData data;
        #endregion

        #region Constructor
        public GrantedTabPrivilege(
            string grantee,
            string owner,
            string tableName,
            string grantor,
            bool grantable,
            ETabPrivilege privilege)
        {
            this.data = new GrantedTabPrivilegeData(grantee, owner, tableName,
                grantor, grantable, privilege);
        }
        #endregion

        #region Properties
        public ETabPrivilege Privilege
        {
            get { return this.data.privilege; }
        }
        #endregion

        #region Privileges data class
        public class GrantedTabPrivilegeData : TableBasedPrivilegeGrant.TableBasedPrivilegeGrantData
        {
            #region Members
            public ETabPrivilege privilege;
            #endregion

            #region Constructor
            public GrantedTabPrivilegeData(
                string grantee,
                string owner,
                string tableName,
                string grantor,
                bool grantable,
                ETabPrivilege privilege):
                base(grantee, owner, tableName, grantor, grantable)
                
            {
                this.privilege = privilege;
            }
            #endregion
        }
        #endregion
    }

    public enum ETabPrivilege
    {
        // table privileges
        Select,
        Update,
        Delete,
        Insert,
        Alter,
        Debug,
        Flashback,
        QueryRewrite,
        OnCommitRefresh,
        References,
        // view privileges
        Under,
        // plsql
        Execute,
        // directories
        Read,
        Write,
        // all
        All
    }
}
