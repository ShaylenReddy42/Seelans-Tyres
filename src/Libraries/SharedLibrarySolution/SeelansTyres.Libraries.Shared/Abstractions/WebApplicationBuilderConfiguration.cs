using ConfigurationSubstitution;                   // EnableSubstitutionsWithDelimitedFallbackDefaults()
using Microsoft.AspNetCore.Builder;                // WebApplicationBuilder
using Microsoft.Extensions.Configuration;          // GetValue(), AddAzureAppConfiguration()
using Microsoft.Extensions.DependencyInjection;    // AddApplicationInsightsTelemetry(), AddApplicationInsightsKubernetesEnricher(), AddHealthChecks()
using Microsoft.Extensions.Logging;                // ClearProviders()
using SeelansTyres.Libraries.Shared.Configuration; // ElasticsearchLoggingSinkOptions, ExternalServiceOptions
using SeelansTyres.Libraries.Shared.Models;        // CommonBuilderConfigurationModel, HealthChecksModel

namespace SeelansTyres.Libraries.Shared.Abstractions;

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
        var azureAppConfigurationOptions =
            builder.Configuration.GetSection("AzureAppConfig")
                .Get<ExternalServiceOptions>()
                    ?? throw new InvalidOperationException("AzureAppConfig configuration section is missing");

        if (azureAppConfigurationOptions.Enabled)
        {
            // Adds Azure App Configuration support using 'SystemDegraded' as the sentinel key to enable configuration refresh
            // It only has 'SystemDegraded' to have it toggled on by a function app and override the state to inform users
            builder.Configuration.AddAzureAppConfiguration(options =>
            {
                options
                    .Connect(azureAppConfigurationOptions.ConnectionString)
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

        var applicationInsightsOptions =
            builder.Configuration.GetSection("AppInsights")
                .Get<ExternalServiceOptions>()
                    ?? throw new InvalidOperationException("AppInsights configuration section is missing");

        // Instruments the solution with application insights
        if (applicationInsightsOptions.Enabled)
        {
            builder.Services.AddApplicationInsightsTelemetry(
                options => options.ConnectionString =
                    applicationInsightsOptions.ConnectionString);

            builder.Services.AddApplicationInsightsKubernetesEnricher();
        }

        var elasticsearchLoggingSinkOptions =
            builder.Configuration.GetSection("LoggingSinks:Elasticsearch")
                .Get<ElasticsearchLoggingSinkOptions>()
                    ?? throw new InvalidOperationException("Elasticsearch Logging Sink configuration section is missing");
        
        builder.Services.AddHealthChecks()
            .AddCommonChecks(
                elasticsearchLoggingSinkOptions, 
                applicationInsightsOptions.Enabled);

        if (azureAppConfigurationOptions.Enabled)
        {
            builder.Services.AddAzureAppConfiguration();
        }

        return builder;
    }
}
