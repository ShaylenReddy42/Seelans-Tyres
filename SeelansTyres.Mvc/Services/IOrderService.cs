using SeelansTyres.Data.Models;

namespace SeelansTyres.Mvc.Services;

public interface IOrderService
{
    Task<OrderModel?> RetrieveSingleAsync(int orderId);
    Task<IEnumerable<OrderModel>> RetrieveAllAsync(Guid? customerId = null, bool notDeliveredOnly = false);
    Task<OrderModel?> CreateAsync(CreateOrderModel order);
    Task<bool> MarkOrderAsDeliveredAsync(int orderId);
}
