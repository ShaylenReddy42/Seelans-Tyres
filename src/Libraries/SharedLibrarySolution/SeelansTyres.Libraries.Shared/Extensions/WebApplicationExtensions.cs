using Microsoft.AspNetCore.Builder;                // WebApplication
using Microsoft.Extensions.Configuration;          // GetValue()
using Microsoft.Extensions.Logging;                // LogInformation()
using SeelansTyres.Libraries.Shared.Configuration; // ExternalServiceOptions

namespace SeelansTyres.Libraries.Shared.Extensions;

public static class WebApplicationExtensions
{
    /// <summary>
    /// Enables Azure App Configuration based on whether it's enabled in the environment
    /// </summary>
    /// <param name="app">The web application used to configure the http pipeline</param>
    /// <returns>The original web application</returns>
    public static WebApplication ConditionallyUseAzureAppConfiguration(this WebApplication app)
    {
        var azureAppConfigurationOptions =
            app.Configuration.GetSection("AzureAppConfig")
                .Get<ExternalServiceOptions>()
                    ?? throw new InvalidOperationException("AzureAppConfig configuration section is missing");

        if (azureAppConfigurationOptions.Enabled)
        {
            app.UseAzureAppConfiguration();
        }

        return app;
    }

    /// <summary>
    /// Configures a startup delay by pulling the 'StartupDelayInSeconds' setting from configuration<br/>
    /// to give dependencies time to warm up
    /// </summary>
    /// <remarks>
    ///     Exists to reduce the errors seen when running the solution in docker compose
    /// </remarks>
    /// <param name="app">The web application used to configure the http pipeline</param>
    /// <returns>The original web application</returns>
    public static WebApplication AddCommonStartupDelay(this WebApplication app)
    {
        int startupDelayInSeconds = app.Configuration.GetValue<int>("StartupDelayInSeconds");

        if (startupDelayInSeconds > 0)
        {
            app.Logger.LogInformation(
                "Adding a {StartupDelay}s startup delay for dependencies to startup",
                startupDelayInSeconds);

            Thread.Sleep(startupDelayInSeconds * 1_000);
        }
        
        return app;
    }
}
