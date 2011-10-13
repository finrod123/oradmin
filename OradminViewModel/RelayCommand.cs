using System;
using System.Windows;
using System.Windows.Input;

namespace oradminviewmodel
{
    public class RelayCommand : ICommand
    {
        #region Members
        private readonly Predicate<object> canExecuteHandler;
        private readonly Action<object> executeHandler;
        #endregion

        #region Constructor
        public RelayCommand(Action<object> executeHandler) :
            this(executeHandler, null)
        { }
        public RelayCommand(Action<object> executeHandler, Predicate<object> canExecuteHandler)
        {
            if (executeHandler == null)
                throw new ArgumentNullException("execute handler");

            this.executeHandler = executeHandler;
            this.canExecuteHandler = canExecuteHandler;
        }
	    #endregion

        #region ICommand Members
        public bool CanExecute(object parameter)
        {
            return this.canExecuteHandler == null ? true : this.canExecuteHandler(parameter);
        }
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public void Execute(object parameter)
        {
            if (this.CanExecute(parameter))
                this.executeHandler(parameter);
        }
        #endregion
    }
}