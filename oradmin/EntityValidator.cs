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
    

    public interface IEntityValidator
    {
        void ValidateEntity();
        void ValidateProperty(string property, object value);
    }

    public abstract class EntityValidator<TEntity, TData, TKey> :
        IEntityValidator,
        IDataErrorInfo,
        IErrorIndicator
        where TEntity : IEntityObject<TData, TKey>
        where TData   : IEntityDataContainer<TKey>
        where TKey    : IEquatable<TKey>
    {
        #region Members
        /// <summary>
        /// Associated entity
        /// </summary>
        protected ValidationContext validationContext;
        /// <summary>
        /// List of entity error messages
        /// </summary>
        protected List<string> entityErrors =
            new List<string>();
        /// <summary>
        /// Lists of property error messages
        /// </summary>
        protected Dictionary<string, List<string>> propertyErrors =
            new Dictionary<string, List<string>>();
        // List of cached current property values (in order to access then during entity validation)
        protected Dictionary<string, object> propertyValues;
        #endregion

        #region Static methods
        protected static void Initialize(
            Type validatorType,
            out EntityValidatorsList entityValidators,
            out PropertyValidatorsLists propertyValidators,
            out List<string> propertyNames)
        {
            // try to set the validator type
            if (!validatorType.IsSubclassOf(typeof(EntityValidator<TEntity, TData, TKey>)))
            {
                entityValidators = null;
                propertyValidators = null;
                propertyNames = null;
                return;
            }

            LoadEntityValidators(validatorType, out entityValidators);
            LoadPropertyValidators(validatorType, out propertyValidators, out propertyNames);
        }
        protected static void LoadEntityValidators(Type validatorType,
            out EntityValidatorsList entityValidators)
        {
            entityValidators =
                from validator in typeof(TEntity).GetCustomAttributes(
                    typeof(MyValidationAttribute), true) as IEnumerable<MyValidationAttribute>
                where validator.TargetValidatorType.Equals(validatorType)
                select validator;
        }
        protected static void LoadPropertyValidators(Type validatorType,
            out PropertyValidatorsLists propertyValidators,
            out List<string> propertyNames)
        {
            propertyValidators = new Dictionary<string, IEnumerable<MyValidationAttribute>>();
            propertyNames = new List<string>();

            foreach (PropertyInfo p in typeof(TEntity).GetProperties())
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
        #endregion

        protected EntityValidatorsList entityValidators;
        protected PropertyValidatorsLists propertyValidators;
        
        #region Constructors
        protected EntityValidator(
            TEntity entity,
            List<string> propertyNames,
            EntityValidatorsList entityValidators,
            PropertyValidatorsLists propertyValidators,
            ValidationContext validationContext)
        {
            // set up references to custom static validators
            this.entityValidators = entityValidators;
            this.propertyValidators = propertyValidators;
            this.validationContext = validationContext;
            // initialize property dictionaries
            this.initializePropertyErrors(propertyNames);
            this.initializePropertyValues();
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
        
        protected void initializePropertyErrors(List<string> propertyNames)
        {
            foreach (string propertyName in propertyNames)
            {
                propertyErrors.Add(propertyName, new List<string>());
            }
        }
        protected abstract void initializePropertyValues();
        #endregion

        #region IEntityWithErrorReporting Members
        public bool HasErrors { get; private set; }
        #endregion
    }
}