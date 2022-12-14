using SeelansTyres.Data.OrderData.Entities;

namespace SeelansTyres.Services.OrderService.Services;

public interface IOrderRepository
{
    Task CreateAsync(Order order);
    Task<IEnumerable<Order>> RetrieveAllAsync(Guid? customerId, bool notDeliveredOnly);
    Task<Order?> RetrieveSingleAsync(int id);
    Task<bool> SaveChangesAsync();
}
