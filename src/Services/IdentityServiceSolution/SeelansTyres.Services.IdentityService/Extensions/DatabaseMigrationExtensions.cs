using IdentityServer4.EntityFramework.DbContexts;               // ConfigurationDbContext, PersistedGrantDbContext
using SeelansTyres.Libraries.Shared.Abstractions;               // MigrateDatabaseAsync()
using SeelansTyres.Services.IdentityService.Data;               // CustomerDbContext
using SeelansTyres.Services.IdentityService.Services;           // AdminAccountSeeder, ConfigurationDataSeeder
using ShaylenReddy42.UnpublishedUpdatesManagement.Abstractions; // MigrateUnpublishedUpdatesManagementDatabaseAsync()

namespace SeelansTyres.Services.IdentityService.Extensions;

public static class DatabaseMigrationExtensions
{
    /// <summary>
    /// <para>
    ///     Migrates all four databases and seeds the admin account to the customer database,<br/>
    ///     and the configuration data for IdentityServer4
    /// </para>
    /// </summary>
    /// <param name="app">The web application used to configure the HTTP pipeline</param>
    /// <returns></returns>
    public static async Task<WebApplication> RunSeedersAsync(this WebApplication app)
    {
        await app.MigrateDatabaseAsync<ConfigurationDbContext>();
        await app.MigrateDatabaseAsync<PersistedGrantDbContext>();
        await app.MigrateDatabaseAsync<CustomerDbContext>();

        await app.MigrateUnpublishedUpdatesManagementDatabaseAsync();

        using var scope = app.Services.CreateScope();

        var adminAccountSeeder = scope.ServiceProvider.GetService<AdminAccountSeeder>();
        await adminAccountSeeder!.CreateAdminAsync();

        var configurationDataSeeder = scope.ServiceProvider.GetService<ConfigurationDataSeeder>();
        await configurationDataSeeder!.SeedConfigurationDataAsync();

        return app;
    }
}
