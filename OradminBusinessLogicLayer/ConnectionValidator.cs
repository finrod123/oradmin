using System;
using System.Collections.Generic;
using myentitylibrary;

namespace oradminbl
{
    using ConnectionKey = String;
    using PropertyValidatorsEnumerations =
        Dictionary<string, IEnumerable<IMyValidationPropertyAttribute>>;
    using CrossPropertyValidatorsEnumeration = IEnumerable<IMyValidationCrossPropertyAttribute>;
    using EntityValidatorsEnumeration = IEnumerable<IMyValidationEntityAttribute>;
    using CrossEntityValidatorsEnumeration = IEnumerable<IMyValidationCrossEntityAttribute>;
    

    public class ConnectionValidator : EntityValidator<Connection, ConnectionData, ConnectionKey>
    {
        #region Static members
        /// <summary>
        /// Static constructor to be run to load custom static members such as
        /// property names and validation attributes of various levels
        /// </summary>
        static ConnectionValidator()
        {
            ConnectionValidator.Initialize(
                typeof(ConnectionValidator),
                out ConnectionValidator.crossEntityValidators,
                out ConnectionValidator.entityValidators,
                out ConnectionValidator.crossPropertyValidators,
                out ConnectionValidator.propertyValidators,
                out ConnectionValidator.propertyNames);
        }

        #region Validators
        static PropertyValidatorsEnumerations propertyValidators;
        static CrossPropertyValidatorsEnumeration crossPropertyValidators;
        static EntityValidatorsEnumeration entityValidators;
        static CrossEntityValidatorsEnumeration crossEntityValidators; 
        static List<string> propertyNames;
        #endregion

        static ValidationContext validationContext;
        #endregion

        #region Constructor
        public ConnectionValidator(Connection connection) :
            base(connection,
             propertyNames,
             crossEntityValidators, entityValidators, crossPropertyValidators, propertyValidators,
             validationContext)
        {

        }
        #endregion

        protected override void initializePropertyValues()
        {
            this.propertyValues[Connection.NAME_PROP_STRING] = this.entity.Name;
            this.propertyValues[Connection.COMMENT_PROP_STRING] = this.entity.Comment;
            this.propertyValues[Connection.USERNAME_PROP_STRING] = this.entity.UserName;
            this.propertyValues[Connection.OSAUTHENTICATE_PROP_STRING] = this.entity.OsAuthenticate;
            this.propertyValues[Connection.DBAPRIVILEGES_PROP_STRING] = this.entity.DbaPrivileges;
            this.propertyValues[Connection.NAMINGMETHOD_PROP_STRING] = this.entity.NamingMethod;
            this.propertyValues[Connection.TNSNAME_PROP_STRING] = this.entity.TnsName;

            // cache also connect descriptor values
            this.propertyValues[ConnectDescriptor.HOST_PROP_STRING] = this.entity.Host;
            this.propertyValues[ConnectDescriptor.PORT_PROP_STRING] = this.entity.Port;
            this.propertyValues[ConnectDescriptor.PROTOCOL_PROP_STRING] = this.entity.Protocol;
            this.propertyValues[ConnectDescriptor.ISUSINGSID_PROP_STRING] = this.entity.IsUsingSid;
            this.propertyValues[ConnectDescriptor.SID_PROP_STRING] = this.entity.Sid;
            this.propertyValues[ConnectDescriptor.SERVICENAME_PROP_STRING] = this.entity.ServiceName;
            this.propertyValues[ConnectDescriptor.INSTANCENAME_PROP_STRING] = this.entity.InstanceName;
            this.propertyValues[ConnectDescriptor.SERVERTYPE_PROP_STRING] = this.entity.ServerType;
        }
    }

    public class ConnectionOpenTestValidator : EntityValidator<Connection, ConnectionData, ConnectionKey>
    {
        /// <summary>
        /// Static constructor to be run to load custom static members such as
        /// property names and validation attributes of various levels
        /// </summary>
        static ConnectionOpenTestValidator()
        {
            ConnectionOpenTestValidator.Initialize(
                typeof(ConnectionOpenTestValidator),
                out ConnectionOpenTestValidator.crossEntityValidators,
                out ConnectionOpenTestValidator.entityValidators,
                out ConnectionOpenTestValidator.crossPropertyValidators,
                out ConnectionOpenTestValidator.propertyValidators,
                out ConnectionOpenTestValidator.propertyNames);
        }

        #region Validators
        static PropertyValidatorsEnumerations propertyValidators;
        static CrossPropertyValidatorsEnumeration crossPropertyValidators;
        static EntityValidatorsEnumeration entityValidators;
        static CrossEntityValidatorsEnumeration crossEntityValidators; 
        static List<string> propertyNames;
        #endregion

        /// <summary>
        /// Validation context static instance
        /// </summary>
        static ValidationContext validationContext;

        #region Constructor
        public ConnectionOpenTestValidator(Connection connection) :
            base(connection,
                  ConnectionOpenTestValidator.propertyNames,
                  ConnectionOpenTestValidator.crossEntityValidators,
                  ConnectionOpenTestValidator.entityValidators,
                  ConnectionOpenTestValidator.crossPropertyValidators,
                  ConnectionOpenTestValidator.propertyValidators,
                  ConnectionOpenTestValidator.validationContext)
        { }
        #endregion

        protected override void initializePropertyValues()
        {
            this.propertyValues[Connection.USERNAME_PROP_STRING] = this.entity.UserName;
            this.propertyValues[Connection.OSAUTHENTICATE_PROP_STRING] = this.entity.OsAuthenticate;
            this.propertyValues[Connection.DBAPRIVILEGES_PROP_STRING] = this.entity.DbaPrivileges;
            this.propertyValues[Connection.NAMINGMETHOD_PROP_STRING] = this.entity.NamingMethod;
            this.propertyValues[Connection.TNSNAME_PROP_STRING] = this.entity.TnsName;

            // cache also connect descriptor values
            this.propertyValues[ConnectDescriptor.HOST_PROP_STRING] = this.entity.Host;
            this.propertyValues[ConnectDescriptor.PORT_PROP_STRING] = this.entity.Port;
            this.propertyValues[ConnectDescriptor.PROTOCOL_PROP_STRING] = this.entity.Protocol;
            this.propertyValues[ConnectDescriptor.ISUSINGSID_PROP_STRING] = this.entity.IsUsingSid;
            this.propertyValues[ConnectDescriptor.SID_PROP_STRING] = this.entity.Sid;
            this.propertyValues[ConnectDescriptor.SERVICENAME_PROP_STRING] = this.entity.ServiceName;
            this.propertyValues[ConnectDescriptor.INSTANCENAME_PROP_STRING] = this.entity.InstanceName;
            this.propertyValues[ConnectDescriptor.SERVERTYPE_PROP_STRING] = this.entity.ServerType;
        }
    }

    /// <summary>
    /// Helper service provider class to provide services to a connection validator
    /// </summary>
    public class ConnectionValidationServiceProvider : IServiceProvider
    {
        #region Members
        ConnectionManager manager;
        #endregion

        #region Constructor
        public ConnectionValidationServiceProvider(ConnectionManager manager)
        {
            if (manager == null)
                throw new ArgumentNullException("manager");

            this.manager = manager;
        }
        #endregion

        #region IServiceProvider Members
        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(Connection))
            {
                return this.manager;
            }

            return new object();
        }
        #endregion
    }
}