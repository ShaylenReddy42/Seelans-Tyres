using Microsoft.AspNetCore.Builder;       // WebApplication, ForwardedHeadersOptions
using Microsoft.AspNetCore.HttpOverrides; // ForwardedHeaders

namespace SeelansTyres.Libraries.Shared.Abstractions;

public static class ReverseProxy
{
    /// <summary>
    /// Allows the protocol and host to be overriden by headers forwarded by a reverse proxy
    /// </summary>
    /// <remarks>
    ///     Originally added to fix IdentityServer4 in docker compose but is now on all applications<br/>
    ///     since they are hosted on Azure App Services
    /// </remarks>
    /// <param name="app">The web application used to configure the http pipeline</param>
    /// <returns></returns>
    public static WebApplication HonorForwardedHeaders(this WebApplication app)
    {
        var forwardedHeaderOptions = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        };
        forwardedHeaderOptions.KnownNetworks.Clear();
        forwardedHeaderOptions.KnownProxies.Clear();
        app.UseForwardedHeaders(forwardedHeaderOptions);

        return app;
    }
}
