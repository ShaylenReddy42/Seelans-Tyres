namespace SeelansTyres.Frontends.Mvc.Services;

/// <summary>
/// A strongly-typed http client to communicate with the Address Microservice
/// </summary>
public interface IAddressService
{
    /// <summary>
    /// Makes a get request for all addresses for a particular customer from the address microservice
    /// </summary>
    /// <param name="customerId">The id of the customer</param>
    /// <returns>A collection of addresses</returns>
    Task<IEnumerable<AddressModel>> RetrieveAllAsync(Guid customerId);

    /// <summary>
    /// Makes a post request for adding an address at the address microservice
    /// </summary>
    /// <param name="address">The model containing the new address</param>
    /// <param name="customerId">The id of the customer the address links to</param>
    /// <returns>A boolean indicating if a success status code is returned</returns>
    Task<bool> CreateAsync(AddressModel address, Guid customerId);

    /// <summary>
    /// Makes a put request to set a particular address as preferred for a customer at the address microservice
    /// </summary>
    /// <param name="customerId">The id of the customer</param>
    /// <param name="addressId">The id of the address to be marked as preferred</param>
    /// <returns>A boolean indicating if a success status code is returned</returns>
    Task<bool> MarkAddressAsPreferredAsync(Guid customerId, Guid addressId);

    /// <summary>
    /// Makes a delete request at the address microservice to delete an address for a customer
    /// </summary>
    /// <param name="customerId">The id of the customer</param>
    /// <param name="addressId">The id of the address to delete</param>
    /// <returns>A boolean indicating if a success status code is returned</returns>
    Task<bool> DeleteAsync(Guid customerId, Guid addressId);
}
