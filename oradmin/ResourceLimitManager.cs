using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

// ---TODO: typ hodnoty resource limitu,
//    doplnit enum converters!!!
//    dodelat privatesga a password function limits classes
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

    public class KernelResourceLimit : UserLimit
    {
        #region Members
        EKernelLimitName limitName;
        protected NumericResourceLimitValue value;
        #endregion

        #region Constructor
        public KernelResourceLimit(string profile,
                                   EKernelLimitName limitName,
                                   NumericResourceLimitValue value) :
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
        public NumericResourceLimitValue Value
        {
            get { return value; }
        }
        #endregion
    }

    public class PasswordLimit : UserLimit
    {
        #region Members
        EPasswordLimitName limitName;
        #endregion

        #region Constructor
        public PasswordLimit(string profile,
                             EPasswordLimitName limitName,
                             NumericResourceLimitValue value)
            base(profile, EResourceType.Password)
        {
            this.limitName = limitName;
        }
        #endregion

        #region Properties
        public EPasswordLimitName LimitName
        {
            get { return limitName; }
        }
        #endregion

    }

    public class NumericResourceLimitValue
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
    }


    public class PrivateSgaKernelResourceLimit : KernelResourceLimit
    {
        #region Members
        SizeClause? numericValue;
        #endregion

        #region Constructor
        public PrivateSgaKernelResourceLimit(string profile,
                                             ENumericResourceLimitValue value,
                                             SizeClause numericValue) :
            base(profile, EKernelLimitName.PrivateSga)
        {
            this.
        }
        #endregion
    }

    public struct SizeClause
    {
        #region Members
        int value;
        ESizeClauseSuffix unit;
        #endregion

        #region Constructor
        public SizeClause(int value, ESizeClauseSuffix unit)
        {
            this.value = value;
            this.unit = unit;
        }
        #endregion

        #region Properties
        public decimal Value
        {
            get { return value; }
        }
        public ESizeClauseSuffix Unit
        {
            get { return unit; }
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
