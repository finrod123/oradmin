using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace oradmin
{
    public abstract class MyValidationAttribute : ValidationAttribute
    {
        #region Properties
        public Type TargetValidatorType { get; private set; }
        #endregion

        #region Constructor
        public MyValidationAttribute(string errorMessage, Type targetvalidatorType) :
            base(errorMessage)
        {
            if (targetvalidatorType == null)
                throw new ArgumentNullException("targetValidator");

            this.TargetValidatorType = targetvalidatorType;
        }
        #endregion

        #region Public validation methods
        public abstract ValidationResult GetValidationResult(object value, ValidationContext context);
        #endregion

        #region Helper methods
        private string FormatErrorMessage(params object[] errorParts)
        {
            return string.Format(ErrorMessage, errorParts);
        }
        #endregion
    }
    public class ValidationResult
    {
        public static ValidationResult Success =
            new ValidationResult(string.Empty, null);

        #region Constructor
        public ValidationResult(string errorMessage, IEnumerable<string> memberNames)
        {
            this.ErrorMessage = errorMessage;
            this.MemberNames = memberNames;
        }
        #endregion

        #region Properties
        public string ErrorMessage { get; private set; }
        public IEnumerable<string> MemberNames { get; private set; }
        #endregion
    }
    public class ValidationContext : IServiceProvider
    {
        #region Members
        public Type ObjectType { get; private set; }
        public object ObjectInstance { get; set; }
        public string MemberName { get; set; }
        public IServiceProvider ServiceProvider { get; private set; }
        #endregion

        #region Constructor
        public ValidationContext(Type objectType, object instance, string memberName,
            IServiceProvider provider)
        {
            if (objectType != null)
                this.ObjectType = objectType;
            else if (instance != null)
                this.ObjectType = instance.GetType();

            this.ObjectInstance = instance;
            this.MemberName = memberName;
            this.ServiceProvider = provider;
        }
        public ValidationContext(Type objectType, object instance, IServiceProvider provider) :
            this(objectType, instance, string.Empty, provider)
        { }
        #endregion

        #region IServiceProvider Members
        public object GetService(Type serviceType)
        {
            if (ServiceProvider != null)
                return ServiceProvider.GetService(serviceType);

            return new object();
        }
        #endregion
    }
}