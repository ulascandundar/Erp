using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

namespace Erp.Infrastructure.Services
{
    /// <summary>
    /// Culture provider that gets culture information from a request header
    /// </summary>
    public class HeaderRequestCultureProvider : RequestCultureProvider
    {
        /// <summary>
        /// The header key name
        /// </summary>
        public string HeaderName { get; set; } = "Accept-Language";

        /// <summary>
        /// Gets the culture from the request header
        /// </summary>
        /// <param name="httpContext">The HTTP context</param>
        /// <returns>The provider culture result</returns>
        public override Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            string cultureName = null;

            // Try to get the culture from the header
            if (httpContext.Request.Headers.TryGetValue(HeaderName, out var values))
            {
                cultureName = values.FirstOrDefault();

                // If the header contains multiple languages, take the first one
                if (!string.IsNullOrEmpty(cultureName) && cultureName.Contains(','))
                {
                    cultureName = cultureName.Split(',')[0].Trim();
                }

                // Remove quality value if present (e.g., "en-US;q=0.8" -> "en-US")
                if (!string.IsNullOrEmpty(cultureName) && cultureName.Contains(';'))
                {
                    cultureName = cultureName.Split(';')[0].Trim();
                }
            }

            if (string.IsNullOrEmpty(cultureName))
            {
                // No culture found in the header, return null to fall back to the next provider
                return NullProviderCultureResult;
            }

            // Validate that the culture exists
            try
            {
                var culture = new CultureInfo(cultureName);
                return Task.FromResult(new ProviderCultureResult(cultureName, cultureName));
            }
            catch (CultureNotFoundException)
            {
                // Invalid culture, return null to fall back to the next provider
                return NullProviderCultureResult;
            }
        }
    }
} 