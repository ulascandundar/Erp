using Erp.Api.Controllers.Abstracts;
using Erp.Domain.Interfaces.BusinessServices;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace Erp.API.Controllers;
public class CultureController : BaseV1Controller
{
    private readonly ILocalizationService _localizationService;

    public CultureController(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

    /// <summary>
    /// Changes the current culture
    /// </summary>
    /// <param name="culture">Culture code (e.g., tr-TR, en-US)</param>
    /// <returns>Success message</returns>
    [HttpGet("setCulture")]
    public IActionResult SetCulture(string culture)
    {
        if (string.IsNullOrEmpty(culture))
        {
            return CustomError("Culture parameter is required");
        }

        // Validate culture
        try
        {
            var cultureInfo = new CultureInfo(culture);
        }
        catch (CultureNotFoundException)
        {
            return CustomError("Invalid culture");
        }

        // Set culture cookie
        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
        );

        return CustomResponse("Culture set successfully");
    }

    /// <summary>
    /// Gets the current culture
    /// </summary>
    /// <returns>Current culture information</returns>
    [HttpGet("getCulture")]
    public IActionResult GetCulture()
    {
        var culture = CultureInfo.CurrentCulture;
        var uiCulture = CultureInfo.CurrentUICulture;

        return CustomResponse(new
        {
            culture = culture.Name,
            cultureName = culture.DisplayName,
            uiCulture = uiCulture.Name,
            uiCultureName = uiCulture.DisplayName
        });
    }

    /// <summary>
    /// Tests the localization service
    /// </summary>
    /// <returns>Localized messages</returns>
    [HttpGet("test")]
    public IActionResult TestLocalization()
    {
        var messages = new
        {
            userNotFound = _localizationService.GetLocalizedString("UserNotFound"),
            requiredField = _localizationService.GetLocalizedString("RequiredField", "Email"),
            operationSuccessful = _localizationService.GetLocalizedString("OperationSuccessful")
        };

        return CustomResponse(messages);
    }
}
