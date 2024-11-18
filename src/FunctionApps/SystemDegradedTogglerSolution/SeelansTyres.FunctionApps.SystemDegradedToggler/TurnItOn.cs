using Microsoft.Extensions.Logging;     // ILogger, ILoggerFactory
using Azure.Data.AppConfiguration;      // ConfigurationClient, ConfigurationSetting
using Azure.Identity;                   // DefaultAzureCredential
using static System.Environment;        // GetEnvironmentVariable()
using Microsoft.Azure.Functions.Worker; // Function, HttpTrigger, AuthorizationLevel
using Microsoft.AspNetCore.Http;        // HttpRequest
using Microsoft.AspNetCore.Mvc;         // IActionResult, OkObjectResult

namespace SeelansTyres.FunctionApps.SystemDegradedToggler;

public class TurnItOn(ILoggerFactory loggerFactory)
{
    private readonly ILogger logger = loggerFactory.CreateLogger<TurnItOn>();

    [Function("TurnItOn")]
    public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Admin, "post")] HttpRequest request)
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

        return new OkObjectResult(null);
    }
}
