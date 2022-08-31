using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SeelansTyres.Data.Entities;
using SeelansTyres.WebApi.Data;

namespace SeelansTyres.WebApi.Services;

public class OrderRepository : IOrderRepository
{
    private readonly SeelansTyresContext context;
    private readonly UserManager<Customer> userManager;

    public OrderRepository(
        SeelansTyresContext context,
        UserManager<Customer> userManager) => 
            (this.context, this.userManager) = (context, userManager);

    public async Task CreateAsync(Order newOrder)
    {
        newOrder.Customer = await userManager.FindByIdAsync(newOrder.CustomerId.ToString());
        newOrder.Address = await context.Addresses.SingleAsync(address => address.Id == newOrder.AddressId);

        newOrder.OrderItems
            .ToList()
            .ForEach(item => item.Tyre = context.Tyres
                        .Include(tyre => tyre.Brand)
                        .Single(tyre => tyre.Id == item.TyreId));

        await context.Orders.AddAsync(newOrder);
    }

    public async Task<IEnumerable<Order>> RetrieveAllAsync(Guid? customerId, bool notDeliveredOnly)
    {
        var collection = notDeliveredOnly switch
        {
            true => context.Orders
                .Include(order => order.Customer)
                .Include(order => order.Address)
                .Include(order => order.OrderItems)
                    .ThenInclude(item => item.Tyre)
                        .ThenInclude(tyre => tyre!.Brand)
                .Where(order => !order.Delivered),
            false => context.Orders
                .Include(order => order.Customer)
                .Include(order => order.Address)
                .Include(order => order.OrderItems)
                    .ThenInclude(item => item.Tyre)
                        .ThenInclude(tyre => tyre!.Brand)
        };

        var orders = customerId switch
        {
            null => await collection
                .ToListAsync(),
            _ => await collection
                .Where(order => order.CustomerId == customerId)
                .ToListAsync()
        };

        return orders;
    }

    public async Task<Order?> RetrieveSingleAsync(int id)
    {
        return await context.Orders
            .Include(order => order.Customer)
            .Include(order => order.Address)
            .Include(order => order.OrderItems)
                .ThenInclude(item => item.Tyre)
                    .ThenInclude(tyre => tyre!.Brand)
            .SingleOrDefaultAsync(order => order.Id == id);
    }

    public async Task<bool> SaveChangesAsync() =>
        await context.SaveChangesAsync() >= 0;
}
