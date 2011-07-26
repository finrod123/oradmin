using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace oradmin
{
    class ObservableCollection<T> : System.Collections.ObjectModel.ObservableCollection<T>
    {
        public ObservableCollection() : base() { }
        // add range method
        public void AddRange(IEnumerable<T> collection)
        {
            ((List<T>)Items).AddRange(collection);
            OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized.NotifyCollectionChangedAction.Reset));
        }
    }
}
