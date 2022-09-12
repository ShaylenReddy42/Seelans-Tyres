using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Serilog;

namespace SeelansTyres.Services.IdentityService.Services;

public class ConfigurationDataSeeder
{
	private readonly ConfigurationDbContext context;
    private readonly IConfiguration configuration;

    public ConfigurationDataSeeder(
        ConfigurationDbContext context,
        IConfiguration configuration) =>
		    (this.context, this.configuration) = (context, configuration);

    public async Task SeedConfigurationDataAsync()
    {
        Config.Configuration = configuration;
        
        Log.Debug("Clients being populated");
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

        Log.Debug("IdentityResources being populated");
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

        Log.Debug("ApiScopes being populated");
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

        Log.Debug("ApiResources being populated");
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
    }
}
