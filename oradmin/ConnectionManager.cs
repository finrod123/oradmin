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
        private Dictionary<int, ConnectionData> id2Connections = new Dictionary<int, ConnectionData>();
        private ObservableCollection<ConnectionData> connections = new ObservableCollection<ConnectionData>();
        /// <summary>
        /// Connection number generator
        /// </summary>
        SequenceGenerator generator = new SequenceGenerator(0, int.MaxValue, 1, false);

        #endregion

        #region Helper methods

        public int NextId
        {
            get { return generator.Next; }
        }

        #endregion

        #region Properties



        #endregion

        #region Constructor

        public ConnectionManager() { }

        #endregion
    }

    public class Connection : IEditableObject, INotifyPropertyChanged
    {
        #region Members

        private ConnectionData data;
        private ConnectDescriptor connectionDescriptor;

        private string name;
        private string userName;
        private EDbaPrivileges dbaPrivileges;
        private bool osAuthenticate;
        private ENamingMethod namingMethod;
        private string tnsName;

        bool valid;
        bool editing;

        bool hasErrors;
        List<ObjectError<EConnectionError>> errors;
        #endregion

        #region Properties

        public string Name
        {
            get
            {
                if (!valid)
                    throw new ObjectDisposedException("Data container disposed", new Exception());

                return propertyGetter<string>(name, data.Name);
            }
            set { propertySetter<string>(ref name, value, "Name"); }
        }
        public string UserName
        {
            get
            {
                if (!valid)
                    throw new ObjectDisposedException("Data container disposed", new Exception());

                return propertyGetter<string>(userName, data.UserName);
            }
            set { propertySetter<string>(ref userName, value, "UserName"); }
        }
        public EDbaPrivileges DbaPrivileges
        {
            get
            {
                if (!valid)
                    throw new ObjectDisposedException("Data container disposed", new Exception());

                return propertyGetter<EDbaPrivileges>(dbaPrivileges, data.DbaPrivileges);
            }
            set { propertySetter<EDbaPrivileges>(ref dbaPrivileges, value, "DbaPrivileges"); }
        }
        public bool OsAuthenticate
        {
            get
            {
                if (!valid)
                    throw new ObjectDisposedException("Data container disposed", new Exception());

                return propertyGetter<bool>(osAuthenticate, data.OsAuthenticate);
            }
            set { propertySetter<bool>(ref osAuthenticate, value, "OsAuthenticate"); }
        }
        public ENamingMethod NamingMethod
        {
            get
            {
                if (!valid)
                    throw new ObjectDisposedException("Data container disposed", new Exception());

                return propertyGetter<ENamingMethod>(namingMethod, data.NamingMethod);
            }
            set { propertySetter<ENamingMethod>(ref namingMethod, value, "NamingMethod"); }
        }
        public ConnectDescriptor ConnectionDescriptor
        {
            get
            {
                if (NamingMethod == ENamingMethod.ConnectDesctiptor)
                    return connectionDescriptor;
                else
                    return null;
            }

        }
        public string TnsName
        {
            get
            {
                if (NamingMethod == ENamingMethod.TnsNaming)
                    return propertyGetter<string>(tnsName, data.TnsName);
                else
                    return null;
            }
            set
            {
                if (NamingMethod != ENamingMethod.TnsNaming)
                    throw new InvalidOperationException("Cannot edit TNS name while in Connect descriptor mode");

                propertySetter<string>(ref tnsName, value, "TnsName");
            }
        }

        public bool IsValid
        {
            get { return valid; }
        }
        public bool IsEditing
        {
            get { return editing; }
        }

        public bool HasErrors
        {
            get { return hasErrors; }
        }
        public ReadOnlyCollection<ObjectError<EConnectionError>> Errors
        {
            get { return errors.AsReadOnly(); }
        }

        #endregion

        #region Constructor

        public Connection(ConnectionManager.ConnectionData data)
        {
            if (data == null)
                throw new ArgumentNullException("Connection data");

            this.data = data;
            editing = false;

            // set up error indication
            hasErrors = !(valid = true);
            errors = new List<ObjectError<EConnectionError>>();
            
            // set up handlers
            data.Invalidated += new ObjectInvalidated(data_Invalidated);
            data.PropertyChanged += new PropertyChangedEventHandler(data_PropertyChanged);
        }

        #endregion

        #region Public interface

        public bool Validate()
        {
            clearErrors();

            if (string.IsNullOrEmpty(Name))
            {
                hasErrors = true;
                errors.Add(new ObjectError<EConnectionError>(
                    this,
                    EConnectionError.EmptyName,
                    "Empty name"));
            }

            if (string.IsNullOrEmpty(UserName))
            {
                hasErrors = true;
                errors.Add(new ObjectError<EConnectionError>(
                    this,
                    EConnectionError.EmptyUserName,
                    "Empty username"));
            }

            if (!Enum.IsDefined(typeof(EDbaPrivileges), DbaPrivileges))
            {
                hasErrors = true;
                errors.Add(new ObjectError<EConnectionError>(
                    this,
                    EConnectionError.InvalidPrivileges,
                    "Invalid privileges"));
            }

            if (!Enum.IsDefined(typeof(ENamingMethod), NamingMethod))
            {
                hasErrors = true;
                errors.Add(new ObjectError<EConnectionError>(
                    this,
                    EConnectionError.InvalidNamingMethod,
                    "Invalid naming method"));
            }

            if (NamingMethod == ENamingMethod.ConnectDesctiptor &&
                !connectionDescriptor.Validate())
            {
                hasErrors = true;
                errors.Add(new ObjectError<EConnectionError>(
                    connectionDescriptor,
                    EConnectionError.InvalidConnectDescriptor,
                    "Invalid connect descriptor data, see details in encapsulated object"));
            }

            if (NamingMethod == ENamingMethod.TnsNaming &&
               string.IsNullOrEmpty(TnsName))
            {
                hasErrors = true;
                errors.Add(new ObjectError<EConnectionError>(
                    this,
                    EConnectionError.EmptyTnsName,
                    "Empty TNS name"));
            }

            return hasErrors;
        }

        #endregion

        #region IEditableObject Members

        public void BeginEdit()
        {
            editing = true;
        }

        public void CancelEdit()
        {
            editing = false;
            connectionDescriptor.CancelEdit();
            resetValues();
        }

        public void EndEdit()
        {
            // ukonceni editace spojeni -> nutne provest validaci z hlediska objektu sameho
            // i jeho spravce
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

        #region Helper methods

        void data_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }
        void data_Invalidated()
        {
            valid = false;
        }
        private T propertyGetter<T>(T editValue, T readValue)
        {
            if (editing)
                return editValue;
            else
                return readValue;
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
        void clearErrors()
        {
            hasErrors = false;
            errors.Clear();
        }
        void resetValues()
        {
            name = Name;
            userName = UserName;
            osAuthenticate = OsAuthenticate;
            dbaPrivileges = DbaPrivileges;
            namingMethod = NamingMethod;
            tnsName = TnsName;
        }

        #endregion
    }

    #region Public delegates

    public delegate void ObjectInvalidated();

    #endregion

    #region Connection enums

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

    #endregion
}
