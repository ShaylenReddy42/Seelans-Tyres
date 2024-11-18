using Microsoft.Azure.Functions.Worker;         // ConfigureFunctionsApplicationInsights
using Microsoft.Extensions.DependencyInjection; // AddApplicationInsightsTelemetryWorkerService
using Microsoft.Extensions.Hosting;             // HostBuilder

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .Build();

await host.RunAsync();
