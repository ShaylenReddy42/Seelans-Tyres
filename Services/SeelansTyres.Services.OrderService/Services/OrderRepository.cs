using Microsoft.EntityFrameworkCore;
using SeelansTyres.Services.OrderService.Data.Entities;
using SeelansTyres.Services.OrderService.Data;

namespace SeelansTyres.Services.OrderService.Services;

public class OrderRepository : IOrderRepository
{
    private readonly OrdersContext context;

    public OrderRepository(OrdersContext context) =>
            this.context = context;

    public async Task CreateAsync(Order newOrder) => 
        await context.Orders.AddAsync(newOrder);

    public async Task<IEnumerable<Order>> RetrieveAllAsync(Guid? customerId, bool notDeliveredOnly)
    {
        var collection = notDeliveredOnly switch
        {
            true  => context.Orders
                .Include(order => order.OrderItems)
                .Where(order => !order.Delivered),
            false => context.Orders
                .Include(order => order.OrderItems)
        };

        var orders = customerId switch
        {
            null => await collection
                .ToListAsync(),
            _    => await collection
                .Where(order => order.CustomerId == customerId)
                .ToListAsync()
        };

        return orders;
    }

    public async Task<Order?> RetrieveSingleAsync(int id) => 
        await context.Orders
            .Include(order => order.OrderItems)
            .SingleOrDefaultAsync(order => order.Id == id);

    public async Task<bool> SaveChangesAsync() =>
        await context.SaveChangesAsync() >= 0;
}
