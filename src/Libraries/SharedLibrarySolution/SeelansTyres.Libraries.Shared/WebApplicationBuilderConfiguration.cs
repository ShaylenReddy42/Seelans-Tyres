using ConfigurationSubstitution;                // EnableSubstitutionsWithDelimitedFallbackDefaults()
using Microsoft.AspNetCore.Builder;             // WebApplicationBuilder
using Microsoft.Extensions.Configuration;       // GetValue(), AddAzureAppConfiguration()
using Microsoft.Extensions.DependencyInjection; // AddApplicationInsightsTelemetry(), AddApplicationInsightsKubernetesEnricher(), AddHealthChecks()
using Microsoft.Extensions.Logging;             // ClearProviders()
using SeelansTyres.Libraries.Shared.Models;     // CommonBuilderConfigurationModel, HealthChecksModel

namespace SeelansTyres.Libraries.Shared;

public static class WebApplicationBuilderConfiguration
{
    /// <summary>
    /// Adds common configuration to the web application builder for all applications in the architecture
    /// </summary>
    /// <param name="builder">The web application builder</param>
    /// <param name="commonBuilderConfigurationModel">A model containing properties to enrich the logs and configure the Elasticsearch sink</param>
    /// <returns>A preconfigured web application builder</returns>
    public static WebApplicationBuilder AddCommonBuilderConfiguration(
        this WebApplicationBuilder builder,
        CommonBuilderConfigurationModel commonBuilderConfigurationModel)
    {
        if (builder.Configuration.GetValue<bool>("AzureAppConfig:Enabled") is true)
        {
            // Adds Azure App Configuration support using 'SystemDegraded' as the sentinel key to enable configuration refresh
            // It only has 'SystemDegraded' to have it toggled on by a function app and override the state to inform users
            builder.Configuration.AddAzureAppConfiguration(options =>
            {
                options
                    .Connect(builder.Configuration["AzureAppConfig:ConnectionString"])
                    .Select("*")
                    .ConfigureRefresh(refreshOptions =>
                    {
                        refreshOptions.Register("SystemDegraded", true);
                        refreshOptions.SetCacheExpiration(TimeSpan.FromSeconds(30));
                    });
            });
        }

        // This has to be added last regardless if there are ten or a hundred configuration sources
        // It proves to be a vital component in configuring the solution
        builder.Configuration.EnableSubstitutionsWithDelimitedFallbackDefaults("$(", ")", " ?? ");

        builder.Logging.ClearProviders();

        builder.Host.UseCommonSerilog(commonBuilderConfigurationModel);

        // Instruments the solution with application insights
        if (builder.Configuration.GetValue<bool>("AppInsights:Enabled") is true)
        {
            builder.Services.AddApplicationInsightsTelemetry(
                options => options.ConnectionString = 
                    builder.Configuration["AppInsights:ConnectionString"]);

            builder.Services.AddApplicationInsightsKubernetesEnricher();
        }

        var healthChecksModel = new HealthChecksModel
        {
            EnableElasticsearchHealthCheck = builder.Configuration.GetValue<bool>("LoggingSinks:Elasticsearch:Enabled"),
            ElasticsearchUrl = builder.Configuration["LoggingSinks:Elasticsearch:Url"]!,

            PublishHealthStatusToAppInsights = builder.Configuration.GetValue<bool>("AppInsights:Enabled")
        };

        builder.Services.AddHealthChecks()
            .AddCommonChecks(healthChecksModel);

        if (builder.Configuration.GetValue<bool>("AzureAppConfig:Enabled") is true)
        {
            builder.Services.AddAzureAppConfiguration();
        }

        return builder;
    }
}
