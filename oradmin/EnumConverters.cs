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

    [ValueConversion(typeof(ESysPrivilege), typeof(string))]
    public class EPrivilegeEnumConverter : EnumValueConverter<ESysPrivilege>
    {
        public override object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string strValue = (string)value;
            ESysPrivilege enumValue;

            #region Enum conversion branching

            if (strValue.Equals("ADMINISTER ANY SQL TUNING SET"))
                enumValue = ESysPrivilege.AdministerAnySqlTuningSet;
            else if (strValue.Equals("ADMINISTER DATABASE TRIGGER"))
                enumValue = ESysPrivilege.AdministerDatabaseTrigger;
            else if (strValue.Equals("ADMINISTER RESOURCE MANAGER"))
                enumValue = ESysPrivilege.AdministerResourceManager;
            else if (strValue.Equals("ADMINISTER SQL TUNING SET"))
                enumValue = ESysPrivilege.AdministerSqlTuningSet;
            else if (strValue.Equals("ADVISOR"))
                enumValue = ESysPrivilege.Advisor;
            else if (strValue.Equals("ALTER ANY CLUSTER"))
                enumValue = ESysPrivilege.AlterAnyCluster;
            else if (strValue.Equals("ALTER ANY DIMENSION"))
                enumValue = ESysPrivilege.AlterAnyDimension;
            else if (strValue.Equals("ALTER ANY EVALUATION CONTEXT"))
                enumValue = ESysPrivilege.AlterAnyEvaluationContext;
            else if (strValue.Equals("ALTER ANY INDEX"))
                enumValue = ESysPrivilege.AlterAnyIndex;
            else if (strValue.Equals("ALTER ANY INDEXTYPE"))
                enumValue = ESysPrivilege.AlterAnyIndextype;
            else if (strValue.Equals("ALTER ANY LIBRARY"))
                enumValue = ESysPrivilege.AlterAnyLibrary;
            else if (strValue.Equals("ALTER ANY MATERIALIZED VIEW"))
                enumValue = ESysPrivilege.AlterAnyMaterializedView;
            else if (strValue.Equals("ALTER ANY OPERATOR"))
                enumValue = ESysPrivilege.AlterAnyOperator;
            else if (strValue.Equals("ALTER ANY OUTLINE"))
                enumValue = ESysPrivilege.AlterAnyOutline;
            else if (strValue.Equals("ALTER ANY PROCEDURE"))
                enumValue = ESysPrivilege.AlterAnyProcedure;
            else if (strValue.Equals("ALTER ANY ROLE"))
                enumValue = ESysPrivilege.AlterAnyRole;
            else if (strValue.Equals("ALTER ANY RULE"))
                enumValue = ESysPrivilege.AlterAnyRule;
            else if (strValue.Equals("ALTER ANY RULE SET"))
                enumValue = ESysPrivilege.AlterAnyRuleSet;
            else if (strValue.Equals("ALTER ANY SEQUENCE"))
                enumValue = ESysPrivilege.AlterAnySequence;
            else if (strValue.Equals("ALTER ANY SQL PROFILE"))
                enumValue = ESysPrivilege.AlterAnySqlProfile;
            else if (strValue.Equals("ALTER ANY TABLE"))
                enumValue = ESysPrivilege.AlterAnyTable;
            else if (strValue.Equals("ALTER ANY TRIGGER"))
                enumValue = ESysPrivilege.AlterAnyTrigger;
            else if (strValue.Equals("ALTER ANY TYPE"))
                enumValue = ESysPrivilege.AlterAnyType;
            else if (strValue.Equals("ALTER DATABASE"))
                enumValue = ESysPrivilege.AlterDatabase;
            else if (strValue.Equals("ALTER PROFILE"))
                enumValue = ESysPrivilege.AlterProfile;
            else if (strValue.Equals("ALTER RESOURCE COST"))
                enumValue = ESysPrivilege.AlterResourceCost;
            else if (strValue.Equals("ALTER ROLLBACK SEGMENT"))
                enumValue = ESysPrivilege.AlterRollbackSegment;
            else if (strValue.Equals("ALTER SESSION"))
                enumValue = ESysPrivilege.AlterSession;
            else if (strValue.Equals("ALTER SYSTEM"))
                enumValue = ESysPrivilege.AlterSystem;
            else if (strValue.Equals("ALTER TABLESPACE"))
                enumValue = ESysPrivilege.AlterTablespace;
            else if (strValue.Equals("ALTER USER"))
                enumValue = ESysPrivilege.AlterUser;
            else if (strValue.Equals("ANALYZE ANY"))
                enumValue = ESysPrivilege.AnalyzeAny;
            else if (strValue.Equals("ANALYZE ANY DICTIONARY"))
                enumValue = ESysPrivilege.AnalyzeAnyDictionary;
            else if (strValue.Equals("AUDIT ANY"))
                enumValue = ESysPrivilege.AuditAny;
            else if (strValue.Equals("AUDIT SYSTEM"))
                enumValue = ESysPrivilege.AuditSystem;
            else if (strValue.Equals("BACKUP ANY TABLE"))
                enumValue = ESysPrivilege.BackupAnyTable;
            else if (strValue.Equals("BECOME USER"))
                enumValue = ESysPrivilege.BecomeUser;
            else if (strValue.Equals("CHANGE NOTIFICATION"))
                enumValue = ESysPrivilege.ChangeNotification;
            else if (strValue.Equals("COMMENT ANY TABLE"))
                enumValue = ESysPrivilege.CommentAnyTable;
            else if (strValue.Equals("CREATE ANY CLUSTER"))
                enumValue = ESysPrivilege.CreateAnyCluster;
            else if (strValue.Equals("CREATE ANY CONTEXT"))
                enumValue = ESysPrivilege.CreateAnyContext;
            else if (strValue.Equals("CREATE ANY DIMENSION"))
                enumValue = ESysPrivilege.CreateAnyDimension;
            else if (strValue.Equals("CREATE ANY DIRECTORY"))
                enumValue = ESysPrivilege.CreateAnyDirectory;
            else if (strValue.Equals("CREATE ANY EVALUATION CONTEXT"))
                enumValue = ESysPrivilege.CreateAnyEvaluationContext;
            else if (strValue.Equals("CREATE ANY INDEX"))
                enumValue = ESysPrivilege.CreateAnyIndex;
            else if (strValue.Equals("CREATE ANY INDEXTYPE"))
                enumValue = ESysPrivilege.CreateAnyIndextype;
            else if (strValue.Equals("CREATE ANY JOB"))
                enumValue = ESysPrivilege.CreateAnyJob;
            else if (strValue.Equals("CREATE ANY LIBRARY"))
                enumValue = ESysPrivilege.CreateAnyLibrary;
            else if (strValue.Equals("CREATE ANY MATERIALIZED VIEW"))
                enumValue = ESysPrivilege.CreateAnyMaterializedView;
            else if (strValue.Equals("CREATE ANY OPERATOR"))
                enumValue = ESysPrivilege.CreateAnyOperator;
            else if (strValue.Equals("CREATE ANY OUTLINE"))
                enumValue = ESysPrivilege.CreateAnyOutline;
            else if (strValue.Equals("CREATE ANY PROCEDURE"))
                enumValue = ESysPrivilege.CreateAnyProcedure;
            else if (strValue.Equals("CREATE ANY RULE"))
                enumValue = ESysPrivilege.CreateAnyRule;
            else if (strValue.Equals("CREATE ANY RULE SET"))
                enumValue = ESysPrivilege.CreateAnyRuleSet;
            else if (strValue.Equals("CREATE ANY SEQUENCE"))
                enumValue = ESysPrivilege.CreateAnySequence;
            else if (strValue.Equals("CREATE ANY SQL PROFILE"))
                enumValue = ESysPrivilege.CreateAnySqlProfile;
            else if (strValue.Equals("CREATE ANY SYNONYM"))
                enumValue = ESysPrivilege.CreateAnySynonym;
            else if (strValue.Equals("CREATE ANY TABLE"))
                enumValue = ESysPrivilege.CreateAnyTable;
            else if (strValue.Equals("CREATE ANY TRIGGER"))
                enumValue = ESysPrivilege.CreateAnyTrigger;
            else if (strValue.Equals("CREATE ANY TYPE"))
                enumValue = ESysPrivilege.CreateAnyType;
            else if (strValue.Equals("CREATE ANY VIEW"))
                enumValue = ESysPrivilege.CreateAnyView;
            else if (strValue.Equals("CREATE CLUSTER"))
                enumValue = ESysPrivilege.CreateCluster;
            else if (strValue.Equals("CREATE DATABASE LINK"))
                enumValue = ESysPrivilege.CreateDatabaseLink;
            else if (strValue.Equals("CREATE DIMENSION"))
                enumValue = ESysPrivilege.CreateDimension;
            else if (strValue.Equals("CREATE EVALUATION CONTEXT"))
                enumValue = ESysPrivilege.CreateEvaluationContext;
            else if (strValue.Equals("CREATE EXTERNAL JOB"))
                enumValue = ESysPrivilege.CreateExternalJob;
            else if (strValue.Equals("CREATE INDEXTYPE"))
                enumValue = ESysPrivilege.CreateIndextype;
            else if (strValue.Equals("CREATE JOB"))
                enumValue = ESysPrivilege.CreateJob;
            else if (strValue.Equals("CREATE LIBRARY"))
                enumValue = ESysPrivilege.CreateLibrary;
            else if (strValue.Equals("CREATE MATERIALIZED VIEW"))
                enumValue = ESysPrivilege.CreateMaterializedView;
            else if (strValue.Equals("CREATE OPERATOR"))
                enumValue = ESysPrivilege.CreateOperator;
            else if (strValue.Equals("CREATE PROCEDURE"))
                enumValue = ESysPrivilege.CreateProcedure;
            else if (strValue.Equals("CREATE PROFILE"))
                enumValue = ESysPrivilege.CreateProfile;
            else if (strValue.Equals("CREATE PUBLIC DATABASE LINK"))
                enumValue = ESysPrivilege.CreatePublicDatabaseLink;
            else if (strValue.Equals("CREATE PUBLIC SYNONYM"))
                enumValue = ESysPrivilege.CreatePublicSynonym;
            else if (strValue.Equals("CREATE ROLE"))
                enumValue = ESysPrivilege.CreateRole;
            else if (strValue.Equals("CREATE ROLLBACK SEGMENT"))
                enumValue = ESysPrivilege.CreateRollbackSegment;
            else if (strValue.Equals("CREATE RULE"))
                enumValue = ESysPrivilege.CreateRule;
            else if (strValue.Equals("CREATE RULE SET"))
                enumValue = ESysPrivilege.CreateRuleSet;
            else if (strValue.Equals("CREATE SEQUENCE"))
                enumValue = ESysPrivilege.CreateSequence;
            else if (strValue.Equals("CREATE SESSION"))
                enumValue = ESysPrivilege.CreateSession;
            else if (strValue.Equals("CREATE SYNONYM"))
                enumValue = ESysPrivilege.CreateSynonym;
            else if (strValue.Equals("CREATE TABLE"))
                enumValue = ESysPrivilege.CreateTable;
            else if (strValue.Equals("CREATE TABLESPACE"))
                enumValue = ESysPrivilege.CreateTablespace;
            else if (strValue.Equals("CREATE TRIGGER"))
                enumValue = ESysPrivilege.CreateTrigger;
            else if (strValue.Equals("CREATE TYPE"))
                enumValue = ESysPrivilege.CreateType;
            else if (strValue.Equals("CREATE USER"))
                enumValue = ESysPrivilege.CreateUser;
            else if (strValue.Equals("CREATE VIEW"))
                enumValue = ESysPrivilege.CreateView;
            else if (strValue.Equals("DEBUG ANY PROCEDURE"))
                enumValue = ESysPrivilege.DebugAnyProcedure;
            else if (strValue.Equals("DEBUG CONNECT SESSION"))
                enumValue = ESysPrivilege.DebugConnectSession;
            else if (strValue.Equals("DELETE ANY TABLE"))
                enumValue = ESysPrivilege.DeleteAnyTable;
            else if (strValue.Equals("DEQUEUE ANY QUEUE"))
                enumValue = ESysPrivilege.DequeueAnyQueue;
            else if (strValue.Equals("DROP ANY CLUSTER"))
                enumValue = ESysPrivilege.DropAnyCluster;
            else if (strValue.Equals("DROP ANY CONTEXT"))
                enumValue = ESysPrivilege.DropAnyContext;
            else if (strValue.Equals("DROP ANY DIMENSION"))
                enumValue = ESysPrivilege.DropAnyDimension;
            else if (strValue.Equals("DROP ANY DIRECTORY"))
                enumValue = ESysPrivilege.DropAnyDirectory;
            else if (strValue.Equals("DROP ANY EVALUATION CONTEXT"))
                enumValue = ESysPrivilege.DropAnyEvaluationContext;
            else if (strValue.Equals("DROP ANY INDEX"))
                enumValue = ESysPrivilege.DropAnyIndex;
            else if (strValue.Equals("DROP ANY INDEXTYPE"))
                enumValue = ESysPrivilege.DropAnyIndextype;
            else if (strValue.Equals("DROP ANY LIBRARY"))
                enumValue = ESysPrivilege.DropAnyLibrary;
            else if (strValue.Equals("DROP ANY MATERIALIZED VIEW"))
                enumValue = ESysPrivilege.DropAnyMaterializedView;
            else if (strValue.Equals("DROP ANY OPERATOR"))
                enumValue = ESysPrivilege.DropAnyOperator;
            else if (strValue.Equals("DROP ANY OUTLINE"))
                enumValue = ESysPrivilege.DropAnyOutline;
            else if (strValue.Equals("DROP ANY PROCEDURE"))
                enumValue = ESysPrivilege.DropAnyProcedure;
            else if (strValue.Equals("DROP ANY ROLE"))
                enumValue = ESysPrivilege.DropAnyRole;
            else if (strValue.Equals("DROP ANY RULE"))
                enumValue = ESysPrivilege.DropAnyRule;
            else if (strValue.Equals("DROP ANY RULE SET"))
                enumValue = ESysPrivilege.DropAnyRuleSet;
            else if (strValue.Equals("DROP ANY SEQUENCE"))
                enumValue = ESysPrivilege.DropAnySequence;
            else if (strValue.Equals("DROP ANY SQL PROFILE"))
                enumValue = ESysPrivilege.DropAnySqlProfile;
            else if (strValue.Equals("DROP ANY SYNONYM"))
                enumValue = ESysPrivilege.DropAnySynonym;
            else if (strValue.Equals("DROP ANY TABLE"))
                enumValue = ESysPrivilege.DropAnyTable;
            else if (strValue.Equals("DROP ANY TRIGGER"))
                enumValue = ESysPrivilege.DropAnyTrigger;
            else if (strValue.Equals("DROP ANY TYPE"))
                enumValue = ESysPrivilege.DropAnyType;
            else if (strValue.Equals("DROP ANY VIEW"))
                enumValue = ESysPrivilege.DropAnyView;
            else if (strValue.Equals("DROP PROFILE"))
                enumValue = ESysPrivilege.DropProfile;
            else if (strValue.Equals("DROP PUBLIC DATABASE LINK"))
                enumValue = ESysPrivilege.DropPublicDatabaseLink;
            else if (strValue.Equals("DROP PUBLIC SYNONYM"))
                enumValue = ESysPrivilege.DropPublicSynonym;
            else if (strValue.Equals("DROP ROLLBACK SEGMENT"))
                enumValue = ESysPrivilege.DropRollbackSegment;
            else if (strValue.Equals("DROP TABLESPACE"))
                enumValue = ESysPrivilege.DropTablespace;
            else if (strValue.Equals("DROP USER"))
                enumValue = ESysPrivilege.DropUser;
            else if (strValue.Equals("ENQUEUE ANY QUEUE"))
                enumValue = ESysPrivilege.EnqueueAnyQueue;
            else if (strValue.Equals("EXECUTE ANY CLASS"))
                enumValue = ESysPrivilege.ExecuteAnyClass;
            else if (strValue.Equals("EXECUTE ANY EVALUATION CONTEXT"))
                enumValue = ESysPrivilege.ExecuteAnyEvaluationContext;
            else if (strValue.Equals("EXECUTE ANY INDEXTYPE"))
                enumValue = ESysPrivilege.ExecuteAnyIndextype;
            else if (strValue.Equals("EXECUTE ANY LIBRARY"))
                enumValue = ESysPrivilege.ExecuteAnyLibrary;
            else if (strValue.Equals("EXECUTE ANY OPERATOR"))
                enumValue = ESysPrivilege.ExecuteAnyOperator;
            else if (strValue.Equals("EXECUTE ANY PROCEDURE"))
                enumValue = ESysPrivilege.ExecuteAnyProcedure;
            else if (strValue.Equals("EXECUTE ANY PROGRAM"))
                enumValue = ESysPrivilege.ExecuteAnyProgram;
            else if (strValue.Equals("EXECUTE ANY RULE"))
                enumValue = ESysPrivilege.ExecuteAnyRule;
            else if (strValue.Equals("EXECUTE ANY RULE SET"))
                enumValue = ESysPrivilege.ExecuteAnyRuleSet;
            else if (strValue.Equals("EXECUTE ANY TYPE"))
                enumValue = ESysPrivilege.ExecuteAnyType;
            else if (strValue.Equals("EXEMPT ACCESS POLICY"))
                enumValue = ESysPrivilege.ExemptAccessPolicy;
            else if (strValue.Equals("EXEMPT IDENTITY POLICY"))
                enumValue = ESysPrivilege.ExemptIdentityPolicy;
            else if (strValue.Equals("EXPORT FULL DATABASE"))
                enumValue = ESysPrivilege.ExportFullDatabase;
            else if (strValue.Equals("FLASHBACK ANY TABLE"))
                enumValue = ESysPrivilege.FlashbackAnyTable;
            else if (strValue.Equals("FORCE ANY TRANSACTION"))
                enumValue = ESysPrivilege.ForceAnyTransaction;
            else if (strValue.Equals("FORCE TRANSACTION"))
                enumValue = ESysPrivilege.ForceTransaction;
            else if (strValue.Equals("GLOBAL QUERY REWRITE"))
                enumValue = ESysPrivilege.GlobalQueryRewrite;
            else if (strValue.Equals("GRANT ANY OBJECT PRIVILEGE"))
                enumValue = ESysPrivilege.GrantAnyObjectPrivilege;
            else if (strValue.Equals("GRANT ANY PRIVILEGE"))
                enumValue = ESysPrivilege.GrantAnyPrivilege;
            else if (strValue.Equals("GRANT ANY ROLE"))
                enumValue = ESysPrivilege.GrantAnyRole;
            else if (strValue.Equals("IMPORT FULL DATABASE"))
                enumValue = ESysPrivilege.ImportFullDatabase;
            else if (strValue.Equals("INSERT ANY TABLE"))
                enumValue = ESysPrivilege.InsertAnyTable;
            else if (strValue.Equals("LOCK ANY TABLE"))
                enumValue = ESysPrivilege.LockAnyTable;
            else if (strValue.Equals("MANAGE ANY FILE GROUP"))
                enumValue = ESysPrivilege.ManageAnyFileGroup;
            else if (strValue.Equals("MANAGE ANY QUEUE"))
                enumValue = ESysPrivilege.ManageAnyQueue;
            else if (strValue.Equals("MANAGE FILE GROUP"))
                enumValue = ESysPrivilege.ManageFileGroup;
            else if (strValue.Equals("MANAGE SCHEDULER"))
                enumValue = ESysPrivilege.ManageScheduler;
            else if (strValue.Equals("MANAGE TABLESPACE"))
                enumValue = ESysPrivilege.ManageTablespace;
            else if (strValue.Equals("MERGE ANY VIEW"))
                enumValue = ESysPrivilege.MergeAnyView;
            else if (strValue.Equals("ON COMMIT REFRESH"))
                enumValue = ESysPrivilege.OnCommitRefresh;
            else if (strValue.Equals("QUERY REWRITE"))
                enumValue = ESysPrivilege.QueryRewrite;
            else if (strValue.Equals("READ ANY FILE GROUP"))
                enumValue = ESysPrivilege.ReadAnyFileGroup;
            else if (strValue.Equals("RESTRICTED SESSION"))
                enumValue = ESysPrivilege.RestrictedSession;
            else if (strValue.Equals("RESUMABLE"))
                enumValue = ESysPrivilege.Resumable;
            else if (strValue.Equals("SELECT ANY DICTIONARY"))
                enumValue = ESysPrivilege.SelectAnyDictionary;
            else if (strValue.Equals("SELECT ANY SEQUENCE"))
                enumValue = ESysPrivilege.SelectAnySequence;
            else if (strValue.Equals("SELECT ANY TABLE"))
                enumValue = ESysPrivilege.SelectAnyTable;
            else if (strValue.Equals("SELECT ANY TRANSACTION"))
                enumValue = ESysPrivilege.SelectAnyTransaction;
            else if (strValue.Equals("SYSDBA"))
                enumValue = ESysPrivilege.Sysdba;
            else if (strValue.Equals("SYSOPER"))
                enumValue = ESysPrivilege.Sysoper;
            else if (strValue.Equals("UNDER ANY TABLE"))
                enumValue = ESysPrivilege.UnderAnyTable;
            else if (strValue.Equals("UNDER ANY TYPE"))
                enumValue = ESysPrivilege.UnderAnyType;
            else if (strValue.Equals("UNDER ANY VIEW"))
                enumValue = ESysPrivilege.UnderAnyView;
            else if (strValue.Equals("UNLIMITED TABLESPACE"))
                enumValue = ESysPrivilege.UnlimitedTablespace;
            else if (strValue.Equals("UPDATE ANY TABLE"))
                enumValue = ESysPrivilege.UpdateAnyTable;
            else
                enumValue = ESysPrivilege.Unknown;

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

    public class EPrivilegeToStringConverter : EnumToStringConverter<ESysPrivilege>
    {

        #region EnumToStringConverter<EPrivilege> Members

        public string EnumValueToString(ESysPrivilege value, object parameter)
        {
            switch (value)
            {
                #region Enum to string branching

                case ESysPrivilege.AdministerAnySqlTuningSet:
                    return "ADMINISTER ANY SQL TUNING SET";
                case ESysPrivilege.AdministerDatabaseTrigger:
                    return "ADMINISTER DATABASE TRIGGER";
                case ESysPrivilege.AdministerResourceManager:
                    return "ADMINISTER RESOURCE MANAGER";
                case ESysPrivilege.AdministerSqlTuningSet:
                    return "ADMINISTER SQL TUNING SET";
                case ESysPrivilege.Advisor:
                    return "ADVISOR";
                case ESysPrivilege.AlterAnyCluster:
                    return "ALTER ANY CLUSTER";
                case ESysPrivilege.AlterAnyDimension:
                    return "ALTER ANY DIMENSION";
                case ESysPrivilege.AlterAnyEvaluationContext:
                    return "ALTER ANY EVALUATION CONTEXT";
                case ESysPrivilege.AlterAnyIndex:
                    return "ALTER ANY INDEX";
                case ESysPrivilege.AlterAnyIndextype:
                    return "ALTER ANY INDEXTYPE";
                case ESysPrivilege.AlterAnyLibrary:
                    return "ALTER ANY LIBRARY";
                case ESysPrivilege.AlterAnyMaterializedView:
                    return "ALTER ANY MATERIALIZED VIEW";
                case ESysPrivilege.AlterAnyOperator:
                    return "ALTER ANY OPERATOR";
                case ESysPrivilege.AlterAnyOutline:
                    return "ALTER ANY OUTLINE";
                case ESysPrivilege.AlterAnyProcedure:
                    return "ALTER ANY PROCEDURE";
                case ESysPrivilege.AlterAnyRole:
                    return "ALTER ANY ROLE";
                case ESysPrivilege.AlterAnyRule:
                    return "ALTER ANY RULE";
                case ESysPrivilege.AlterAnyRuleSet:
                    return "ALTER ANY RULE SET";
                case ESysPrivilege.AlterAnySequence:
                    return "ALTER ANY SEQUENCE";
                case ESysPrivilege.AlterAnySqlProfile:
                    return "ALTER ANY SQL PROFILE";
                case ESysPrivilege.AlterAnyTable:
                    return "ALTER ANY TABLE";
                case ESysPrivilege.AlterAnyTrigger:
                    return "ALTER ANY TRIGGER";
                case ESysPrivilege.AlterAnyType:
                    return "ALTER ANY TYPE";
                case ESysPrivilege.AlterDatabase:
                    return "ALTER DATABASE";
                case ESysPrivilege.AlterProfile:
                    return "ALTER PROFILE";
                case ESysPrivilege.AlterResourceCost:
                    return "ALTER RESOURCE COST";
                case ESysPrivilege.AlterRollbackSegment:
                    return "ALTER ROLLBACK SEGMENT";
                case ESysPrivilege.AlterSession:
                    return "ALTER SESSION";
                case ESysPrivilege.AlterSystem:
                    return "ALTER SYSTEM";
                case ESysPrivilege.AlterTablespace:
                    return "ALTER TABLESPACE";
                case ESysPrivilege.AlterUser:
                    return "ALTER USER";
                case ESysPrivilege.AnalyzeAny:
                    return "ANALYZE ANY";
                case ESysPrivilege.AnalyzeAnyDictionary:
                    return "ANALYZE ANY DICTIONARY";
                case ESysPrivilege.AuditAny:
                    return "AUDIT ANY";
                case ESysPrivilege.AuditSystem:
                    return "AUDIT SYSTEM";
                case ESysPrivilege.BackupAnyTable:
                    return "BACKUP ANY TABLE";
                case ESysPrivilege.BecomeUser:
                    return "BECOME USER";
                case ESysPrivilege.ChangeNotification:
                    return "CHANGE NOTIFICATION";
                case ESysPrivilege.CommentAnyTable:
                    return "COMMENT ANY TABLE";
                case ESysPrivilege.CreateAnyCluster:
                    return "CREATE ANY CLUSTER";
                case ESysPrivilege.CreateAnyContext:
                    return "CREATE ANY CONTEXT";
                case ESysPrivilege.CreateAnyDimension:
                    return "CREATE ANY DIMENSION";
                case ESysPrivilege.CreateAnyDirectory:
                    return "CREATE ANY DIRECTORY";
                case ESysPrivilege.CreateAnyEvaluationContext:
                    return "CREATE ANY EVALUATION CONTEXT";
                case ESysPrivilege.CreateAnyIndex:
                    return "CREATE ANY INDEX";
                case ESysPrivilege.CreateAnyIndextype:
                    return "CREATE ANY INDEXTYPE";
                case ESysPrivilege.CreateAnyJob:
                    return "CREATE ANY JOB";
                case ESysPrivilege.CreateAnyLibrary:
                    return "CREATE ANY LIBRARY";
                case ESysPrivilege.CreateAnyMaterializedView:
                    return "CREATE ANY MATERIALIZED VIEW";
                case ESysPrivilege.CreateAnyOperator:
                    return "CREATE ANY OPERATOR";
                case ESysPrivilege.CreateAnyOutline:
                    return "CREATE ANY OUTLINE";
                case ESysPrivilege.CreateAnyProcedure:
                    return "CREATE ANY PROCEDURE";
                case ESysPrivilege.CreateAnyRule:
                    return "CREATE ANY RULE";
                case ESysPrivilege.CreateAnyRuleSet:
                    return "CREATE ANY RULE SET";
                case ESysPrivilege.CreateAnySequence:
                    return "CREATE ANY SEQUENCE";
                case ESysPrivilege.CreateAnySqlProfile:
                    return "CREATE ANY SQL PROFILE";
                case ESysPrivilege.CreateAnySynonym:
                    return "CREATE ANY SYNONYM";
                case ESysPrivilege.CreateAnyTable:
                    return "CREATE ANY TABLE";
                case ESysPrivilege.CreateAnyTrigger:
                    return "CREATE ANY TRIGGER";
                case ESysPrivilege.CreateAnyType:
                    return "CREATE ANY TYPE";
                case ESysPrivilege.CreateAnyView:
                    return "CREATE ANY VIEW";
                case ESysPrivilege.CreateCluster:
                    return "CREATE CLUSTER";
                case ESysPrivilege.CreateDatabaseLink:
                    return "CREATE DATABASE LINK";
                case ESysPrivilege.CreateDimension:
                    return "CREATE DIMENSION";
                case ESysPrivilege.CreateEvaluationContext:
                    return "CREATE EVALUATION CONTEXT";
                case ESysPrivilege.CreateExternalJob:
                    return "CREATE EXTERNAL JOB";
                case ESysPrivilege.CreateIndextype:
                    return "CREATE INDEXTYPE";
                case ESysPrivilege.CreateJob:
                    return "CREATE JOB";
                case ESysPrivilege.CreateLibrary:
                    return "CREATE LIBRARY";
                case ESysPrivilege.CreateMaterializedView:
                    return "CREATE MATERIALIZED VIEW";
                case ESysPrivilege.CreateOperator:
                    return "CREATE OPERATOR";
                case ESysPrivilege.CreateProcedure:
                    return "CREATE PROCEDURE";
                case ESysPrivilege.CreateProfile:
                    return "CREATE PROFILE";
                case ESysPrivilege.CreatePublicDatabaseLink:
                    return "CREATE PUBLIC DATABASE LINK";
                case ESysPrivilege.CreatePublicSynonym:
                    return "CREATE PUBLIC SYNONYM";
                case ESysPrivilege.CreateRole:
                    return "CREATE ROLE";
                case ESysPrivilege.CreateRollbackSegment:
                    return "CREATE ROLLBACK SEGMENT";
                case ESysPrivilege.CreateRule:
                    return "CREATE RULE";
                case ESysPrivilege.CreateRuleSet:
                    return "CREATE RULE SET";
                case ESysPrivilege.CreateSequence:
                    return "CREATE SEQUENCE";
                case ESysPrivilege.CreateSession:
                    return "CREATE SESSION";
                case ESysPrivilege.CreateSynonym:
                    return "CREATE SYNONYM";
                case ESysPrivilege.CreateTable:
                    return "CREATE TABLE";
                case ESysPrivilege.CreateTablespace:
                    return "CREATE TABLESPACE";
                case ESysPrivilege.CreateTrigger:
                    return "CREATE TRIGGER";
                case ESysPrivilege.CreateType:
                    return "CREATE TYPE";
                case ESysPrivilege.CreateUser:
                    return "CREATE USER";
                case ESysPrivilege.CreateView:
                    return "CREATE VIEW";
                case ESysPrivilege.DebugAnyProcedure:
                    return "DEBUG ANY PROCEDURE";
                case ESysPrivilege.DebugConnectSession:
                    return "DEBUG CONNECT SESSION";
                case ESysPrivilege.DeleteAnyTable:
                    return "DELETE ANY TABLE";
                case ESysPrivilege.DequeueAnyQueue:
                    return "DEQUEUE ANY QUEUE";
                case ESysPrivilege.DropAnyCluster:
                    return "DROP ANY CLUSTER";
                case ESysPrivilege.DropAnyContext:
                    return "DROP ANY CONTEXT";
                case ESysPrivilege.DropAnyDimension:
                    return "DROP ANY DIMENSION";
                case ESysPrivilege.DropAnyDirectory:
                    return "DROP ANY DIRECTORY";
                case ESysPrivilege.DropAnyEvaluationContext:
                    return "DROP ANY EVALUATION CONTEXT";
                case ESysPrivilege.DropAnyIndex:
                    return "DROP ANY INDEX";
                case ESysPrivilege.DropAnyIndextype:
                    return "DROP ANY INDEXTYPE";
                case ESysPrivilege.DropAnyLibrary:
                    return "DROP ANY LIBRARY";
                case ESysPrivilege.DropAnyMaterializedView:
                    return "DROP ANY MATERIALIZED VIEW";
                case ESysPrivilege.DropAnyOperator:
                    return "DROP ANY OPERATOR";
                case ESysPrivilege.DropAnyOutline:
                    return "DROP ANY OUTLINE";
                case ESysPrivilege.DropAnyProcedure:
                    return "DROP ANY PROCEDURE";
                case ESysPrivilege.DropAnyRole:
                    return "DROP ANY ROLE";
                case ESysPrivilege.DropAnyRule:
                    return "DROP ANY RULE";
                case ESysPrivilege.DropAnyRuleSet:
                    return "DROP ANY RULE SET";
                case ESysPrivilege.DropAnySequence:
                    return "DROP ANY SEQUENCE";
                case ESysPrivilege.DropAnySqlProfile:
                    return "DROP ANY SQL PROFILE";
                case ESysPrivilege.DropAnySynonym:
                    return "DROP ANY SYNONYM";
                case ESysPrivilege.DropAnyTable:
                    return "DROP ANY TABLE";
                case ESysPrivilege.DropAnyTrigger:
                    return "DROP ANY TRIGGER";
                case ESysPrivilege.DropAnyType:
                    return "DROP ANY TYPE";
                case ESysPrivilege.DropAnyView:
                    return "DROP ANY VIEW";
                case ESysPrivilege.DropProfile:
                    return "DROP PROFILE";
                case ESysPrivilege.DropPublicDatabaseLink:
                    return "DROP PUBLIC DATABASE LINK";
                case ESysPrivilege.DropPublicSynonym:
                    return "DROP PUBLIC SYNONYM";
                case ESysPrivilege.DropRollbackSegment:
                    return "DROP ROLLBACK SEGMENT";
                case ESysPrivilege.DropTablespace:
                    return "DROP TABLESPACE";
                case ESysPrivilege.DropUser:
                    return "DROP USER";
                case ESysPrivilege.EnqueueAnyQueue:
                    return "ENQUEUE ANY QUEUE";
                case ESysPrivilege.ExecuteAnyClass:
                    return "EXECUTE ANY CLASS";
                case ESysPrivilege.ExecuteAnyEvaluationContext:
                    return "EXECUTE ANY EVALUATION CONTEXT";
                case ESysPrivilege.ExecuteAnyIndextype:
                    return "EXECUTE ANY INDEXTYPE";
                case ESysPrivilege.ExecuteAnyLibrary:
                    return "EXECUTE ANY LIBRARY";
                case ESysPrivilege.ExecuteAnyOperator:
                    return "EXECUTE ANY OPERATOR";
                case ESysPrivilege.ExecuteAnyProcedure:
                    return "EXECUTE ANY PROCEDURE";
                case ESysPrivilege.ExecuteAnyProgram:
                    return "EXECUTE ANY PROGRAM";
                case ESysPrivilege.ExecuteAnyRule:
                    return "EXECUTE ANY RULE";
                case ESysPrivilege.ExecuteAnyRuleSet:
                    return "EXECUTE ANY RULE SET";
                case ESysPrivilege.ExecuteAnyType:
                    return "EXECUTE ANY TYPE";
                case ESysPrivilege.ExemptAccessPolicy:
                    return "EXEMPT ACCESS POLICY";
                case ESysPrivilege.ExemptIdentityPolicy:
                    return "EXEMPT IDENTITY POLICY";
                case ESysPrivilege.ExportFullDatabase:
                    return "EXPORT FULL DATABASE";
                case ESysPrivilege.FlashbackAnyTable:
                    return "FLASHBACK ANY TABLE";
                case ESysPrivilege.ForceAnyTransaction:
                    return "FORCE ANY TRANSACTION";
                case ESysPrivilege.ForceTransaction:
                    return "FORCE TRANSACTION";
                case ESysPrivilege.GlobalQueryRewrite:
                    return "GLOBAL QUERY REWRITE";
                case ESysPrivilege.GrantAnyObjectPrivilege:
                    return "GRANT ANY OBJECT PRIVILEGE";
                case ESysPrivilege.GrantAnyPrivilege:
                    return "GRANT ANY PRIVILEGE";
                case ESysPrivilege.GrantAnyRole:
                    return "GRANT ANY ROLE";
                case ESysPrivilege.ImportFullDatabase:
                    return "IMPORT FULL DATABASE";
                case ESysPrivilege.InsertAnyTable:
                    return "INSERT ANY TABLE";
                case ESysPrivilege.LockAnyTable:
                    return "LOCK ANY TABLE";
                case ESysPrivilege.ManageAnyFileGroup:
                    return "MANAGE ANY FILE GROUP";
                case ESysPrivilege.ManageAnyQueue:
                    return "MANAGE ANY QUEUE";
                case ESysPrivilege.ManageFileGroup:
                    return "MANAGE FILE GROUP";
                case ESysPrivilege.ManageScheduler:
                    return "MANAGE SCHEDULER";
                case ESysPrivilege.ManageTablespace:
                    return "MANAGE TABLESPACE";
                case ESysPrivilege.MergeAnyView:
                    return "MERGE ANY VIEW";
                case ESysPrivilege.OnCommitRefresh:
                    return "ON COMMIT REFRESH";
                case ESysPrivilege.QueryRewrite:
                    return "QUERY REWRITE";
                case ESysPrivilege.ReadAnyFileGroup:
                    return "READ ANY FILE GROUP";
                case ESysPrivilege.RestrictedSession:
                    return "RESTRICTED SESSION";
                case ESysPrivilege.Resumable:
                    return "RESUMABLE";
                case ESysPrivilege.SelectAnyDictionary:
                    return "SELECT ANY DICTIONARY";
                case ESysPrivilege.SelectAnySequence:
                    return "SELECT ANY SEQUENCE";
                case ESysPrivilege.SelectAnyTable:
                    return "SELECT ANY TABLE";
                case ESysPrivilege.SelectAnyTransaction:
                    return "SELECT ANY TRANSACTION";
                case ESysPrivilege.Sysdba:
                    return "SYSDBA";
                case ESysPrivilege.Sysoper:
                    return "SYSOPER";
                case ESysPrivilege.UnderAnyTable:
                    return "UNDER ANY TABLE";
                case ESysPrivilege.UnderAnyType:
                    return "UNDER ANY TYPE";
                case ESysPrivilege.UnderAnyView:
                    return "UNDER ANY VIEW";
                case ESysPrivilege.UnlimitedTablespace:
                    return "UNLIMITED TABLESPACE";
                case ESysPrivilege.UpdateAnyTable:
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
            specs.Add(typeof(ESysPrivilege), new EPrivilegeToStringConverter());
        }

        public static object GetConverter(Type type)
        {
            return specs[type];
        }
    }
}
