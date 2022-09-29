using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;

namespace SeelansTyres.Services.IdentityService.Services;

public class ConfigurationDataSeeder
{
	private readonly ConfigurationDbContext context;
    private readonly IConfiguration configuration;
    private readonly ILogger<ConfigurationDataSeeder> logger;

    public ConfigurationDataSeeder(
        ConfigurationDbContext context,
        IConfiguration configuration,
        ILogger<ConfigurationDataSeeder> logger) =>
		    (this.context, this.configuration, this.logger) = (context, configuration, logger);

    public async Task SeedConfigurationDataAsync()
    {
        logger.LogInformation("Service => Seeding configuration data");
        
        Config.Configuration = configuration;
        
        logger.LogDebug("Clients being populated");
        Config.Clients.ToList().ForEach(client =>
        {
            var clientEntity =
                context.Clients.SingleOrDefault(
                    entity => entity.ClientId == client.ClientId);

            if (clientEntity is null)
            {
                context.Clients.Add(client.ToEntity());
            }
        });
        await context.SaveChangesAsync();

        logger.LogDebug("IdentityResources being populated");
        Config.IdentityResources.ToList().ForEach(identityResource =>
        {
            var identityResourceEntity =
                context.IdentityResources.SingleOrDefault(
                    resource => resource.Name == identityResource.Name);

            if (identityResourceEntity is null)
            {
                context.IdentityResources.Add(identityResource.ToEntity());
            }
        });
        await context.SaveChangesAsync();

        logger.LogDebug("ApiScopes being populated");
        Config.ApiScopes.ToList().ForEach(apiScope =>
        {
            var apiScopeEntity =
                context.ApiScopes.SingleOrDefault(
                    scope => scope.Name == apiScope.Name);

            if (apiScopeEntity is null)
            {
                context.ApiScopes.Add(apiScope.ToEntity());
            }
        });
        await context.SaveChangesAsync();

        logger.LogDebug("ApiResources being populated");
        Config.ApiResources.ToList().ForEach(apiResource =>
        {
            var apiResourceEntity =
                context.ApiResources.SingleOrDefault(
                    resource => resource.Name == apiResource.Name);

            if (apiResourceEntity is null)
            {
                context.ApiResources.Add(apiResource.ToEntity());
            }
        });
        await context.SaveChangesAsync();

        logger.LogInformation("Configuration data seeded successfully");
    }
}
