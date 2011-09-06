using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace oradmin
{
    /// <summary>
    /// A class describing an error with an object, that contains an error
    /// </summary>
    /// <typeparam name="T">The type of an enum to use as an error code</typeparam>
    public class ObjectError<T>
        where T : struct
    {
        #region Members
        object o;
        T errorCode;
        string message;
        #endregion
        #region  Properties
        public T Error
        {
            get { return errorCode; }
        }
        public string Message
        {
            get { return message; }
        }
        public object Object
        {
            get { return o; }
        }
        #endregion
        #region Constructor
        public ObjectError(object o, T errorCode, string message)
        {
            this.errorCode = errorCode;
            this.message = message;
            this.o = o;
        }
        #endregion
    }

    public interface IValidatableObject<T>
    {
        public bool HasErrors { get; }
        public ReadOnlyCollection<ObjectError<T>> Errors { get; }
        public bool Validate(out ReadOnlyCollection<ObjectError<T>> errors);
        public bool HasError(T error);
    }
}