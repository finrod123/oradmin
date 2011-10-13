using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace myentitylibrary
{
    public enum EValidationLevel
    {
        Property,
        CrossProperty,
        Entity,
        CrossEntity
    }

    /// <summary>
    /// Equality comparer for MyValidation attributes.
    /// Two MyValidationAttributes instances are equal, if and only if
    /// their types equal. This is valid approach assuming there will
    /// be only one instance of MyValidationAttribute on a class or property.
    /// Otherwise it would be necessary to add ID resolution.
    /// </summary>
    #region MyValidationAttribute equality comparer
    public class MyValidationAttributeEqualityComparer :
    EqualityComparer<IMyValidationAttribute>
    {
        public override bool Equals(IMyValidationAttribute x, IMyValidationAttribute y)
        {
            return x.GetType()
                    .Equals(
                   y.GetType());
        }
        public override int GetHashCode(IMyValidationAttribute obj)
        {
            return obj.GetHashCode();
        }
    } 
    #endregion
    
    /// <summary>
    /// Interface to add to custom attributes to support custom validator targetting
    /// and validation levels.
    /// </summary>
    public interface IMyValidationAttribute
    {
        Type TargetValidatorType { get; }
        string ErrorMessage { get; }
        
        ValidationResult GetValidationResult(object value, ValidationContext context);
        string FormatErrorMessage(params object[] errorParts);
    }

    /// <summary>
    /// Interface for single-property level validation
    /// </summary>
    public interface IMyValidationPropertyAttribute : IMyValidationAttribute
    { }

    /// <summary>
    /// Interface for cross-property level validation. It provides the enumeration of properties,
    /// which participate in cross-property validation process.
    /// </summary>
    public interface IMyValidationCrossPropertyAttribute : IMyValidationAttribute
    {
        IEnumerable<string> MemberNames { get; }
    }

    /// <summary>
    /// Interface for entity-level validation.
    /// </summary>
    public interface IMyValidationEntityAttribute : IMyValidationAttribute
    { }

    /// <summary>
    /// Interface for cross-entity level validation.
    /// </summary>
    public interface IMyValidationCrossEntityAttribute : IMyValidationAttribute
    { }

    /// <summary>
    /// Validation result class with an error message and a list of erroneous members
    /// </summary>
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

    /// <summary>
    /// Validation context with object type, ibject instance and member fields
    /// to describe the context of validation. Arbitrarily it provides validation
    /// service to custom entities.
    /// </summary>
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