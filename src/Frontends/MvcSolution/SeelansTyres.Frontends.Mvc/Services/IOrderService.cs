namespace SeelansTyres.Frontends.Mvc.Services;

/// <summary>
/// A strongly-typed http client to communicate with the Order Microservice
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Makes a get request to the orders microservice to retrieve an order
    /// </summary>
    /// <param name="orderId">The id of the order to retrieve</param>
    /// <returns>An order if it exists or null</returns>
    Task<OrderModel?> RetrieveSingleAsync(int orderId);

    /// <summary>
    /// Makes a get request for all orders - optionally for a customer - at the orders microservice
    /// </summary>
    /// <param name="customerId">The id of customer</param>
    /// <param name="notDeliveredOnly">Indicates if only undelivered orders should be retrieved</param>
    /// <returns>A collection of orders</returns>
    Task<IEnumerable<OrderModel>> RetrieveAllAsync(Guid? customerId = null, bool notDeliveredOnly = false);

    /// <summary>
    /// Makes a post request at the orders microservice to place an order
    /// </summary>
    /// <param name="order">The new order</param>
    /// <returns>The placed order with it's order id from the database or null on failure</returns>
    Task<OrderModel?> CreateAsync(OrderModel order);

    /// <summary>
    /// Makes a put request to mark an order as delivered at the order microservice
    /// </summary>
    /// <param name="orderId">The id of the order to mark as delivered</param>
    /// <returns>A boolean indicating if a success status code is returned</returns>
    Task<bool> MarkOrderAsDeliveredAsync(int orderId);
}
