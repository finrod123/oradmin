using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace oradmin
{
    class GrantedTabPrivilege
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
            string owner,
            string table_name,
            string grantor,
            bool grantable,
            ETabPrivilege privilege)
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
