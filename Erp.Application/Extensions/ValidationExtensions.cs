using Erp.Domain.Constants;
using Erp.Domain.Interfaces.BusinessServices;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Application.Extensions
{
    /// <summary>
    /// Extension methods for validation
    /// </summary>
    public static class ValidationExtensions
    {
        /// <summary>
        /// Adds a localized error message to the rule
        /// </summary>
        /// <typeparam name="T">Type of the object being validated</typeparam>
        /// <typeparam name="TProperty">Type of the property being validated</typeparam>
        /// <param name="rule">The rule</param>
        /// <param name="localizationService">Localization service</param>
        /// <param name="resourceKey">Resource key for the error message</param>
        /// <param name="args">Arguments for the error message</param>
        /// <returns>The rule with the localized error message</returns>
        public static IRuleBuilderOptions<T, TProperty> WithLocalizedMessage<T, TProperty>(
            this IRuleBuilderOptions<T, TProperty> rule,
            ILocalizationService localizationService,
            string resourceKey,
            params object[] args)
        {
            return rule.WithMessage(localizationService.GetLocalizedString(resourceKey, args));
        }

        /// <summary>
        /// Adds a localized error message for required field
        /// </summary>
        /// <typeparam name="T">Type of the object being validated</typeparam>
        /// <typeparam name="TProperty">Type of the property being validated</typeparam>
        /// <param name="rule">The rule</param>
        /// <param name="localizationService">Localization service</param>
        /// <param name="fieldName">Name of the field</param>
        /// <returns>The rule with the localized error message</returns>
        public static IRuleBuilderOptions<T, TProperty> WithRequiredMessage<T, TProperty>(
            this IRuleBuilderOptions<T, TProperty> rule,
            ILocalizationService localizationService,
            string fieldName)
        {
            return rule.WithMessage(localizationService.GetLocalizedString(ResourceKeys.Validation.RequiredField, fieldName));
        }
    }
} 