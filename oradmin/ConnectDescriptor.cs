using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace oradmin
{
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
    public class ConnectDescriptorData : IConnectDescriptorBase
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
    }

    public class ConnectDescriptor : IConnectDescriptorBase, IEntityWithChangeTracker
    {
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
    }

    public class ConnectDescriptorChangeTracker : IEntityChangeTracker
    {

    }

    public enum EProtocolType
    {
        Tcp,
        Ipc
    }
}
