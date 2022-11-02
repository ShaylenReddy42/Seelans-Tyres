using SeelansTyres.Models.OrderModels.V1;

namespace SeelansTyres.Frontends.Mvc.Services;

public interface IOrderService
{
    Task<OrderModel?> RetrieveSingleAsync(int orderId);
    Task<IEnumerable<OrderModel>> RetrieveAllAsync(Guid? customerId = null, bool notDeliveredOnly = false);
    Task<OrderModel?> CreateAsync(OrderModel order);
    Task<bool> MarkOrderAsDeliveredAsync(int orderId);
}
