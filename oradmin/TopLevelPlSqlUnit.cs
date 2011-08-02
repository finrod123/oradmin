using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

namespace oradmin
{
    #region Procedure class
    public class PlSqlUnit : IEditableObject
    {
        #region Members
        SessionManager.Session session;
        OracleConnection conn;

        PlSqlUnitData data;
        #endregion

        #region Constructor
        public PlSqlUnit(PlSqlUnitData data, SessionManager.Session session)
        {
            if (session == null)
                throw new ArgumentNullException("Session");

            this.session = session;
            this.conn = this.session.Connection;
        }
        #endregion

        #region Properties
        public string Owner
        {
            get { return this.data.owner; }
            set { this.data.owner = value; }
        }
        public string ObjectName
        {
            get { return this.data.objectName; }
            set { this.data.objectName = value; }
        }
        public string ProcedureName
        {
            get { return this.data.procedureName; }
            set { this.data.procedureName = value; }
        }
        public bool? Parallel
        {
            get { return this.data.parallel; }
            set { this.data.parallel = value; }
        }
        #endregion

        #region IEditableObject Members

        public void BeginEdit()
        {
            throw new NotImplementedException();
        }

        public void CancelEdit()
        {
            throw new NotImplementedException();
        }

        public void EndEdit()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
    #endregion

    #region Procedure class data
    public struct PlSqlUnitData
    {
        #region Members
        public string owner;
        public string objectName;
        public string procedureName;
        public bool? parallel;
        #endregion

        #region Constructor
        public PlSqlUnitData(
            string owner,
            string objectName,
            string procedureName,
            bool? parallel)
        {
            this.owner = owner;
            this.objectName = objectName;
            this.procedureName = procedureName;
            this.parallel = parallel;
        }
        #endregion
    }
    #endregion
}
