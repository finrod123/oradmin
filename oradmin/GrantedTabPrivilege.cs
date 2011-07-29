using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace oradmin
{
    public class GrantedTabPrivilege : PrivilegeBase
    {
        #region Members
        string grantee;
        string owner;
        string table_name;
        string grantor;
        bool grantable;
        ETabPrivilege privilege;
        #endregion

        #region Constructor
        public GrantedTabPrivilege(
            string grantee,
            string rootGrantee,
            string owner,
            string table_name,
            string grantor,
            bool grantable,
            ETabPrivilege privilege):
            base(rootGrantee)
        {
            this.grantee = grantee;
            this.owner = owner;
            this.table_name = table_name;
            this.grantor = grantor;
            this.grantable = grantable;
            this.privilege = privilege;
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
