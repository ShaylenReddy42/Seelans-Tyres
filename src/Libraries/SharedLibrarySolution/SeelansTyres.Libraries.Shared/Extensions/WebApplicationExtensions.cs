using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SeelansTyres.Libraries.Shared.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication AddCommonStartupDelay(this WebApplication app)
    {
        int startupDelayInSeconds = app.Configuration.GetValue<int>("StartupDelayInSeconds");

        if (startupDelayInSeconds > 0)
        {
            app.Logger.LogInformation(
                "Adding a {startupDelay}s startup delay for dependencies to startup",
                startupDelayInSeconds);

            Thread.Sleep(startupDelayInSeconds * 1_000);
        }
        
        return app;
    }
}
