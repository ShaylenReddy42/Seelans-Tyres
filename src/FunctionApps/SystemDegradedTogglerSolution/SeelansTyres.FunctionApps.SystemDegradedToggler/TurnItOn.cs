using Microsoft.AspNetCore.Mvc;                // IActionResult, OkResult()
using Microsoft.Azure.WebJobs;                 // FunctionName, HttpTrigger
using Microsoft.Azure.WebJobs.Extensions.Http; // AuthorizationLevel
using Microsoft.AspNetCore.Http;               // HttpRequest
using Microsoft.Extensions.Logging;            // ILogger
using Azure.Data.AppConfiguration;             // ConfigurationClient, ConfigurationSetting
using Azure.Identity;                          // DefaultAzureCredential
using static System.Environment;               // GetEnvironmentVariable()

namespace SeelansTyres.FunctionApps.SystemDegradedToggler;

public static class TurnItOn
{
    [FunctionName("TurnItOn")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Admin, "post", Route = null)] HttpRequest request,
        ILogger logger)
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

        return new OkResult();
    }
}
