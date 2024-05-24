using Microsoft.Extensions.Hosting; // HostBuilder

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .Build();

await host.RunAsync();
