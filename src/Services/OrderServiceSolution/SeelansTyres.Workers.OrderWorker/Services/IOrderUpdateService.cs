using SeelansTyres.Libraries.Shared.Messages; // BaseMessage

namespace SeelansTyres.Workers.OrderWorker.Services;

/// <summary>
/// Used to update orders in the database based on events in the system
/// </summary>
public interface IOrderUpdateService
{
    /// <summary>
    /// Deletes all orders linked to a customer
    /// </summary>
    /// <param name="message">Contains information on the event being processed</param>
    /// <returns></returns>
    Task DeleteAccountAsync(BaseMessage message);

    /// <summary>
    /// Updates a customer's first name, last name and phone number on all their orders
    /// </summary>
    /// <param name="message">Contains information on the event being processed</param>
    /// <returns></returns>
    Task UpdateAccountAsync(BaseMessage message);

    /// <summary>
    /// Updates a tyre's name on all relevant order items
    /// </summary>
    /// <param name="message">Contains information on the event being processed</param>
    /// <returns></returns>
    Task UpdateTyreAsync(BaseMessage message);
}
