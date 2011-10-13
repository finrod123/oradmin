using System;
using System.Windows.Data;
using System.ComponentModel;
using oradminbl;

namespace oradminviewmodel
{
    public interface IViewModelBase<TEntity> :
        INotifyPropertyChanged,
        IDataErrorInfo
        where TEntity : IBusinessObject
    {
        string DisplayName { get; }
    }

    /// <summary>
    /// Base template class for ViewModel classes.
    /// It is strongly typed to use the provided type of business object.
    /// Wraps around the business object for GUI to consume the business logic.
    /// Provides the commands to perform actions on business logic.
    /// GUI binds to the ViewModel class and the state info is stored here.
    /// ViewModel class is being displayed in GUI using a WPF data template.
    /// </summary>
    /// <typeparam name="TEntity">Type of the business object</typeparam>
    public abstract class ViewModelBase<TEntity> :
        IViewModelBase<TEntity>
        where TEntity : IBusinessObject
    {
        #region Members
        protected TEntity businessObject;
        #endregion

        #region Constructor
        public ViewModelBase(TEntity businessObject)
        {
            if (businessObject == null)
                throw new ArgumentNullException("business object");

            this.businessObject = businessObject;

            // set up business object event handling
            this.businessObject.PropertyChanged += new PropertyChangedEventHandler(businessObject_PropertyChanged);
        }

        #endregion

        #region IViewModelBase<TEntity> Members
        public string DisplayName
        {
            get { return this.businessObject.BusinessObjectName; }
        }
        #endregion

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        void businessObject_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.OnPropertyChanged(e.PropertyName);
        }
        #endregion

        #region IDataErrorInfo Members
        public string Error
        {
            get { return this.businessObject.Error; }
        }
        public string this[string columnName]
        {
            get { return this.businessObject[columnName]; }
        }
        #endregion
    }
}