using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace oradmin
{
    public delegate void ObjectLockedChanged(bool locked);

    class ObjectError<T>
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
        string Host { get; set; }
        int Port { get; set; }
        bool UsingSid { get; set; }
        string ServiceName { get; set; }
        string InstanceName { get; set; }
        string Sid { get; set; }
        EServerType ServerType { get; set; }
    }

    class ConnectDescriptorData : IConnectDescriptor, INotifyPropertyChanged, IDisposable
    {
        #region constants
        public static readonly int DEFAULT_PORT = 1521;
        public static readonly int MIN_PORT = 0;
        public static readonly int MAX_PORT = 65535;
        #endregion

        protected ConnectDescriptorData() { }

        #region Member data
        private bool locked = false;
        private string host = string.Empty;
        private int port = DEFAULT_PORT;
        private bool usingSid = false;
        private string serviceName = string.Empty;
        private string instanceName = string.Empty;
        private string sid = string.Empty;
        private EServerType serverType = EServerType.Dedicated;
        #endregion

        #region Properties

        public bool Locked
        {
            get { return locked; }
            set
            {
                if (!locked && value)
                {
                    OnLocked(true);
                    locked = true;
                } else if (locked && !value)
                {
                    OnLocked(false);
                    locked = false;
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

        #region Events
        public event ObjectInvalidated Invalidated;
        public event ObjectLockedChanged LockedChanged;
        #endregion

        #region Helper methods
        private void propertySetAndNotify<T>(ref T property, T newValue, string propertyName)
        {
            if (!EqualityComparer<T>.Default.Equals(property, newValue))
            {
                property = newValue;
                OnPropertyChanged(propertyName);
            }
        }
        private void OnInvalidated()
        {
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

    class ReadOnlyConnectDescriptor : IConnectDescriptor, INotifyPropertyChanged
    {
        #region Members
        private bool valid = true;
        private bool sourceLocked = false;
        ConnectDescriptorData data;

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

        public ReadOnlyConnectDescriptor(ConnectDescriptorData data)
        {
            if (data == null)
                throw new ArgumentNullException("Connect descriptor data container");

            this.data = data;
            // load data to cache
            refreshCachedData();
            // set up handlers for data container events
            data.Invalidated += new ObjectInvalidated(data_Invalidated);
            data.PropertyChanged += new PropertyChangedEventHandler(data_PropertyChanged);
            data.LockedChanged += new ObjectLockedChanged(data_Locked);
        }

        void data_Locked(bool locked)
        {
            if (locked)
            {
                refreshCachedData();
                sourceLocked = true;
            } else
            {
                sourceLocked = false;
                refreshCachedData();
            }
        }

        #endregion

        #region Properties

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

        #region Helper methods
        void data_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }

        void data_Invalidated()
        {
            valid = false;
            OnInvalidated();
        }

        void checkValidity()
        {
            if (!valid)
                throw new ObjectDisposedException("Connect descriptor data container disposed",
                    new Exception());
        }

        void refreshCachedData()
        {
            host = data.Host;
            port = data.Port;
            usingSid = data.UsingSid;
            serviceName = data.ServiceName;
            instanceName = data.InstanceName;
            sid = data.Sid;
            serverType = data.ServerType;
        }

        #endregion

        #region Events

        public event ObjectInvalidated Invalidated;
        private void OnInvalidated()
        {
            if (Invalidated != null)
            {
                Invalidated();
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion


    }

    public class ConnectDescriptor : IEditableObject, INotifyPropertyChanged
    {
        #region Members

        /// <summary>
        /// Is in editing state?
        /// </summary>
        private bool editing;
        /// <summary>
        /// Is valid handle?
        /// </summary>
        private bool valid;
        private bool sourceLocked;
        /// <summary>
        /// indikator pritomnosti chyb v datech objektu
        /// </summary>
        private bool hasErrors;
        private List<ObjectError<EConnectDescriptorError>> errors;

        /// <summary>
        /// Originalni datovy kontejner
        /// </summary>
        ConnectDescriptorData data;
        #endregion


        #region  Backup and cache data
        private string bHost, cHost;
        private int bPort, cPort;
        private bool bUsingSid, cUsingSid;
        private string bServiceName, cServiceName;
        private string bInstanceName, cInstanceName;
        private string bSid, cSid;
        private EServerType bServerType, cServerType;
        #endregion


        #region Constructor

        public ConnectDescriptor(ConnectDescriptorData data)
        {
            if (data == null)
                throw new ArgumentException("Null data container!");

            this.data = data;
            editing = false;

            // set up error indication
            valid = true;
            hasErrors = false;
            errors = new List<ObjectError<EConnectDescriptorError>>();            

            // register with data container events
            data.PropertyChanged += new PropertyChangedEventHandler(data_PropertyChanged);
            data.Invalidated += new ObjectInvalidated(data_Invalidated);
            data.LockedChanged += new ObjectLockedChanged(data_Locked);
        }

        void data_Locked(bool locked)
        {
            if (locked)
            {
                refreshCachedData();
                sourceLocked = true;
            } else
            {
                sourceLocked = false;
                refreshCachedData();
            }
        }

        #endregion

        #region Properties

        public bool IsValid
        {
            get { return valid; }
        }
        public bool IsEditing
        {
            get { return editing; }
        }
        
        public string Host
        {
            get
            {
                checkValidity();
                
                if (editing)
                    return bHost;

                if (sourceLocked)
                    return cHost;

                return data.Host;
            }
            set { propertySetter<string>(ref bHost, value, "Host"); }
        }
        public int Port
        {
            get
            {
                checkValidity();

                if (editing)
                    return bPort;

                if (sourceLocked)
                    return cPort;

                return data.Port;
            }
            set { propertySetter<int>(ref bPort, value, "Port"); }
        }
        public bool UsingSid
        {
            get
            {
                checkValidity();

                if (editing)
                    return bUsingSid;

                if (sourceLocked)
                    return cUsingSid;

                return data.UsingSid;
            }
            set { propertySetter<bool>(ref bUsingSid, value, "UsingSid"); }
        }
        public string ServiceName
        {
            get
            {
                checkValidity();

                if (editing)
                    return bServiceName;

                if (sourceLocked)
                    return cServiceName;

                return data.ServiceName;
            }
            set { propertySetter<string>(ref bServiceName, value, "ServiceName"); }
        }
        public string InstanceName
        {
            get
            {
                checkValidity();

                if (editing)
                    return bInstanceName;

                if (sourceLocked)
                    return cInstanceName;

                return data.InstanceName;
            }
            set { propertySetter<string>(ref bInstanceName, value, "InstanceName"); }
        }
        public string Sid
        {
            get
            {
                checkValidity();

                if (editing)
                    return bSid;

                if (sourceLocked)
                    return cSid;

                return data.Sid;
            }
            set { propertySetter<string>(ref bSid, value, "Sid"); }
        }
        public EServerType ServerType
        {
            get
            {
                checkValidity();

                if (editing)
                    return bServerType;

                if (sourceLocked)
                    return cServerType;

                return data.ServerType;
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

        private void checkValidity()
        {
            if (!valid)
                throw new ObjectDisposedException("Object disposed", new Exception());
        }

        private void data_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }
        private void data_Invalidated()
        {
            valid = false;
        }

        private void propertySetter<T>(ref T property, T newValue, string propertyName)
        {
            if (!valid)
                throw new ObjectDisposedException("Object disposed", new Exception());

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

        private void refreshCachedData()
        {
            cHost = data.Host;
            cPort = data.Port;
            cUsingSid = data.UsingSid;
            cServiceName = data.ServiceName;
            cInstanceName = data.InstanceName;
            cSid = data.Sid;
            cServerType = data.ServerType;
        }

        private void resetValues()
        {
            if (sourceLocked)
            {
                bHost = cHost;
                bPort = cPort;
                bUsingSid = cUsingSid;
                bServiceName = cServiceName;
                bInstanceName = cInstanceName;
                bSid = cSid;
                bServerType = cServerType;
            } else
            {
                bHost = data.Host;
                bPort = data.Port;
                bUsingSid = data.UsingSid;
                bServiceName = data.ServiceName;
                bInstanceName = data.InstanceName;
                bSid = data.Sid;
                bServerType = data.ServerType;
            }
        }
        public void clearErrors()
        {
            hasErrors = false;
            errors.Clear();
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
                object o = new object();
                lock (o)
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
