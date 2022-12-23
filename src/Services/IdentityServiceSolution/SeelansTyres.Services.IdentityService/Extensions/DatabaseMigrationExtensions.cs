using IdentityServer4.EntityFramework.DbContexts;
using SeelansTyres.Libraries.Shared;
using SeelansTyres.Libraries.Shared.DbContexts;
using SeelansTyres.Services.IdentityService.Data;
using SeelansTyres.Services.IdentityService.Services;

namespace SeelansTyres.Services.IdentityService.Extensions;

public static class DatabaseMigrationExtensions
{
    public static async Task<WebApplication> RunSeedersAsync(this WebApplication app)
    {
        await app.MigrateDatabaseAsync<ConfigurationDbContext>();
        await app.MigrateDatabaseAsync<PersistedGrantDbContext>();
        await app.MigrateDatabaseAsync<CustomerDbContext>();
        await app.MigrateDatabaseAsync<UnpublishedUpdateDbContext>();

        using var scope = app.Services.CreateScope();

        var adminAccountSeeder = scope.ServiceProvider.GetService<AdminAccountSeeder>();
        await adminAccountSeeder!.CreateAdminAsync();

        var configurationDataSeeder = scope.ServiceProvider.GetService<ConfigurationDataSeeder>();
        await configurationDataSeeder!.SeedConfigurationDataAsync();

        return app;
    }
}
