using Microsoft.EntityFrameworkCore;                        // ExecuteDeleteAsync()
using SeelansTyres.Data.AddressData;                        // AddressDbContext
using ShaylenReddy42.UnpublishedUpdatesManagement.Messages; // BaseMessage
using System.Diagnostics;                                   // Stopwatch

namespace SeelansTyres.Workers.AddressWorker.Services;

public class AddressUpdateService(
    ILogger<AddressUpdateService> logger,
    AddressDbContext context) : IAddressUpdateService
{
    private readonly Stopwatch stopwatch = new();

    public async Task DeleteAsync(BaseMessage message)
    {
        logger.LogInformation(
            "Service => Attempting to remove all addresses for customer {CustomerId}",
            message.IdOfEntityToUpdate);

        stopwatch.Start();
        try
        {
            await context.Addresses
                .Where(address => address.CustomerId == Guid.Parse(message.IdOfEntityToUpdate))
                .ExecuteDeleteAsync();
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            logger.LogError(
                ex,
                "{Announcement} ({StopwatchElapsedTime}ms): Attempt to remove all addresses for customer {CustomerId} was unsuccessful",
                "FAILED", stopwatch.ElapsedMilliseconds, message.IdOfEntityToUpdate);

            throw ex.GetBaseException();
        }
        stopwatch.Stop();

        logger.LogInformation(
            "{Announcement} ({StopwatchElapsedTime}ms): Attempt to remove all addresses for customer {CustomerId} completed successfully",
            "SUCCEEDED", stopwatch.ElapsedMilliseconds, message.IdOfEntityToUpdate);
    }
}
