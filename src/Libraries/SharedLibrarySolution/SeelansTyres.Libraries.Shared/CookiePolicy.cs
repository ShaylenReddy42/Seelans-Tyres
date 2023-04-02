using Microsoft.AspNetCore.Builder;       // WebApplication, CookiePolicyOptions
using Microsoft.AspNetCore.Http;          // SameSiteMode
using Microsoft.Extensions.Configuration; // GetValue()

namespace SeelansTyres.Libraries.Shared;

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
        if (app.Configuration.GetValue<bool>("InContainer") is false &&
            app.Configuration.GetValue<bool>("InAzure") is false)
        {
            app.UseCookiePolicy(new CookiePolicyOptions
            {
                MinimumSameSitePolicy = SameSiteMode.Lax
            });
        }

        return app;
    }
}
