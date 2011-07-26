using System;
using System.Windows.Data;
using System.ComponentModel;
using System.Collections.Generic;

namespace oradmin
{
    #region Value converters

    /// <summary>
    /// Base class for all enum converters
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EnumValueConverter<T> : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!Enum.IsDefined(typeof(T), value))
                return value.ToString();

            EnumToStringConverter<T> converter = EnumConverterMapper.GetConverter(typeof(T)) as EnumToStringConverter<T>;
            return converter.EnumValueToString((T)value, parameter);
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Enum.Parse(typeof(T), value.ToString());
        }

        #endregion
    }

    #region Custom value converters
    [ValueConversion(typeof(EDbaPrivileges), typeof(string))]
    public class EDbaPrivilegesEnumConverter : EnumValueConverter<EDbaPrivileges>
    { }

    [ValueConversion(typeof(ENamingMethod), typeof(string))]
    public class ENamingMethodEnumConverter : EnumValueConverter<ENamingMethod>
    { }

    [ValueConversion(typeof(EServerType), typeof(string))]
    public class EServerTypeEnumConverter : EnumValueConverter<EServerType>
    { }

    [ValueConversion(typeof(EPrivilege), typeof(string))]
    public class EPrivilegeEnumConverter : EnumValueConverter<EPrivilege>
    {
        public override object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string strValue = (string)value;
            EPrivilege enumValue;

            #region Enum conversion branching

            if (strValue.Equals("ADMINISTER ANY SQL TUNING SET"))
                enumValue = EPrivilege.AdministerAnySqlTuningSet;
            else if (strValue.Equals("ADMINISTER DATABASE TRIGGER"))
                enumValue = EPrivilege.AdministerDatabaseTrigger;
            else if (strValue.Equals("ADMINISTER RESOURCE MANAGER"))
                enumValue = EPrivilege.AdministerResourceManager;
            else if (strValue.Equals("ADMINISTER SQL TUNING SET"))
                enumValue = EPrivilege.AdministerSqlTuningSet;
            else if (strValue.Equals("ADVISOR"))
                enumValue = EPrivilege.Advisor;
            else if (strValue.Equals("ALTER ANY CLUSTER"))
                enumValue = EPrivilege.AlterAnyCluster;
            else if (strValue.Equals("ALTER ANY DIMENSION"))
                enumValue = EPrivilege.AlterAnyDimension;
            else if (strValue.Equals("ALTER ANY EVALUATION CONTEXT"))
                enumValue = EPrivilege.AlterAnyEvaluationContext;
            else if (strValue.Equals("ALTER ANY INDEX"))
                enumValue = EPrivilege.AlterAnyIndex;
            else if (strValue.Equals("ALTER ANY INDEXTYPE"))
                enumValue = EPrivilege.AlterAnyIndextype;
            else if (strValue.Equals("ALTER ANY LIBRARY"))
                enumValue = EPrivilege.AlterAnyLibrary;
            else if (strValue.Equals("ALTER ANY MATERIALIZED VIEW"))
                enumValue = EPrivilege.AlterAnyMaterializedView;
            else if (strValue.Equals("ALTER ANY OPERATOR"))
                enumValue = EPrivilege.AlterAnyOperator;
            else if (strValue.Equals("ALTER ANY OUTLINE"))
                enumValue = EPrivilege.AlterAnyOutline;
            else if (strValue.Equals("ALTER ANY PROCEDURE"))
                enumValue = EPrivilege.AlterAnyProcedure;
            else if (strValue.Equals("ALTER ANY ROLE"))
                enumValue = EPrivilege.AlterAnyRole;
            else if (strValue.Equals("ALTER ANY RULE"))
                enumValue = EPrivilege.AlterAnyRule;
            else if (strValue.Equals("ALTER ANY RULE SET"))
                enumValue = EPrivilege.AlterAnyRuleSet;
            else if (strValue.Equals("ALTER ANY SEQUENCE"))
                enumValue = EPrivilege.AlterAnySequence;
            else if (strValue.Equals("ALTER ANY SQL PROFILE"))
                enumValue = EPrivilege.AlterAnySqlProfile;
            else if (strValue.Equals("ALTER ANY TABLE"))
                enumValue = EPrivilege.AlterAnyTable;
            else if (strValue.Equals("ALTER ANY TRIGGER"))
                enumValue = EPrivilege.AlterAnyTrigger;
            else if (strValue.Equals("ALTER ANY TYPE"))
                enumValue = EPrivilege.AlterAnyType;
            else if (strValue.Equals("ALTER DATABASE"))
                enumValue = EPrivilege.AlterDatabase;
            else if (strValue.Equals("ALTER PROFILE"))
                enumValue = EPrivilege.AlterProfile;
            else if (strValue.Equals("ALTER RESOURCE COST"))
                enumValue = EPrivilege.AlterResourceCost;
            else if (strValue.Equals("ALTER ROLLBACK SEGMENT"))
                enumValue = EPrivilege.AlterRollbackSegment;
            else if (strValue.Equals("ALTER SESSION"))
                enumValue = EPrivilege.AlterSession;
            else if (strValue.Equals("ALTER SYSTEM"))
                enumValue = EPrivilege.AlterSystem;
            else if (strValue.Equals("ALTER TABLESPACE"))
                enumValue = EPrivilege.AlterTablespace;
            else if (strValue.Equals("ALTER USER"))
                enumValue = EPrivilege.AlterUser;
            else if (strValue.Equals("ANALYZE ANY"))
                enumValue = EPrivilege.AnalyzeAny;
            else if (strValue.Equals("ANALYZE ANY DICTIONARY"))
                enumValue = EPrivilege.AnalyzeAnyDictionary;
            else if (strValue.Equals("AUDIT ANY"))
                enumValue = EPrivilege.AuditAny;
            else if (strValue.Equals("AUDIT SYSTEM"))
                enumValue = EPrivilege.AuditSystem;
            else if (strValue.Equals("BACKUP ANY TABLE"))
                enumValue = EPrivilege.BackupAnyTable;
            else if (strValue.Equals("BECOME USER"))
                enumValue = EPrivilege.BecomeUser;
            else if (strValue.Equals("CHANGE NOTIFICATION"))
                enumValue = EPrivilege.ChangeNotification;
            else if (strValue.Equals("COMMENT ANY TABLE"))
                enumValue = EPrivilege.CommentAnyTable;
            else if (strValue.Equals("CREATE ANY CLUSTER"))
                enumValue = EPrivilege.CreateAnyCluster;
            else if (strValue.Equals("CREATE ANY CONTEXT"))
                enumValue = EPrivilege.CreateAnyContext;
            else if (strValue.Equals("CREATE ANY DIMENSION"))
                enumValue = EPrivilege.CreateAnyDimension;
            else if (strValue.Equals("CREATE ANY DIRECTORY"))
                enumValue = EPrivilege.CreateAnyDirectory;
            else if (strValue.Equals("CREATE ANY EVALUATION CONTEXT"))
                enumValue = EPrivilege.CreateAnyEvaluationContext;
            else if (strValue.Equals("CREATE ANY INDEX"))
                enumValue = EPrivilege.CreateAnyIndex;
            else if (strValue.Equals("CREATE ANY INDEXTYPE"))
                enumValue = EPrivilege.CreateAnyIndextype;
            else if (strValue.Equals("CREATE ANY JOB"))
                enumValue = EPrivilege.CreateAnyJob;
            else if (strValue.Equals("CREATE ANY LIBRARY"))
                enumValue = EPrivilege.CreateAnyLibrary;
            else if (strValue.Equals("CREATE ANY MATERIALIZED VIEW"))
                enumValue = EPrivilege.CreateAnyMaterializedView;
            else if (strValue.Equals("CREATE ANY OPERATOR"))
                enumValue = EPrivilege.CreateAnyOperator;
            else if (strValue.Equals("CREATE ANY OUTLINE"))
                enumValue = EPrivilege.CreateAnyOutline;
            else if (strValue.Equals("CREATE ANY PROCEDURE"))
                enumValue = EPrivilege.CreateAnyProcedure;
            else if (strValue.Equals("CREATE ANY RULE"))
                enumValue = EPrivilege.CreateAnyRule;
            else if (strValue.Equals("CREATE ANY RULE SET"))
                enumValue = EPrivilege.CreateAnyRuleSet;
            else if (strValue.Equals("CREATE ANY SEQUENCE"))
                enumValue = EPrivilege.CreateAnySequence;
            else if (strValue.Equals("CREATE ANY SQL PROFILE"))
                enumValue = EPrivilege.CreateAnySqlProfile;
            else if (strValue.Equals("CREATE ANY SYNONYM"))
                enumValue = EPrivilege.CreateAnySynonym;
            else if (strValue.Equals("CREATE ANY TABLE"))
                enumValue = EPrivilege.CreateAnyTable;
            else if (strValue.Equals("CREATE ANY TRIGGER"))
                enumValue = EPrivilege.CreateAnyTrigger;
            else if (strValue.Equals("CREATE ANY TYPE"))
                enumValue = EPrivilege.CreateAnyType;
            else if (strValue.Equals("CREATE ANY VIEW"))
                enumValue = EPrivilege.CreateAnyView;
            else if (strValue.Equals("CREATE CLUSTER"))
                enumValue = EPrivilege.CreateCluster;
            else if (strValue.Equals("CREATE DATABASE LINK"))
                enumValue = EPrivilege.CreateDatabaseLink;
            else if (strValue.Equals("CREATE DIMENSION"))
                enumValue = EPrivilege.CreateDimension;
            else if (strValue.Equals("CREATE EVALUATION CONTEXT"))
                enumValue = EPrivilege.CreateEvaluationContext;
            else if (strValue.Equals("CREATE EXTERNAL JOB"))
                enumValue = EPrivilege.CreateExternalJob;
            else if (strValue.Equals("CREATE INDEXTYPE"))
                enumValue = EPrivilege.CreateIndextype;
            else if (strValue.Equals("CREATE JOB"))
                enumValue = EPrivilege.CreateJob;
            else if (strValue.Equals("CREATE LIBRARY"))
                enumValue = EPrivilege.CreateLibrary;
            else if (strValue.Equals("CREATE MATERIALIZED VIEW"))
                enumValue = EPrivilege.CreateMaterializedView;
            else if (strValue.Equals("CREATE OPERATOR"))
                enumValue = EPrivilege.CreateOperator;
            else if (strValue.Equals("CREATE PROCEDURE"))
                enumValue = EPrivilege.CreateProcedure;
            else if (strValue.Equals("CREATE PROFILE"))
                enumValue = EPrivilege.CreateProfile;
            else if (strValue.Equals("CREATE PUBLIC DATABASE LINK"))
                enumValue = EPrivilege.CreatePublicDatabaseLink;
            else if (strValue.Equals("CREATE PUBLIC SYNONYM"))
                enumValue = EPrivilege.CreatePublicSynonym;
            else if (strValue.Equals("CREATE ROLE"))
                enumValue = EPrivilege.CreateRole;
            else if (strValue.Equals("CREATE ROLLBACK SEGMENT"))
                enumValue = EPrivilege.CreateRollbackSegment;
            else if (strValue.Equals("CREATE RULE"))
                enumValue = EPrivilege.CreateRule;
            else if (strValue.Equals("CREATE RULE SET"))
                enumValue = EPrivilege.CreateRuleSet;
            else if (strValue.Equals("CREATE SEQUENCE"))
                enumValue = EPrivilege.CreateSequence;
            else if (strValue.Equals("CREATE SESSION"))
                enumValue = EPrivilege.CreateSession;
            else if (strValue.Equals("CREATE SYNONYM"))
                enumValue = EPrivilege.CreateSynonym;
            else if (strValue.Equals("CREATE TABLE"))
                enumValue = EPrivilege.CreateTable;
            else if (strValue.Equals("CREATE TABLESPACE"))
                enumValue = EPrivilege.CreateTablespace;
            else if (strValue.Equals("CREATE TRIGGER"))
                enumValue = EPrivilege.CreateTrigger;
            else if (strValue.Equals("CREATE TYPE"))
                enumValue = EPrivilege.CreateType;
            else if (strValue.Equals("CREATE USER"))
                enumValue = EPrivilege.CreateUser;
            else if (strValue.Equals("CREATE VIEW"))
                enumValue = EPrivilege.CreateView;
            else if (strValue.Equals("DEBUG ANY PROCEDURE"))
                enumValue = EPrivilege.DebugAnyProcedure;
            else if (strValue.Equals("DEBUG CONNECT SESSION"))
                enumValue = EPrivilege.DebugConnectSession;
            else if (strValue.Equals("DELETE ANY TABLE"))
                enumValue = EPrivilege.DeleteAnyTable;
            else if (strValue.Equals("DEQUEUE ANY QUEUE"))
                enumValue = EPrivilege.DequeueAnyQueue;
            else if (strValue.Equals("DROP ANY CLUSTER"))
                enumValue = EPrivilege.DropAnyCluster;
            else if (strValue.Equals("DROP ANY CONTEXT"))
                enumValue = EPrivilege.DropAnyContext;
            else if (strValue.Equals("DROP ANY DIMENSION"))
                enumValue = EPrivilege.DropAnyDimension;
            else if (strValue.Equals("DROP ANY DIRECTORY"))
                enumValue = EPrivilege.DropAnyDirectory;
            else if (strValue.Equals("DROP ANY EVALUATION CONTEXT"))
                enumValue = EPrivilege.DropAnyEvaluationContext;
            else if (strValue.Equals("DROP ANY INDEX"))
                enumValue = EPrivilege.DropAnyIndex;
            else if (strValue.Equals("DROP ANY INDEXTYPE"))
                enumValue = EPrivilege.DropAnyIndextype;
            else if (strValue.Equals("DROP ANY LIBRARY"))
                enumValue = EPrivilege.DropAnyLibrary;
            else if (strValue.Equals("DROP ANY MATERIALIZED VIEW"))
                enumValue = EPrivilege.DropAnyMaterializedView;
            else if (strValue.Equals("DROP ANY OPERATOR"))
                enumValue = EPrivilege.DropAnyOperator;
            else if (strValue.Equals("DROP ANY OUTLINE"))
                enumValue = EPrivilege.DropAnyOutline;
            else if (strValue.Equals("DROP ANY PROCEDURE"))
                enumValue = EPrivilege.DropAnyProcedure;
            else if (strValue.Equals("DROP ANY ROLE"))
                enumValue = EPrivilege.DropAnyRole;
            else if (strValue.Equals("DROP ANY RULE"))
                enumValue = EPrivilege.DropAnyRule;
            else if (strValue.Equals("DROP ANY RULE SET"))
                enumValue = EPrivilege.DropAnyRuleSet;
            else if (strValue.Equals("DROP ANY SEQUENCE"))
                enumValue = EPrivilege.DropAnySequence;
            else if (strValue.Equals("DROP ANY SQL PROFILE"))
                enumValue = EPrivilege.DropAnySqlProfile;
            else if (strValue.Equals("DROP ANY SYNONYM"))
                enumValue = EPrivilege.DropAnySynonym;
            else if (strValue.Equals("DROP ANY TABLE"))
                enumValue = EPrivilege.DropAnyTable;
            else if (strValue.Equals("DROP ANY TRIGGER"))
                enumValue = EPrivilege.DropAnyTrigger;
            else if (strValue.Equals("DROP ANY TYPE"))
                enumValue = EPrivilege.DropAnyType;
            else if (strValue.Equals("DROP ANY VIEW"))
                enumValue = EPrivilege.DropAnyView;
            else if (strValue.Equals("DROP PROFILE"))
                enumValue = EPrivilege.DropProfile;
            else if (strValue.Equals("DROP PUBLIC DATABASE LINK"))
                enumValue = EPrivilege.DropPublicDatabaseLink;
            else if (strValue.Equals("DROP PUBLIC SYNONYM"))
                enumValue = EPrivilege.DropPublicSynonym;
            else if (strValue.Equals("DROP ROLLBACK SEGMENT"))
                enumValue = EPrivilege.DropRollbackSegment;
            else if (strValue.Equals("DROP TABLESPACE"))
                enumValue = EPrivilege.DropTablespace;
            else if (strValue.Equals("DROP USER"))
                enumValue = EPrivilege.DropUser;
            else if (strValue.Equals("ENQUEUE ANY QUEUE"))
                enumValue = EPrivilege.EnqueueAnyQueue;
            else if (strValue.Equals("EXECUTE ANY CLASS"))
                enumValue = EPrivilege.ExecuteAnyClass;
            else if (strValue.Equals("EXECUTE ANY EVALUATION CONTEXT"))
                enumValue = EPrivilege.ExecuteAnyEvaluationContext;
            else if (strValue.Equals("EXECUTE ANY INDEXTYPE"))
                enumValue = EPrivilege.ExecuteAnyIndextype;
            else if (strValue.Equals("EXECUTE ANY LIBRARY"))
                enumValue = EPrivilege.ExecuteAnyLibrary;
            else if (strValue.Equals("EXECUTE ANY OPERATOR"))
                enumValue = EPrivilege.ExecuteAnyOperator;
            else if (strValue.Equals("EXECUTE ANY PROCEDURE"))
                enumValue = EPrivilege.ExecuteAnyProcedure;
            else if (strValue.Equals("EXECUTE ANY PROGRAM"))
                enumValue = EPrivilege.ExecuteAnyProgram;
            else if (strValue.Equals("EXECUTE ANY RULE"))
                enumValue = EPrivilege.ExecuteAnyRule;
            else if (strValue.Equals("EXECUTE ANY RULE SET"))
                enumValue = EPrivilege.ExecuteAnyRuleSet;
            else if (strValue.Equals("EXECUTE ANY TYPE"))
                enumValue = EPrivilege.ExecuteAnyType;
            else if (strValue.Equals("EXEMPT ACCESS POLICY"))
                enumValue = EPrivilege.ExemptAccessPolicy;
            else if (strValue.Equals("EXEMPT IDENTITY POLICY"))
                enumValue = EPrivilege.ExemptIdentityPolicy;
            else if (strValue.Equals("EXPORT FULL DATABASE"))
                enumValue = EPrivilege.ExportFullDatabase;
            else if (strValue.Equals("FLASHBACK ANY TABLE"))
                enumValue = EPrivilege.FlashbackAnyTable;
            else if (strValue.Equals("FORCE ANY TRANSACTION"))
                enumValue = EPrivilege.ForceAnyTransaction;
            else if (strValue.Equals("FORCE TRANSACTION"))
                enumValue = EPrivilege.ForceTransaction;
            else if (strValue.Equals("GLOBAL QUERY REWRITE"))
                enumValue = EPrivilege.GlobalQueryRewrite;
            else if (strValue.Equals("GRANT ANY OBJECT PRIVILEGE"))
                enumValue = EPrivilege.GrantAnyObjectPrivilege;
            else if (strValue.Equals("GRANT ANY PRIVILEGE"))
                enumValue = EPrivilege.GrantAnyPrivilege;
            else if (strValue.Equals("GRANT ANY ROLE"))
                enumValue = EPrivilege.GrantAnyRole;
            else if (strValue.Equals("IMPORT FULL DATABASE"))
                enumValue = EPrivilege.ImportFullDatabase;
            else if (strValue.Equals("INSERT ANY TABLE"))
                enumValue = EPrivilege.InsertAnyTable;
            else if (strValue.Equals("LOCK ANY TABLE"))
                enumValue = EPrivilege.LockAnyTable;
            else if (strValue.Equals("MANAGE ANY FILE GROUP"))
                enumValue = EPrivilege.ManageAnyFileGroup;
            else if (strValue.Equals("MANAGE ANY QUEUE"))
                enumValue = EPrivilege.ManageAnyQueue;
            else if (strValue.Equals("MANAGE FILE GROUP"))
                enumValue = EPrivilege.ManageFileGroup;
            else if (strValue.Equals("MANAGE SCHEDULER"))
                enumValue = EPrivilege.ManageScheduler;
            else if (strValue.Equals("MANAGE TABLESPACE"))
                enumValue = EPrivilege.ManageTablespace;
            else if (strValue.Equals("MERGE ANY VIEW"))
                enumValue = EPrivilege.MergeAnyView;
            else if (strValue.Equals("ON COMMIT REFRESH"))
                enumValue = EPrivilege.OnCommitRefresh;
            else if (strValue.Equals("QUERY REWRITE"))
                enumValue = EPrivilege.QueryRewrite;
            else if (strValue.Equals("READ ANY FILE GROUP"))
                enumValue = EPrivilege.ReadAnyFileGroup;
            else if (strValue.Equals("RESTRICTED SESSION"))
                enumValue = EPrivilege.RestrictedSession;
            else if (strValue.Equals("RESUMABLE"))
                enumValue = EPrivilege.Resumable;
            else if (strValue.Equals("SELECT ANY DICTIONARY"))
                enumValue = EPrivilege.SelectAnyDictionary;
            else if (strValue.Equals("SELECT ANY SEQUENCE"))
                enumValue = EPrivilege.SelectAnySequence;
            else if (strValue.Equals("SELECT ANY TABLE"))
                enumValue = EPrivilege.SelectAnyTable;
            else if (strValue.Equals("SELECT ANY TRANSACTION"))
                enumValue = EPrivilege.SelectAnyTransaction;
            else if (strValue.Equals("SYSDBA"))
                enumValue = EPrivilege.Sysdba;
            else if (strValue.Equals("SYSOPER"))
                enumValue = EPrivilege.Sysoper;
            else if (strValue.Equals("UNDER ANY TABLE"))
                enumValue = EPrivilege.UnderAnyTable;
            else if (strValue.Equals("UNDER ANY TYPE"))
                enumValue = EPrivilege.UnderAnyType;
            else if (strValue.Equals("UNDER ANY VIEW"))
                enumValue = EPrivilege.UnderAnyView;
            else if (strValue.Equals("UNLIMITED TABLESPACE"))
                enumValue = EPrivilege.UnlimitedTablespace;
            else if (strValue.Equals("UPDATE ANY TABLE"))
                enumValue = EPrivilege.UpdateAnyTable;
            else
                enumValue = EPrivilege.Unknown;

            #endregion

            return enumValue;
        }
    }

    #endregion

    #endregion
    /// <summary>
    /// Interface for enum to string converters
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface EnumToStringConverter<T>
    {
        string EnumValueToString(T value, object parameter);
    }

    #region Enum to string converters
    public class EDbaPrivilegesToStringConverter : EnumToStringConverter<EDbaPrivileges>
    {
        public string EnumValueToString(EDbaPrivileges value, object parameter)
        {
            switch (value)
            {
                case EDbaPrivileges.Normal:
                    return "Normální";
                case EDbaPrivileges.SYSDBA:
                    return "SYSDBA";
                case EDbaPrivileges.SYSOPER:
                    return "SYSOPER";
                default:
                    return value.ToString();
            }
        }
    }
    public class EServerTypeToStringConverter : EnumToStringConverter<EServerType>
    {
        #region EnumToStringConverter<EServerType> Members

        public string EnumValueToString(EServerType value, object parameter)
        {
            switch (value)
            {
                case EServerType.Dedicated:
                    return "Vyhrazaný server";
                case EServerType.Shared:
                    return "Sdílený server";
                case EServerType.Pooled:
                    return "Pooled server";
                default:
                    return value.ToString();
            }
        }

        #endregion
    }
    public class ENamingMethodToStringConverter : EnumToStringConverter<ENamingMethod>
    {
        #region EnumToStringConverter<ENamingMethod> Members

        public string EnumValueToString(ENamingMethod value, object parameter)
        {
            switch (value)
            {
                case ENamingMethod.ConnectDescriptor:
                    return "Přímé zadání údajů";
                case ENamingMethod.TnsNaming:
                    return "TNS identifikátor";
                default:
                    return value.ToString();
            }
        }

        #endregion
    }

    public class EPrivilegeToStringConverter : EnumToStringConverter<EPrivilege>
    {

        #region EnumToStringConverter<EPrivilege> Members

        public string EnumValueToString(EPrivilege value, object parameter)
        {
            switch (value)
            {
                #region Enum to string branching

                case EPrivilege.AdministerAnySqlTuningSet:
                    return "ADMINISTER ANY SQL TUNING SET";
                case EPrivilege.AdministerDatabaseTrigger:
                    return "ADMINISTER DATABASE TRIGGER";
                case EPrivilege.AdministerResourceManager:
                    return "ADMINISTER RESOURCE MANAGER";
                case EPrivilege.AdministerSqlTuningSet:
                    return "ADMINISTER SQL TUNING SET";
                case EPrivilege.Advisor:
                    return "ADVISOR";
                case EPrivilege.AlterAnyCluster:
                    return "ALTER ANY CLUSTER";
                case EPrivilege.AlterAnyDimension:
                    return "ALTER ANY DIMENSION";
                case EPrivilege.AlterAnyEvaluationContext:
                    return "ALTER ANY EVALUATION CONTEXT";
                case EPrivilege.AlterAnyIndex:
                    return "ALTER ANY INDEX";
                case EPrivilege.AlterAnyIndextype:
                    return "ALTER ANY INDEXTYPE";
                case EPrivilege.AlterAnyLibrary:
                    return "ALTER ANY LIBRARY";
                case EPrivilege.AlterAnyMaterializedView:
                    return "ALTER ANY MATERIALIZED VIEW";
                case EPrivilege.AlterAnyOperator:
                    return "ALTER ANY OPERATOR";
                case EPrivilege.AlterAnyOutline:
                    return "ALTER ANY OUTLINE";
                case EPrivilege.AlterAnyProcedure:
                    return "ALTER ANY PROCEDURE";
                case EPrivilege.AlterAnyRole:
                    return "ALTER ANY ROLE";
                case EPrivilege.AlterAnyRule:
                    return "ALTER ANY RULE";
                case EPrivilege.AlterAnyRuleSet:
                    return "ALTER ANY RULE SET";
                case EPrivilege.AlterAnySequence:
                    return "ALTER ANY SEQUENCE";
                case EPrivilege.AlterAnySqlProfile:
                    return "ALTER ANY SQL PROFILE";
                case EPrivilege.AlterAnyTable:
                    return "ALTER ANY TABLE";
                case EPrivilege.AlterAnyTrigger:
                    return "ALTER ANY TRIGGER";
                case EPrivilege.AlterAnyType:
                    return "ALTER ANY TYPE";
                case EPrivilege.AlterDatabase:
                    return "ALTER DATABASE";
                case EPrivilege.AlterProfile:
                    return "ALTER PROFILE";
                case EPrivilege.AlterResourceCost:
                    return "ALTER RESOURCE COST";
                case EPrivilege.AlterRollbackSegment:
                    return "ALTER ROLLBACK SEGMENT";
                case EPrivilege.AlterSession:
                    return "ALTER SESSION";
                case EPrivilege.AlterSystem:
                    return "ALTER SYSTEM";
                case EPrivilege.AlterTablespace:
                    return "ALTER TABLESPACE";
                case EPrivilege.AlterUser:
                    return "ALTER USER";
                case EPrivilege.AnalyzeAny:
                    return "ANALYZE ANY";
                case EPrivilege.AnalyzeAnyDictionary:
                    return "ANALYZE ANY DICTIONARY";
                case EPrivilege.AuditAny:
                    return "AUDIT ANY";
                case EPrivilege.AuditSystem:
                    return "AUDIT SYSTEM";
                case EPrivilege.BackupAnyTable:
                    return "BACKUP ANY TABLE";
                case EPrivilege.BecomeUser:
                    return "BECOME USER";
                case EPrivilege.ChangeNotification:
                    return "CHANGE NOTIFICATION";
                case EPrivilege.CommentAnyTable:
                    return "COMMENT ANY TABLE";
                case EPrivilege.CreateAnyCluster:
                    return "CREATE ANY CLUSTER";
                case EPrivilege.CreateAnyContext:
                    return "CREATE ANY CONTEXT";
                case EPrivilege.CreateAnyDimension:
                    return "CREATE ANY DIMENSION";
                case EPrivilege.CreateAnyDirectory:
                    return "CREATE ANY DIRECTORY";
                case EPrivilege.CreateAnyEvaluationContext:
                    return "CREATE ANY EVALUATION CONTEXT";
                case EPrivilege.CreateAnyIndex:
                    return "CREATE ANY INDEX";
                case EPrivilege.CreateAnyIndextype:
                    return "CREATE ANY INDEXTYPE";
                case EPrivilege.CreateAnyJob:
                    return "CREATE ANY JOB";
                case EPrivilege.CreateAnyLibrary:
                    return "CREATE ANY LIBRARY";
                case EPrivilege.CreateAnyMaterializedView:
                    return "CREATE ANY MATERIALIZED VIEW";
                case EPrivilege.CreateAnyOperator:
                    return "CREATE ANY OPERATOR";
                case EPrivilege.CreateAnyOutline:
                    return "CREATE ANY OUTLINE";
                case EPrivilege.CreateAnyProcedure:
                    return "CREATE ANY PROCEDURE";
                case EPrivilege.CreateAnyRule:
                    return "CREATE ANY RULE";
                case EPrivilege.CreateAnyRuleSet:
                    return "CREATE ANY RULE SET";
                case EPrivilege.CreateAnySequence:
                    return "CREATE ANY SEQUENCE";
                case EPrivilege.CreateAnySqlProfile:
                    return "CREATE ANY SQL PROFILE";
                case EPrivilege.CreateAnySynonym:
                    return "CREATE ANY SYNONYM";
                case EPrivilege.CreateAnyTable:
                    return "CREATE ANY TABLE";
                case EPrivilege.CreateAnyTrigger:
                    return "CREATE ANY TRIGGER";
                case EPrivilege.CreateAnyType:
                    return "CREATE ANY TYPE";
                case EPrivilege.CreateAnyView:
                    return "CREATE ANY VIEW";
                case EPrivilege.CreateCluster:
                    return "CREATE CLUSTER";
                case EPrivilege.CreateDatabaseLink:
                    return "CREATE DATABASE LINK";
                case EPrivilege.CreateDimension:
                    return "CREATE DIMENSION";
                case EPrivilege.CreateEvaluationContext:
                    return "CREATE EVALUATION CONTEXT";
                case EPrivilege.CreateExternalJob:
                    return "CREATE EXTERNAL JOB";
                case EPrivilege.CreateIndextype:
                    return "CREATE INDEXTYPE";
                case EPrivilege.CreateJob:
                    return "CREATE JOB";
                case EPrivilege.CreateLibrary:
                    return "CREATE LIBRARY";
                case EPrivilege.CreateMaterializedView:
                    return "CREATE MATERIALIZED VIEW";
                case EPrivilege.CreateOperator:
                    return "CREATE OPERATOR";
                case EPrivilege.CreateProcedure:
                    return "CREATE PROCEDURE";
                case EPrivilege.CreateProfile:
                    return "CREATE PROFILE";
                case EPrivilege.CreatePublicDatabaseLink:
                    return "CREATE PUBLIC DATABASE LINK";
                case EPrivilege.CreatePublicSynonym:
                    return "CREATE PUBLIC SYNONYM";
                case EPrivilege.CreateRole:
                    return "CREATE ROLE";
                case EPrivilege.CreateRollbackSegment:
                    return "CREATE ROLLBACK SEGMENT";
                case EPrivilege.CreateRule:
                    return "CREATE RULE";
                case EPrivilege.CreateRuleSet:
                    return "CREATE RULE SET";
                case EPrivilege.CreateSequence:
                    return "CREATE SEQUENCE";
                case EPrivilege.CreateSession:
                    return "CREATE SESSION";
                case EPrivilege.CreateSynonym:
                    return "CREATE SYNONYM";
                case EPrivilege.CreateTable:
                    return "CREATE TABLE";
                case EPrivilege.CreateTablespace:
                    return "CREATE TABLESPACE";
                case EPrivilege.CreateTrigger:
                    return "CREATE TRIGGER";
                case EPrivilege.CreateType:
                    return "CREATE TYPE";
                case EPrivilege.CreateUser:
                    return "CREATE USER";
                case EPrivilege.CreateView:
                    return "CREATE VIEW";
                case EPrivilege.DebugAnyProcedure:
                    return "DEBUG ANY PROCEDURE";
                case EPrivilege.DebugConnectSession:
                    return "DEBUG CONNECT SESSION";
                case EPrivilege.DeleteAnyTable:
                    return "DELETE ANY TABLE";
                case EPrivilege.DequeueAnyQueue:
                    return "DEQUEUE ANY QUEUE";
                case EPrivilege.DropAnyCluster:
                    return "DROP ANY CLUSTER";
                case EPrivilege.DropAnyContext:
                    return "DROP ANY CONTEXT";
                case EPrivilege.DropAnyDimension:
                    return "DROP ANY DIMENSION";
                case EPrivilege.DropAnyDirectory:
                    return "DROP ANY DIRECTORY";
                case EPrivilege.DropAnyEvaluationContext:
                    return "DROP ANY EVALUATION CONTEXT";
                case EPrivilege.DropAnyIndex:
                    return "DROP ANY INDEX";
                case EPrivilege.DropAnyIndextype:
                    return "DROP ANY INDEXTYPE";
                case EPrivilege.DropAnyLibrary:
                    return "DROP ANY LIBRARY";
                case EPrivilege.DropAnyMaterializedView:
                    return "DROP ANY MATERIALIZED VIEW";
                case EPrivilege.DropAnyOperator:
                    return "DROP ANY OPERATOR";
                case EPrivilege.DropAnyOutline:
                    return "DROP ANY OUTLINE";
                case EPrivilege.DropAnyProcedure:
                    return "DROP ANY PROCEDURE";
                case EPrivilege.DropAnyRole:
                    return "DROP ANY ROLE";
                case EPrivilege.DropAnyRule:
                    return "DROP ANY RULE";
                case EPrivilege.DropAnyRuleSet:
                    return "DROP ANY RULE SET";
                case EPrivilege.DropAnySequence:
                    return "DROP ANY SEQUENCE";
                case EPrivilege.DropAnySqlProfile:
                    return "DROP ANY SQL PROFILE";
                case EPrivilege.DropAnySynonym:
                    return "DROP ANY SYNONYM";
                case EPrivilege.DropAnyTable:
                    return "DROP ANY TABLE";
                case EPrivilege.DropAnyTrigger:
                    return "DROP ANY TRIGGER";
                case EPrivilege.DropAnyType:
                    return "DROP ANY TYPE";
                case EPrivilege.DropAnyView:
                    return "DROP ANY VIEW";
                case EPrivilege.DropProfile:
                    return "DROP PROFILE";
                case EPrivilege.DropPublicDatabaseLink:
                    return "DROP PUBLIC DATABASE LINK";
                case EPrivilege.DropPublicSynonym:
                    return "DROP PUBLIC SYNONYM";
                case EPrivilege.DropRollbackSegment:
                    return "DROP ROLLBACK SEGMENT";
                case EPrivilege.DropTablespace:
                    return "DROP TABLESPACE";
                case EPrivilege.DropUser:
                    return "DROP USER";
                case EPrivilege.EnqueueAnyQueue:
                    return "ENQUEUE ANY QUEUE";
                case EPrivilege.ExecuteAnyClass:
                    return "EXECUTE ANY CLASS";
                case EPrivilege.ExecuteAnyEvaluationContext:
                    return "EXECUTE ANY EVALUATION CONTEXT";
                case EPrivilege.ExecuteAnyIndextype:
                    return "EXECUTE ANY INDEXTYPE";
                case EPrivilege.ExecuteAnyLibrary:
                    return "EXECUTE ANY LIBRARY";
                case EPrivilege.ExecuteAnyOperator:
                    return "EXECUTE ANY OPERATOR";
                case EPrivilege.ExecuteAnyProcedure:
                    return "EXECUTE ANY PROCEDURE";
                case EPrivilege.ExecuteAnyProgram:
                    return "EXECUTE ANY PROGRAM";
                case EPrivilege.ExecuteAnyRule:
                    return "EXECUTE ANY RULE";
                case EPrivilege.ExecuteAnyRuleSet:
                    return "EXECUTE ANY RULE SET";
                case EPrivilege.ExecuteAnyType:
                    return "EXECUTE ANY TYPE";
                case EPrivilege.ExemptAccessPolicy:
                    return "EXEMPT ACCESS POLICY";
                case EPrivilege.ExemptIdentityPolicy:
                    return "EXEMPT IDENTITY POLICY";
                case EPrivilege.ExportFullDatabase:
                    return "EXPORT FULL DATABASE";
                case EPrivilege.FlashbackAnyTable:
                    return "FLASHBACK ANY TABLE";
                case EPrivilege.ForceAnyTransaction:
                    return "FORCE ANY TRANSACTION";
                case EPrivilege.ForceTransaction:
                    return "FORCE TRANSACTION";
                case EPrivilege.GlobalQueryRewrite:
                    return "GLOBAL QUERY REWRITE";
                case EPrivilege.GrantAnyObjectPrivilege:
                    return "GRANT ANY OBJECT PRIVILEGE";
                case EPrivilege.GrantAnyPrivilege:
                    return "GRANT ANY PRIVILEGE";
                case EPrivilege.GrantAnyRole:
                    return "GRANT ANY ROLE";
                case EPrivilege.ImportFullDatabase:
                    return "IMPORT FULL DATABASE";
                case EPrivilege.InsertAnyTable:
                    return "INSERT ANY TABLE";
                case EPrivilege.LockAnyTable:
                    return "LOCK ANY TABLE";
                case EPrivilege.ManageAnyFileGroup:
                    return "MANAGE ANY FILE GROUP";
                case EPrivilege.ManageAnyQueue:
                    return "MANAGE ANY QUEUE";
                case EPrivilege.ManageFileGroup:
                    return "MANAGE FILE GROUP";
                case EPrivilege.ManageScheduler:
                    return "MANAGE SCHEDULER";
                case EPrivilege.ManageTablespace:
                    return "MANAGE TABLESPACE";
                case EPrivilege.MergeAnyView:
                    return "MERGE ANY VIEW";
                case EPrivilege.OnCommitRefresh:
                    return "ON COMMIT REFRESH";
                case EPrivilege.QueryRewrite:
                    return "QUERY REWRITE";
                case EPrivilege.ReadAnyFileGroup:
                    return "READ ANY FILE GROUP";
                case EPrivilege.RestrictedSession:
                    return "RESTRICTED SESSION";
                case EPrivilege.Resumable:
                    return "RESUMABLE";
                case EPrivilege.SelectAnyDictionary:
                    return "SELECT ANY DICTIONARY";
                case EPrivilege.SelectAnySequence:
                    return "SELECT ANY SEQUENCE";
                case EPrivilege.SelectAnyTable:
                    return "SELECT ANY TABLE";
                case EPrivilege.SelectAnyTransaction:
                    return "SELECT ANY TRANSACTION";
                case EPrivilege.Sysdba:
                    return "SYSDBA";
                case EPrivilege.Sysoper:
                    return "SYSOPER";
                case EPrivilege.UnderAnyTable:
                    return "UNDER ANY TABLE";
                case EPrivilege.UnderAnyType:
                    return "UNDER ANY TYPE";
                case EPrivilege.UnderAnyView:
                    return "UNDER ANY VIEW";
                case EPrivilege.UnlimitedTablespace:
                    return "UNLIMITED TABLESPACE";
                case EPrivilege.UpdateAnyTable:
                    return "UPDATE ANY TABLE";

                #endregion
            }
        }

        #endregion
    }
    #endregion


    /// <summary>
    /// Class to store type binding to enum converters
    /// </summary>
    public static class EnumConverterMapper
    {
        private static Dictionary<Type, object> specs;

        static EnumConverterMapper()
        {
            specs = new Dictionary<Type, object>();

            specs.Add(typeof(EServerType), new EServerTypeToStringConverter());
            specs.Add(typeof(EDbaPrivileges), new EDbaPrivilegesToStringConverter());
            specs.Add(typeof(ENamingMethod), new ENamingMethodToStringConverter());
            specs.Add(typeof(EPrivilege), new EPrivilegeToStringConverter());
        }

        public static object GetConverter(Type type)
        {
            return specs[type];
        }
    }
}
