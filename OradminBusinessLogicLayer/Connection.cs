using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using myentitylibrary;

namespace oradminbl
{
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

    /// <summary>
    /// Basic interface for connection entities and data containers
    /// </summary>
    public interface IConnectionBase
    {
        string Name { get; set; }
        string Comment { get; set; }
        string UserName { get; set; }
        EDbaPrivileges DbaPrivileges { get; set; }
        bool OsAuthenticate { get; set; }
        ENamingMethod NamingMethod { get; set; }
    }

    public interface IConnection : IConnectionBase, IConnectDescriptorBase
    {
        string TnsName { get; set; }
    }

    [XmlRoot("Connection")]
    public class ConnectionData :
        IEntityDataContainer<ConnectionKey>,
        IConnectionBase
    {
        #region IConnectionBase Members
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
        IConnection,
        IBusinessObject
    {
        #region Members
        string name,
               comment,
               userName;

        EDbaPrivileges dbaPrivileges;
        bool osAuthenticate;
        ENamingMethod namingMethod;

        string tnsName;
        #endregion

        #region Constants
        public static const string NAME_PROP_STRING = "Name";
        public static const string COMMENT_PROP_STRING = "Comment";
        public static const string USERNAME_PROP_STRING = "UserName";
        public static const string DBAPRIVILEGES_PROP_STRING = "DbaPrivileges";
        public static const string OSAUTHENTICATE_PROP_STRING = "OsAuthenticate";
        public static const string NAMINGMETHOD_PROP_STRING = "NamingMethod";
        public static const string TNSNAME_PROP_STRING = "TnsName";
        #endregion

        #region Constructor
        public Connection()
        {

        }
        #endregion
        
        public override void RejectChanges()
        {
            throw new NotImplementedException();
        }
        protected override void readCurrentData(ConnectionData data)
        {
            throw new NotImplementedException();
        }
        public override string DataKey
        {
            get { return this.Name; }
        }

        #region IConnectionBase Members
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.propertySetter(ref this.name, value, Connection.NAME_PROP_STRING);
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

        #region IBusinessObject Members
        public string BusinessObjectName
        {
            get { return this.Name; }
        }
        #endregion

        #region IConnection Members

        public string TnsName
        {
            get
            {
                return this.tnsName;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}