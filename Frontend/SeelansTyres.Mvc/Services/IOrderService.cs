using SeelansTyres.Mvc.Models.External;

namespace SeelansTyres.Mvc.Services;

public interface IOrderService
{
    Task<OrderModel?> RetrieveSingleAsync(int orderId);
    Task<IEnumerable<OrderModel>> RetrieveAllAsync(Guid? customerId = null, bool notDeliveredOnly = false);
    Task<OrderModel?> CreateAsync(OrderModel order);
    Task<bool> MarkOrderAsDeliveredAsync(int orderId);
}
