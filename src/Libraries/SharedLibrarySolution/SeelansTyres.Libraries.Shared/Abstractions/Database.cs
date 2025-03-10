﻿using Microsoft.AspNetCore.Builder;             // WebApplication
using Microsoft.EntityFrameworkCore;            // Database
using Microsoft.Extensions.DependencyInjection; // CreateScope()
using Microsoft.Extensions.Logging;             // LogInformation(), LogError()
using SeelansTyres.Libraries.Shared.Constants;  // LoggerConstants
using System.Diagnostics;                       // Stopwatch

namespace SeelansTyres.Libraries.Shared.Abstractions;

public static class Database
{
    /// <summary>
    /// Provides an abstraction to migrate a database
    /// </summary>
    /// <remarks>
    /// Any time a database needs to be migrated using code,<br/>
    /// a scope has to be created and the dbcontext has to be retrieved<br/>
    /// from the service collection
    /// </remarks>
    /// <typeparam name="T">The DbContext to migrate</typeparam>
    /// <param name="app">The web application used to configure the http pipeline</param>
    /// <returns>The web application used to configure the http pipeline</returns>
    public static async Task<WebApplication> MigrateDatabaseAsync<T>(this WebApplication app) where T : DbContext
    {
        app.Logger.LogInformation(
            "Attempting to migrate database {DbContext}",
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
                "{Announcement} ({StopwatchElapsedTime}ms): Attempt to migrate database {DbContext} was unsuccessful",
                LoggerConstants.FailedAnnouncement, stopwatch.ElapsedMilliseconds, typeof(T).Name);

            throw ex.GetBaseException();
        }
        stopwatch.Stop();

        app.Logger.LogInformation(
            "{Announcement} ({StopwatchElapsedTime}ms): Attempt to migrate database {DbContext} completed successfully",
            LoggerConstants.SucceededAnnouncement, stopwatch.ElapsedMilliseconds, typeof(T).Name);

        return app;
    }
}
