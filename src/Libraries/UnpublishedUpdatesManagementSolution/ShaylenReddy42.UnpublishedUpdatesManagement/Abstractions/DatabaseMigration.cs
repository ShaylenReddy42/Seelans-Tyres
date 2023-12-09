using Microsoft.AspNetCore.Builder;                     // WebApplication
using Microsoft.EntityFrameworkCore;                    // MigrateAsync()
using Microsoft.Extensions.DependencyInjection;         // CreateScope()
using Microsoft.Extensions.Logging;                     // LogError
using ShaylenReddy42.UnpublishedUpdatesManagement.Data; // UnpublishedUpdateDbContext
using System.Diagnostics;                               // Stopwatch

namespace ShaylenReddy42.UnpublishedUpdatesManagement.Abstractions;

public static class DatabaseMigration
{
    /// <summary>
    /// Provides an abstraction over migrating the <see cref="UnpublishedUpdateDbContext"/> database
    /// </summary>
    /// <remarks>
    /// Any time a database needs to be migrated using code,<br/>
    /// a scope has to be created and the dbcontext has to be retrieved<br/>
    /// from the service collection
    /// </remarks>
    /// <param name="app">The built web application</param>
    /// <returns>A task of type WebApplication, returning the originally built web application</returns>
    public static async Task<WebApplication> MigrateUnpublishedUpdatesManagementDatabaseAsync(this WebApplication app)
    {
        app.Logger.LogInformation("Attempting to migrate the Unpublished Updates Management database");
        
        using var scope = app.Services.CreateScope();

        var stopwatch = new Stopwatch();

        stopwatch.Start();
        try
        {
            var unpublishedUpdatesManagementDatabase = scope.ServiceProvider.GetRequiredService<UnpublishedUpdateDbContext>();

            await unpublishedUpdatesManagementDatabase.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            app.Logger.LogError(
                ex,
                "{announcement} ({stopwatchElapsedTime}ms): Attempt to migrate the Unpublished Updates Management database was unsuccessful",
                "FAILED", stopwatch.ElapsedMilliseconds);

            return app;
        }
        stopwatch.Stop();

        app.Logger.LogInformation(
            "{announcement} ({stopwatchElapsedTime}ms): Attempt to migrate the Unpublished Updates Management database completed successfully",
            "SUCCEEDED", stopwatch.ElapsedMilliseconds);

        return app;
    }
}
