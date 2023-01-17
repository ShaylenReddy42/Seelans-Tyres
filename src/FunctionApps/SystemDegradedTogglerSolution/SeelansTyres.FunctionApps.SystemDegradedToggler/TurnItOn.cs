using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Data.AppConfiguration;
using Azure.Identity;
using static System.Environment;

namespace SeelansTyres.FunctionApps.SystemDegradedToggler;

public static class TurnItOn
{
    [FunctionName("TurnItOn")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest request,
        ILogger logger)
    {
        var appConfigUri = new Uri($"https://{GetEnvironmentVariable("AzureAppConfigName")}.azconfig.io");

        logger.LogInformation("Connecting to Azure App Configuration with a Managed Identity");

        var configurationClient = new ConfigurationClient(appConfigUri, new DefaultAzureCredential());

        logger.LogInformation("Turning on the System-Degraded state");

        await configurationClient.SetConfigurationSettingAsync("SystemDegraded", "true");

        return new OkResult();
    }
}
