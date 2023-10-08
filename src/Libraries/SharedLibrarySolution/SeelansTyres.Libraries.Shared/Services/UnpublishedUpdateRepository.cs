using Microsoft.EntityFrameworkCore;                                                // ToListAsync()
using Microsoft.Extensions.Logging;                                                 // ILogger
using SeelansTyres.Libraries.Shared.DbContexts;                                     // UnpublishedUpdateDbContext
using SeelansTyres.Libraries.Shared.DbContexts.UnpublishedUpdateDbContext_Entities; // UnpublishedUpdate
using System.Diagnostics;                                                           // Stopwatch

namespace SeelansTyres.Libraries.Shared.Services;

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
                "{announcement} ({stopwatchElapsedTime}ms): Attempt to add a new unpublished update was unsuccessful",
                "FAILED", stopwatch.ElapsedMilliseconds);

            throw ex.GetBaseException();
        }
        stopwatch.Stop();

        logger.LogInformation(
            "{announcement} ({stopwatchElapsedTime}ms): Attempt to add a new unpublished update completed successfully",
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
                "{announcement} ({stopwatchElapsedTime}ms): Attempt to retrieve all unpublished updates was unsuccessful",
                "FAILED", stopwatch.ElapsedMilliseconds);

            throw ex.GetBaseException();
        }
        stopwatch.Stop();

        logger.LogInformation(
            "{announcement} ({stopwatchElapsedTime}ms): Attempt to retrieve all unpublished updates completed successfully with {unpublishedUpdatesCount} updates",
            "SUCCEEDED", stopwatch.ElapsedMilliseconds, unpublishedUpdates.Count);

        return unpublishedUpdates;
    }

    public Task DeleteAsync(UnpublishedUpdate unpublishedUpdate)
    {
        logger.LogInformation(
            "Repository => Attempting to remove unpublished update {updateId}",
            unpublishedUpdate.Id);

        context.UnpublishedUpdates.Remove(unpublishedUpdate);

        return Task.CompletedTask;
    }

    public async Task<bool> SaveChangesAsync() =>
        await context.SaveChangesAsync() >= 0;
}
