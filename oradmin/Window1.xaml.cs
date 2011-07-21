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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace oradmin
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// odkaz na connection managera nacteny pri startu z Application
        /// -> rychlejsi pristup nez pres property Application
        /// </summary>
        private readonly ConnectionManager connectionMgr;

        #region Properties

        /// <summary>
        /// odkaz na connection manager pro pouziti z ostatnich trid
        /// </summary>
        public ConnectionManager ConnectionMgr
        {
            get { return connectionMgr; }
        }
        
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            // uloz si odkaz na connection manager z aplikace
            OracleAdmin app = Application.Current as OracleAdmin;
            connectionMgr = app.ConnectionManager;

            masterView.ItemsSource = new CompositeCollection(2) { connectionMgr };

        }

        private void pridejSpojeni(object sender, RoutedEventArgs e)
        {
            // dialog pro nove spojeni
            ConnectionDialog c = new ConnectionDialog();
            c.Owner = this;

            if (c.ShowDialog().Value)
            {
                MessageBox.Show("OK");
            }
        }
    }
}
