﻿using Microsoft.EntityFrameworkCore;                        // ExecuteDeleteAsync(), ExecuteUpdateAsync()
using SeelansTyres.Data.OrderData;                          // OrderDbContext
using SeelansTyres.Libraries.Shared.Constants;              // LoggerConstants
using ShaylenReddy42.UnpublishedUpdatesManagement.Messages; // BaseMessage
using System.Diagnostics;                                   // Stopwatch
using System.Text.Json;                                     // JsonSerializer

namespace SeelansTyres.Workers.OrderWorker.Services;

public class OrderUpdateService(
    ILogger<OrderUpdateService> logger,
    OrderDbContext context) : IOrderUpdateService
{
    private readonly Stopwatch stopwatch = new();

    public async Task DeleteAccountAsync(BaseMessage message)
    {
        logger.LogInformation(
            "Service => Attempting to remove all orders for customer {CustomerId}",
            message.IdOfEntityToUpdate);

        stopwatch.Start();
        try
        {
            await context.Orders
                .Where(order => order.CustomerId == Guid.Parse(message.IdOfEntityToUpdate))
                .ExecuteDeleteAsync();
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            logger.LogError(
                ex,
                "{Announcement} ({StopwatchElapsedTime}ms): Attempt to remove all orders for customer {CustomerId} was unsuccessful",
                LoggerConstants.FailedAnnouncement, stopwatch.ElapsedMilliseconds, message.IdOfEntityToUpdate);

            throw ex.GetBaseException();
        }
        stopwatch.Stop();

        logger.LogInformation(
            "{Announcement} ({StopwatchElapsedTime}ms): Attempt to remove all orders for customer {CustomerId} completed successfully",
            LoggerConstants.SucceededAnnouncement, stopwatch.ElapsedMilliseconds, message.IdOfEntityToUpdate);
    }

    public async Task UpdateAccountAsync(BaseMessage message)
    {
        logger.LogInformation(
            "Service => Attempting to update all orders for customer {CustomerId}",
            message.IdOfEntityToUpdate);

        var updateAccountModel = JsonSerializer.Deserialize<UpdateAccountModel>(message.SerializedModel);

        stopwatch.Start();
        try
        {
            await context.Orders
                .Where(order => order.CustomerId == Guid.Parse(message.IdOfEntityToUpdate))
                .ExecuteUpdateAsync(
                    updates => updates
                        .SetProperty(order => order.FirstName, updateAccountModel!.FirstName)
                        .SetProperty(order => order.LastName, updateAccountModel!.LastName)
                        .SetProperty(order => order.PhoneNumber, updateAccountModel!.PhoneNumber));
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            logger.LogError(
                ex,
                "{Announcement} ({StopwatchElapsedTime}ms): Attempt to update all orders for customer {CustomerId} was unsuccessful",
                LoggerConstants.FailedAnnouncement, stopwatch.ElapsedMilliseconds, message.IdOfEntityToUpdate);

            throw ex.GetBaseException();
        }
        stopwatch.Stop();

        logger.LogInformation(
            "{Announcement} ({StopwatchElapsedTime}ms): Attempt to update all orders for customer {CustomerId} completed successfully",
            LoggerConstants.SucceededAnnouncement, stopwatch.ElapsedMilliseconds, message.IdOfEntityToUpdate);
    }

    public async Task UpdateTyreAsync(BaseMessage message)
    {
        logger.LogInformation(
            "Service => Attempting to update all orders with tyre {TyreId}",
            message.IdOfEntityToUpdate);

        var tyreModel = JsonSerializer.Deserialize<TyreModel>(message.SerializedModel);

        stopwatch.Start();
        try
        {
            // The price will not be updated because the data will lose integrity
            await context.OrderItems
                .Where(item => item.TyreId == Guid.Parse(message.IdOfEntityToUpdate))
                .ExecuteUpdateAsync(
                    updates => updates
                        .SetProperty(item => item.TyreName, tyreModel!.Name));
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            logger.LogError(
                ex,
                "{Announcement} ({StopwatchElapsedTime}ms): Attempt to update all orders with tyre {TyreId} was unsuccessful",
                LoggerConstants.FailedAnnouncement, stopwatch.ElapsedMilliseconds, message.IdOfEntityToUpdate);

            throw ex.GetBaseException();
        }
        stopwatch.Stop();

        logger.LogInformation(
            "{Announcement} ({StopwatchElapsedTime}ms): Attempt to update all orders with tyre {TyreId} completed successfully",
            LoggerConstants.SucceededAnnouncement, stopwatch.ElapsedMilliseconds, message.IdOfEntityToUpdate);
    }
}
