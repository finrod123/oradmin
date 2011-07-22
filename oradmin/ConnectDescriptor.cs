using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace oradmin
{
    public delegate void ObjectLockedChanged(bool locked);

    public class ObjectError<T>
        where T : struct
    {
        #region Members

        object o;
        T errorCode;
        string message;
        
        #endregion

        #region  Properties

        public T Error
        {
            get { return errorCode; }
        }
        public string Message
        {
            get { return message; }
        }
        public object Object
        {
            get { return o; }
        }

        #endregion

        public ObjectError(object o, T errorCode, string message)
        {
            this.errorCode = errorCode;
            this.message = message;
            this.o = o;
        }
    }

    public interface IConnectDescriptor
    {
        string Host { get; }
        int Port { get; }
        bool UsingSid { get; }
        string ServiceName { get; }
        string InstanceName { get; }
        string Sid { get; }
        EServerType ServerType { get; }
    }

    public class ConnectDescriptorData : IConnectDescriptor, INotifyPropertyChanged, IDisposable
    {
        #region constants
        public static readonly int DEFAULT_PORT = 1521;
        public static readonly int MIN_PORT = 0;
        public static readonly int MAX_PORT = 65535;
        #endregion

        #region Constructor

        public ConnectDescriptorData() { }
        public ConnectDescriptorData(IConnectDescriptor data)
        {
            host = data.Host;
            port = data.Port;

            if (usingSid = data.UsingSid)
            {
                sid = data.Sid;
                serviceName = string.Empty;
                instanceName = string.Empty;
            } else
            {
                serviceName = data.ServiceName;
                instanceName = data.InstanceName;
                sid = string.Empty;
            }

            serverType = data.ServerType;
        }

        #endregion

        #region Member data
        private bool valid;
        private bool locked = false;
        private string host = string.Empty;
        private int port = DEFAULT_PORT;
        private bool usingSid = false;
        private string serviceName = string.Empty;
        private string instanceName = string.Empty;
        private string sid = string.Empty;
        private EServerType serverType = EServerType.Dedicated;
        #endregion


        #region Events
        public event ObjectInvalidated Invalidated;
        public event ObjectLockedChanged LockedChanged;
        #endregion

        #region Properties

        public bool Valid
        {
            get { return valid; }
        }
        public bool Locked
        {
            get { return locked; }
            set
            {
                if (locked != value)
                {
                    locked = value;
                    OnLocked(value);
                }
            }
        }

        public string Host
        {
            get { return host; }
            set
            {
                propertySetAndNotify<string>(ref host, value, "Host");
            }
        }
        public int Port
        {
            get { return port; }
            set
            {
                propertySetAndNotify<int>(ref port, value, "Port");
            }
        }
        public bool UsingSid
        {
            get { return usingSid; }
            set { propertySetAndNotify<bool>(ref usingSid, value, "UsingSid"); }
        }
        public string ServiceName
        {
            get { return serviceName; }
            set { propertySetAndNotify<string>(ref serviceName, value, "ServiceName"); }
        }
        public string InstanceName
        {
            get { return instanceName; }
            set { propertySetAndNotify<string>(ref instanceName, value, "InstanceName"); }
        }
        public string Sid
        {
            get { return sid; }
            set { propertySetAndNotify<string>(ref sid, value, "Sid"); }
        }
        public EServerType ServerType
        {
            get { return serverType; }
            set { propertySetAndNotify<EServerType>(ref serverType, value, "ServerType"); }
        }

        #endregion Properties


        #region Helper methods
        private void propertySetAndNotify<T>(ref T property, T newValue, string propertyName)
        {
            if (!EqualityComparer<T>.Default.Equals(property, newValue))
            {
                property = newValue;
                if (!locked)
                    OnPropertyChanged(propertyName);
            }
        }
        private void OnInvalidated()
        {
            valid = false;
            if (Invalidated != null)
            {
                Invalidated();
            }
        }
        private void OnLocked(bool locked)
        {
            if (LockedChanged != null)
            {
                LockedChanged(locked);
            }
        }
        #endregion
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        #endregion


        #region IDisposable Members

        public void Dispose()
        {
            OnInvalidated();
        }

        #endregion
    }

    public class CachingConnectDescriptor : IConnectDescriptor, INotifyPropertyChanged
    {
        #region Members

        bool valid = true;
        protected bool sourceLocked = false;
        protected ConnectDescriptorData data;

        /// <summary>
        /// Cached data
        /// </summary>
        private string host;
        private int port;
        private bool usingSid;
        private string serviceName;
        private string instanceName;
        private string sid;
        private EServerType serverType;

        #endregion

        #region Constructor
        public CachingConnectDescriptor(ConnectDescriptorData data)
        {
            if (data == null)
                throw new ArgumentNullException("Connect descriptor data");

            this.data = data;
            // set up handlers
            data.Invalidated += new ObjectInvalidated(data_Invalidated);
            data.PropertyChanged += new PropertyChangedEventHandler(data_PropertyChanged);
            data.LockedChanged += new ObjectLockedChanged(data_LockedChanged);
        }

        #endregion
        
        #region Helper methods
        void data_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(!sourceLocked)
                OnPropertyChanged(e.PropertyName);
        }
        void data_Invalidated()
        {
            OnInvalidated();
        }
        void data_LockedChanged(bool locked)
        {
            sourceLocked = locked;
            RefreshCachedData();
        }
        protected void OnInvalidated()
        {
            valid = false;
            if(Invalidated != null)
            {
                Invalidated();
            }
        }
        void checkValidity()
        {
            if (!valid)
                throw new ObjectDisposedException("Connect descriptor data container disposed",
                    new Exception());
        }
        public void RefreshCachedData()
        {
            string nHost = data.Host;
            int nPort = data.Port;
            bool nUsingSid = data.UsingSid;
            string nServiceName = data.ServiceName;
            string nInstanceName = data.InstanceName;
            string nSid = data.Sid;
            EServerType nServerType = data.ServerType;

            if (nHost != host)
                OnPropertyChanged("Host");
            if (nPort != port)
                OnPropertyChanged("Port");
            if (nUsingSid != usingSid)
                OnPropertyChanged("UsingSid");

            if (nUsingSid)
            {
                if (nSid != sid)
                    OnPropertyChanged("Sid");

                if (!usingSid)
                {
                    OnPropertyChanged("ServiceName");
                    OnPropertyChanged("InstanceName");
                }
            } else
            {
                if (nServiceName != serviceName)
                    OnPropertyChanged("ServiceName");
                if (nInstanceName != instanceName)
                    OnPropertyChanged("InstanceName");

                if (usingSid)
                    OnPropertyChanged("Sid");
            }

            if (nServerType != serverType)
                OnPropertyChanged("ServerType");

            // nastav hodnoty
            host = nHost;
            port = nPort;
            usingSid = nUsingSid;
            serviceName = nServiceName;
            instanceName = nInstanceName;
            sid = nSid;
            serverType = nServerType;
        }
        #endregion

        #region Events
        public event ObjectInvalidated Invalidated;
        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
        #endregion

        #region Properties
        public bool Valid
        {
            get { return valid; }
        }

        #region IConnectDescriptor Members
        public string Host
        {
            get
            {
                checkValidity();

                if (sourceLocked)
                    return host;
                else
                    return data.Host;
            }
        }
        public int Port
        {
            get
            {
                checkValidity();

                if (sourceLocked)
                    return port;
                else
                    return data.Port;
            }
        }
        public bool UsingSid
        {
            get
            {
                checkValidity();

                if (sourceLocked)
                    return usingSid;
                else
                    return data.UsingSid;
            }
        }
        public string ServiceName
        {
            get
            {
                checkValidity();

                if (sourceLocked)
                    return serviceName;
                else
                    return data.ServiceName;
            }
        }
        public string InstanceName
        {
            get
            {
                checkValidity();

                if (sourceLocked)
                    return instanceName;
                else
                    return data.InstanceName;
            }
        }
        public string Sid
        {
            get
            {
                checkValidity();

                if (sourceLocked)
                    return sid;
                else
                    return data.Sid;
            }
        }
        public EServerType ServerType
        {
            get
            {
                checkValidity();

                if (sourceLocked)
                    return serverType;
                else
                    return data.ServerType;
            }
        }
        #endregion
        #endregion 
    }

    class ReadOnlyConnectDescriptor : CachingConnectDescriptor
    {
        #region Constructor

        public ReadOnlyConnectDescriptor(ConnectDescriptorData data) :
            base(data)
        { }

        #endregion
    }

    public class ConnectDescriptor : CachingConnectDescriptor, IConnectDescriptor
    {
        #region Members
        /// <summary>
        /// Is in editing state?
        /// </summary>
        private bool editing;
        /// <summary>
        /// indikator pritomnosti chyb v datech objektu
        /// </summary>
        private bool hasErrors = false;
        private List<ObjectError<EConnectDescriptorError>> errors = new List<ObjectError<EConnectDescriptorError>>();

        #endregion

        #region  Backup and cache data
        private string bHost;
        private int bPort;
        private bool bUsingSid;
        private string bServiceName;
        private string bInstanceName;
        private string bSid;
        private EServerType bServerType;
        #endregion


        #region Constructor

        public ConnectDescriptor(ConnectDescriptorData data) :
            base(data)
        { }

        #endregion

        #region Properties

        public bool Editing
        {
            get { return editing; }
        }
        public new string Host
        {
            get
            {
                return base.Host;
            }
            set { propertySetter<string>(ref bHost, value, "Host"); }
        }
        public new int Port
        {
            get
            {
                return base.Port;
            }
            set { propertySetter<int>(ref bPort, value, "Port"); }
        }
        public new bool UsingSid
        {
            get
            {
                return base.UsingSid;
            }
            set { propertySetter<bool>(ref bUsingSid, value, "UsingSid"); }
        }
        public new string ServiceName
        {
            get
            {
                return base.ServiceName;
            }
            set { propertySetter<string>(ref bServiceName, value, "ServiceName"); }
        }
        public new string InstanceName
        {
            get
            {
                return base.InstanceName;
            }
            set { propertySetter<string>(ref bInstanceName, value, "InstanceName"); }
        }
        public new string Sid
        {
            get
            {
                return base.Sid;
            }
            set { propertySetter<string>(ref bSid, value, "Sid"); }
        }
        public new EServerType ServerType
        {
            get
            {
                return base.ServerType;
            }
            set
            {
                if (!Enum.IsDefined(typeof(EServerType), value))
                    throw new ArgumentOutOfRangeException("ServerType");
                else
                    propertySetter<EServerType>(ref bServerType, value, "ServerType");
            }
        }

        public bool HasErrors
        {
            get { return hasErrors; }
        }
        public ReadOnlyCollection<ObjectError<EConnectDescriptorError>> Errors
        {
            get { return errors.AsReadOnly(); }
        }

        #endregion

        #region Public interface
        public bool Validate()
        {
            clearErrors();

            if (string.IsNullOrEmpty(Host))
            {
                hasErrors = true;
                errors.Add(new ObjectError<EConnectDescriptorError>(
                    this,
                    EConnectDescriptorError.EmptyHost,
                    "Empty host"));
            }

            if (Port < ConnectDescriptorData.MIN_PORT ||
               Port > ConnectDescriptorData.MAX_PORT)
            {
                hasErrors = true;
                errors.Add(new ObjectError<EConnectDescriptorError>(
                    this,
                    EConnectDescriptorError.InvalidPort,
                    "Invalid port"));
            }

            if (UsingSid &&
               string.IsNullOrEmpty(ServiceName))
            {
                hasErrors = true;
                errors.Add(new ObjectError<EConnectDescriptorError>(
                    this,
                    EConnectDescriptorError.EmptyServiceName,
                    "Empty service name"));
            }

            if (!UsingSid &&
               string.IsNullOrEmpty(Sid))
            {
                hasErrors = true;
                errors.Add(new ObjectError<EConnectDescriptorError>(
                    this,
                    EConnectDescriptorError.EmptySid,
                    "Empty SID"));
            }

            if (!Enum.IsDefined(typeof(EServerType), ServerType))
            {
                hasErrors = true;
                errors.Add(new ObjectError<EConnectDescriptorError>(
                    this,
                    EConnectDescriptorError.InvalidServerType,
                    "Invalid server type"));
            }

            return hasErrors;
        }
        #endregion

        #region Helper methods

        private void data_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }
        private void data_Invalidated()
        {
            OnInvalidated();
        }
        private void propertySetter<T>(ref T property, T newValue, string propertyName)
        {
            if (editing)
            {
                if (!EqualityComparer<T>.Default.Equals(property, newValue))
                {
                    property = newValue;
                    OnPropertyChanged(propertyName);
                }
            } else
                throw new InvalidOperationException("Not editing!");
        }
        public void clearErrors()
        {
            hasErrors = false;
            errors.Clear();
        }
        private void resetValues()
        {
            bHost = Host;
            bPort = Port;
            bUsingSid = UsingSid;
            bServiceName = ServiceName;
            bInstanceName = InstanceName;
            bSid = Sid;
            bServerType = ServerType;
        }

        #endregion

        #region IEditableObject Members

        public void BeginEdit()
        {
            if (!editing)
            {
                resetValues();
                editing = true;
            }
        }
        public void CancelEdit()
        {
            editing = false;
        }
        /// <summary>
        /// Provede validaci objektu a zapise nove hodnoty
        /// </summary>
        public void EndEdit()
        {
            if (!editing)
                return;

            if (Validate())
            {
                if (sourceLocked)
                    throw new Exception("Data locked");

                lock(data)
                {
                    data.Locked = true;
                    data.Host = bHost;
                    data.Port = bPort;
                    if (data.UsingSid = bUsingSid)
                    {
                        data.Sid = bSid;
                        data.ServiceName = string.Empty;
                        data.InstanceName = string.Empty;
                    } else
                    {
                        data.ServiceName = bServiceName;
                        data.InstanceName = bInstanceName;
                        data.Sid = string.Empty;
                    }

                    data.ServerType = bServerType;
                }
            }
        }

        #endregion
    }

    public enum EConnectDescriptorError
    {
        EmptyHost,
        EmptyServiceName,
        EmptySid,
        InvalidPort,
        InvalidServerType
    }
}
