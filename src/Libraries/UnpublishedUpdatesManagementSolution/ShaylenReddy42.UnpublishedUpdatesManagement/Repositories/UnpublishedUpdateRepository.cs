using Microsoft.EntityFrameworkCore;                             // ToListAsync()
using Microsoft.Extensions.Logging;                              // ILogger
using ShaylenReddy42.UnpublishedUpdatesManagement.Data;          // UnpublishedUpdateDbContext
using ShaylenReddy42.UnpublishedUpdatesManagement.Data.Entities; // UnpublishedUpdate
using System.Diagnostics;                                        // Stopwatch

namespace ShaylenReddy42.UnpublishedUpdatesManagement.Repositories;

public class UnpublishedUpdateRepository : IUnpublishedUpdateRepository
{
    private readonly UnpublishedUpdateDbContext context;
    private readonly ILogger<UnpublishedUpdateRepository> logger;
    private readonly Stopwatch stopwatch = new();

    public UnpublishedUpdateRepository(
        UnpublishedUpdateDbContext context,
        ILogger<UnpublishedUpdateRepository> logger)
    {
        this.context = context;
        this.logger = logger;
    }

    public async Task CreateAsync(UnpublishedUpdate unpublishedUpdate)
    {
        logger.LogInformation("Repository => Attempting to add an unpublished update");

        stopwatch.Start();
        try
        {
            await context.UnpublishedUpdates.AddAsync(unpublishedUpdate);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            logger.LogError(
                ex,
                "{Announcement} ({StopwatchElapsedTime}ms): Attempt to add a new unpublished update was unsuccessful",
                "FAILED", stopwatch.ElapsedMilliseconds);

            throw ex.GetBaseException();
        }
        stopwatch.Stop();

        logger.LogInformation(
            "{Announcement} ({StopwatchElapsedTime}ms): Attempt to add a new unpublished update completed successfully",
            "SUCCEEDED", stopwatch.ElapsedMilliseconds);
    }

    public async Task<List<UnpublishedUpdate>> RetrieveAllAsync()
    {
        logger.LogInformation("Repository => Attempting to retrieve all unpublished updates");

        List<UnpublishedUpdate> unpublishedUpdates;

        stopwatch.Start();
        try
        {
            unpublishedUpdates = await context.UnpublishedUpdates.ToListAsync();
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            logger.LogError(
                ex,
                "{Announcement} ({StopwatchElapsedTime}ms): Attempt to retrieve all unpublished updates was unsuccessful",
                "FAILED", stopwatch.ElapsedMilliseconds);

            throw ex.GetBaseException();
        }
        stopwatch.Stop();

        logger.LogInformation(
            "{Announcement} ({StopwatchElapsedTime}ms): Attempt to retrieve all unpublished updates completed successfully with {UnpublishedUpdatesCount} updates",
            "SUCCEEDED", stopwatch.ElapsedMilliseconds, unpublishedUpdates.Count);

        return unpublishedUpdates;
    }

    public Task DeleteAsync(UnpublishedUpdate unpublishedUpdate)
    {
        logger.LogInformation(
            "Repository => Attempting to remove unpublished update {UpdateId}",
            unpublishedUpdate.Id);

        context.UnpublishedUpdates.Remove(unpublishedUpdate);

        return Task.CompletedTask;
    }

    public async Task<bool> SaveChangesAsync() =>
        await context.SaveChangesAsync() >= 0;
}
