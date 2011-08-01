using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

// ---TODO:
//    doplnit enum converters!!!
//    dodelat password function limits classes
namespace oradmin
{
    public delegate void ResourceLimitsRefreshedHandler();

    public class ResourceLimitManager
    {
        #region Members
        #region SQL SELECTS
        public static const string DBA_PROFILE_SELECT = @"
            SELECT
                profile, resource_name, resource_type, limit
            FROM
                DBA_PROFILES";
        public static const string DBA_PROFILE_PROFILE_SELECT = @"
            SELECT
                profile, resource_name, resource_type, limit
            FROM
                DBA_PROFILES
            WHERE
                profile = :profile";
        public static const string USER_RESOURCE_LIMITS_SELECT = @"
            SELECT
                resource_name, limit
            FROM
                USER_RESOURCE_LIMITS";
        public static const string USER_PASSWORD_LIMITS_SELECT = @"
            SELECT
                resource_name, limit
            FROM
                USER_PASSWORD_LIMITS";
        #endregion
        SessionManager.Session session;
        OracleConnection conn;

        List<UserLimit> limits = new List<UserLimit>();
        #endregion

        #region Constructor
        public ResourceLimitManager(SessionManager.Session session)
        {
            if (session == null)
                throw new ArgumentNullException("Session");

            this.session = session;
            this.conn = this.session.Connection;
        }
        #endregion

        #region Pubic interface
        public void RefreshResourceLimits()
        {
            OracleCommand cmd = new OracleCommand(DBA_PROFILE_SELECT, conn);
            OracleDataReader odr = cmd.ExecuteReader();

            // purge limits
            if (odr.HasRows)
                limits.Clear();

            while (odr.Read())
            {

            }
        }
        public bool RefreshResourceLimits(string profile)
        {
            OracleCommand cmd = new OracleCommand(DBA_PROFILE_SELECT, conn);
            OracleDataReader odr = cmd.ExecuteReader();

            // purge limits
            if (!odr.HasRows)
                return false;

            limits.Clear();

            while (odr.Read())
            {

            }
        }
        #endregion

        #region Events
        public event ResourceLimitsRefreshedHandler ResourceLimitsRefreshed;
        #endregion

        #region Helper methods
        private void OnResourceLimitsRefreshed()
        {
            if (ResourceLimitsRefreshed != null)
            {
                ResourceLimitsRefreshed();
            }
        }
        #endregion

        #region Public static interface
        public static UserLimit LoadLimit(OracleDataReader odr)
        {

        }
        #endregion
    }

    public abstract class UserLimit
    {
        #region Members
        protected string profile;
        protected EResourceType resourceType;
        #endregion

        #region Constructor
        public UserLimit(string profile, EResourceType resourceType)
        {
            this.profile = profile;
            this.resourceType = resourceType;
        }
        #endregion

        #region Properties
        public bool IsProfileAssociated
        {
            get { return !string.IsNullOrEmpty(profile); }
        }
        public string Profile
        {
            get { return profile; }
        }
        public EResourceType ResourceType
        {
            get { return resourceType; }
        }
        #endregion
    }
    public abstract class NumericUserLimit : UserLimit
    {
        #region Members
        protected EKernelLimitName limitName;
        protected ENumericResourceLimitValue value;
        #endregion

        #region Constructor
        public NumericUserLimit(string profile, EKernelLimitName limitName,
                                ENumericResourceLimitValue value) :
            base(profile, EResourceType.Kernel)
        {
            this.limitName = limitName;
            this.value = value;
        }
        #endregion

        #region Properties
        public EKernelLimitName LimitName
        {
            get { return limitName; }
        }
        public ENumericResourceLimitValue Value
        {
            get { return this.value; }
        }
        #endregion
    }
    public class KernelResourceLimit : NumericUserLimit
    {
        #region Members
        decimal? numericValue;
        #endregion

        #region Constructor
        public KernelResourceLimit(string profile,
                                   EKernelLimitName limitName,
                                   ENumericResourceLimitValue value,
                                   decimal? numericValue) :
            base(profile, limitName, value)
        {
            this.numericValue = numericValue;
        }
        #endregion

        #region Properties
        public decimal? NumericValue
        {
            get { return numericValue; }
        }
        #endregion
    }
    public class PasswordLimit : UserLimit
    {
        #region Members
        EPasswordLimitName limitName;
        ExpressionPasswordLimitValue value;
        #endregion

        #region Constructor
        public PasswordLimit(string profile,
                             EPasswordLimitName limitName,
                             ExpressionPasswordLimitValue value):
            base(profile, EResourceType.Password)
        {
            this.limitName = limitName;
            this.value = value;
        }
        #endregion

        #region Properties
        public EPasswordLimitName LimitName
        {
            get { return limitName; }
        }
        public ExpressionPasswordLimitValue Value
        {
            get { return value; }
        }
        #endregion

    }
    public struct NumericResourceLimitValue
    {
        #region Members
        protected ENumericResourceLimitValue value;
        decimal? numericValue;
        #endregion

        #region Constructor
        public NumericResourceLimitValue(ENumericResourceLimitValue value, decimal? numericValue)
        {
            this.value = value;
            this.numericValue = numericValue;
        }
        #endregion

        #region Properties
		public ENumericResourceLimitValue Value
        {
            get { return value;}
        }
        public decimal? NumericValue
        {
            get { return numericValue;}
        }
	    #endregion
    }
    public struct ExpressionPasswordLimitValue
    {
        #region Members
        EPasswordLimitValue value;
        string expression;
	    #endregion

        #region Constructor
        public ExpressionPasswordLimitValue(EPasswordLimitValue value, string expression)
        {
            this.value = value;
            this.expression = expression;
        }
        #endregion

        #region Properties
        public EPasswordLimitValue Value
        {
            get { return this.value; }
        }
        public string Expression
        {
            get { return this.expression; }
        }
        #endregion
    }

    public class PrivateSgaKernelResourceLimit : NumericUserLimit
    {
        #region Members
        SizeClause numericValue;
        #endregion

        #region Constructor
        public PrivateSgaKernelResourceLimit(string profile,
                                             ENumericResourceLimitValue value,
                                             decimal? numericValue,
                                             ESizeClauseSuffix unit) :
            base(profile, EKernelLimitName.PrivateSga, value)
        {
            this.numericValue.value = numericValue;
            this.numericValue.unit = unit;
        }
        #endregion

        #region Properties
        public SizeClause NumericValue
        {
            get { return this.numericValue; }
        }
        #endregion
    }

    public struct SizeClause
    {
        #region Members
        public decimal? value;
        public ESizeClauseSuffix unit;
        #endregion

        #region Constructor
        public SizeClause(decimal? value, ESizeClauseSuffix unit)
        {
            this.value = value;
            this.unit = unit;
        }
        #endregion
    }




    #region Resource enums
    public enum EResourceType
    {
        Kernel,
        Password
    }
    public enum EKernelLimitName
    {
        SessionsPerUser,
        CpuPerSession,
        CpuPerCall,
        ConnectTime,
        IdleTime,
        LogicalReadsPerSession,
        LogicalReadsPerCall,
        CompositeLimit,
        PrivateSga
    }

    public enum EPasswordLimitName
    {
        FailedLoginAttempts,
        PasswordLifeTime,
        PasswordReuseTime,
        PasswordReuseMax,
        PasswordLockTime,
        PasswordGraceTime,
        PasswordVerifyFunction
    }

    public enum ENumericResourceLimitValue
    {
        Number,
        Default,
        Unlimited
    }

    public enum EPasswordLimitValue
    {
        Expression,
        Unlimited,
        Default
    }

    public enum EPasswordVerifyFunctionValue
    {
        Function,
        Null,
        Default
    }
    public enum ESizeClauseSuffix
    {
        K,
        M
    }
    #endregion
}
