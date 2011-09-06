using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace oradmin
{
    public interface IConnection
    {
        string Name { get; }
        string Comment { get; }
        string UserName { get; }
        EDbaPrivileges DbaPrivileges { get; }
        bool OsAuthenticate { get; }
        ENamingMethod NamingMethod { get; }
        string TnsName { get; }
    }

    public struct ConnectionData : IConnection
    {
        #region Members
        /// <summary>
        /// Business logic related members
        /// </summary>
        private string name = string.Empty;
        private string comment = string.Empty;
        private string userName = string.Empty;
        private EDbaPrivileges dbaPrivileges = EDbaPrivileges.Normal;
        private bool osAuthenticate = false;
        private ENamingMethod namingMethod = ENamingMethod.ConnectDescriptor;
        private string tnsName;
        #endregion

        #region IConnection Members
        public string Name
        {
            get { return this.name; }
            set
            {
                this.name = value;
            }
        }
        public string Comment
        {
            get { return this.comment; }
            set
            {
                this.comment = value;
            }
        }
        public string UserName
        {
            get { return this.userName; }
            set
            {
                this.userName = value;
            }
        }
        public EDbaPrivileges DbaPrivileges
        {
            get { return this.dbaPrivileges; }
            set
            {
                if (Enum.IsDefined(typeof(EDbaPrivileges), value))
                {
                    this.dbaPrivileges = value;
                }
            }
        }
        public bool OsAuthenticate
        {
            get { return this.osAuthenticate; }
            set
            {
                this.osAuthenticate = value;
            }
        }
        public ENamingMethod NamingMethod
        {
            get { return this.namingMethod; }
            set
            {
                if (Enum.IsDefined(typeof(ENamingMethod), value))
                {
                    this.namingMethod = value;
                }
            }
        }
        public string TnsName
        {
            get { return this.tnsName; }
            set
            {
                this.tnsName = value;
            }
        }
        #endregion
    }
    public class Connection : IConnection, IConnectDescriptor,
        INotifyPropertyChanged, IEditableObject,
        IValidatableObject<EConnectionError>
    {
        #region Members
        // manager reference
        ConnectionManager manager;
        // current and backup data
        private readonly int id;
        ConnectionData data, backupData;
        ConnectDescriptor connectDescriptor;
        // editable pattern data
        bool isEditing = false;
        // sequence generator
        SequenceGenerator generator = new SequenceGenerator(0, int.MaxValue, 1, false);
        // validation data
        bool hasErrors = false;
        Dictionary<EConnectionError, ObjectError<EConnectionError>> errors =
            new Dictionary<EConnectionError, ObjectError<EConnectionError>>();
        #endregion

        #region Constructor
        public Connection(ConnectionManager manager)
        {
            if (manager == null)
                throw new ArgumentNullException("Connection manager");

            this.id = generator.Next;
            this.manager = manager;
            connectDescriptor = new ConnectDescriptor();
            // set up events
            connectDescriptor.PropertyChanged += new PropertyChangedEventHandler(connectDescriptor_PropertyChanged);
        }
        #endregion

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Properties
        public int Id
        {
            get { return this.id; }
        }
        public ConnectionManager Manager
        {
            get { return this.manager; }
        }
        public bool Editing
        {
            get { return this.isEditing; }
        }

        #region IConnection Members
        public string Name
        {
            get { return this.data.Name; }
            set
            {
                if (this.isEditing)
                {
                    if (!this.data.Name.Equals(value))
                    {
                        this.data.Name = value;
                        OnPropertyChanged("Name");
                    }
                }
            }
        }
        public string Comment
        {
            get { return this.data.Comment; }
            set
            {
                if (this.isEditing)
                {
                    if (!this.data.Comment.Equals(value))
                    {
                        this.data.Comment = value;
                        OnPropertyChanged("Comment");
                    }
                }
            }
        }
        public string UserName
        {
            get { return this.data.UserName; }
            set
            {
                if (this.isEditing)
                {
                    if (!this.data.Equals(value))
                    {
                        this.data.UserName = value;
                        OnPropertyChanged("UserName");
                    }
                }
            }
        }
        public EDbaPrivileges DbaPrivileges
        {
            get { return this.data.DbaPrivileges; }
            set
            {
                if (this.isEditing)
                {
                    if (!EqualityComparer<EDbaPrivileges>.Default.Equals(
                        this.data.DbaPrivileges, value))
                    {
                        this.data.DbaPrivileges = value;
                        OnPropertyChanged("DbaPrivileges");
                    }
                }
            }
        }
        public bool OsAuthenticate
        {
            get { return this.data.OsAuthenticate; }
            set
            {
                if (this.isEditing)
                {
                    if (this.data.OsAuthenticate != value)
                    {
                        this.data.OsAuthenticate = value;
                        OnPropertyChanged("OsAuthenticate");
                    }
                }
            }
        }
        public ENamingMethod NamingMethod
        {
            get { return this.data.NamingMethod; }
            set
            {
                if (this.isEditing)
                {
                    if (!EqualityComparer<ENamingMethod>.Default.Equals(
                        this.data.NamingMethod, value))
                    {
                        this.data.NamingMethod = value;
                        OnPropertyChanged("NamingMethod");
                    }
                }
            }
        }
        public string TnsName
        {
            get { return this.data.TnsName; }
            set
            {
                if (this.isEditing)
                {
                    if (!this.data.TnsName.Equals(value))
                    {
                        this.data.TnsName = value;
                        OnPropertyChanged("TnsName");
                    }
                }
            }
        }
        #endregion

        #region IConnectDescriptor Members
        public string Host
        {
            get { return this.connectDescriptor.Host; }
            set
            {
                if (this.isEditing)
                    this.connectDescriptor.Host = value;
            }
        }
        public int Port
        {
            get { return this.connectDescriptor.Port; }
            set
            {
                if (this.isEditing)
                    this.connectDescriptor.Port = value;
            }
        }
        public bool UsingSid
        {
            get { return this.connectDescriptor.UsingSid; }
            set
            {
                if (this.isEditing)
                    this.connectDescriptor.UsingSid = value;
            }
        }
        public string ServiceName
        {
            get { return this.connectDescriptor.ServiceName; }
            set
            {
                if (this.isEditing)
                    this.connectDescriptor.ServiceName = value;
            }
        }
        public string InstanceName
        {
            get { return this.connectDescriptor.InstanceName; }
            set
            {
                if (this.isEditing)
                    this.connectDescriptor.InstanceName = value;
            }
        }
        public string Sid
        {
            get { return this.connectDescriptor.Sid; }
            set
            {
                if (this.isEditing)
                    this.connectDescriptor.Sid = value;
            }
        }
        public EServerType ServerType
        {
            get { return this.connectDescriptor.ServerType; }
            set
            {
                if (this.isEditing)
                    this.connectDescriptor.ServerType = value;
            }
        }
        #endregion

        #endregion

        #region IEditableObject Members
        public void BeginEdit()
        {
            if (this.isEditing)
                return;

            this.backupData = this.data;
            this.connectDescriptor.BeginEdit();
            this.isEditing = true;
        }
        public void CancelEdit()
        {
            if (!this.isEditing)
                return;

            this.isEditing = false;
            this.data = this.backupData;
            this.connectDescriptor.CancelEdit();
        }
        public void EndEdit()
        {
            if (!this.isEditing)
                return;

            // validuj spojeni i jeho casti (connect descriptor)
            ReadOnlyCollection<ObjectError<EConnectionError>> errorsList;
            if (!Validate(out errorsList))
                return;

            // spojeni je validni, zapis data
            this.isEditing = false;
            this.backupData = new ConnectionData();
            this.connectDescriptor.EndEdit();
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
        private void connectDescriptor_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }
        void clearErrors()
        {
            hasErrors = false;
            errors.Clear();
        }
        #endregion

        #region IValidatableObject<EConnectionError> Members
        public bool HasErrors
        {
            get { return hasErrors; }
        }
        public ReadOnlyCollection<ObjectError<EConnectionError>> Errors
        {
            get { return errors.Values.ToList<ObjectError<EConnectionError>>().AsReadOnly(); }
        }
        public bool Validate(out ReadOnlyCollection<ObjectError<EConnectionError>> errorsList)
        {
            // while not editing, connection is always considered valid
            if (!this.isEditing)
            {
                errorsList = null;
                return true;
            }

            // clear errors
            clearErrors();
            // test properties for validity
            if (string.IsNullOrEmpty(this.data.Name))
            {
                hasErrors = true;
                errors.Add(EConnectionError.EmptyName,
                    new ObjectError<EConnectionError>(
                    this,
                    EConnectionError.EmptyName,
                    "Empty name"));
            }

            if (string.IsNullOrEmpty(this.data.UserName))
            {
                hasErrors = true;
                errors.Add(
                    EConnectionError.EmptyUserName,
                    new ObjectError<EConnectionError>(
                    this,
                    EConnectionError.EmptyUserName,
                    "Empty username"));
            }

            if (!Enum.IsDefined(typeof(EDbaPrivileges), this.data.DbaPrivileges))
            {
                hasErrors = true;
                errors.Add(
                    EConnectionError.InvalidPrivileges,
                    new ObjectError<EConnectionError>(
                    this,
                    EConnectionError.InvalidPrivileges,
                    "Invalid privileges"));
            }

            if (!Enum.IsDefined(typeof(ENamingMethod), this.data.NamingMethod))
            {
                hasErrors = true;
                errors.Add(
                    EConnectionError.InvalidNamingMethod,
                    new ObjectError<EConnectionError>(
                    this,
                    EConnectionError.InvalidNamingMethod,
                    "Invalid naming method"));
            }

            if (this.data.NamingMethod == ENamingMethod.ConnectDescriptor)
            {
                ReadOnlyCollection<ObjectError<EConnectDescriptorError>> connectDescriptorErrors;
                if (!this.connectDescriptor.Validate(out connectDescriptorErrors))
                {
                    hasErrors = true;
                    errors.Add(
                        EConnectionError.InvalidConnectDescriptor,
                        new ObjectError<EConnectionError>(
                        connectDescriptor,
                        EConnectionError.InvalidConnectDescriptor,
                        "Invalid connect descriptor data, see details in encapsulated object"));
                }
            } else if (this.data.NamingMethod == ENamingMethod.TnsNaming &&
               string.IsNullOrEmpty(TnsName))
            {
                hasErrors = true;
                errors.Add(
                    EConnectionError.EmptyTnsName,
                    new ObjectError<EConnectionError>(
                    this,
                    EConnectionError.EmptyTnsName,
                    "Empty TNS name"));
            }

            errorsList = errors.Values.ToList().AsReadOnly();
            return hasErrors;
        }
        public bool HasError(EConnectionError error)
        {
            return errors.ContainsKey(error);
        }
        #endregion
    }
}