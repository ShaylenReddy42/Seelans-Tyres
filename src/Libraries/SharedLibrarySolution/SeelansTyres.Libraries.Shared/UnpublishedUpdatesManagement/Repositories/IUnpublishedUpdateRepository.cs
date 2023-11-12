using SeelansTyres.Libraries.Shared.UnpublishedUpdatesManagement.Data.Entities; // UnpublishedUpdate

namespace SeelansTyres.Libraries.Shared.UnpublishedUpdatesManagement.Repositories;

/// <summary>
/// Provides functionality to work with unpublished updates in the database
/// </summary>
public interface IUnpublishedUpdateRepository
{
    /// <summary>
    /// Adds an unpublished update to the EF Core change tracker
    /// </summary>
    /// <param name="unpublishedUpdate">The failed update entity</param>
    Task CreateAsync(UnpublishedUpdate unpublishedUpdate);

    /// <summary>
    /// Retrieves all unpublished updates from the database
    /// </summary>
    /// <returns>A collection of UnpublishedUpdate entities</returns>
    Task<List<UnpublishedUpdate>> RetrieveAllAsync();

    /// <summary>
    /// Removes an unpublished update from the database
    /// </summary>
    /// <param name="unpublishedUpdate"></param>
    /// <returns></returns>
    Task DeleteAsync(UnpublishedUpdate unpublishedUpdate);

    /// <summary>
    /// Persists changes in the EF Core change tracker to the database
    /// </summary>
    /// <returns>A boolean indicating if changes were persisted</returns>
    Task<bool> SaveChangesAsync();
}
