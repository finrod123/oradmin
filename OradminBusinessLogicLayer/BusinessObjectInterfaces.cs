using System;
using System.ComponentModel;
using myentitylibrary;

namespace oradminbl
{
    /// <summary>
    /// interface for general business objects
    /// </summary>
    public interface IBusinessObject :
        INotifyPropertyChanged,
        IDataErrorInfo,
        IEditableObject,
        IEditableObjectInfo
    {
        string BusinessObjectName { get; }
    }

    public interface IConnectionBusinessObject :
        IConnection,
        IBusinessObject
    { }
}