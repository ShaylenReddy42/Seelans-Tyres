using Microsoft.EntityFrameworkCore;
using SeelansTyres.Data.OrderData;
using SeelansTyres.Libraries.Shared.Messages;
using System.Diagnostics;
using System.Text.Json;

namespace SeelansTyres.Workers.OrderWorker.Services;

public class OrderUpdateService : IOrderUpdateService
{
    private readonly ILogger<OrderUpdateService> logger;
    private readonly OrderDbContext context;
    private readonly Stopwatch stopwatch = new();

    public OrderUpdateService(
        ILogger<OrderUpdateService> logger,
        OrderDbContext context)
    {
        this.logger = logger;
        this.context = context;
    }

    public async Task DeleteAccountAsync(BaseMessage message)
    {
        logger.LogInformation(
            "Service => Attempting to remove all orders for customer {customerId}",
            message.IdOfEntityToUpdate);

        stopwatch.Start();
        try
        {
            context.Orders.RemoveRange(
                context.Orders
                    .Where(order => order.CustomerId == message.IdOfEntityToUpdate));
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            logger.LogError(
                ex,
                "{announcement} ({stopwatchElapsedTime}ms): Attempt to remove all orders for customer {customerId} was unsuccessful",
                "FAILED", stopwatch.ElapsedMilliseconds, message.IdOfEntityToUpdate);

            throw ex.GetBaseException();
        }
        stopwatch.Stop();

        logger.LogInformation(
            "{announcement} ({stopwatchElapsedTime}ms): Attempt to remove all orders for customer {customerId} completed successfully",
            "SUCCEEDED", stopwatch.ElapsedMilliseconds, message.IdOfEntityToUpdate);

        await context.SaveChangesAsync();
    }

    public async Task UpdateAccountAsync(BaseMessage message)
    {
        logger.LogInformation(
            "Service => Attempting to update all orders for customer {customerId}",
            message.IdOfEntityToUpdate);

        var updateAccountModel = JsonSerializer.Deserialize<UpdateAccountModel>(message.SerializedModel);

        stopwatch.Start();
        try
        {
            var orders = context.Orders.Where(order => order.CustomerId == message.IdOfEntityToUpdate);

            await orders.ForEachAsync(order =>
            {
                order.FirstName = updateAccountModel!.FirstName;
                order.LastName = updateAccountModel!.LastName;
                order.PhoneNumber = updateAccountModel!.PhoneNumber;
            });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            logger.LogError(
                ex,
                "{announcement} ({stopwatchElapsedTime}ms): Attempt to update all orders for customer {customerId} was unsuccessful",
                "FAILED", stopwatch.ElapsedMilliseconds, message.IdOfEntityToUpdate);

            throw ex.GetBaseException();
        }
        stopwatch.Stop();

        logger.LogInformation(
            "{announcement} ({stopwatchElapsedTime}ms): Attempt to update all orders for customer {customerId} completed successfully",
            "SUCCEEDED", stopwatch.ElapsedMilliseconds, message.IdOfEntityToUpdate);

        await context.SaveChangesAsync();
    }

    public async Task UpdateTyreAsync(BaseMessage message)
    {
        logger.LogInformation(
            "Service => Attempting to update all orders with tyre {tyreId}",
            message.IdOfEntityToUpdate);

        var tyreModel = JsonSerializer.Deserialize<TyreModel>(message.SerializedModel);

        stopwatch.Start();
        try
        {
            // The price will not be updated because the data will lose integrity
            var orderItems = context.OrderItems.Where(item => item.TyreId == message.IdOfEntityToUpdate);

            await orderItems.ForEachAsync(item =>
            {
                item.TyreName = tyreModel!.Name;
            });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            logger.LogError(
                ex,
                "{announcement} ({stopwatchElapsedTime}ms): Attempt to update all orders with tyre {tyreId} was unsuccessful",
                "FAILED", stopwatch.ElapsedMilliseconds, message.IdOfEntityToUpdate);

            throw ex.GetBaseException();
        }
        stopwatch.Stop();

        logger.LogInformation(
            "{announcement} ({stopwatchElapsedTime}ms): Attempt to update update all orders with tyre {tyreId} completed successfully",
            "SUCCEEDED", stopwatch.ElapsedMilliseconds, message.IdOfEntityToUpdate);

        await context.SaveChangesAsync();
    }
}
