using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace oradmin
{
    using PropertyValidatorsLists = Dictionary<string, IEnumerable<MyValidationAttribute>>;
    using PropertyValidatorsPair = KeyValuePair<string, IEnumerable<MyValidationAttribute>>;
    using EntityValidatorsList = IEnumerable<MyValidationAttribute>;

    using PropertyAttributesPair = KeyValuePair<string, IEnumerable<Attribute>>;
    using PropertyAttributesLists = IEnumerable<KeyValuePair<string, IEnumerable<Attribute>>>;
    using EntityAttributesList = IEnumerable<Attribute>;
    
    public interface IEntityValidator
    {
        void ValidateEntity();
        void ValidateProperty(string property);
    }

    public abstract class EntityValidator<TEntity> : IEntityValidator, IDataErrorInfo
        where TEntity : EntityObject
    {
        #region Members
        /// <summary>
        /// Associated entity
        /// </summary>
        protected TEntity entity;
        protected static Type validatorType;
        /// <summary>
        /// Lists of entity-level validators
        /// </summary>
        protected static EntityValidatorsList entityValidators;
        /// <summary>
        /// Lists of property-level validators for the entity
        /// </summary>
        protected static PropertyValidatorsLists propertyValidators;
        #endregion

        #region Static methods
        protected void Initialize(Type type)
        {
            // try to set the validator type
            if (!SetValidatorType(type))
                return;

            // load validation attributes from an entity type
            LoadEntityValidators();
        }
        protected static void LoadEntityValidators()
        {
            Type entityType = typeof(TEntity);

            entityValidators = 
        }
        private static void LoadPropertyValidators()
        {

        }
        private static bool SetValidatorType(Type type)
        {
            if(EntityValidator<TEntity>.validatorType != null ||
               type == null ||
               !type.IsSubclassOf(typeof(EntityValidator<TEntity>)))
            {
                return false;
            }

            EntityValidator<TEntity>.validatorType = type;

            return true;
        }
        #endregion
        
        #region Constructors
        

        public EntityValidator(EntityObject entity)
        {
            if (entity == null)
                throw new ArgumentNullException("Entity");

            this.entity = entity;
        }
        #endregion
        
        #region IDataErrorInfo Members
        public string Error
        {
            get { throw new NotImplementedException(); }
        }
        public string this[string columnName]
        {
            get { throw new NotImplementedException(); }
        }
        #endregion

        #region IEntityValidator Members
        public void ValidateEntity()
        {
            throw new NotImplementedException();
        }
        public void ValidateProperty(string property)
        {
            throw new NotImplementedException();
        }
        #endregion
    }

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
        public abstract ValidationResult GetValidationResult(object value);
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
        public object ObjectInstance { get; private set; }
        public string MemberName { get; set; }
        #endregion

        #region Constructor
        public ValidationContext(object instance, string memberName,
            IServiceProvider provider)
        {
            if (instance == null)
                throw new ArgumentNullException("instance");

            this.ObjectType = instance.GetType();
            this.ObjectInstance = instance;
            this.MemberName = memberName;
        }
        public ValidationContext(object instance, IServiceProvider provider) :
            this(instance, string.Empty, provider)
        { }
        #endregion

        #region IServiceProvider Members
        public object GetService(Type serviceType)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}