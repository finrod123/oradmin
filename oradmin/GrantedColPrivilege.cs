using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace oradmin
{
    public class GrantedColPrivilege : PrivilegeBase
    {
        #region Members
        string grantee;
        string grantor;
        string owner;
        string tableName;
        string columnName;
        bool grantable;
        EColPrivilege privilege;
        #endregion
    }

    public enum EColPrivilege
    {
        Insert,
        Update,
        References
    }
}
