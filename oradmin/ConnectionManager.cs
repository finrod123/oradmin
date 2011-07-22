using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace oradmin
{
    /// <summary>
    /// Trida pro spravu definic pripojeni k databazi
    /// </summary>
    public class ConnectionManager
    {
        #region Members
        /// <summary>
        /// Collections to hold connections by id and name
        /// </summary>
        private Dictionary<int, ReadOnlyConnection> id2Connections = new Dictionary<int, ReadOnlyConnection>();
        private Dictionary<string, ReadOnlyConnection> name2Connections = new Dictionary<string, ReadOnlyConnection>();
        private ObservableCollection<ReadOnlyConnection> connections = new ObservableCollection<ReadOnlyConnection>();
        private ListCollectionView view;
        /// <summary>
        /// Connection number generator
        /// </summary>
        SequenceGenerator generator = new SequenceGenerator(0, int.MaxValue, 1, false);

        #endregion

        #region Constructor

        public ConnectionManager()
        {
            view = new ListCollectionView(connections);
        }

        #endregion

        #region Properties

        public ListCollectionView DefaultView
        {
            get { return view; }
        }

        #endregion

        #region Protected interface

        protected bool validate(Connection connection,
            out List<ObjectError<EConnectionError>> errors)
        {
            ReadOnlyConnection exist;
            bool valid = true;
            errors = new List<ObjectError<EConnectionError>>();

            if (!connection.HasError(EConnectionError.EmptyName))
            {
                if (name2Connections.TryGetValue(connection.Name, out exist))
                {
                    if (connection.Id != exist.Id)
                    {
                        valid = false;
                        errors.Add(new ObjectError<EConnectionError>(
                            connection,
                            EConnectionError.DuplicateName,
                            "Connection name is duplicate"));
                    }
                }
            }
            return valid;
        }
        protected bool isPermanentConnection(int id)
        {
            return id2Connections.ContainsKey(id);
        }

        #endregion

        #region Public interface

        /// <summary>
        /// Method tries to add a new connection based on a writable proposal
        /// passed via "connection" parameter
        /// </summary>
        /// <param name="connection">Writeable connection to validate</param>
        /// <param name="added">New read-only collection handle</param>
        /// <param name="errors">List of errors</param>
        /// <returns></returns>
        public bool AddConnection(Connection connection,
                                  out ReadOnlyConnection added,
                                  out List<ObjectError<EConnectionError>> errors)
        {
            if (isPermanentConnection(connection.Id))
                throw new Exception("Connection already added");

            errors = null;
            added = null;

            // testuj validitu spojeni jako takoveho
            bool valid = connection.Validate();

            // testuj validitu v ramci manageru
            if (!connection.HasError(EConnectionError.EmptyName))
            {
                if (!validate(connection, out errors))
                    valid = false;
            }

            // pokud byla data validni, pridej spojeni
            if (valid)
            {
                added = new ReadOnlyConnection(new ConnectionData(connection, this));
                id2Connections.Add(added.Id, added);
                name2Connections.Add(added.Name, added);
            }

            return valid;
        }

        #endregion

        #region Helper methods

        public int NextId
        {
            get { return generator.Next; }
        }

        #endregion

        #region Inner classes
        public class ConnectionData : IConnection, INotifyPropertyChanged, IDisposable
        {
            #region Members

            private readonly int id;
            private readonly ConnectionManager manager;
            private bool locked = false;
            private bool valid = true;

            /// <summary>
            /// Business logic related members
            /// </summary>
            private string name = string.Empty;
            private string userName = string.Empty;
            private EDbaPrivileges dbaPrivileges = EDbaPrivileges.Normal;
            private bool osAuthenticate = false;
            private ENamingMethod namingMethod = ENamingMethod.ConnectDesctiptor;
            private string tnsName;
            private ConnectDescriptorData connectDescriptor;

            #endregion

            #region Constructor

            public ConnectionData(ConnectionManager manager)
            {
                if (manager == null)
                    throw new ArgumentNullException("Connection manager reference");

                this.manager = manager;
                // nastav nove id
                id = manager.NextId;
            }

            public ConnectionData(IConnection connection, ConnectionManager manager):
                this(manager)
            {
                fillConnectionData(connection);
            }

            #endregion

            #region Events

            public event ObjectInvalidated Invalidated;
            public event ObjectLockedChanged LockedChanged;

            #endregion

            #region Properties

            public int Id
            {
                get { return id; }
            }
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
                        OnLockedChanged(value);
                    }
                }
            }
            public ConnectionManager Manager
            {
                get { return manager; }
            }
            public ConnectDescriptorData ConnectDescriptor
            {
                get { return connectDescriptor; }
            }
            public string Name
            {
                get { return name; }
                set { propertySetAndNotify<string>(ref name, value, "Name"); }
            }
            public string UserName
            {
                get { return userName; }
                set { propertySetAndNotify<string>(ref userName, value, "UserName"); }
            }
            public EDbaPrivileges DbaPrivileges
            {
                get { return dbaPrivileges; }
                set { propertySetAndNotify<EDbaPrivileges>(ref dbaPrivileges, value, "DbaPrivileges"); }
            }
            public bool OsAuthenticate
            {
                get { return osAuthenticate; }
                set { propertySetAndNotify<bool>(ref osAuthenticate, value, "OsAuthenticate"); }
            }
            public ENamingMethod NamingMethod
            {
                get { return namingMethod; }
                set { propertySetAndNotify<ENamingMethod>(ref namingMethod, value, "NamingMethod"); }
            }
            public string TnsName
            {
                get { return tnsName; }
                set { propertySetAndNotify<string>(ref tnsName, value, "TnsName"); }
            }

            #endregion

            #region Helper methods
            private void OnLockedChanged(bool value)
            {
                if (LockedChanged != null)
                {
                    OnLockedChanged(value);
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

            #region Helper methods

            void fillConnectionData(IConnection data)
            {
                name = data.Name;
                userName = data.UserName;
                osAuthenticate = data.OsAuthenticate;
                dbaPrivileges = data.DbaPrivileges;

                if ((namingMethod = data.NamingMethod) == ENamingMethod.ConnectDesctiptor)
                {
                    connectDescriptor = new ConnectDescriptorData(data);
                    tnsName = string.Empty;
                } else
                {
                    connectDescriptor = new ConnectDescriptorData();
                    tnsName = data.TnsName;
                } 
            }
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

            #endregion

            #region IConnectDescriptor Members

            public string Host
            {
                get
                {
                    return connectDescriptor.Host;
                }
                set
                {
                    connectDescriptor.Host = value;
                }
            }
            public int Port
            {
                get
                {
                    return connectDescriptor.Port;
                }
                set
                {
                    connectDescriptor.Port = value;
                }
            }
            public bool UsingSid
            {
                get
                {
                    return connectDescriptor.UsingSid;
                }
                set
                {
                    connectDescriptor.UsingSid = value;
                }
            }
            public string ServiceName
            {
                get
                {
                    return connectDescriptor.ServiceName;
                }
                set
                {
                    connectDescriptor.ServiceName = value;
                }
            }
            public string InstanceName
            {
                get
                {
                    return connectDescriptor.InstanceName;
                }
                set
                {
                    connectDescriptor.InstanceName = value;
                }
            }
            public string Sid
            {
                get
                {
                    return connectDescriptor.Sid;
                }
                set
                {
                    connectDescriptor.Sid = value;
                }
            }
            public EServerType ServerType
            {
                get
                {
                    return connectDescriptor.ServerType;
                }
                set
                {
                    connectDescriptor.ServerType = value;
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
        public class CachingConnection : IConnection, INotifyPropertyChanged, IDisposable
        {
            #region Members
            private bool valid = true;
            protected bool sourceLocked = false;
            protected ConnectionData data;

            private string bName = string.Empty;
            private string bUserName = string.Empty;
            private EDbaPrivileges bDbaPrivileges = EDbaPrivileges.Normal;
            private bool bOsAuthenticate = false;
            private ENamingMethod bNamingMethod = ENamingMethod.ConnectDesctiptor;
            private string bTnsName = string.Empty;
            protected CachingConnectDescriptor connectDescriptor;

            #endregion

            public CachingConnection(ConnectionData data)
            {
                if (data == null)
                    throw new ArgumentNullException("Connection data");

                this.data = data;
                data.Invalidated += new ObjectInvalidated(data_Invalidated);
                data.PropertyChanged += new PropertyChangedEventHandler(data_PropertyChanged);
                data.LockedChanged += new ObjectLockedChanged(data_LockedChanged);
            }

            #region Events
            public event ObjectInvalidated Invalidated;
            #endregion

            #region Helper methods
            protected void connectDescriptor_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                OnPropertyChanged(e.PropertyName);
            }


            void data_LockedChanged(bool locked)
            {
                sourceLocked = locked;
                refreshCachedData();
            }
            void data_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                OnPropertyChanged(e.PropertyName);
            }
            void data_Invalidated()
            {
                OnInvalidated();
            }
            void OnInvalidated()
            {
                valid = false;
                if (Invalidated != null)
                {
                    Invalidated();
                }
            }
            protected void OnPropertyChanged(string propertyName)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }
            void refreshCachedData()
            {
                string nName = data.Name;
                string nUserName = data.UserName;
                bool nOsAuthenticate = data.OsAuthenticate;
                EDbaPrivileges nDbaPrivileges = data.DbaPrivileges;
                ENamingMethod nNamingMethod = data.NamingMethod;
                string nTnsName = data.TnsName;

                if (nName != bName)
                    OnPropertyChanged("Name");
                if (nUserName != bUserName)
                    OnPropertyChanged("UserName");
                if (nOsAuthenticate != bOsAuthenticate)
                    OnPropertyChanged("OsAuthenticate");
                if (nDbaPrivileges != bDbaPrivileges)
                    OnPropertyChanged("DbaPrivileges");
                if (nNamingMethod != bNamingMethod)
                    OnPropertyChanged("NamingMethod");
                if (nTnsName != bTnsName)
                    OnPropertyChanged("TnsName");

                bName = data.Name;
                bUserName = data.UserName;
                bOsAuthenticate = data.OsAuthenticate;
                bDbaPrivileges = data.DbaPrivileges;
                bNamingMethod = data.NamingMethod;
                bTnsName = data.TnsName;

                connectDescriptor.RefreshCachedData();
            }
            protected void checkValidity()
            {
                if (!valid)
                    throw new ObjectDisposedException("Connection disposed",
                        new Exception());
            }

            #endregion
            #region IConnection Members

            public bool Valid
            {
                get { return valid; }
            }

            public int Id
            {
                get
                {
                    checkValidity();
                    return data.Id;
                }
            }
            public string Name
            {
                get
                {
                    checkValidity();
                    if (sourceLocked)
                        return bName;

                    return data.Name;
                }
            }
            public string UserName
            {
                get
                {
                    checkValidity();
                    if (sourceLocked)
                        return bUserName;

                    return data.UserName;
                }
            }
            public EDbaPrivileges DbaPrivileges
            {
                get
                {
                    checkValidity();
                    if (sourceLocked)
                        return bDbaPrivileges;

                    return data.DbaPrivileges;
                }
            }
            public bool OsAuthenticate
            {
                get
                {
                    checkValidity();
                    if (sourceLocked)
                        return bOsAuthenticate;

                    return data.OsAuthenticate;
                }
            }
            public ENamingMethod NamingMethod
            {
                get
                {
                    checkValidity();
                    if (sourceLocked)
                        return bNamingMethod;

                    return data.NamingMethod;
                }
            }
            public string TnsName
            {
                get
                {
                    checkValidity();
                    if (sourceLocked)
                        return bTnsName;

                    return data.TnsName;
                }
            }

            #endregion

            #region IConnectDescriptor Members

            public string Host
            {
                get
                {
                    return connectDescriptor.Host;
                }
            }
            public int Port
            {
                get
                {
                    return connectDescriptor.Port;
                }
            }
            public bool UsingSid
            {
                get
                {
                    return connectDescriptor.UsingSid;
                }
            }
            public string ServiceName
            {
                get
                {
                    return connectDescriptor.ServiceName;
                }
            }
            public string InstanceName
            {
                get
                {
                    return connectDescriptor.InstanceName;
                }
            }
            public string Sid
            {
                get
                {
                    return connectDescriptor.Sid;
                }
            }
            public EServerType ServerType
            {
                get
                {
                    return connectDescriptor.ServerType;
                }
            }

            #endregion

            #region INotifyPropertyChanged Members

            public event PropertyChangedEventHandler PropertyChanged;

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
                data.Dispose();
            }

            #endregion
        }
        public class ReadOnlyConnection : CachingConnection
        {
            #region Constructor

            public ReadOnlyConnection(ConnectionData data) :
                base(data)
            {
                connectDescriptor = new ReadOnlyConnectDescriptor(data.ConnectDescriptor);
                connectDescriptor.PropertyChanged += new PropertyChangedEventHandler(connectDescriptor_PropertyChanged);
            }

            #endregion
        }
        public class Connection : CachingConnection, IEditableObject
        {
            #region Members

            ConnectionManager manager;

            private string bName;
            private string bUserName;
            private EDbaPrivileges bDbaPrivileges;
            private bool bOsAuthenticate;
            private ENamingMethod bNamingMethod;
            private string bTnsName;

            bool editing = false;

            bool hasErrors = false;
            Dictionary<EConnectionError, ObjectError<EConnectionError>> errors =
                new Dictionary<EConnectionError, ObjectError<EConnectionError>>();
            #endregion

            #region Constructor

            public Connection(ConnectionData data) :
                base(data)
            {
                manager = data.Manager;
                connectDescriptor = new ConnectDescriptor(data.ConnectDescriptor);
                connectDescriptor.PropertyChanged += new PropertyChangedEventHandler(connectDescriptor_PropertyChanged);
            }

            #endregion

            #region Properties

            public new string Name
            {
                get
                {
                    if (editing)
                        return bName;

                    return base.Name;
                }
                set { propertySetter<string>(ref bName, value, "Name"); }
            }
            public new string UserName
            {
                get
                {
                    if (editing)
                        return bUserName;

                    return base.UserName;
                }
                set { propertySetter<string>(ref bUserName, value, "UserName"); }
            }
            public new EDbaPrivileges DbaPrivileges
            {
                get
                {
                    if (editing)
                        return bDbaPrivileges;

                    return base.DbaPrivileges;
                }
                set { propertySetter<EDbaPrivileges>(ref bDbaPrivileges, value, "DbaPrivileges"); }
            }
            public new bool OsAuthenticate
            {
                get
                {
                    if (editing)
                        return bOsAuthenticate;

                    return base.OsAuthenticate;
                }
                set { propertySetter<bool>(ref bOsAuthenticate, value, "OsAuthenticate"); }
            }
            public new ENamingMethod NamingMethod
            {
                get
                {
                    if (editing)
                        return bNamingMethod;

                    return base.NamingMethod;
                }
                set { propertySetter<ENamingMethod>(ref bNamingMethod, value, "NamingMethod"); }
            }
            public new string TnsName
            {
                get
                {
                    if (editing)
                        return bTnsName;

                    return base.TnsName;
                }
                set
                {
                    if (NamingMethod != ENamingMethod.TnsNaming)
                        throw new InvalidOperationException("Cannot edit TNS name while in Connect descriptor mode");

                    propertySetter<string>(ref bTnsName, value, "TnsName");
                }
            }

            public bool Editing
            {
                get { return editing; }
            }
            public bool HasErrors
            {
                get { return hasErrors; }
            }
            public ReadOnlyCollection<ObjectError<EConnectionError>> Errors
            {
                get { return errors.Values.ToList<ObjectError<EConnectionError>>().AsReadOnly(); }
            }

            #endregion

            #region Public interface

            public bool Validate()
            {
                clearErrors();

                if (string.IsNullOrEmpty(Name))
                {
                    hasErrors = true;
                    errors.Add(EConnectionError.EmptyName,
                        new ObjectError<EConnectionError>(
                        this,
                        EConnectionError.EmptyName,
                        "Empty name"));
                }

                if (string.IsNullOrEmpty(UserName))
                {
                    hasErrors = true;
                    errors.Add(
                        EConnectionError.EmptyUserName,
                        new ObjectError<EConnectionError>(
                        this,
                        EConnectionError.EmptyUserName,
                        "Empty username"));
                }

                if (!Enum.IsDefined(typeof(EDbaPrivileges), DbaPrivileges))
                {
                    hasErrors = true;
                    errors.Add(
                        EConnectionError.InvalidPrivileges,
                        new ObjectError<EConnectionError>(
                        this,
                        EConnectionError.InvalidPrivileges,
                        "Invalid privileges"));
                }

                if (!Enum.IsDefined(typeof(ENamingMethod), NamingMethod))
                {
                    hasErrors = true;
                    errors.Add(
                        EConnectionError.InvalidNamingMethod,
                        new ObjectError<EConnectionError>(
                        this,
                        EConnectionError.InvalidNamingMethod,
                        "Invalid naming method"));
                }

                if (NamingMethod == ENamingMethod.ConnectDesctiptor &&
                    !((connectDescriptor as ConnectDescriptor).Validate()))
                {
                    hasErrors = true;
                    errors.Add(
                        EConnectionError.InvalidConnectDescriptor,
                        new ObjectError<EConnectionError>(
                        connectDescriptor,
                        EConnectionError.InvalidConnectDescriptor,
                        "Invalid connect descriptor data, see details in encapsulated object"));
                }

                if (NamingMethod == ENamingMethod.TnsNaming &&
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

                return hasErrors;
            }
            public bool HasError(EConnectionError error)
            {
                return errors.ContainsKey(error);
            }

            #endregion

            #region Events
            public event ConnectionChangedHandler ConnectionChanged;
            #endregion

            #region IEditableObject Members

            public void BeginEdit()
            {
                if(!editing)
                {
                    resetValues();
                    (connectDescriptor as ConnectDescriptor).BeginEdit();
                    editing = true;
                }
            }

            public void CancelEdit()
            {
                editing = false;
            }

            public void EndEdit()
            {
                // ukonceni editace spojeni -> nutne provest validaci z hlediska objektu sameho
                // i jeho spravce
                // validuj
                bool valid = Validate();

                // pokud je spojeni ulozeno v connection manageru, zkontroluj ho i z toho
                // hlediska
                if (manager.isPermanentConnection(Id))
                {
                    List<ObjectError<EConnectionError>> mgrErrors;
                    if (!manager.validate(this, out mgrErrors))
                    {
                        addValidationErrors(mgrErrors);
                        valid = false;
                    }
                }

                if (valid)
                {
                    // pokud jsou data uzamcena, ohlas chybu
                    if (sourceLocked)
                        throw new Exception("Connection locked");

                    // pokus se ziskat zamek
                    lock (data)
                    {
                        // ohlas zamknuti
                        data.Locked = true;
                        // upravy
                        data.Name = Name;
                        data.UserName = UserName;
                        data.OsAuthenticate = OsAuthenticate;
                        data.DbaPrivileges = DbaPrivileges;
                        if ((data.NamingMethod = NamingMethod) == ENamingMethod.ConnectDesctiptor)
                        {
                            (connectDescriptor as ConnectDescriptor).EndEdit();
                        } else
                        {
                            data.TnsName = TnsName;
                        }
                        // ohlas zmenu dat managerovi
                        OnConnectionChanged();
                        // ohlas odemknuti
                        data.Locked = false;
                    }
                }
            }

            #endregion

            #region Helper methods

            private void addValidationErrors(List<ObjectError<EConnectionError>> errorsList)
            {
                foreach (ObjectError<EConnectionError> error in errorsList)
                {
                    errors.Add(error.Error, error);
                }
            }

            private void propertySetter<T>(ref T property, T newValue, string propertyName)
            {
                checkValidity();

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
            void clearErrors()
            {
                hasErrors = false;
                errors.Clear();
            }
            void resetValues()
            {
                bName = Name;
                bUserName = UserName;
                bOsAuthenticate = OsAuthenticate;
                bDbaPrivileges = DbaPrivileges;
                bNamingMethod = NamingMethod;
                bTnsName = TnsName;
            }
            void OnConnectionChanged()
            {
                if (ConnectionChanged != null)
                {
                    ConnectionChanged(this);
                }
            }

            #endregion

            #region IConnectDescriptor Members

            public new string Host
            {
                get
                {
                    return connectDescriptor.Host;
                }
                set
                {
                    (connectDescriptor as ConnectDescriptor).Host = value;
                }
            }
            public new int Port
            {
                get
                {
                    return connectDescriptor.Port;
                }
                set
                {
                    (connectDescriptor as ConnectDescriptor).Port = value;
                }
            }
            public new bool UsingSid
            {
                get
                {
                    return connectDescriptor.UsingSid;
                }
                set
                {
                    (connectDescriptor as ConnectDescriptor).UsingSid = value;
                }
            }
            public new string ServiceName
            {
                get
                {
                    return connectDescriptor.ServiceName;
                }
                set
                {
                    (connectDescriptor as ConnectDescriptor).ServiceName = value;
                }
            }
            public new string InstanceName
            {
                get
                {
                    return connectDescriptor.InstanceName;
                }
                set
                {
                    (connectDescriptor as ConnectDescriptor).InstanceName = value;
                }
            }
            public new string Sid
            {
                get
                {
                    return connectDescriptor.Sid;
                }
                set
                {
                    (connectDescriptor as ConnectDescriptor).Sid = value;
                }
            }
            public new EServerType ServerType
            {
                get
                {
                    return connectDescriptor.ServerType;
                }
                set
                {
                    (connectDescriptor as ConnectDescriptor).ServerType = value;
                }
            }

            #endregion
        }

        #endregion
    }

    #region Public delegates

    public delegate void ObjectInvalidated();
    public delegate void ConnectionChangedHandler(ConnectionManager.Connection connection);

    #endregion

    #region Connection enums

    public enum EConnectionError
    {
        EmptyName,
        EmptyUserName,
        EmptyTnsName,
        DuplicateName,
        InvalidPrivileges,
        InvalidNamingMethod,
        InvalidConnectDescriptor
    }

    public enum EDbaPrivileges
    {
        Normal,
        SYSDBA,
        SYSOPER
    }

    public enum ENamingMethod
    {
        ConnectDesctiptor,
        TnsNaming
    }

    public enum EServerType
    {
        Dedicated,
        Shared,
        Pooled
    }

    #endregion
}
