﻿using Microsoft.EntityFrameworkCore;
using SeelansTyres.Services.AddressService.Data.Entities;
using SeelansTyres.Services.AddressService.Data;
using System.Diagnostics;

namespace SeelansTyres.Services.AddressService.Services;

public class AddressRepository : IAddressRepository
{
    private readonly AddressContext context;
    private readonly ILogger<AddressRepository> logger;
    private readonly Stopwatch stopwatch = new();

    public AddressRepository(AddressContext context, ILogger<AddressRepository> logger) => 
        (this.context, this.logger) = (context, logger);

    public async Task<IEnumerable<Address>> RetrieveAllAsync(Guid customerId)
    {
        logger.LogInformation(
            "Repository => Retrieving addresses for customer {customerId}", 
            customerId);
        
        IEnumerable<Address> addresses = Enumerable.Empty<Address>();
        
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
                "{announcement} ({stopwatchElapsedTime}ms): Attempt to retrieve all addresses for customer {customerId} was unsuccessful",
                "FAILED", stopwatch.ElapsedMilliseconds, customerId);

            throw ex.GetBaseException();
        }
        stopwatch.Stop();

        logger.LogInformation(
            "{announcement} ({stopwatchElapsedTime}ms): Attempt to retrieve all addresses for customer {customerId} completed successfully, returning {addressCount} address(es)",
            "SUCCEEDED", stopwatch.ElapsedMilliseconds, customerId, addresses.Count());

        return addresses;
    }

    public async Task<Address?> RetrieveSingleAsync(Guid customerId, Guid addressId)
    {
        logger.LogInformation(
            "Repository => Retrieving address {addressId} for customer {customerId}", 
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
                "{announcement} ({stopwatchElapsedTime}ms): Attempt to retrieve address {addressId} for customer {customerId} was unsuccessful",
                "FAILED", stopwatch.ElapsedMilliseconds, addressId, customerId);

            throw ex.GetBaseException();
        }
        stopwatch.Stop();

        logger.LogInformation(
            "{announcement} ({stopwatchElapsedTime}ms): Attempt to retrieve address {addressId} for customer {customerId} completed successfully",
            "SUCCEEDED", stopwatch.ElapsedMilliseconds, addressId, customerId);

        return address;
    }

    public async Task CreateAsync(Guid customerId, Address newAddress)
    {
        logger.LogInformation(
            "Repository => Adding a new address for customer {customerId}", 
            customerId);
        
        newAddress.CustomerId = customerId;

        stopwatch.Start();
        try
        {
            if (newAddress.PreferredAddress is true)
            {
                logger.LogInformation("Customer marked the new address as preferred. Setting the rest to false");
                
                await context.Addresses
                    .Where(address => address.CustomerId == customerId)
                    .ForEachAsync(address => address.PreferredAddress = false);
            }

            await context.Addresses.AddAsync(newAddress);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            logger.LogError(
                ex,
                "{announcement} ({stopwatchElapsedTime}ms): Attempt to add a new address for customer {customerId} was unsuccessful",
                "FAILED", stopwatch.ElapsedMilliseconds, customerId);

            throw ex.GetBaseException();
        }
        stopwatch.Stop();

        logger.LogInformation(
            "{announcement} ({stopwatchElapsedTime}ms): Attempt to add a new address for customer {customerId} completed successfully",
            "SUCCEEDED", stopwatch.ElapsedMilliseconds, customerId);
    }

    public async Task MarkAsPrefferedAsync(Guid customerId, Address addressToMarkAsPreferred)
    {
        logger.LogInformation(
            "Repository => Marking address {addressId} as preferred for customer {customerId}", 
            addressToMarkAsPreferred.Id, customerId);
        
        try
        {
            logger.LogInformation("Setting preference for other addresses to false for customer {customerId}", customerId);
            
            await context.Addresses
                .Where(address => address.CustomerId == customerId)
                .ForEachAsync(address => address.PreferredAddress = false);

            addressToMarkAsPreferred.PreferredAddress = true;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            logger.LogError(
                ex,
                "{announcement} ({stopwatchElapsedTime}ms): Attempt to mark address {addressId} as preferred for customer {customerId} was unsuccessful",
                "FAILED", stopwatch.ElapsedMilliseconds, addressToMarkAsPreferred.Id, customerId);

            throw ex.GetBaseException();
        }
        stopwatch.Stop();

        logger.LogInformation(
            "{announcement} ({stopwatchElapsedTime}ms): Attempt to mark address {addressId} as preferred for customer {customerId} completed successfully",
            "SUCCEEDED", stopwatch.ElapsedMilliseconds, addressToMarkAsPreferred.Id, customerId);
    }

    public async Task<bool> SaveChangesAsync() =>
        await context.SaveChangesAsync() >= 0;
}
