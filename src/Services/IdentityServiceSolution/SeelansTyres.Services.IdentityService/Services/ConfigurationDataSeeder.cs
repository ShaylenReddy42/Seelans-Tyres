using IdentityServer4.EntityFramework.DbContexts; // ConfigurationDbContext
using IdentityServer4.EntityFramework.Mappers;    // ToEntity()
using Microsoft.EntityFrameworkCore;              // ExecuteDelete
using SeelansTyres.Libraries.Shared.Constants;    // LoggerConstants
using System.Diagnostics;                         // Stopwatch

namespace SeelansTyres.Services.IdentityService.Services;

public class ConfigurationDataSeeder
{
	private readonly ConfigurationDbContext context;
    private readonly IConfiguration configuration;
    private readonly ILogger<ConfigurationDataSeeder> logger;
    private readonly Stopwatch stopwatch = new();

    public ConfigurationDataSeeder(
        ConfigurationDbContext context,
        IConfiguration configuration,
        ILogger<ConfigurationDataSeeder> logger) =>
		    (this.context, this.configuration, this.logger) = (context, configuration, logger);

    public async Task SeedConfigurationDataAsync()
    {
        logger.LogInformation("Service => Seeding configuration data");

        stopwatch.Start();
        
        Config.Configuration = configuration;
        
        logger.LogDebug("Clients being populated");
        Config.Clients.ForEach(client =>
        {
            context.Clients
                .Where(entity => entity.ClientId == client.ClientId)
                .ExecuteDelete();

            context.Clients
                .Add(client.ToEntity());
        });
        await context.SaveChangesAsync();

        logger.LogDebug("IdentityResources being populated");
        Config.IdentityResources.ForEach(identityResource =>
        {
            var identityResourceEntity =
                context.IdentityResources.SingleOrDefault(
                    resource => resource.Name == identityResource.Name);

            if (identityResourceEntity is null)
            {
                context.IdentityResources
                    .Add(identityResource.ToEntity());
            }
        });
        await context.SaveChangesAsync();

        logger.LogDebug("ApiScopes being populated");
        Config.ApiScopes.ForEach(apiScope =>
        {
            var apiScopeEntity =
                context.ApiScopes.SingleOrDefault(
                    scope => scope.Name == apiScope.Name);

            if (apiScopeEntity is null)
            {
                context.ApiScopes
                    .Add(apiScope.ToEntity());
            }
        });
        await context.SaveChangesAsync();

        logger.LogDebug("ApiResources being populated");
        Config.ApiResources.ForEach(apiResource =>
        {
            var apiResourceEntity =
                context.ApiResources.SingleOrDefault(
                    resource => resource.Name == apiResource.Name);

            if (apiResourceEntity is null)
            {
                context.ApiResources
                    .Add(apiResource.ToEntity());
            }
        });
        await context.SaveChangesAsync();

        stopwatch.Stop();

        logger.LogInformation(
            "{Announcement} ({StopwatchElapsedTime}ms): Configuration data seeded successfully",
            LoggerConstants.SucceededAnnouncement, stopwatch.ElapsedMilliseconds);
    }
}
