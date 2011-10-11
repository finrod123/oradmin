using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace oradmin
{
    #region Using directives

    #region Validation attributes
    using ValidatorsEnumeration = IEnumerable<IMyValidationAttribute>;
    using PropertyValidatorsEnumeration = IEnumerable<IMyValidationPropertyAttribute>;
    using PropertyValidatorsEnumerations =
        Dictionary<string, IEnumerable<IMyValidationPropertyAttribute>>;
    using CrossPropertyValidatorsEnumeration = IEnumerable<IMyValidationCrossPropertyAttribute>;
    using EntityValidatorsEnumeration = IEnumerable<IMyValidationEntityAttribute>;
    using CrossEntityValidatorsEnumeration = IEnumerable<IMyValidationCrossEntityAttribute>;
    #endregion

    #region Validation attributes error holders
    using ValidatorsErrorMessages = Dictionary<IMyValidationAttribute, string>;
    using PropertyValidatorsErrorMessages = Dictionary<IMyValidationPropertyAttribute, string>;
    using PropertiesValidatorsErrorMessages =
        Dictionary<string, Dictionary<IMyValidationPropertyAttribute, string>>;
    
    using CrossPropertyValidatorsErrorMessages =
        Dictionary<IMyValidationCrossPropertyAttribute, string>;
    using EntityValidatorsErrorMessages = Dictionary<IMyValidationEntityAttribute, string>;
    using CrossEntityValidatorsErrorMessages =
        Dictionary<IMyValidationCrossEntityAttribute, string>;
    #endregion

    #region Helper structures
    using ValidatorsList = List<IMyValidationAttribute>;
    using CrossPropertyValidatorsList = List<IMyValidationCrossPropertyAttribute>;
    using EntityValidatorsList = List<IMyValidationEntityAttribute>;
    using CrossEntityValidatorsList = List<IMyValidationCrossEntityAttribute>;
    using PropertyErrorIndicators = Dictionary<string, bool>;
    #endregion

    #endregion

    #region Interfaces
    public interface IEntityValidator
    {
        void ValidateEntity();
        void ValidateProperty(string property, object value);
    }
    #endregion

    #region Entity validation class
    public abstract class EntityValidator<TEntity, TData, TKey> :
    IEntityValidator,
    IDataErrorInfo,
    IErrorIndicator
        where TEntity : IEntityObject<TData, TKey>
        where TData : IEntityDataContainer<TKey>
        where TKey : IEquatable<TKey>
    {
        #region Members
        /// <summary>
        /// Associated entity
        /// </summary>
        protected TEntity entity;

        /// <summary>
        /// Validation context
        /// </summary>
        protected ValidationContext validationContext;

        // List of cached current property values (in order to access then during entity validation)
        protected Dictionary<string, object> propertyValues;
        #endregion

        #region Static methods
        protected static void Initialize(
            Type validatorType,
            out ValidatorsEnumeration crossEntityValidators,
            out ValidatorsEnumeration entityValidators,
            out ValidatorsEnumeration crossPropertyValidators,
            out PropertyValidatorsEnumerations propertyValidators,
            out List<string> propertyNames)
        {
            // try to set the validator type
            if (!validatorType.IsSubclassOf(typeof(EntityValidator<TEntity, TData, TKey>)))
            {
                crossEntityValidators = null;
                entityValidators = null;
                crossPropertyValidators = null;
                propertyValidators = null;
                propertyNames = null;
                return;
            }

            // load cross-property validators, entity validators and cross-entity validators
            LoadEntityLevelValidators(
                validatorType,
                out crossEntityValidators,
                out entityValidators,
                out crossPropertyValidators);
            
            // load property validators
            LoadPropertyValidators(validatorType, out propertyValidators, out propertyNames);
        }
        protected static void LoadEntityLevelValidators(
            Type validatorType,
            out CrossEntityValidatorsEnumeration crossEntityValidators,
            out EntityValidatorsEnumeration entityValidators,
            out CrossPropertyValidatorsEnumeration crossPropertyValidators)
        {
            CrossEntityValidatorsList crossEntityValidatorsList =
                new CrossEntityValidatorsList();
            EntityValidatorsList entityValidatorsList =
                new EntityValidatorsList();
            CrossPropertyValidatorsList crossPropertyValidatorsList =
                new CrossPropertyValidatorsList();

            foreach (IMyValidationAttribute validator
                in typeof(TEntity).GetCustomAttributes(typeof(IMyValidationAttribute), true))
            {
                if (!validator.TargetValidatorType.Equals(validatorType))
                    continue;

                if (typeof(IMyValidationCrossPropertyAttribute).Equals(validator.GetType()))
                    crossPropertyValidatorsList.Add(validator as IMyValidationCrossPropertyAttribute);
                else if (typeof(IMyValidationEntityAttribute).Equals(validator.GetType()))
                    entityValidatorsList.Add(validator as IMyValidationEntityAttribute);
                else if (typeof(IMyValidationCrossEntityAttribute).Equals(validator.GetType()))
                    crossEntityValidatorsList.Add(validator as IMyValidationCrossEntityAttribute);
            }

            crossEntityValidators = crossEntityValidatorsList.AsEnumerable();
            entityValidators = entityValidatorsList.AsEnumerable();
            crossPropertyValidators = crossPropertyValidatorsList.AsEnumerable();
        }
        
        protected static void LoadPropertyValidators(
            Type validatorType,
            out PropertyValidatorsEnumerations propertyValidators,
            out List<string> propertyNames)
        {
            propertyValidators = new PropertyValidatorsEnumerations();
            propertyNames = new List<string>();

            foreach (PropertyInfo p in typeof(TEntity).GetProperties())
            {
                PropertyValidatorsEnumeration atts =
                    from attribute in p.GetCustomAttributes(typeof(IMyValidationPropertyAttribute), true)
                    as PropertyValidatorsEnumeration
                    where attribute.TargetValidatorType.Equals(validatorType)
                    select attribute;

                if (atts.Count() > 0)
                {
                    propertyValidators.Add(p.Name, atts);
                    propertyNames.Add(p.Name);
                }
            }
        }
        #endregion

        #region Error detection members
        private bool hasEntityErrors;
        private PropertyErrorIndicators propertyErrorsIndicators =
            new PropertyErrorIndicators();
        private int propertyErrorsCount;
        #endregion

        #region Entity and property Validators
        protected CrossEntityValidatorsEnumeration crossEntityValidators;
        protected EntityValidatorsEnumeration entityValidators;
        protected CrossPropertyValidatorsEnumeration crossPropertyValidators;
        protected PropertyValidatorsEnumerations propertyValidators;
        #endregion

        #region ValidationAttributes in error holders
        protected CrossEntityValidatorsErrorMessages crossEntityErrors =
            new CrossEntityValidatorsErrorMessages();
        protected EntityValidatorsErrorMessages entityErrors =
            new EntityValidatorsErrorMessages();
        protected CrossPropertyValidatorsErrorMessages crossPropertyErrors =
            new CrossPropertyValidatorsErrorMessages();

        // TODO: nutno inicializovat
        protected PropertiesValidatorsErrorMessages propertiesErrors =
            new PropertiesValidatorsErrorMessages();
        #endregion

        #region Constructors
        protected EntityValidator(
            TEntity entity,
            List<string> propertyNames,
            CrossEntityValidatorsEnumeration crossEntityValidators,
            EntityValidatorsEnumeration entityValidators,
            CrossPropertyValidatorsEnumeration crossPropertyValidators,
            PropertyValidatorsEnumerations propertyValidators,
            ValidationContext validationContext)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            this.entity = entity;
            // set up references to custom static validators
            this.crossEntityValidators = crossEntityValidators;
            this.entityValidators = entityValidators;
            this.crossPropertyValidators = crossPropertyValidators;
            this.propertyValidators = propertyValidators;
            // set up validation context
            this.validationContext = validationContext;
            // initialize property dictionaries
            this.initializePropertyErrorIndicatorsAndMessages(propertyNames);
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
                return "";
            }
        }
        public string this[string columnName]
        {
            get
            {
                return "";
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

            foreach (IMyValidationAttribute validator in entityValidators)
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
            validationContext.MemberName = propertyName;

            foreach (IMyValidationAttribute validator in this.propertyValidators[propertyName])
            {
                ValidationResult result = validator.GetValidationResult(
                    value, validationContext);

                if (result != ValidationResult.Success)
                {
                    this.addValidatorErrorToProperties(validator, result);
                    HasErrors = true;
                }
            }
        }
        #endregion

        #region Helper methods
        private void addValidatorErrorToProperties(
            IMyValidationAttribute validator,
            ValidationResult badResult)
        {
            // get validation result data
            IEnumerable<string> badPropertiesNames = badResult.MemberNames;
            string errorMessage = badResult.ErrorMessage;

            // walk all bad properties and add the error message to their errors
            foreach (string propertyName in badPropertiesNames)
            {
                ValidatorsErrorMessages propertyErrorsMessages =
                    this.propertiesErrors[propertyName];

                // add error information to property errors dictionary
                if (!propertyErrorsMessages.ContainsKey(validator))
                    propertyErrorsMessages.Add(validator, errorMessage);
                else
                    propertyErrorsMessages[validator] = errorMessage;

                // set error indicator for the affected bad property member to true
                this.propertyErrorsIndicators[propertyName] = true;
            }
        }
        private void clearPropertyValidationError(string propertyName,
            IMyValidationPropertyAttribute validator)
        {
            PropertyValidatorsErrorMessages propertyErrors =
                this.propertiesErrors[propertyName];
            // remove validation error
            propertyErrors.Remove(validator);
            // check for property errors left
            this.propertyErrorsIndicators[propertyName] = propertyErrors.Count > 0;
        }

        private void clearValidatorErrorFromProperties(IMyValidationAttribute validator)
        {
            foreach (string possiblyAffectedMember in validator.MemberNames)
            {
                ValidatorsErrorMessages propertyErrorMessages =
                    this.propertiesErrors[possiblyAffectedMember];
                // remove validator error from property validator error set
                propertyErrorMessages.Remove(validator);
                // check for remaining property errors
                this.propertyErrorsIndicators[possiblyAffectedMember] =
                    propertyErrorMessages.Count > 0;
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
        protected void initializePropertyErrorIndicatorsAndMessages(List<string> propertyNames)
        {
            foreach (string propertyName in propertyNames)
            {
                this.propertyErrorsIndicators.Add(propertyName, false);
                this.propertiesErrors.Add(
                    propertyName,
                    new Dictionary<IMyValidationAttribute, string>());
            }
        }
        /// <summary>
        /// Initializes property values by reading them from the associated entity
        /// in custom subclass.
        /// </summary>
        protected abstract void initializePropertyValues();
        #endregion

        #region IEntityWithErrorReporting Members
        public bool HasErrors { get; private set; }
        #endregion
    } 
    #endregion
}