using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace oradmin
{
    public abstract class TableBasedPrivilegeGrant : PrivilegeBase
    {
        #region Members
        TableBasedPrivilegeGrantData data;
        #endregion

        #region Constructor
        public TableBasedPrivilegeGrant() { }
        #endregion

        #region Properties
        public string Owner
        {
            get { return this.data.owner; }
        }
        public string TableName
        {
            get { return this.data.tableName; }
        }
        public string Grantor
        {
            get { return this.data.grantor; }
        }
        public bool Grantable
        {
            get { return this.data.grantable; }
            set { this.data.grantable = value; }
        }
        #endregion

        #region TableBasedPrivilegeGrant class data
        public abstract class TableBasedPrivilegeGrantData : PrivilegeBase.PrivilegeBaseData
        {
            #region Members
            public string owner;
            public string tableName;
            public string grantor;
            public bool grantable;
            #endregion

            #region Constructor
            public TableBasedPrivilegeGrantData(
                string grantee,
                string owner,
                string tableName,
                string grantor,
                bool grantable) :
                base(grantee)
            {
                this.owner = owner;
                this.tableName = tableName;
                this.grantor = grantor;
                this.grantable = grantable;
            }
            #endregion
        }
        #endregion
    }
}
