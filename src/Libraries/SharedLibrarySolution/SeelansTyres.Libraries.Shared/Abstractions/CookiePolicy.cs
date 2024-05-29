using Microsoft.AspNetCore.Builder;                // WebApplication, CookiePolicyOptions
using Microsoft.AspNetCore.Http;                   // SameSiteMode
using Microsoft.Extensions.Configuration;          // GetValue()
using SeelansTyres.Libraries.Shared.Configuration; // ApplicationHostingOptions

namespace SeelansTyres.Libraries.Shared.Abstractions;

public static class CookiePolicy
{
    /// <summary>
    /// <para>Sets the cookie policy when the solution runs on localhost</para>
    /// 
    /// <para>
    ///     When the solution runs using docker compose or in Azure<br/>
    ///     it runs over https so the cookie policy is not needed
    /// </para>
    /// </summary>
    /// <remarks>Only the Mvc Frontend and the Identity Service needs the cookie policy</remarks>
    /// <param name="app">The web application used to configure the http pipeline</param>
    /// <returns>The web application used to configure the http pipeline with the configured cookie policy</returns>
    public static WebApplication UseCommonCookiePolicy(this WebApplication app)
    {
        var applicationHostingOptions =
            app.Configuration
                .Get<ApplicationHostingOptions>()
                    ?? throw new InvalidOperationException("InAzure and InContainer settings are missing in configuration");

        if (!applicationHostingOptions.InContainer &&
            !applicationHostingOptions.InAzure)
        {
            app.UseCookiePolicy(new CookiePolicyOptions
            {
                MinimumSameSitePolicy = SameSiteMode.Lax
            });
        }

        return app;
    }
}
