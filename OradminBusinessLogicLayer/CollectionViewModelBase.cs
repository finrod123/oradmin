using System;
using System.Windows;
using System.Windows.Input;
using oradminbl;
using myentitylibrary;

namespace oradminviewmodel
{
    /// <summary>
    /// Basic type for view models of collections of business objects.
    /// Represents business objects collections as collections of
    /// business object view models.
    /// </summary>
    /// <typeparam name="TBusinessObject">Business object type</typeparam>
    public interface ICollectionViewModelBase<TBusinessObject>
        where TBusinessObject : IBusinessObject
    {

    }

    public abstract class CollectionViewModelBase<TBusinessObject> :
        ICollectionViewModelBase<TBusinessObject>
        where TBusinessObject : IBusinessObject
    {
        #region Members
        ObservableCollection<ViewModelBase<TBusinessObject>> viewModels =
            new ObservableCollection<ViewModelBase<TBusinessObject>>();
        #endregion
    }
}