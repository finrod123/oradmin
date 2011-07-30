using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

namespace oradmin
{
    public class QuotasManager
    {
        #region Members
        #region SQL SELECTS
        public static const string DBA_TS_QUOTAS_SELECT = @"
            SELECT
                tablespace_name, username,
                blocks, max_blocks, bytes, max_bytes
            FROM
                DBA_TS_QUOTAS";
        public static const string USER_TS_QUOTAS_SELECT = @"
            SELECT
                tablespace_name, username,
                blocks, max_blocks, bytes, max_bytes
            FROM
                USER_TS_QUOTAS";
        #endregion
        SessionManager.Session session;
        OracleConnection conn;
        List<Quota> quotas = new List<Quota>();
        #endregion

        #region Constructor
        public QuotasManager(SessionManager.Session session)
        {
            if (session == null)
                throw new ArgumentNullException("Session");

            this.session = session;
            this.conn = session.Connection;
        }
        #endregion

        #region Public interface
        public void RefreshQuotas()
        {
            OracleCommand cmd = new OracleCommand(DBA_TS_QUOTAS_SELECT, conn);
            OracleDataReader odr = cmd.ExecuteReader();

            if (odr.HasRows)
                quotas.Clear();

            while (odr.Read())
            {
                Quota quota = LoadQuota(odr);
                quotas.Add(quota);
            }
            
            // notify users
            OnAllQuotasRefreshed();
        }
        public void RefreshQuotas(ReadOnlyCollection<UserManager.User> users)
        {
            // generate a dynamic SQL SELECT based on a user selection
            OracleCommand cmd = new OracleCommand(generateManyUsersSelectSql(users), conn);
            OracleDataReader odr = cmd.ExecuteReader();

            if (odr.HasRows)
                // purge old quotas information
                clearQuotasOfUsers((from user in users select user.Name) as StringCollection);

            while (odr.Read())
            {
                Quota quota = LoadQuota(odr);
                quotas.Add(quota);
            }

            // notify affected users
            OnQuotasRefreshed(users);
        }
        #endregion

        #region Public static interface
        public static Quota LoadQuota(OracleDataReader odr)
        {
            string tablespaceName = odr.GetString(odr.GetOrdinal("tablespace_name"));
            string userName = odr.GetString(odr.GetOrdinal("username"));
            decimal? bytes = null,
                     maxBytes = null,
                     maxBlocks = null;
            
            decimal blocks = odr.GetDecimal(odr.GetOrdinal("blocks"));

            if (!odr.IsDBNull(odr.GetOrdinal("bytes")))
                bytes = odr.GetDecimal(odr.GetOrdinal("bytes"));

            if (!odr.IsDBNull(odr.GetOrdinal("max_bytes")))
                maxBytes = odr.GetDecimal(odr.GetOrdinal("max_bytes"));

            if (!odr.IsDBNull(odr.GetOrdinal("max_blocks")))
                maxBlocks = odr.GetDecimal(odr.GetOrdinal("max_blocks"));

            return new Quota(tablespaceName, userName, blocks, maxBlocks, bytes, maxBytes);
        }
        #endregion

        #region Public events
        public event AllQuotasRefreshedHandler AllQuotasRefreshed;
        public event QuotasRefreshedHandler QuotasRefreshed;
        #endregion

        #region Helper methods
        private void OnAllQuotasRefreshed()
        {
            if (AllQuotasRefreshed != null)
            {
                AllQuotasRefreshed();
            }
        }
        private void OnQuotasRefreshed(ReadOnlyCollection<UserManager.User> affected)
        {
            if (QuotasRefreshed != null)
            {
                QuotasRefreshed(affected);
            }
        }
        private string generateManyUsersSelectSql(ReadOnlyCollection<UserManager.User> users)
        {
            string[] whereClauses =
                (string[])(from user in users
                 select string.Format("username = '{0}'", user.Name));

            return string.Format("{0} WHERE {1}", DBA_TS_QUOTAS_SELECT, string.Join(" or \n", whereClauses)); 
        }
        private void clearQuotasOfUsers(StringCollection userNames)
        {
            // filter out the specified users' quotas
            quotas = quotas.SkipWhile((quota) => userNames.Contains(quota.UserName)) as List<Quota>;
        }
        #endregion
    }

    public class Quota
    {
        #region Members
        string tablespaceName;
        string username;
        decimal blocks;
        decimal? maxBlocks;
        decimal? bytes, maxBytes;
        #endregion

        #region Constructor
        public Quota(
            string tablespaceName,
            string username,
            decimal blocks,
            decimal? maxBlocks,
            decimal? bytes, decimal? maxBytes)
        {
            this.tablespaceName = tablespaceName;
            this.username = username;
            this.blocks = blocks;
            this.maxBlocks = maxBlocks;
            this.bytes = bytes;
            this.maxBytes = maxBytes;
        }
        #endregion

        #region Properties
        public string TablespaceName
        {
            get { return tablespaceName; }
        }
        public string UserName
        {
            get { return username; }
        }
        public decimal Blocks
        {
            get { return blocks; }
        }
        public decimal? MaxBlocks
        {
            get { return maxBlocks; }
        }
        public decimal? Bytes
        {
            get { return bytes; }
        }
        public decimal? MaxBytes
        {
            get { return maxBytes; }
        }
        #endregion
    }

    public delegate void AllQuotasRefreshedHandler();
    public delegate void QuotasRefreshedHandler(ReadOnlyCollection<UserManager.User> affected);
}
