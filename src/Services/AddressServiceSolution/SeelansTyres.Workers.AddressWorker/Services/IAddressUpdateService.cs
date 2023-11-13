using ShaylenReddy42.UnpublishedUpdatesManagement.Messages; // BaseMessage

namespace SeelansTyres.Workers.AddressWorker.Services;

/// <summary>
/// Used to update addresses in the database based on events in the system
/// </summary>
public interface IAddressUpdateService
{
    /// <summary>
    /// Deletes all addresses linked to a customer
    /// </summary>
    /// <param name="message">Contains information on the event being processed</param>
    /// <returns></returns>
    Task DeleteAsync(BaseMessage message);
}
