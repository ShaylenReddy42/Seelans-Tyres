using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace SeelansTyres.Libraries.Shared;

public static class CookiePolicy
{
    public static WebApplication UseCommonCookiePolicy(this WebApplication app)
    {
        app.UseCookiePolicy(new CookiePolicyOptions
        {
            MinimumSameSitePolicy = SameSiteMode.Lax
        });

        return app;
    }
}
