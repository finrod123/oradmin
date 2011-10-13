using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Diagnostics;
using myentitylibrary;

namespace oradminbl
{
    using ConnectionKey = String;
    /// <summary>
    /// Trida pro spravu definic pripojeni k databazi
    /// </summary>
    public class ConnectionManager : EntityManager<ConnectionCollection, Connection, ConnectionData, ConnectionKey>
    {
        #region Constructor
        public ConnectionManager(string connectionsFileName):
            base(new ConnectionAdapter(connectionsFileName),
            new ConnectionSaver(connectionsFileName))
        {

        }
        #endregion

        protected override void createEntityCollection()
        {
            this.entityCollection = new ConnectionCollection();
        }
        protected override void createEntityStateManager()
        {
            this.entityStateManager = new ConnectionStateManager();
        }
    }
}
