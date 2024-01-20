using Microsoft.Extensions.Logging;          // ILogger, ILoggerFactory
using Azure.Data.AppConfiguration;           // ConfigurationClient, ConfigurationSetting
using Azure.Identity;                        // DefaultAzureCredential
using static System.Environment;             // GetEnvironmentVariable()
using Microsoft.Azure.Functions.Worker;      // Function, HttpTrigger, AuthorizationLevel
using Microsoft.Azure.Functions.Worker.Http; // HttpResponseData, HttpRequestData
using System.Net;                            // HttpStatusCode

namespace SeelansTyres.FunctionApps.SystemDegradedToggler;

public class TurnItOn(ILoggerFactory loggerFactory)
{
    private readonly ILogger logger = loggerFactory.CreateLogger<TurnItOn>();

    [Function("TurnItOn")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Admin, "post")] HttpRequestData request)
    {
        var appConfigurationUri = new Uri($"https://{GetEnvironmentVariable("AzureAppConfigName")}.azconfig.io");

        logger.LogInformation("Connecting to Azure App Configuration with a Managed Identity");

        var configurationClient = new ConfigurationClient(appConfigurationUri, new DefaultAzureCredential());

        var systemDegradedConfigurationSetting =
            new ConfigurationSetting("SystemDegraded", "true")
            {
                ContentType = "application/json"
            };

        logger.LogInformation("Turning on the System-Degraded state");

        await configurationClient.SetConfigurationSettingAsync(systemDegradedConfigurationSetting);

        var response = request.CreateResponse(HttpStatusCode.OK);

        return response;
    }
}
