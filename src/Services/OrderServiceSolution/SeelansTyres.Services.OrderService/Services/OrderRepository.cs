using Microsoft.EntityFrameworkCore;        // Include()
using SeelansTyres.Data.OrderData.Entities; // Order
using SeelansTyres.Data.OrderData;          // OrderDbContext
using System.Diagnostics;                   // Stopwatch

namespace SeelansTyres.Services.OrderService.Services;

public class OrderRepository(
    OrderDbContext context,
    ILogger<OrderRepository> logger) : IOrderRepository
{
    private readonly Stopwatch stopwatch = new();

    public async Task CreateAsync(Order order)
    {
        logger.LogInformation("Repository => Attempting to place a new order");

        stopwatch.Start();
        try
        {
            await context.Orders.AddAsync(order);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            logger.LogError(
                ex,
                "{Announcement} ({StopwatchElapsedTime}ms): Attempt to place a new order was unsuccessful",
                "FAILED", stopwatch.ElapsedMilliseconds);

            throw ex.GetBaseException();
        }
        stopwatch.Stop();

        logger.LogInformation(
            "{Announcement} ({StopwatchElapsedTime}ms): Attempt to place a new order completed successfully",
            "SUCCEEDED", stopwatch.ElapsedMilliseconds);
    }

    public async Task<IEnumerable<Order>> RetrieveAllAsync(Guid? customerId, bool notDeliveredOnly)
    {
        logger.LogInformation(
            "Repository => Attempting to retrieve all orders{For}{CustomerId}{ExceptDelivered}",
            customerId is not null ? " for customer " : "", customerId is not null ? customerId : "", notDeliveredOnly ? " except delivered ones" : "");

        IEnumerable<Order> orders = Enumerable.Empty<Order>();

        stopwatch.Start();
        try
        {
            var collection = notDeliveredOnly switch
            {
                true => context.Orders.Include(order => order.OrderItems).Where(order => !order.Delivered),
                _    => context.Orders.Include(order => order.OrderItems)
            };

            orders = customerId switch
            {
                null => await collection
                                .ToListAsync(),
                _    => await collection
                                .Where(order => order.CustomerId == customerId)
                                .ToListAsync()
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            logger.LogError(
                ex,
                "{Announcement} ({StopwatchElapsedTime}ms): Attempt to retrieve all orders{For}{CustomerId}{ExceptDelivered} was unsuccessful",
                "FAILED", stopwatch.ElapsedMilliseconds,
                customerId is not null ? " for customer " : "", customerId is not null ? customerId : "", notDeliveredOnly ? " except delivered ones" : "");

            throw ex.GetBaseException();
        }
        stopwatch.Stop();

        logger.LogInformation(
            "{Announcement} ({StopwatchElapsedTime}ms): Attempt to retrieve all orders{For}{CustomerId}{ExceptDelivered} completed successfully with {OrdersCount} order(s)",
            "SUCCEEDED", stopwatch.ElapsedMilliseconds,
            customerId is not null ? " for customer " : "", customerId is not null ? customerId : "", notDeliveredOnly ? " except delivered ones" : "",
            orders.Count());

        return orders;
    }

    public async Task<Order?> RetrieveSingleAsync(int orderId)
    {
        logger.LogInformation(
            "Repository => Attempting to retrieve order {OrderId}",
            orderId);

        Order? order = null;

        stopwatch.Start();
        try
        {
            order = await context.Orders
                            .Include(order => order.OrderItems)
                            .SingleOrDefaultAsync(order => order.Id == orderId);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            logger.LogError(
                ex,
                "{Announcement} ({StopwatchElapsedTime}ms): Attempt to retrieve order {OrderId} was unsuccessful",
                "FAILED", stopwatch.ElapsedMilliseconds, orderId);
            
            throw ex.GetBaseException();
        }
        stopwatch.Stop();

        logger.LogInformation(
            "{Announcement} ({StopwatchElapsedTime}ms): Attempt to retrieve order {OrderId} completed successfully",
            "SUCCEEDED", stopwatch.ElapsedMilliseconds, orderId);

        return order;
    }

    public async Task<bool> SaveChangesAsync() =>
        await context.SaveChangesAsync() >= 0;
}
