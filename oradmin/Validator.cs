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
        void ValidateProperty(string property, object value);
    }

    public abstract class EntityValidator : IEntityValidator, IDataErrorInfo,
        IEntityWithErrorReporting
    {
        #region Members
        /// <summary>
        /// Associated entity
        /// </summary>
        protected static Type validatorType;
        protected ValidationContext validationContext;
        IServiceProvider validationServiceProvider;
        /// <summary>
        /// Lists of entity-level validators
        /// </summary>
        protected static EntityValidatorsList entityValidators;
        /// <summary>
        /// Lists of property-level validators for the entity
        /// </summary>
        protected static PropertyValidatorsLists propertyValidators;
        protected static List<string> propertyNames;

        protected List<string> entityErrors =
            new List<string>();
        protected Dictionary<string, List<string>> propertyErrors =
            new Dictionary<string, List<string>>();
        protected Dictionary<string, object> propertyValues =
            new Dictionary<string, object>();
        #endregion

        #region Static methods
        protected static void Initialize(Type validatorType, Type entityType)
        {
            // try to set the validator type
            if (!SetValidatorType(validatorType))
                return;

            LoadEntityValidators(entityType);
            LoadPropertyValidators(entityType);
        }
        protected static void LoadEntityValidators(Type entityType)
        {
            entityValidators =
                from validator in entityType.GetCustomAttributes(
                    typeof(MyValidationAttribute), true) as IEnumerable<MyValidationAttribute>
                where validator.TargetValidatorType.Equals(validatorType)
                select validator;
        }
        protected static void LoadPropertyValidators(Type entityType)
        {
            propertyValidators = new Dictionary<string, IEnumerable<MyValidationAttribute>>();
            propertyNames = new List<string>();

            foreach (PropertyInfo p in entityType.GetProperties())
            {
                IEnumerable<MyValidationAttribute> atts =
                    from attribute in p.GetCustomAttributes(typeof(MyValidationAttribute), true)
                    as IEnumerable<MyValidationAttribute>
                    where attribute.TargetValidatorType.Equals(validatorType)
                    select attribute;

                if(atts.Count() > 0)
                {
                    propertyValidators.Add(p.Name, atts);
                    propertyNames.Add(p.Name);
                }
            }
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
        public EntityValidator(IEntityObject entity, IServiceProvider validationServiceProvider)
        {
            this.validationContext = new ValidationContext(entity, validationServiceProvider);

            // initialize property dictionaries
            initializePropertyDictionaries();
            // set up event bindings
            entity.PropertyChangedPassingValue += new PropertyChangedPassingValueHandler(entity_PropertyChangedPassingValue);
        }
        #endregion
        
        #region IDataErrorInfo Members
        public string Error
        {
            get
            {
                return
                    string.Join(Environment.NewLine,
                                entityErrors.ToArray());
            }
        }
        public string this[string columnName]
        {
            get
            {
                return
                    string.Join(Environment.NewLine,
                                propertyErrors[columnName].ToArray());
            }
        }
        #endregion

        #region IEntityValidator Members
        public void ValidateEntity()
        {
            // validate properties
            clearPropertyErrors();
            foreach (KeyValuePair<string, object> pair in propertyValues)
            {
                ValidateProperty(pair.Key, pair.Value);
            }

            // if there are no errors, proceed to entity validation

            if (HasErrors)
                return;

            // clear errors
            clearEntityOnlyErrors();
            validationContext.MemberName = string.Empty;

            foreach (MyValidationAttribute validator in entityValidators)
            {
                ValidationResult result =
                    validator.GetValidationResult(null, validationContext);

                // error occured -> 
                if (result != ValidationResult.Success)
                {
                    HasErrors = true;
                    entityErrors.Add(result.ErrorMessage);
                    addPropertyErrorsToProperties(result.ErrorMessage, result.MemberNames);
                }
            }
        }
        public void ValidateProperty(string propertyName, object value)
        {
            clearPropertyError(propertyName);
            validationContext.MemberName = propertyName;

            foreach (MyValidationAttribute validator in propertyValidators[propertyName])
            {
                ValidationResult result = validator.GetValidationResult(
                    value, validationContext);

                if (result != ValidationResult.Success)
                {
                    addPropertyErrorsToProperties(result.ErrorMessage, result.MemberNames);
                    HasErrors = true;
                }
            }
        }
        #endregion

        #region Helper methods
        private void clearErrors()
        {
            clearEntityOnlyErrors();
            clearPropertyErrors();
        }
        private void clearEntityOnlyErrors()
        {
            entityErrors.Clear();
        }
        private void clearPropertyErrors()
        {
            foreach (List<string> errors in propertyErrors.Values)
            {
                errors.Clear();
            }
        }
        private void clearPropertyError(string propertyName)
        {
            propertyErrors[propertyName].Clear();
        }
        private void addPropertyErrorsToProperties(string errorMessage,
            IEnumerable<string> propertyNames)
        {
            foreach (string propertyName in propertyNames)
            {
                propertyErrors[propertyName].Add(errorMessage);
            }
        }
        void entity_PropertyChangedPassingValue(object sender, PropertyChangedPassingValueEventArgs e)
        {
            string propertyName = e.PropertyName;

            if (propertyValues.ContainsKey(propertyName))
            {
                propertyValues[propertyName] = e.Value;
            }
        }
        void initializePropertyDictionaries()
        {
            propertyValues = new Dictionary<string, object>();
            propertyErrors = new Dictionary<string, List<string>>();

            foreach (string propertyName in propertyNames)
            {
                propertyValues.Add(propertyName, null);
                propertyErrors.Add(propertyName, new List<string>());
            }
        }
        #endregion

        #region IEntityWithErrorReporting Members
        public bool HasErrors { get; private set; }
        #endregion
    }
}