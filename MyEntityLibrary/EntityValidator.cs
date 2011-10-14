using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace myentitylibrary
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
    using PropertiesCrossPropertyValidatorsErrorMessages =
        Dictionary<string, Dictionary<IMyValidationCrossPropertyAttribute, string>>;
    using PropertiesPropertyValidatorsErrorMessages =
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
    public interface IEntityValidatorForEntityObject<TData, TKey>:
        IEntityValidator
        where TData : IEntityDataContainer<TKey>
        where TKey : IEquatable<TKey>
    {
        void AssociateWithEntity(IEntityObjectBase<TData, TKey> entity);
        void DisassociateFromEntity();
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
            out CrossEntityValidatorsEnumeration crossEntityValidators,
            out EntityValidatorsEnumeration entityValidators,
            out CrossPropertyValidatorsEnumeration crossPropertyValidators,
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
        private PropertyErrorIndicators propertyFieldErrorsIndicators =
            new PropertyErrorIndicators();
        private PropertyErrorIndicators propertyCrossFieldErrorIndicators =
            new PropertyErrorIndicators();

        bool hasCrossEntityErrors,
             hasEntityErrors,
             hasCrossPropertyErrors,
             hasPropertyErrors;

        int propertiesWithFieldErrorCount = 0;
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
        protected PropertiesPropertyValidatorsErrorMessages propertiesFieldErrors =
            new PropertiesPropertyValidatorsErrorMessages();
        protected PropertiesCrossPropertyValidatorsErrorMessages propertiesCrossFieldErrors =
            new PropertiesCrossPropertyValidatorsErrorMessages();
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
            // validates properties and removes the dependent validators' errors
            this.validateProperties();

            // if there are no errors, proceed to entity validation
            if (this.HasErrors)
                return;

            // unset the member name
            validationContext.MemberName = string.Empty;
            
            // run cross-property validation and remove the dependent (entity-level)
            // validators' errors
            this.runCrossPropertyLevelValidation();

            // if there are errors, return
            if (this.HasErrors)
                return;

            // run entity-level validation
            this.runEntityLevelValidation();

            // if there are errors, return
            if (this.HasErrors)
                return;

            // run cross-entity-level validation
            this.runCrossEntityLevelValidation();

            // determine the final HasErrors value

        }
        public void ValidateProperty(string propertyName, object value)
        {
            // set a member name to a validation context
            validationContext.MemberName = propertyName;

            // remove errors
            this.removePropertyErrors(propertyName);

            bool hasPropertyLevelErrors;

            foreach (IMyValidationPropertyAttribute validator in this.propertyValidators[propertyName])
            {
                ValidationResult result = validator.GetValidationResult(
                    value, validationContext);

                if (result != ValidationResult.Success)
                {
                    this.addPropertyValidatorError(propertyName, validator, result);
                    hasPropertyLevelErrors = true;
                }
            }

            // check for property-level on a property
            this.propertyFieldErrorsIndicators[propertyName] = hasPropertyLevelErrors;

            // if it has no errors, call the higher level to try validating
            if (!hasPropertyLevelErrors)
            {
                this.runCrossPropertyValidatorsWhenPropertyBecomesValid(propertyName);
            }
        }
        #endregion

        #region Helper methods
        private void runCrossEntityLevelValidation()
        {

        }
        private void runEntityLevelValidation()
        {

        }
        private void runCrossPropertyLevelValidation()
        {

        }
        private void validateProperties()
        {
            // remove all validation errors including the higher layer
        }
        private void validatePropertyHelper(string propertyName)
        {

        }
        private void runCrossPropertyValidatorsWhenPropertyBecomesValid(
            string propertyName)
        {

        }

        private void reEvaluatePropertyFieldErrorIndicator(string propertyName)
        {
            bool oldHasFieldErrors = this.propertyFieldErrorsIndicators[propertyName],
                 newHasFieldErrors = this.propertiesFieldErrors[propertyName].Count > 0;

            // if the error state changed, decide whether to increase or decrease
            // erroneous fields count
            if (oldHasFieldErrors != newHasFieldErrors)
            {
                if (newHasFieldErrors)
                    ++this.propertiesWithFieldErrorCount;
                else
                    --this.propertiesWithFieldErrorCount;
            }

            // set the new error indicator value
            this.propertyFieldErrorsIndicators[propertyName] = newHasFieldErrors;
        }

        private void removePropertyErrors(string propertyName)
        {
            // remove errors on this level
            this.removePropertyFieldErrors(propertyName);
            // invalidate higher level dependent (cross-property) validators
            this.removePropertyCrossFieldErrors(propertyName);
        }
        private void removePropertyFieldErrors(string propertyName)
        {
            // clear property errors from a dictionary
            foreach (string field in this.propertiesFieldErrors.Keys)
            {
                this.propertiesFieldErrors[field].Clear();
            }
            // set field-level error indicator for the property
            this.reEvaluatePropertyFieldErrorIndicator(propertyName);
        }
        private void removePropertyCrossFieldErrors(string propertyName)
        {
            // remove dependent entity errors
            this.removeEntityErrors();

            // remove cross-property level validators for propertyName
            IEnumerable<IMyValidationCrossPropertyAttribute> validators =
                this.propertiesCrossFieldErrors[propertyName].Keys;

            // remove cross-field level validators from all properties and
            // set error indicators for the properties
            foreach (IMyValidationCrossPropertyAttribute validator in validators)
            {
                removeCrossFieldError(validator);
            }

            // set hasCrossFieldErrors error indicator accordingly
            this.hasCrossPropertyErrors = this.crossPropertyErrors.Count > 0;
        }
        private void removeCrossFieldError(IMyValidationCrossPropertyAttribute validator)
        {
            // remove the validator from the cross-property validators dictionary
            this.crossPropertyErrors.Remove(validator);

            // remove the validator from all properties
            foreach (string propertyName in validator.MemberNames)
            {
                this.removeCrossFieldErrorFromProperty(validator, propertyName);
            }
        }
        private void removeCrossFieldErrorFromProperty(
            IMyValidationCrossPropertyAttribute validator,
            string propertyName)
        {
            Dictionary<IMyValidationCrossPropertyAttribute, string> crossFieldErrors =
                this.propertiesCrossFieldErrors[propertyName];
            // remove the validator from cross-field level property dictionary
            crossFieldErrors.Remove(validator);
            // set error indicator for cross-property level errors for the property
            this.reEvaluatePropertyFieldErrorIndicator(propertyName);
        }
        private void removeEntityErrors()
        {
            // remove cross-entity errors
            this.removeCrossEntityErrors();
            // remove entity errors
            this.entityErrors.Clear();
            // set hasEntityErrors accordingly
            this.hasEntityErrors = false;
        }
        private void removeCrossEntityErrors()
        {
            // remove cross-entity errors
            this.crossEntityErrors.Clear();
            // set hasCrossEntityErrors accordingly
            this.hasCrossEntityErrors = false;
        }

        private void addPropertyValidatorError(
            string propertyName,
            IMyValidationPropertyAttribute validator,
            ValidationResult badResult)
        {
            string errorMessage = badResult.ErrorMessage;

            // walk all bad properties and add the error message to their errors
            foreach (string propertyName in badPropertiesNames)
            {
                PropertyValidatorsErrorMessages propertyErrorsMessages =
                    this.propertiesFieldErrors[propertyName];

                // add error information to property errors dictionary
                if (!propertyErrorsMessages.ContainsKey(validator))
                    propertyErrorsMessages.Add(validator, errorMessage);
                else
                    propertyErrorsMessages[validator] = errorMessage;

                // set error indicator for the affected bad property member to true
                this.propertyFieldErrorsIndicators[propertyName] = true;
            }
        }
        private void clearPropertyValidationError(string propertyName,
            IMyValidationPropertyAttribute validator)
        {
            PropertyValidatorsErrorMessages propertyErrors =
                this.propertiesFieldErrors[propertyName];
            // remove validation error
            propertyErrors.Remove(validator);
            // check for property errors left
            this.propertyFieldErrorsIndicators[propertyName] = propertyErrors.Count > 0;
        }

        private void clearValidatorErrorFromProperties(IMyValidationAttribute validator)
        {
            //foreach (string possiblyAffectedMember in validator.MemberNames)
            //{
            //    ValidatorsErrorMessages propertyErrorMessages =
            //        this.propertiesErrors[possiblyAffectedMember];
            //    // remove validator error from property validator error set
            //    propertyErrorMessages.Remove(validator);
            //    // check for remaining property errors
            //    this.propertyErrorsIndicators[possiblyAffectedMember] =
            //        propertyErrorMessages.Count > 0;
            //}
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
                this.propertyFieldErrorsIndicators.Add(propertyName, false);
                this.propertiesFieldErrors.Add(
                    propertyName,
                    new Dictionary<IMyValidationPropertyAttribute, string>());
            }
        }
        /// <summary>
        /// Initializes property values by reading them from the associated entity
        /// in custom subclass.
        /// </summary>
        protected abstract void initializePropertyValues();
        #endregion

        /// <summary>
        /// Clears all errors and reads the new entity data.
        /// </summary>
        /// <param name="entity">Entity to be associated with.</param>
        public void AssociateWithEntity(TEntity entity)
        {
            
        }

        #region IEntityWithErrorReporting Members
        public bool HasErrors { get; private set; }
        #endregion
    } 
    #endregion
}