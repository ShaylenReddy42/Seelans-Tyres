using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SeelansTyres.Libraries.Shared.Models;

namespace SeelansTyres.Libraries.Shared;

public static class HealthChecks
{
    public static IHealthChecksBuilder AddCommonChecks(this IHealthChecksBuilder healthChecks, HealthChecksModel healthChecksModel)
    {
        healthChecks
            .AddCheck(
                name: "self", 
                check: () => HealthCheckResult.Healthy(),
                tags: new[] { "self" });

        if (healthChecksModel.EnableElasticsearchHealthCheck is true)
        {
            healthChecks
                .AddElasticsearch(
                    elasticsearchUri: healthChecksModel.ElasticsearchUrl, 
                    failureStatus: HealthStatus.Degraded);
        }

        if (healthChecksModel.PublishHealthStatusToAppInsights is true)
        {
            healthChecks.AddApplicationInsightsPublisher();
        }

        return healthChecks;
    }

    public static WebApplication MapCommonHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks(app.Configuration["HealthCheckEndpoint"]!, new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.MapHealthChecks(app.Configuration["LivenessCheckEndpoint"]!, new HealthCheckOptions
        {
            Predicate = healthCheckRegistration => healthCheckRegistration.Tags.Contains("self"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        return app;
    }
}
