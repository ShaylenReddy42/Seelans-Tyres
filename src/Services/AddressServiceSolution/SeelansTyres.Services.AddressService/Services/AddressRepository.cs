using Microsoft.EntityFrameworkCore;           // ExecuteUpdateAsync(), ToListAsync(), SingleOrDefaultAsync(), ExecuteDeleteAsync()
using SeelansTyres.Data.AddressData.Entities;  // Address
using SeelansTyres.Data.AddressData;           // AddressDbContext
using System.Diagnostics;                      // Stopwatch
using SeelansTyres.Libraries.Shared.Constants; // LoggerConstants                   

namespace SeelansTyres.Services.AddressService.Services;

public class AddressRepository : IAddressRepository
{
    private readonly AddressDbContext context;
    private readonly ILogger<AddressRepository> logger;
    private readonly Stopwatch stopwatch = new();

    public AddressRepository(AddressDbContext context, ILogger<AddressRepository> logger) => 
        (this.context, this.logger) = (context, logger);

    public async Task CreateAsync(Guid customerId, Address newAddress)
    {
        logger.LogInformation(
            "Repository => Adding a new address for customer {CustomerId}",
            customerId);

        newAddress.CustomerId = customerId;

        stopwatch.Start();
        try
        {
            if (newAddress.PreferredAddress)
            {
                logger.LogInformation("Customer marked the new address as preferred. Setting the rest to false");

                await context.Addresses
                    .Where(address => address.CustomerId == customerId)
                    .ExecuteUpdateAsync(
                        updates => updates
                            .SetProperty(address => address.PreferredAddress, false));
            }

            await context.Addresses.AddAsync(newAddress);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            logger.LogError(
                ex,
                "{Announcement} ({StopwatchElapsedTime}ms): Attempt to add a new address for customer {CustomerId} was unsuccessful",
                LoggerConstants.FailedAnnouncement, stopwatch.ElapsedMilliseconds, customerId);

            throw ex.GetBaseException();
        }
        stopwatch.Stop();

        logger.LogInformation(
            "{Announcement} ({StopwatchElapsedTime}ms): Attempt to add a new address for customer {CustomerId} completed successfully",
            LoggerConstants.SucceededAnnouncement, stopwatch.ElapsedMilliseconds, customerId);
    }

    public async Task<IEnumerable<Address>> RetrieveAllAsync(Guid customerId)
    {
        logger.LogInformation(
            "Repository => Retrieving addresses for customer {CustomerId}", 
            customerId);
        
        IEnumerable<Address> addresses = [];
        
        stopwatch.Start();
        try
        {
            addresses = await context.Addresses.Where(address => address.CustomerId == customerId).ToListAsync();
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            logger.LogError(
                ex,
                "{Announcement} ({StopwatchElapsedTime}ms): Attempt to retrieve all addresses for customer {CustomerId} was unsuccessful",
                LoggerConstants.FailedAnnouncement, stopwatch.ElapsedMilliseconds, customerId);

            throw ex.GetBaseException();
        }
        stopwatch.Stop();

        logger.LogInformation(
            "{Announcement} ({StopwatchElapsedTime}ms): Attempt to retrieve all addresses for customer {CustomerId} completed successfully, returning {AddressesCount} address(es)",
            LoggerConstants.SucceededAnnouncement, stopwatch.ElapsedMilliseconds, customerId, addresses.Count());

        return addresses;
    }

    public async Task<Address?> RetrieveSingleAsync(Guid customerId, Guid addressId)
    {
        logger.LogInformation(
            "Repository => Retrieving address {AddressId} for customer {CustomerId}", 
            addressId, customerId);

        Address? address = null;

        stopwatch.Start();
        try
        {
            address = await context.Addresses.SingleOrDefaultAsync(address => address.Id == addressId && address.CustomerId == customerId);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            logger.LogError(
                ex,
                "{Announcement} ({StopwatchElapsedTime}ms): Attempt to retrieve address {AddressId} for customer {CustomerId} was unsuccessful",
                LoggerConstants.FailedAnnouncement, stopwatch.ElapsedMilliseconds, addressId, customerId);

            throw ex.GetBaseException();
        }
        stopwatch.Stop();

        logger.LogInformation(
            "{Announcement} ({StopwatchElapsedTime}ms): Attempt to retrieve address {AddressId} for customer {CustomerId} completed successfully",
            LoggerConstants.SucceededAnnouncement, stopwatch.ElapsedMilliseconds, addressId, customerId);

        return address;
    }

    public async Task MarkAsPreferredAsync(Guid customerId, Address addressToMarkAsPreferred)
    {
        logger.LogInformation(
            "Repository => Marking address {AddressId} as preferred for customer {CustomerId}", 
            addressToMarkAsPreferred.Id, customerId);

        stopwatch.Start();
        try
        {
            logger.LogInformation("Setting preference for other addresses to false for customer {CustomerId}", customerId);

            await context.Addresses
                .Where(address => address.CustomerId == customerId)
                .ExecuteUpdateAsync(
                    updates => updates
                        .SetProperty(address => address.PreferredAddress, false));

            addressToMarkAsPreferred.PreferredAddress = true;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            logger.LogError(
                ex,
                "{Announcement} ({StopwatchElapsedTime}ms): Attempt to mark address {AddressId} as preferred for customer {CustomerId} was unsuccessful",
                LoggerConstants.FailedAnnouncement, stopwatch.ElapsedMilliseconds, addressToMarkAsPreferred.Id, customerId);

            throw ex.GetBaseException();
        }
        stopwatch.Stop();

        logger.LogInformation(
            "{Announcement} ({StopwatchElapsedTime}ms): Attempt to mark address {AddressId} as preferred for customer {CustomerId} completed successfully",
            LoggerConstants.SucceededAnnouncement, stopwatch.ElapsedMilliseconds, addressToMarkAsPreferred.Id, customerId);
    }

    public async Task DeleteAsync(Guid customerId, Guid addressId)
    {
        logger.LogInformation(
            "Repository => Attempting to delete address {AddressId} for customer {CustomerId}",
            addressId, customerId);

        stopwatch.Start();
        try
        {
            await context.Addresses
                .Where(address => address.Id == addressId)
                .ExecuteDeleteAsync();
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            logger.LogError(
                ex,
                "{Announcement} ({StopwatchElapsedTime}ms): Attempt to delete address {AddressId} for customer {CustomerId} was unsuccessful",
                LoggerConstants.FailedAnnouncement, stopwatch.ElapsedMilliseconds, addressId, customerId);

            throw ex.GetBaseException();
        }
        stopwatch.Stop();

        logger.LogInformation(
            "{Announcement} ({StopwatchElapsedTime}ms): Attempt to delete address {AddressId} for customer {CustomerId} completed successfully",
            LoggerConstants.SucceededAnnouncement, stopwatch.ElapsedMilliseconds, addressId, customerId);
    }

    public async Task<bool> SaveChangesAsync() =>
        await context.SaveChangesAsync() >= 0;
}
