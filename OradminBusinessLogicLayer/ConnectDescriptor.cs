using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using myentitylibrary;

namespace oradminbl
{
    using ConnectDescriptorKey = IConnectDescriptorBase;

    public interface IConnectDescriptorBase
    {
        // listener identification data
        string Host { get; set; }
        EProtocolType Protocol { get; set; }
        int Port { get; set; }
        /// <summary>
        /// connect data section
        /// </summary>
        /// is connect descriptor using sid?
        bool IsUsingSid { get; set; }
        // usingSid == false
        string ServiceName { get; set; }
        string InstanceName { get; set; }
        // usingSid == true
        string Sid { get; set; }

        // database server process type to use
        EServerType ServerType { get; set; }
    }
    
    [XmlRoot("ConnectDescriptor")]
    public class ConnectDescriptorData :
        IConnectDescriptorBase,
        IEntityDataContainer<IConnectDescriptorBase>,
        IEquatable<IConnectDescriptorBase>
    {
        #region IConnectDescriptorData Members
        [XmlElement("Host")]
        public string Host { get; set; }
        [XmlElement("Protocol")]
        public EProtocolType Protocol { get; set; }
        [XmlElement("Port")]
        public int Port { get; set; }
        [XmlElement("UsingSid")]
        public bool IsUsingSid { get; set; }
        [XmlElement("ServiceName")]
        public string ServiceName { get; set; }
        [XmlElement("InstanceName")]
        public string InstanceName { get; set; }
        [XmlElement("Sid")]
        public string Sid { get; set; }
        [XmlElement("ServerType")]
        public EServerType ServerType { get; set; }
        #endregion

        #region IEntityDataContainer<ConnectDescriptorData> Members
        public IConnectDescriptorBase DataKey
        {
            get { return this; }
        }
        #endregion

        #region IEquatable<IConnectDescriptorBase> Members
        public bool Equals(IConnectDescriptorBase other)
        {
            bool equals =
                this.Host.Equals(other.Host) &&
                this.Port.Equals(other.Port) &&
                this.Protocol.Equals(other.Protocol) &&
                this.ServerType.Equals(other.ServerType) &&
                this.IsUsingSid.Equals(other.IsUsingSid);

            if (!equals)
                return false;

            if (this.IsUsingSid)
            {
                equals = this.Sid.Equals(other.Sid);
            } else
            {
                equals =
                    this.ServiceName.Equals(other.ServiceName) &&
                    (
                     (string.IsNullOrEmpty(this.InstanceName) &&
                      string.IsNullOrEmpty(other.InstanceName))
                      ||
                     (!string.IsNullOrEmpty(this.InstanceName) &&
                      !string.IsNullOrEmpty(other.InstanceName) &&
                      this.InstanceName.Equals(other.InstanceName))
                      );
            }

            return equals;
        }
        #endregion
    }

    public class ConnectDescriptor : EntityObjectBase<ConnectDescriptorData, ConnectDescriptorKey>,
        IConnectDescriptorBase
    {
        #region Constants
        public static const string HOST_PROP_STRING = "Host";
        public static const string PROTOCOL_PROP_STRING = "Protocol";
        public static const string PORT_PROP_STRING = "Port";
        public static const string ISUSINGSID_PROP_STRING = "IsUsingSid";
        public static const string SERVICENAME_PROP_STRING = "ServiceName";
        public static const string INSTANCENAME_PROP_STRING = "InstanceName";
        public static const string SID_PROP_STRING = "Sid";
        public static const string SERVERTYPE_PROP_STRING = "ServerType";
        #endregion

        #region IConnectDescriptorData Members
        public string Host { get; set; }
        public EProtocolType Protocol { get; set; }
        public int Port { get; set; }
        public bool IsUsingSid { get; set; }
        public string ServiceName { get; set; }
        public string InstanceName { get; set; }
        public string Sid { get; set; }
        public EServerType ServerType { get; set; }
        #endregion

        public override void RejectChanges()
        {
            throw new NotImplementedException();
        }
        protected override void readCurrentData(ConnectDescriptorData data)
        {
            throw new NotImplementedException();
        }
        public override IConnectDescriptorBase DataKey
        {
            get { return this; }
        }
    }

    public class ConnectDescriptorChangeTracker :
        EntityChangeTracker<ConnectDescriptor, ConnectDescriptorData, ConnectDescriptorKey>
    {
        protected override void createVersionedFields()
        {
            throw new NotImplementedException();
        }
        protected override void readInitialData()
        {
            throw new NotImplementedException();
        }
        protected override bool readChanges()
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
        public override bool Merge(ConnectDescriptorData data, EMergeOptions mergeOptions)
        {
            throw new NotImplementedException();
        }
    }
}
