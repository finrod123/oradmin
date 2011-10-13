using System;
using System.Windows.Data;
using oradminbl;

namespace oradminviewmodel
{
    /// <summary>
    /// Interface to server as a connection business object wrapper
    /// </summary>
    public interface IConnectionViewModelBusinessObjectWrapper :
        IConnection
    {

    }

    /// <summary>
    /// Command interface for connections = operations available for GUI
    /// </summary>
    public interface IConnectionViewModelCommandInterface
    {
        RelayCommand EditCommand { get; }
        RelayCommand OpenCommand { get; }
        RelayCommand TestCommand { get; }
        RelayCommand DeleteCommand { get; }
        RelayCommand SaveCommand { get; }
    }

    public class ConnectionViewModel : ViewModelBase<IConnectionBusinessObject>,
        IConnectionViewModelBusinessObjectWrapper
    {
        #region Constructor
        public ConnectionViewModel(IConnectionBusinessObject connection) :
            base(connection)
        { }
        #endregion

        #region IConnection Members
        public string TnsName
        {
            get
            {
                return this.businessObject.TnsName;
            }
            set
            {
                if (this.businessObject.IsEditing)
                {
                    this.businessObject.TnsName = value;
                }
            }
        }
        #endregion

        #region IConnectionBase Members
        public string Name
        {
            get
            {
                return this.businessObject.Name;
            }
            set
            {
                if (this.businessObject.IsEditing)
                {
                    this.businessObject.Name = value;
                }
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
    }
}