using ConfigurationSubstitution;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SeelansTyres.Libraries.Shared.Models;

namespace SeelansTyres.Libraries.Shared;

public static class WebApplicationBuilderConfiguration
{
    public static WebApplicationBuilder AddCommonBuilderConfiguration(
        this WebApplicationBuilder builder,
        CommonBuilderConfigurationModel commonBuilderConfigurationModel)
    {
        if (builder.Configuration.GetValue<bool>("AzureAppConfig:Enabled") is true)
        {
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

        builder.Configuration.EnableSubstitutionsWithDelimitedFallbackDefaults("$(", ")", " ?? ");

        builder.Logging.ClearProviders();

        builder.Host.UseCommonSerilog(commonBuilderConfigurationModel);

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
