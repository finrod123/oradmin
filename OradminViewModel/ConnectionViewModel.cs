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
        RelayCommand DisplayCommand { get; }
        RelayCommand CloseDisplayCommand { get; }
        RelayCommand EditCommand { get; }
        RelayCommand OpenCommand { get; }
        RelayCommand OpenWithNewPasswordCommand { get; }
        RelayCommand TestCommand { get; }
        RelayCommand TestWithNewPasswordCommand { get; }
        RelayCommand DeleteCommand { get; }
        RelayCommand SaveCommand { get; }
    }

    public interface IConnectionViewModelStateInterface
    {
        bool Displayed { get; }
    }

    public class ConnectionViewModel : ViewModelBase<IConnectionBusinessObject>,
        IConnectionViewModelBusinessObjectWrapper,
        IConnectionViewModelCommandInterface,
        IConnectionViewModelStateInterface
    {
        #region Members
        bool displayed = false;
        #endregion
        
        #region Constructor
        public ConnectionViewModel(IConnectionBusinessObject connection) :
            base(connection)
        {
            // initialize state
            this.initializeState();
            // initialize commands
            this.initializeCommands();
        }
        #endregion

        #region Helper methods
        private void initializeCommands()
        {
            
        }
        private void initializeState()
        {

        }
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
                return this.businessObject.Comment;
            }
            set
            {
                if (this.businessObject.IsEditing)
                {
                    this.businessObject.Comment = value;
                }
            }
        }
        public string UserName
        {
            get
            {
                return this.businessObject.UserName;
            }
            set
            {
                if (this.businessObject.IsEditing)
                {
                    this.businessObject.UserName = value;
                }
            }
        }
        public EDbaPrivileges DbaPrivileges
        {
            get
            {
                return this.businessObject.DbaPrivileges;
            }
            set
            {
                if (this.businessObject.IsEditing)
                {
                    this.businessObject.DbaPrivileges = value;
                }
            }
        }
        public bool OsAuthenticate
        {
            get
            {
                return this.businessObject.OsAuthenticate;
            }
            set
            {
                if (this.businessObject.IsEditing)
                {
                    this.businessObject.OsAuthenticate = value;
                }
            }
        }
        public ENamingMethod NamingMethod
        {
            get
            {
                return this.businessObject.NamingMethod;
            }
            set
            {
                if (this.businessObject.IsEditing)
                {
                    this.businessObject.NamingMethod = value;
                }
            }
        }
        #endregion

        #region IConnectDescriptorBase Members
        public string Host
        {
            get
            {
                return this.businessObject.Host;
            }
            set
            {
                if (this.businessObject.IsEditing)
                {
                    this.businessObject.Host = value;
                }
            }
        }
        public EProtocolType Protocol
        {
            get
            {
                return this.businessObject.Protocol;
            }
            set
            {
                if (this.businessObject.IsEditing)
                {
                    this.businessObject.Protocol = value;
                }
            }
        }
        public int Port
        {
            get
            {
                return this.businessObject.Port;
            }
            set
            {
                if (this.businessObject.IsEditing)
                {
                    this.businessObject.Port = value;
                }
            }
        }
        public bool IsUsingSid
        {
            get
            {
                return this.businessObject.IsUsingSid;
            }
            set
            {
                if (this.businessObject.IsEditing)
                {
                    this.businessObject.IsUsingSid = value;
                }
            }
        }
        public string ServiceName
        {
            get
            {
                return this.businessObject.ServiceName;
            }
            set
            {
                if (this.businessObject.IsEditing)
                {
                    this.businessObject.ServiceName = value;
                }
            }
        }
        public string InstanceName
        {
            get
            {
                return this.businessObject.InstanceName;
            }
            set
            {
                if (this.businessObject.IsEditing)
                {
                    this.businessObject.InstanceName = value;
                }
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
                return this.businessObject.ServerType;
            }
            set
            {
                if (this.businessObject.IsEditing)
                {
                    this.businessObject.ServerType = value;
                }
            }
        }
        #endregion

        #region IConnectionViewModelCommandInterface Members
        public RelayCommand DisplayCommand { get; private set; }
        public RelayCommand CloseDisplayCommand { get; private set; }
        public RelayCommand EditCommand { get; private set; }
        public RelayCommand OpenCommand { get; private set; }
        public RelayCommand OpenWithNewPasswordCommand { get; private set; }
        public RelayCommand TestCommand { get; private set; }
        public RelayCommand TestWithNewPasswordCommand { get; private set; }
        public RelayCommand DeleteCommand { get; private set; }
        public RelayCommand SaveCommand { get; private set; }
        #endregion

        #region IConnectionViewModelStateInterface Members
        public bool Displayed { get; private set; }
        #endregion
    }
}