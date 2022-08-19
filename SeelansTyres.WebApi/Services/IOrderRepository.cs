using SeelansTyres.Data.Entities;

namespace SeelansTyres.WebApi.Services;

public interface IOrderRepository
{
    Task AddNewOrderAsync(Order newOrder);
    Task<IEnumerable<Order>> GetAllOrdersAsync(Guid? customerId, bool notDeliveredOnly);
    Task<Order?> GetOrderByIdAsync(int id);
    Task<bool> SaveChangesAsync();
}
