using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace oradmin
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class OracleAdmin : Application
    {
        /// <summary>
        /// connection manager
        /// </summary>
        private ConnectionManager connectionManager;

        /// <summary>
        /// main window of an application
        /// </summary>
        MainWindow mainWindow;

        #region Property setters and getters

        public ConnectionManager ConnectionManager
        {
            get { return connectionManager; }
        }

        #endregion

        private void appStartup(object sender, StartupEventArgs e)
        {
            // create connection manager
            connectionManager = new ConnectionManager();
            // create main window and display it
            mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}
