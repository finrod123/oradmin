using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace oradmin
{
    /// <summary>
    /// Interaction logic for ConnectionDialog.xaml
    /// </summary>
    public partial class ConnectionDialog : Window
    {
        #region Members
        private bool isNew;
        ConnectionManager.Connection connection;
        #endregion

        public ConnectionDialog(ConnectionManager.Connection connection,
                                bool isNew)
        {
            InitializeComponent();
            if (connection == null)
                throw new ArgumentNullException("Connection");
            
            // initialize members
            this.connection = connection;
            this.isNew = isNew;
            // set data context
            DataContext = this.connection;
            // begin editing connection
        }

        #region Helper methods

        private void okB_Click(object sender, RoutedEventArgs e)
        {
            if (save())
                DialogResult = true;
        }
        private bool save()
        {
            System.Windows.MessageBox.Show("Readonly connection name " + connection.UserName);
            // testuje vsechny prvky na validacni chyby
            if (hasValidationErrors())
            {
                MessageBox.Show("Neni mozne ulozit stav spojeni, nebot dialog obsahuje chyby!");
                return false;
            }

            if (!connection.Editing)
                return true;

            // pokud nejsou chyby, proved pokus o ulozeni spojeni
            if (isNew)
            {
                ConnectionManager mgr = connection.Manager;
                ConnectionManager.ReadOnlyConnection newConnection;
                List<ObjectError<EConnectionError>> errorsList;

                
                if (!mgr.AddConnection(connection, out newConnection, out errorsList))
                {
                    MessageBox.Show("Spojeni se nepovedlo pridat!");
                    // TODO: zobraz chyby!
                    return false;
                } else
                {
                    MessageBox.Show("Spojeni pridano");
                }
            } else
            {
                // spojeni jiz existuje, pouze jej ulozim
                connection.EndEdit();

                if (connection.HasErrors)
                {
                    MessageBox.Show("Spojeni se nepovedlo ulozit, nebot stale obsahuje chyby!");
                    // TODO: nejak zobraz chyby
                    return false;
                }
            }

            return true;
        }
        /// <summary>
        /// Pokusi se ulozit spojeni
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveB_Click(object sender, RoutedEventArgs e)
        {
            if (save())
            {
                isNew = false;
                connection.BeginEdit();
                MessageBox.Show("Connection saved");
            } else
            {
                MessageBox.Show("Connection not saved");
            }
        }
        private void cancelB_Click(object sender, RoutedEventArgs e)
        {
            // zrus provedene zmeny a zavri dialog
            connection.CancelEdit();
            DialogResult = false;
        }
        private bool hasValidationErrors()
        {
            return Validation.GetHasError(this.connName) ||
                   Validation.GetHasError(this.userName) ||
                   this.connDescDisplay.HasError;
        }
        #endregion
    }
}
