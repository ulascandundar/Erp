using Erp.Domain.Interfaces.BusinessServices;
using Erp.Domain.Resources;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Application.Services.LocalizationServices
{
    /// <summary>
    /// Implementation of the localization service
    /// </summary>
    public class LocalizationService : ILocalizationService
    {
        private readonly IStringLocalizer<SharedResource> _localizer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizer">String localizer</param>
        public LocalizationService(IStringLocalizer<SharedResource> localizer)
        {
            _localizer = localizer;
        }

        /// <summary>
        /// Gets the localized string based on the key
        /// </summary>
        /// <param name="key">Resource key</param>
        /// <returns>Localized string</returns>
        public string GetLocalizedString(string key)
        {
            return _localizer[key];
        }

        /// <summary>
        /// Gets the localized string based on the key and format parameters
        /// </summary>
        /// <param name="key">Resource key</param>
        /// <param name="args">Format parameters</param>
        /// <returns>Localized and formatted string</returns>
        public string GetLocalizedString(string key, params object[] args)
        {
            return _localizer[key, args];
        }
    }
} 