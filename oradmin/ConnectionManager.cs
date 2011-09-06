using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Diagnostics;

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
        private Dictionary<int, Connection> id2Connections = new Dictionary<int, Connection>();
        private Dictionary<string, Connection> name2Connections = new Dictionary<string, Connection>();
        private ObservableCollection<Connection> connections = new ObservableCollection<Connection>();
        private ListCollectionView view;
        #endregion

        #region Constructor
        public ConnectionManager()
        {
            view = new ListCollectionView(connections);
        }
        #endregion

        #region Public interface
        /// <summary>
        /// Validates a connection
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="errorsList"></param>
        /// <returns></returns>
        public bool ValidateConnection(
            Connection connection,
            out ReadOnlyCollection<ObjectError<EConnectionError>> errorsList)
        {
            ReadOnlyCollection<ObjectError<EConnectionError>> connErrors;
            bool valid = connection.Validate(out connErrors);

            // check validity from the manager's point of view

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
        ConnectDescriptor,
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
