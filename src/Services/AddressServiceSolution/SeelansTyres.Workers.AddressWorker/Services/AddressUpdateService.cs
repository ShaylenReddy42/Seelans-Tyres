using Microsoft.EntityFrameworkCore;                        // ExecuteDeleteAsync()
using SeelansTyres.Data.AddressData;                        // AddressDbContext
using ShaylenReddy42.UnpublishedUpdatesManagement.Messages; // BaseMessage
using System.Diagnostics;                                   // Stopwatch

namespace SeelansTyres.Workers.AddressWorker.Services;

public class AddressUpdateService : IAddressUpdateService
{
    private readonly ILogger<AddressUpdateService> logger;
    private readonly AddressDbContext context;
    private readonly Stopwatch stopwatch = new();

    public AddressUpdateService(
        ILogger<AddressUpdateService> logger,
        AddressDbContext context)
    {
        this.logger = logger;
        this.context = context;
    }
    
    public async Task DeleteAsync(BaseMessage message)
    {
        logger.LogInformation(
            "Service => Attempting to remove all addresses for customer {customerId}",
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
                "{announcement} ({stopwatchElapsedTime}ms): Attempt to remove all addresses for customer {customerId} was unsuccessful",
                "FAILED", stopwatch.ElapsedMilliseconds, message.IdOfEntityToUpdate);

            throw ex.GetBaseException();
        }
        stopwatch.Stop();

        logger.LogInformation(
            "{announcement} ({stopwatchElapsedTime}ms): Attempt to remove all addresses for customer {customerId} completed successfully",
            "SUCCEEDED", stopwatch.ElapsedMilliseconds, message.IdOfEntityToUpdate);
    }
}
