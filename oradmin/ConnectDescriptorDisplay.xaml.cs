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
using System.ComponentModel;

namespace oradmin
{
    /// <summary>
    /// Interaction logic for ConnectDescriptorDisplay.xaml
    /// </summary>
    public partial class ConnectDescriptorDisplay : UserControl, INotifyPropertyChanged
    {
        #region Members
        bool writeable = false;
        #endregion

        #region Constructor
        public ConnectDescriptorDisplay()
        {
            InitializeComponent();
        }
        #endregion

        #region Properties

        public bool Writeable
        {
            get { return writeable; }
            set
            {
                if (writeable != value)
                {
                    writeable = value;
                    OnPropertyChanged("Writeable");
                }
            }
        }
        public bool HasError
        {
            get
            {
                bool hasError = Validation.GetHasError(this.host) ||
                                Validation.GetHasError(this.port);

                if (this.serviceNameB.IsChecked.HasValue)
                {
                    if (this.serviceNameB.IsChecked.Value)
                    {
                        hasError = hasError ||
                                   Validation.GetHasError(this.serviceName) ||
                                   Validation.GetHasError(this.instanceName);
                    } else
                    {
                        hasError = hasError ||
                                   Validation.GetHasError(sid);
                    }
                } else
                    hasError = true;

                return hasError;
            }
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
    }
}
