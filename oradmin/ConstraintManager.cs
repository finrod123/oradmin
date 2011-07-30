using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

namespace oradmin
{
    public delegate void AllConstraintsRefreshedHandler();
    public delegate void ConstraintsRefreshedHandler(string schema);
    // ---TODO:
    // dokoncit inicializaci clenu v konstruktoru
    // properties
    // tridy privilegii
    // nacitani privilegii
    public class ConstraintManager
    {
        #region Members
        #region SQL SELECTS
        public static const string ALL_CONSTRAINTS_SELECT = @"
            SELECT
                owner, constraint_name, constraint_type, table_name,
                search_condition, r_owner, r_constraint_name,
                delete_rule, status, deferrable, deferred,
                validated, rely, invalid
            FROM
                ALL_CONSTRAINTS";
        public static const string ALL_CONSTRAINTS_SCHEMA_SELECT = @"
            SELECT
                owner, constraint_name, constraint_type, table_name,
                search_condition, r_owner, r_constraint_name,
                delete_rule, status, deferrable, deferred,
                validated, rely, invalid
            FROM
                ALL_CONSTRAINTS
            WHERE
                owner = :owner";
        public static const string ALL_CONSTRAINTS_TABLE_SELECT = @"
            SELECT
                owner, constraint_name, constraint_type, table_name,
                search_condition, r_owner, r_constraint_name,
                delete_rule, status, deferrable, deferred,
                validated, rely, invalid
            FROM
                ALL_CONSTRAINTS
            WHERE
                owner = :owner and
                table_name = :table_name";
        #endregion
        SessionManager.Session session;
        ConstraintColumnManager columnManager;
        OracleConnection conn;

        List<ConstraintBase> constraints = new List<ConstraintBase>();
        #endregion

        #region Constructor
        public ConstraintManager(SessionManager.Session session)
        {
            if (session == null)
                throw new ArgumentNullException("Session");

            this.session = session;
            this.columnManager = new ConstraintColumnManager(this.session);
            this.conn = this.session.Connection;
        }
        #endregion

        #region Public interface
        public void Refresh()
        {
            OracleCommand cmd = new OracleCommand(ALL_CONSTRAINTS_SELECT, conn);
            OracleDataReader odr = cmd.ExecuteReader();

            if (!odr.HasRows)
                return;

            // purge old data
            constraints.Clear();

            while (odr.Read())
            {
                ConstraintBase constraint = LoadConstraint(odr);
                constraints.Add(constraint);
            }
        }
        public bool Refresh(string schema)
        {
            OracleCommand cmd = new OracleCommand(ALL_CONSTRAINTS_SCHEMA_SELECT, conn);
            cmd.BindByName = true;
            // set up parameters
            OracleParameter schemaParam = cmd.CreateParameter();
            schemaParam.ParameterName = "owner";
            schemaParam.OracleDbType = OracleDbType.Char;
            schemaParam.Direction = System.Data.ParameterDirection.Input;
            schemaParam.Value = schema;
            // add it
            cmd.Parameters.Add(schemaParam);
            // execute
            OracleDataReader odr = cmd.ExecuteReader();

            if (!odr.HasRows)
                return false;

            // purge old data
            purgeConstraintData(schema);

            while (odr.Read())
            {
                ConstraintBase constraint = LoadConstraint(odr);
                constraints.Add(constraint);
            }

            return true;
        }
        public bool Refresh(TableManager.Table table)
        {
            OracleCommand cmd = new OracleCommand(ALL_CONSTRAINTS_TABLE_SELECT, conn);
            cmd.BindByName = true;
            // set up parameters
            // schemaParam
            OracleParameter schemaParam = cmd.CreateParameter();
            schemaParam.ParameterName = "owner";
            schemaParam.OracleDbType = OracleDbType.Char;
            schemaParam.Direction = System.Data.ParameterDirection.Input;
            schemaParam.Value = table.Owner;
            cmd.Parameters.Add(schemaParam);
            // tableParam
            OracleParameter tableParam = cmd.CreateParameter();
            tableParam.ParameterName = "table_name";
            tableParam.OracleDbType = OracleDbType.Char;
            tableParam.Direction = System.Data.ParameterDirection.Input;
            tableParam.Value = table.Name;
            cmd.Parameters.Add(tableParam);
            // execute
            OracleDataReader odr = cmd.ExecuteReader();

            if (!odr.HasRows)
                return false;

            // purge old data
            purgeConstraintData(table);

            while (odr.Read())
            {
                ConstraintBase constraint = LoadConstraint(odr);
                constraints.Add(constraint);
            }

            return true;
        }
        #endregion

        #region Helper methods
        private void purgeConstraintData(string schema)
        {
            constraints.RemoveAll((constraint) => (schema.Equals(constraint.Owner))); 
        }
        private void purgeConstraintData(TableManager.Table table)
        {
            constraints.RemoveAll((constraint) => (constraint.Table == table));
        }
        #endregion

        #region Public static interface
        /// <summary>
        /// Loads an instance of a constraint out of an OracleDataReader result set
        /// </summary>
        /// <param name="odr">OracleDataReader instance</param>
        /// <returns>Loaded constraint</returns>
        public static ConstraintBase LoadConstraint(OracleDataReader odr)
        {
            ConstraintBase constraint;
            string owner;
            string constraintName;
            EConstraintType? constraintType = null;
            string tableName;
            EConstraintStatus? constraintStatus = null;
            EConstraintDeferrability? deferrable = null;
            EConstraintDeferredState? deferred = null;
            EConstraintValidation? validation = null;
            EConstraintReliability? reliability = null;
            bool? invalid = null;


        }
        #endregion

        #region Constraint class
        public abstract class ConstraintBase
        {
            #region Members
            protected string owner;
            protected string tableName;
            protected TableManager.Table tableRef;
            protected string constraintName;
            protected EConstraintType? constraintType;
            protected EConstraintStatus? constraintStatus;
            protected EConstraintDeferrability? deferrableState;
            protected EConstraintDeferredState? deferredState;
            protected EConstraintValidation? validationState;
            protected EConstraintReliability? reliabilityState;
            protected bool? invalid;
            #endregion

            #region Constructor
            public ConstraintBase(
                string owner,
                string tableName,
                string constraintName,
                EConstraintType? constraintType,
                EConstraintStatus? constraintStatus,
                EConstraintDeferrability? deferrableState,
                EConstraintDeferredState? deferredState,
                EConstraintValidation? validationState,
                EConstraintReliability? reliabilityState,
                bool? invalid)
            {
                this.owner = owner;
                this.tableName = tableName;
                this.tableRef = null;
                this.constraintName = constraintName;
                this.constraintType = constraintType;
                this.deferrableState = deferrableState;
                this.deferredState = deferredState;
                this.validationState = validationState;
                this.reliabilityState = reliabilityState;
                this.invalid = invalid;
            }
            #endregion

            #region Properties
            public string Owner
            {
                get { return owner; }
                set { this.owner = value; }
            }
            public string TableName
            {
                get { return tableName; }
                set { tableName = value; }
            }
            public TableManager.Table Table
            {
                get { return tableRef; }
                set { tableRef = value; }
            }
            public string ConstraintName
            {
                get { return constraintName; }
            }
            public EConstraintType? ConstraintType
            {
                get { return constraintType; }
            }
            public EConstraintStatus? ConstraintStatus
            {
                get { return constraintStatus; }
                set { constraintStatus = value;}
            }
            public EConstraintDeferrability? Deferrability
            {
                get { return deferrableState; }
                set { deferrableState = value; }
            }
            public EConstraintDeferredState? DeferredState
            {
                get { return deferredState; }
                set { deferredState = value; }
            }
            public EConstraintValidation? ValidationState
            {
                get { return validationState; }
                set { validationState = value; }
            }
            public EConstraintReliability? ReliabilityState
            {
                get { return reliabilityState; }
                set { reliabilityState = value; }
            }
            public bool? Invalid
            {
                get { return invalid; }
                set { invalid = value; }
            }
            #endregion
        }
        #endregion

        public class CheckConstraint : ConstraintBase
        {
            #region Members
            string searchCondition;
            #endregion

            #region Constructor
            public CheckConstraint(
                string owner,
                string tableName,
                TableManager.Table tableRef,
                string constraintName,
                EConstraintStatus? constraintStatus,
                EConstraintDeferrability? deferrableState,
                EConstraintDeferredState? deferredState,
                EConstraintValidation? validationState,
                EConstraintReliability? reliabilityState,
                bool? invalid,
                string searchCondition) :
                base(owner, tableName, constraintName, EConstraintType.Check, constraintStatus,
                      deferrableState, deferredState, validationState, reliabilityState, invalid)
            {
                this.searchCondition = searchCondition;
            }
            #endregion

            #region Properties
            public string SearchCondition
            {
                get { return searchCondition; }
            }
            #endregion
        }

        public abstract class ColumnBasedConstraint : ConstraintBase
        {
            #region Members
            protected List<ConstraintColumnManager.ConstraintColumn> columns =
                new List<ConstraintColumnManager.ConstraintColumn>();
            #endregion

            #region Constructor
            public ColumnBasedConstraint(
                string owner,
                string tableName,
                string constraintName,
                EConstraintType? constraintType,
                EConstraintStatus? constraintStatus,
                EConstraintDeferrability? deferrableState,
                EConstraintDeferredState? deferredState,
                EConstraintValidation? validationState,
                EConstraintReliability? reliabilityState,
                bool? invalid) :
                base(owner, tableName, constraintName, constraintType, constraintStatus,
                 deferrableState, deferredState, validationState, reliabilityState, invalid)
            { }
            #endregion

            #region Properties
            public ReadOnlyCollection<ConstraintColumnManager.ConstraintColumn> Columns
            {
                get { return columns.AsReadOnly(); }
            }
            #endregion
        }

        public abstract class CandidateKeyConstraint : ColumnBasedConstraint
        {
            public CandidateKeyConstraint(
                string owner,
                string tableName,
                string constraintName,
                EConstraintType? constraintType,
                EConstraintStatus? constraintStatus,
                EConstraintDeferrability? deferrableState,
                EConstraintDeferredState? deferredState,
                EConstraintValidation? validationState,
                EConstraintReliability? reliabilityState,
                bool? invalid) :
                base(owner, tableName, constraintName, constraintType, constraintStatus,
                      deferrableState, deferredState, validationState, reliabilityState, invalid)
            { }
        }

        public class PrimaryKeyConstraint : CandidateKeyConstraint
        {
            public PrimaryKeyConstraint(
                string owner,
                string tableName,
                string constraintName,
                EConstraintStatus? constraintStatus,
                EConstraintDeferrability? deferrableState,
                EConstraintDeferredState? deferredState,
                EConstraintValidation? validationState,
                EConstraintReliability? reliabilityState,
                bool? invalid) :
                base(owner, tableName, constraintName, EConstraintType.PrimaryKey, constraintStatus,
                 deferrableState, deferredState, validationState, reliabilityState, invalid)
            { }
        }

        public class UniqueKeyConstraint : CandidateKeyConstraint
        {
            public UniqueKeyConstraint(
                string owner,
                string tableName,
                string constraintName,
                EConstraintStatus? constraintStatus,
                EConstraintDeferrability? deferrableState,
                EConstraintDeferredState? deferredState,
                EConstraintValidation? validationState,
                EConstraintReliability? reliabilityState,
                bool? invalid) :
                base(owner, tableName, constraintName, EConstraintType.UniqueKey, constraintStatus,
                 deferrableState, deferredState, validationState, reliabilityState, invalid)
            { }
        }

        public class ForeignKeyConstraint : ColumnBasedConstraint
        {
            #region Members
            string referencedConstraintName;
            string referencedTableOwner;
            CandidateKeyConstraint referencedConstraint;
            EDeleteRule? deleteRule;
            #endregion

            public ForeignKeyConstraint(
                string owner,
                string tableName,
                string constraintName,
                EConstraintType? constraintType,
                EConstraintStatus? constraintStatus,
                EConstraintDeferrability? deferrableState,
                EConstraintDeferredState? deferredState,
                EConstraintValidation? validationState,
                EConstraintReliability? reliabilityState,
                bool? invalid,
                string referencedTableOwner,
                string referencedConstraintName,
                EDeleteRule? deleteRule) :
                base(owner, tableName, constraintName, EConstraintType.ForeignKey, constraintStatus,
                       deferrableState, deferredState, validationState, reliabilityState, invalid)
            {
                this.deleteRule = deleteRule;
                this.referencedTableOwner = referencedTableOwner;
                this.referencedConstraintName = referencedConstraintName;
            }

            #region Properties
            public string ReferencedTableOwner
            {
                get { return this.referencedTableOwner; }
            }
            public string ReferencedConstraintName
            {
                get { return this.referencedConstraintName; }
            }
            public CandidateKeyConstraint ReferencedConstraint
            {
                get { return referencedConstraint; }
                set { this.referencedConstraint = value; }
            }
            public EDeleteRule? DeleteRule
            {
                get { return this.deleteRule; }
            }
            #endregion
        }
    }

    public enum EConstraintType
    {
        PrimaryKey,
        UniqueKey,
        ForeignKey,
        Check,
        // for views
        WithCheckOption,
        WithReadOnly
    }
    public enum EConstraintStatus
    {
        Enabled,
        Disabled
    }
    public enum EConstraintDeferrability
    {
        Deferrable,
        NoDeferrable
    }
    public enum EConstraintDeferredState
    {
        Immediate,
        Deferred
    }
    public enum EConstraintValidation
    {
        Validated,
        NotValidated
    }
    public enum EConstraintReliability
    {
        NoRely,
        Rely
    }
    public enum EDeleteRule
    {
        Cascade,
        NoAction
    }
}
