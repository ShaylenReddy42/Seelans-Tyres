using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.EntityFrameworkCore;
using SeelansTyres.Libraries.Shared.DbContexts;
using SeelansTyres.Services.IdentityService.Data;
using SeelansTyres.Services.IdentityService.Services;
using System.Diagnostics;

namespace SeelansTyres.Services.IdentityService.Extensions;

public static class DatabaseMigrationExtensions
{
    public static async Task<WebApplication> RunSeedersAsync(this WebApplication app)
    {
        app.Logger.LogInformation("Attempting to migrate databases");

        var stopwatch = new Stopwatch();

        using var scope = app.Services.CreateScope();

        var configurationDbContext = scope.ServiceProvider.GetService<ConfigurationDbContext>();
        var persistedGrantDbContext = scope.ServiceProvider.GetService<PersistedGrantDbContext>();
        var customerDbContext = scope.ServiceProvider.GetService<CustomerDbContext>();
        var unpublishedUpdateDbContext = scope.ServiceProvider.GetService<UnpublishedUpdateDbContext>();

        stopwatch.Start();
        try
        {
            await Task.WhenAll(
                Task.Run(() => configurationDbContext!.Database.MigrateAsync()),
                Task.Run(() => persistedGrantDbContext!.Database.MigrateAsync()),
                Task.Run(() => customerDbContext!.Database.MigrateAsync()),
                Task.Run(() => unpublishedUpdateDbContext!.Database.MigrateAsync()));
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            app.Logger.LogError(
                ex,
                "{announcement} ({stopwatchElapsedTime}ms): Attempt to migrate databases was unsuccessful",
                "FAILED", stopwatch.ElapsedMilliseconds);

            throw ex.GetBaseException();
        }
        stopwatch.Stop();

        app.Logger.LogInformation(
            "{announcement} ({stopwatchElapsedTime}ms): Attempt to migrate databases completed successfully",
            "SUCCEEDED", stopwatch.ElapsedMilliseconds);

        var adminAccountSeeder = scope.ServiceProvider.GetService<AdminAccountSeeder>();
        await adminAccountSeeder!.CreateAdminAsync();

        var configurationDataSeeder = scope.ServiceProvider.GetService<ConfigurationDataSeeder>();
        await configurationDataSeeder!.SeedConfigurationDataAsync();

        return app;
    }
}
