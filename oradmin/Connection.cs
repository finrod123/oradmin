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
    using ConnectionKey = String;

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
    public class ConnectionData : IConnectionBase, IEntityDataContainer<string>
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

    public class Connection : EntityObject<ConnectionKey>, IConnectionBase, IConnectDescriptorBase
    {
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

        public override void BeginEdit()
        {
            throw new NotImplementedException();
        }
        public override void CancelEdit()
        {
            throw new NotImplementedException();
        }
        public override void EndEdit()
        {
            throw new NotImplementedException();
        }
        public override void RejectChanges()
        {
            throw new NotImplementedException();
        }
        public override void AcceptChanges()
        {
            throw new NotImplementedException();
        }
        public override bool IsChanged
        {
            get { throw new NotImplementedException(); }
        }
        public override bool HasErrors
        {
            get { throw new NotImplementedException(); }
        }
        /// <summary>
        /// Merges connection data into connection object
        /// </summary>
        /// <param name="data"></param>
        /// <param name="mergeOptions"></param>
        public override void Merge(IEntityDataContainer<string> data, EMergeOptions mergeOptions)
        {
            throw new NotImplementedException();
        }
    }

    public class ConnectionDataAdapter : EntityDataAdapter<Connection, ConnectionData, string>
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

            data = this.serializer.Deserialize(reader) as IEnumerable<ConnectionData>;

            if (data == null)
                return false;

            return true;
        }
    }

    public class ConnectionManager : EntityManager<Connection, ConnectionData, string>
    {
        #region Constructor
        public ConnectionManager(ConnectionDataAdapter dataAdapter):
            base(dataAdapter)
        {

        }
        #endregion

        #region Helper methods
        
        #endregion

        ///!!!TODO - pridelit klice a vhodny stav!
        public override Connection CreateObject()
        {
            return new Connection(this);
        }
    }

    public class ConnectionValidator : EntityValidator
    {
        static ConnectionValidator()
        {
            Initialize(typeof(ConnectionValidator), typeof(Connection));
        }

        public ConnectionValidator(Connection connection,
            ConnectionValidationServiceProvider provider):
            base(connection, provider)
        {}
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
}