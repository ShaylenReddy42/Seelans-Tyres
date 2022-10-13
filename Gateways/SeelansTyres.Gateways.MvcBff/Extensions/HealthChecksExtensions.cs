using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace SeelansTyres.Gateways.MvcBff.Extensions;

public static class HealthChecksExtensions
{
    public static IHealthChecksBuilder AddDownstreamChecks(this IHealthChecksBuilder healthChecks, IConfiguration configuration)
    {
        var serviceList = new List<string>()
        {
            "AddressService",
            "OrderService",
            "TyresService"
        };

        serviceList.ForEach(service =>
        {
            string scheme = configuration[$"Services:{service}:Scheme"],
                   host   = configuration[$"Services:{service}:Host"],
                   port   = configuration[$"Services:{service}:Port"];

            string serviceUrl = $"{scheme}://{host}:{port}";

            healthChecks
                .AddUrlGroup(
                    uri: new($"{serviceUrl}{configuration["HealthCheckEndpoint"]}"),
                    name: service,
                    failureStatus: HealthStatus.Degraded);
        });

        return healthChecks;
    }
}
