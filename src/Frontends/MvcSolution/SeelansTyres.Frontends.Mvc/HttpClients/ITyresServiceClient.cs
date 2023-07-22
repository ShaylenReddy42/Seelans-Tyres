namespace SeelansTyres.Frontends.Mvc.HttpClients;

/// <summary>
/// A strongly-typed http client to communicate with the Tyres Microservice
/// </summary>
public interface ITyresServiceClient
{
    /// <summary>
    /// Makes a get request to the brands endpoint of the tyres microservice
    /// </summary>
    /// <returns>A collection of brands</returns>
    Task<IEnumerable<BrandModel>> RetrieveAllBrandsAsync();

    /// <summary>
    /// Makes a post request to the tyres endpoint to add a tyre to the database
    /// </summary>
    /// <param name="tyre">The newly created tyre to be added to the request body</param>
    /// <returns>A boolean indicating if a success status code is returned</returns>
    Task<bool> CreateTyreAsync(TyreModel tyre);

    /// <summary>
    /// Makes a get request to the tyres endpoint of the tyres microservice
    /// </summary>
    /// <param name="availableOnly">Indicates if all tyres should be included, even those out of stock</param>
    /// <returns>A collection of tyres</returns>
    Task<IEnumerable<TyreModel>> RetrieveAllTyresAsync(bool availableOnly = true);

    /// <summary>
    /// Makes a get request to the tyres endpoint to retrieve one tyre by id
    /// </summary>
    /// <param name="tyreId">The id of the tyre to retrieve</param>
    /// <returns>A tyre if exists or null</returns>
    Task<TyreModel?> RetrieveSingleTyreAsync(Guid tyreId);

    /// <summary>
    /// Makes a put request to the endpoint of a particular tyre at the tyres microservice to update a tyre
    /// </summary>
    /// <param name="tyreId">The id of the tyre to update</param>
    /// <param name="tyre">The model containing the newly updated info to add to the body of the request</param>
    /// <returns>A boolean indicating if a success status code is returned</returns>
    Task<bool> UpdateTyreAsync(Guid tyreId, TyreModel tyre);

    /// <summary>
    /// Makes a delete request to the tyres endpoint of a particular tyre at the tyres microservice to delete a tyre
    /// </summary>
    /// <param name="tyreId">The id of the tyre to delete</param>
    /// <returns></returns>
    Task<bool> DeleteTyreAsync(Guid tyreId);
}
