using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace oradmin
{
    /// <summary>
    /// Interface for describing a listener to connect to and database
    /// service identification data
    /// </summary>
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

    public struct ConnectDescriptorData : IConnectDescriptor
    {
        #region Constants
        public static readonly int DEFAULT_PORT = 1521;
        public static readonly int MIN_PORT = 0;
        public static readonly int MAX_PORT = 65535;
        #endregion
        #region Member data
        private string host = string.Empty;
        private int port = DEFAULT_PORT;
        private bool usingSid = false;
        private string serviceName = string.Empty;
        private string instanceName = string.Empty;
        private string sid = string.Empty;
        private EServerType serverType = EServerType.Dedicated;
        #endregion
        #region Properties
        public string Host
        {
            get { return host; }
            set
            {
                host = value;
            }
        }
        public int Port
        {
            get { return port; }
            set
            {
                port = value;
            }
        }
        public bool UsingSid
        {
            get { return usingSid; }
            set { usingSid = value;}
        }
        public string ServiceName
        {
            get { return serviceName; }
            set { this.serviceName = value;}
        }
        public string InstanceName
        {
            get { return instanceName; }
            set { this.instanceName = value;}
        }
        public string Sid
        {
            get { return sid; }
            set { this.sid = value;}
        }
        public EServerType ServerType
        {
            get { return serverType; }
            set { this.serverType = value; }
        }
        #endregion Properties
    }

    public class ConnectDescriptor : IConnectDescriptor, INotifyPropertyChanged, IEditableObject,
        IValidatableObject<EConnectDescriptorError>
    {
        #region Members
        /// <summary>
        /// Is in editing state?
        /// </summary>
        private bool isEditing;
        /// <summary>
        /// indikator pritomnosti chyb v datech objektu
        /// </summary>
        private bool hasErrors = false;
        private Dictionary<EConnectDescriptorError, ObjectError<EConnectDescriptorError>> errors =
            new Dictionary<EConnectDescriptorError, ObjectError<EConnectDescriptorError>>();
        #endregion
        #region  Current and backup data
        ConnectDescriptorData data, backupData;
        #endregion

        #region Constructor
        public ConnectDescriptor()
        { }
        #endregion

        #region Properties
        public bool Editing
        {
            get { return isEditing; }
        }

        #region IConnectDescriptor Members
        public string Host
        {
            get { return this.data.Host; }
            set
            {
                if (isEditing)
                {
                    if (this.data.Host != value)
                    {
                        this.data.Host = value;
                        OnPropertyChanged("Host");
                    }
                }
            }
        }
        public int Port
        {
            get { return this.data.Port; }
            set
            {
                if (isEditing)
                {
                    if (this.data.Port != value)
                    {
                        this.data.Port = value;
                        OnPropertyChanged("Port");
                    }
                }
            }
        }
        public bool UsingSid
        {
            get { return this.data.UsingSid; }
            set
            {
                if (isEditing)
                {
                    if (this.data.UsingSid != value)
                    {
                        this.data.UsingSid = value;
                        OnPropertyChanged("UsingSid");
                    }
                }
            }
        }
        public string ServiceName
        {
            get { return this.data.ServiceName; }
            set
            {
                if (isEditing)
                {
                    if (this.data.ServiceName != value)
                    {
                        this.data.ServiceName = value;
                        OnPropertyChanged("ServiceName");
                    }
                }
            }
        }
        public string InstanceName
        {
            get { return this.data.InstanceName; }
            set
            {
                if (isEditing)
                {
                    if (this.data.InstanceName != value)
                    {
                        this.data.InstanceName = value;
                        OnPropertyChanged("InstanceName");
                    }
                }
            }
        }
        public string Sid
        {
            get { return this.data.Sid; }
            set
            {
                if (isEditing)
                {
                    if (this.data.Sid != value)
                    {
                        this.data.Sid = value;
                        OnPropertyChanged("Sid");
                    }
                }
            }
        }
        public EServerType ServerType
        {
            get { return this.data.ServerType; }
            set
            {
                if (isEditing)
                {
                    if (this.data.ServerType != value)
                    {
                        this.data.ServerType = value;
                        OnPropertyChanged("ServerType");
                    }
                }
            }
        }
        #endregion

        #endregion

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
        #region IEditableObject Members
        public void BeginEdit()
        {
            if (isEditing)
                return;

            // begin editing
            this.isEditing = true;
            this.backupData = this.data;
        }
        public void CancelEdit()
        {
            if (!isEditing)
                return;

            // cancel editing
            this.data = this.backupData;
            this.isEditing = false;
        }
        /// <summary>
        /// Ends editing: discards the backup data; validation is required prior to calling
        /// this function
        /// </summary>
        public void EndEdit()
        {
            if (!this.isEditing)
                return;

            this.backupData = new ConnectDescriptorData();
            this.isEditing = false;
        }
        #endregion

        #region Helper methods
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region IValidatableObject<EConnectDescriptorError> Members
        public bool HasErrors
        {
            get { return this.hasErrors; }
        }
        public ReadOnlyCollection<ObjectError<EConnectDescriptorError>> Errors
        {
            get { return this.errors.Values.ToList().AsReadOnly(); }
        }
        public bool Validate(out ReadOnlyCollection<ObjectError<EConnectDescriptorError>> errorsList)
        {
            // the object is valid while not editing
            if (!isEditing)
            {
                errorsList = null;
                return true;
            }
            
            // check validity
            errors.Clear();
            hasErrors = false;

            // walk through all properties and check them
            if (string.IsNullOrEmpty(this.data.Host))
            {
                errors.Add(EConnectDescriptorError.EmptyHost,
                    new ObjectError<EConnectDescriptorError>(
                    this, EConnectDescriptorError.EmptyHost, "Host is empty"));
                hasErrors = true;
            }

            int port = this.data.Port;
            if (port < ConnectDescriptorData.MIN_PORT ||
               ConnectDescriptorData.MAX_PORT < port)
            {
                errors.Add(EConnectDescriptorError.InvalidPort,
                    new ObjectError<EConnectDescriptorError>(
                    this, EConnectDescriptorError.InvalidPort, "Port out of range"));
                hasErrors = true;
            }

            if (this.data.UsingSid)
            {
                if (string.IsNullOrEmpty(this.data.Sid))
                {
                    errors.Add(EConnectDescriptorError.EmptySid,
                        new ObjectError<EConnectDescriptorError>(
                        this, EConnectDescriptorError.EmptySid, "Empty sid"));
                    hasErrors = true;
                }
            }
            else if (string.IsNullOrEmpty(this.data.ServiceName))
            {
                errors.Add(EConnectDescriptorError.EmptyServiceName,
                    new ObjectError<EConnectDescriptorError>(
                    this, EConnectDescriptorError.EmptyServiceName, "Empty service name"));
                hasErrors = true;
            }

            if (!Enum.IsDefined(typeof(EServerType), this.data.ServerType))
            {
                errors.Add(EConnectDescriptorError.InvalidServerType,
                    new ObjectError<EConnectDescriptorError>(
                    this, EConnectDescriptorError.InvalidServerType, "Invalid server type"));
                hasErrors = true;
            }

            errorsList = errors.Values.ToList().AsReadOnly();
            return hasErrors;
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
