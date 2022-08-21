using SeelansTyres.Data.Models;

namespace SeelansTyres.Mvc.Services;

public interface IOrderService
{
    Task<OrderModel?> GetOrderByIdAsync(int orderId);
    Task<IEnumerable<OrderModel>> GetAllOrdersAsync(Guid? customerId = null, bool notDeliveredOnly = false);
    Task<OrderModel?> PlaceNewOrderAsync(CreateOrderModel order);
    Task<bool> MarkOrderAsDeliveredAsync(int orderId);
}
