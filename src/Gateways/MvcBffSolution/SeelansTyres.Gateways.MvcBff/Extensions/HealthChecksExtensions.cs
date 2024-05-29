using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace SeelansTyres.Gateways.MvcBff.Extensions;

public static class HealthChecksExtensions
{
    /// <summary>
    /// Adds health checks for the Address, Order and Tyres microservices to the health checks builder
    /// </summary>
    /// <param name="healthChecks">A builder used to register health checks</param>
    /// <param name="configuration">An instance of IConfiguration from the WebApplicationBuilder</param>
    /// <returns>The original builder used to register health checks with the added downstream health checks</returns>
    public static IHealthChecksBuilder AddDownstreamChecks(this IHealthChecksBuilder healthChecks, IConfiguration configuration)
    {
        var serviceList = new List<string>()
        {
            "Address Service",
            "Order Service",
            "Tyres Service"
        };

        serviceList.ForEach(service =>
        {
            string scheme = configuration[$"Services:{service}:Scheme"]!,
                   host   = configuration[$"Services:{service}:Host"]!,
                   port   = configuration[$"Services:{service}:Port"]!;

            string serviceUrl = $"{scheme}://{host}:{port}";

            healthChecks
                .AddUrlGroup(
                    uri: new($"{serviceUrl}{configuration["LivenessCheckEndpoint"]}"),
                    name: service,
                    failureStatus: HealthStatus.Degraded);
        });

        return healthChecks;
    }
}
