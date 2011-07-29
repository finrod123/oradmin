using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// ---TODO: typ hodnoty resource limitu, dve tridy pro dva typy?,
//    enum pro nazvy resource limitu, doplnit enum converters!!!---
namespace oradmin
{
    public class ResourceLimitManager
    {

    }

    public class ResourceLimit
    {
        #region Members
        string profile;
        EResourceType resourceType;
        string resourceName;
        
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
    }
    public enum ESizeClauseSuffix
    {
        K,
        M
    }

    public class KernelResourceLimitValue
    {
        #region Members
        protected bool isUnlimited;
        protected bool isDefault;
        protected decimal? value;
        #endregion

        #region Constructor
        public KernelResourceLimitValue(decimal? value, bool isUnlimited, bool isDefault)
        {
            this.value = value;
            this.isUnlimited = isUnlimited;
            this.isDefault = isDefault;
        }
        #endregion
    }
    public class KernelResourceLimitSizeClauseBasedValue : KernelResourceLimitValue
    {
        #region Members
        protected ESizeClauseSuffix unit;
        #endregion
    }

    public enum EResourceType
    {
        Kernel,
        Password
    }
    public enum EKernelResourceName
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
    public enum EPasswordResourceName
    {
        FailedLoginAttempts,
        PasswordLifeTime,
        PasswordReuseTime,
        PasswordReuseMax,
        PasswordLockTime,
        PasswordGraceTime,
        PasswordVerifyFunction
    }
}
