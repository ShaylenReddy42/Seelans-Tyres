using SeelansTyres.Data.Entities;

namespace SeelansTyres.WebApi.Services;

public interface IOrderRepository
{
    Task CreateAsync(Order newOrder);
    Task<IEnumerable<Order>> RetrieveAllAsync(Guid? customerId, bool notDeliveredOnly);
    Task<Order?> RetrieveSingleAsync(int id);
    Task<bool> SaveChangesAsync();
}
