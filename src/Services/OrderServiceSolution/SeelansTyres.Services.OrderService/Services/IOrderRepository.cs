using SeelansTyres.Data.OrderData.Entities; // Order

namespace SeelansTyres.Services.OrderService.Services;

/// <summary>
/// Used to work with orders in the database
/// </summary>
public interface IOrderRepository
{
    /// <summary>
    /// Adds an Order entity to the EF Core change tracker to be persisted to the database
    /// </summary>
    /// <param name="order">Order entity</param>
    /// <returns></returns>
    Task CreateAsync(Order order);

    /// <summary>
    /// Retrieves all orders from the database
    /// </summary>
    /// <param name="customerId">
    /// Used to filter orders for a specific customer or null for all orders
    /// </param>
    /// <param name="notDeliveredOnly">
    /// <para>
    ///     Links to a property in the Order entity 'Delivered',<br/>
    ///     used to filter for orders based on delivery status
    /// </para>
    /// 
    /// <para>
    ///     NOTE: Filtering is done on the client side [and is considered bad practice], however,<br/>
    ///           it was done to improve performance on the client to prevent making extra calls to the API<br/>
    ///           from the Admin Portal
    /// </para>
    /// </param>
    /// <returns>A collection of Order entities</returns>
    Task<IEnumerable<Order>> RetrieveAllAsync(Guid? customerId, bool notDeliveredOnly);

    /// <summary>
    /// Retrieves an order from the database if it exists
    /// </summary>
    /// <param name="orderId">Id of the order in the database</param>
    /// <returns>An order entity or null</returns>
    Task<Order?> RetrieveSingleAsync(int orderId);


    /// <summary>
    /// Persists changes in the EF Core change tracker to the database
    /// </summary>
    /// <returns>A boolean indicating if changes were persisted</returns>
    Task<bool> SaveChangesAsync();
}
