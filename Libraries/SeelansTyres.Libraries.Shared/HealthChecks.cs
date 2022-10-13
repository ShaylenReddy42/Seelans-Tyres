using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SeelansTyres.Libraries.Shared.Models;

namespace SeelansTyres.Libraries.Shared;

public static class HealthChecks
{
    public static IHealthChecksBuilder AddCommonChecks(this IHealthChecksBuilder healthChecks, HealthChecksModel healthChecksModel)
    {
        healthChecks
            .AddCheck("self", () => HealthCheckResult.Healthy());

        if (healthChecksModel.EnableElasticsearchHealthCheck is true)
        {
            healthChecks
                .AddElasticsearch(
                    elasticsearchUri: healthChecksModel.ElasticsearchUrl, 
                    failureStatus: HealthStatus.Degraded);
        }

        return healthChecks;
    }
}
