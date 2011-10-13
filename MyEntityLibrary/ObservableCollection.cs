using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace myentitylibrary
{
    public class ObservableCollection<T> : System.Collections.ObjectModel.ObservableCollection<T>
    {
        public ObservableCollection() : base() { }
        // add range method
        public void AddRange(IEnumerable<T> collection)
        {
            ((List<T>)Items).AddRange(collection);
            OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new System.Collections.Specialized.NotifyCollectionChangedEventArgs(
                System.Collections.Specialized.NotifyCollectionChangedAction.Reset,
                new List<T>(collection)));
        }

        public void RemoveAll(Predicate<T> match)
        {
            // get the list of the objects to remove
            IEnumerable<T> toRemoveList = ((List<T>)Items).Where(match as Func<T, bool>);
            // remove them
            ((List<T>)Items).RemoveAll(match);
            // notify
            OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Reset, new List<T>(toRemoveList)));
        }
    }
}
