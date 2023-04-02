using SeelansTyres.Services.TyresService.Data.Entities; // Brand, Tyre

namespace SeelansTyres.Services.TyresService.Services;

/// <summary>
/// Used to work with brands and tyres in the database
/// </summary>
public interface ITyresRepository
{
    /***** Brands *****/

    /// <summary>
    /// Retrieves all brands from the database
    /// </summary>
    /// <returns>A collection of Brand entities</returns>
    Task<IEnumerable<Brand>> RetrieveAllBrandsAsync();

    /***** Tyres *****/

    /// <summary>
    /// Adds a Tyre entity to the EF Core change tracker to be persisted to the database
    /// </summary>
    /// <param name="tyre">Tyre entity</param>
    Task CreateTyreAsync(Tyre tyre);

    /// <summary>
    /// Retrieves all tyres from the database
    /// </summary>
    /// <param name="availableOnly">
    ///     Links to a property in the Tyre entity 'Available', used to filter for tyres based on availibility
    /// </param>
    /// <returns>A collection of Tyre entities</returns>
    Task<IEnumerable<Tyre>> RetrieveAllTyresAsync(bool availableOnly);

    /// <summary>
    /// Retrieves a tyre from the database if it exists
    /// </summary>
    /// <param name="tyreId">Id of the tyre in the database</param>
    /// <returns>A Tyre entity or null</returns>
    Task<Tyre?> RetrieveSingleTyreAsync(Guid tyreId);


    /// <summary>
    /// Deletes a tyre from the database
    /// </summary>
    /// <param name="tyreId">Id of the tyre in the database</param>
    /// <returns></returns>
    Task DeleteTyreAsync(Guid tyreId);
    

    /// <summary>
    /// Persists changes in the EF Core change tracker to the database
    /// </summary>
    /// <returns>A boolean indicating if changes were persisted</returns>
    Task<bool> SaveChangesAsync();
}
