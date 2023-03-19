using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace SeelansTyres.Libraries.Shared;

public static class CookiePolicy
{
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
