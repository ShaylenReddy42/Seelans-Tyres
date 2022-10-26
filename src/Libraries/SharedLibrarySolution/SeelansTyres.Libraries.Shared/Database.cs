using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace SeelansTyres.Libraries.Shared;

public static class Database
{
    public static async Task<WebApplication> MigrateDatabaseAsync<T>(this WebApplication app) where T : DbContext
    {
        app.Logger.LogInformation(
            "Attempting to migrate database {dbContext}",
            typeof(T).Name);

        var stopwatch = new Stopwatch();

        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetService<T>();

        stopwatch.Start();
        try
        {
            await dbContext!.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            app.Logger.LogError(
                ex,
                "{announcement} ({stopwatchElapsedTime}ms): Attempt to migrate database {dbContext} was unsuccessful",
                "FAILED", stopwatch.ElapsedMilliseconds, typeof(T).Name);

            throw ex.GetBaseException();
        }
        stopwatch.Stop();

        app.Logger.LogInformation(
            "{announcement} ({stopwatchElapsedTime}ms): Attempt to migrate database {dbContext} completed successfully",
            "SUCCEEDED", stopwatch.ElapsedMilliseconds, typeof(T).Name);

        return app;
    }
}
