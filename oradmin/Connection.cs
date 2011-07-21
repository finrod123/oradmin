using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace oradmin
{
    public class ConnectionData : INotifyPropertyChanged
    {
        #region Members

        private readonly int  id;
        private readonly ConnectionManager manager;
        
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

        #endregion

        #region Events

        public event ObjectInvalidated Invalidated;

        #endregion

        #region Properties

        public int Id
        {
            get { return id; }
        }

        public ConnectionManager Manager
        {
            get { return manager; }
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
        public ConnectDescriptorData ConnectDescriptor
        {
            get { return connectDescriptor; }
        }
        public string TnsName
        {
            get { return tnsName; }
            set { propertySetAndNotify<string>(ref tnsName, value, "TnsName"); }
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

        #endregion
    }
}
