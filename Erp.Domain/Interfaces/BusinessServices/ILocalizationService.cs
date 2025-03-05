using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.Interfaces.BusinessServices;

/// <summary>
/// Interface for localization service
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Gets the localized string based on the key
    /// </summary>
    /// <param name="key">Resource key</param>
    /// <returns>Localized string</returns>
    string GetLocalizedString(string key);

    /// <summary>
    /// Gets the localized string based on the key and format parameters
    /// </summary>
    /// <param name="key">Resource key</param>
    /// <param name="args">Format parameters</param>
    /// <returns>Localized and formatted string</returns>
    string GetLocalizedString(string key, params object[] args);
}
