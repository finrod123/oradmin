using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace oradmin
{
    using EntityValidators = IEnumerable<IMyValidationAttribute>;
    using PropertyValidators = Dictionary<string, IEnumerable<IMyValidationAttribute>>;
    using ConnectionKey = String;

    public class UpdateException : Exception
    {
        #region Constructor
        public UpdateException(string message, Exception innerException):
            base(message, innerException)
        {

        }
        #endregion
    }
    public class DataReadException : Exception
    {
        #region Constructor
        public DataReadException(string message, Exception innerException) :
            base(message, innerException)
        { }
        #endregion
    }

    public interface IConnectionBase
    {
        string Name { get; set; }
        string Comment { get; set; }
        string UserName { get; set; }
        EDbaPrivileges DbaPrivileges { get; set; }
        bool OsAuthenticate { get; set; }
        ENamingMethod NamingMethod { get; set; }
    }

    [XmlRoot("Connection")]
    public class ConnectionData : IConnectionBase, IEntityDataContainer<ConnectionKey>
    {
        #region IConnectionData Members
        [XmlElement("Name")]
        public string Name { get; set; }
        [XmlElement("Comment")]
        public string Comment { get; set; }
        [XmlElement("UserName")]
        public string UserName { get; set; }
        [XmlElement("DbaPrivileges")]
        public EDbaPrivileges DbaPrivileges { get; set; }
        [XmlElement("OsAuthenticate")]
        public bool OsAuthenticate { get; set; }
        [XmlElement("NamingMethod")]
        public ENamingMethod NamingMethod { get; set; }
        #endregion
        // naming methods specific data
        [XmlElement("TnsName")]
        public string TnsName { get; set; }
        [XmlElement("ConnectDescriptor")]
        public ConnectDescriptorData ConnectDescriptor { get; set; }

        #region IEntityDataContainer<string> Members
        string IEntityDataContainer<string>.DataKey
        {
            get { return this.Name; }
        }
        #endregion
    }

    public class Connection : EntityObjectBase<ConnectionData, ConnectionKey>,
        IConnectionBase, IConnectDescriptorBase
    {
        #region Constants
        public static const string NAME_PROP_STRING = "Name";
        public static const string COMMENT_PROP_STRING= "Comment";
        public static const string USERNAME_PROP_STRING = "UserName";
        public static const string DBAPRIVILEGES_PROP_STRING = "DbaPrivileges";
        public static const string OSAUTHENTICATE_PROP_STRING = "OsAuthenticate";
        public static const string NAMINGMETHOD_PROP_STRING = "NamingMethod";
        public static const string TNSNAME_PROP_STRING = "TnsName";
	    #endregion
        
        #region Constructor
        // !!!TODO: konstrukce z datoveho kontejneru
        public Connection(ConnectionData data, ConnectionManager manager):
            this(manager)
        {
            // construct a connection from a data container
        }
        public Connection(ConnectionManager manager) :
            base(manager)
        {

        }
        #endregion

        #region Helper methods
        protected override void createValidator()
        {
            this.validator = new ConnectionValidator(this,
                new ConnectionValidationServiceProvider(this.manager as ConnectionManager));
        }
        #endregion

        #region IConnectionData Members
        [Tracked]
        public string Name
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        [Tracked]
        public string Comment
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        [Tracked]
        public string UserName
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        [Tracked]
        public EDbaPrivileges DbaPrivileges
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        [Tracked]
        public bool OsAuthenticate
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        [Tracked]
        public ENamingMethod NamingMethod
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        #endregion

        /// <summary>
        /// Tracked by ConnectDescriptor change tracker
        /// </summary>
        #region IConnectDescriptorBase Members
        public string Host
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        public EProtocolType Protocol
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        public int Port
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        public bool IsUsingSid
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        public string ServiceName
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        public string InstanceName
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        public string Sid
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        public EServerType ServerType
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        #endregion
    }

    public class ConnectionDataAdapter : EntityDataAdapter<Connection, ConnectionData, ConnectionKey>
    {
        #region Members
        string connectionsFileName;
        XmlSerializer serializer = new XmlSerializer(typeof(ConnectionData),
                                                     new XmlRootAttribute("Connections"));
        #endregion

        #region Constructor
        public ConnectionDataAdapter(string connectionsFileName)
        {
            if (!File.Exists(connectionsFileName))
                throw new FileNotFoundException("Connections file not found", connectionsFileName);

            this.connectionsFileName = connectionsFileName;
        }
        #endregion
        
        public override bool GetChanges(Connection entity, out ConnectionData data)
        {
            throw new NotImplementedException();
        }
        public override bool GetChanges(IEnumerable<Connection> entities, out IEnumerable<ConnectionData> data)
        {
            throw new NotImplementedException();
        }
        public override bool GetChanges(out IEnumerable<ConnectionData> data)
        {
            // access a file and read all connections
            XmlTextReader reader = new XmlTextReader(new FileStream(this.connectionsFileName, FileMode.Open));

            if (!this.serializer.CanDeserialize(reader))
            {
                data = null;
                return false;
            }

            try
            {
                data = this.serializer.Deserialize(reader) as IEnumerable<ConnectionData>;
                if (data == null)
                    return false;
            }
            catch (InvalidOperationException e)
            {
                throw new DataReadException("Cannot read connections from a file", e);
            }
            finally
            {
                reader.Close();
            }

            return true;
        }
    }

    public class ConnectionManager : EntityManager<Connection, ConnectionData, ConnectionKey>
    {
        #region Constructor
        public ConnectionManager(ConnectionDataAdapter dataAdapter):
            base(null, dataAdapter)
        {
            
        }
        #endregion
        // asi jedina implementovana refresh metoda pro connections
        public override bool Refresh()
        {
            
        }

        // asi nebude implementovano
        public override bool Refresh(IEnumerable<Connection> entities)
        {
            throw new NotImplementedException();
        }
        public override bool BelongsTo(ConnectionData keyedData)
        {
            return true;
        }
        public override Connection CreateObject()
        {
            return new Connection(this);
        }
    }

    /// <summary>
    /// Trida provadejici ukladani pripojeni
    /// </summary>
    public class ConnectionSaver :
        IEntityDataSaver<ConnectionManager, Connection, ConnectionData, ConnectionKey>
    {

        #region Members
        string connectionsFileName;
        XmlSerializer serializer = new XmlSerializer(typeof(ConnectionData),
                                                     new XmlRootAttribute("Connections"));
        #endregion

        #region Constructor
        public ConnectionSaver(string connectionsFileName)
        {
            if (!File.Exists(connectionsFileName))
                throw new FileNotFoundException("Connections file not found!", connectionsFileName);

            this.connectionsFileName = connectionsFileName;
        }
        #endregion

        #region IEntityDataSaver<ConnectionManager,Connection,ConnectionData,string> Members

        /// <summary>
        /// Currently the only implemented saving method
        /// </summary>
        /// <param name="entityManager">Connection manager</param>
        public void Update(ConnectionManager entityManager)
        {
            // open the connections file and truncate it
            XmlTextWriter writer = new XmlTextWriter(
                new FileStream(this.connectionsFileName, FileMode.Truncate), Encoding.Default);

            try
            {
                // serialize connections
                serializer.Serialize(writer,
                    entityManager.View.SourceCollection as IEnumerable<Connection>);
            }
            catch (InvalidOperationException e)
            {
                throw new UpdateException("Connections cannot be serialized!", e);
            }
            finally
            {
                writer.Close();
            }
        }
        public void Update(IEnumerable<Connection> entities)
        {
            throw new NotImplementedException();
        }
        public void Update(Connection entity)
        {
            throw new NotImplementedException();
        }
        #endregion
    }

    public class ConnectionValidator : EntityValidator<Connection, ConnectionData, ConnectionKey>
    {
        #region Static members
        static EntityValidators entityValidators;
        static PropertyValidators propertyValidators;
        static ValidationContext validationContext =
            new ValidationContext(typeof(Connection), null,
                new ConnectionValidationServiceProvider());
        #endregion
        
    }

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

    public class ConnectionChangeTracker : EntityChangeTracker<Connection, ConnectionData, string>
    {
        #region EntityChangeTracker<Connection, ConnectionData, string> members
        protected override void createVersionedFields()
        {
            this.versionedFields = new Dictionary<string, VersionedFieldBase>
            {
                { Connection.NAME_PROP_STRING, new VersionedFieldClonable<string>(this.entity.Name)}
            };
        }

        protected override void readInitialData()
        {
            throw new NotImplementedException();
        } 
        #endregion
    }
}