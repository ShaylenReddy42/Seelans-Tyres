using SeelansTyres.Data.AddressData.Entities; // Address

namespace SeelansTyres.Services.AddressService.Services;

/// <summary>
/// Used to work with addresses in the database
/// </summary>
public interface IAddressRepository
{
    /// <summary>
    /// <para>Creates a new address attached to a customer</para>
    /// 
    /// <para>
    ///     If the new address is marked as preferred,<br/>
    ///     all other addresses are set to false for that customer
    /// </para>
    /// 
    /// <para>
    ///     Addition of the address happens on the EF Core change tracker,<br/>
    ///     the setting of the preferred addresses happens directly on the database
    /// </para>
    /// </summary>
    /// <param name="customerId">Id of the customer the address is linked to</param>
    /// <param name="newAddress">Address entity</param>
    Task CreateAsync(Guid customerId, Address newAddress);

    /// <summary>
    /// Retrieves all addresses for a customer
    /// </summary>
    /// <param name="customerId">Id of the customer the address is linked to</param>
    /// <returns>A collection of Address entities</returns>
    Task<IEnumerable<Address>> RetrieveAllAsync(Guid customerId);

    /// <summary>
    /// Retrieves an address for a customer if it exists
    /// </summary>
    /// <param name="customerId">Id of the customer the address is linked to</param>
    /// <param name="addressId">Id of the address in the database</param>
    /// <returns>An Address entity or null</returns>
    Task<Address?> RetrieveSingleAsync(Guid customerId, Guid addressId);

    /// <summary>
    /// <para>
    ///     Marks an address as preferred for a customer<br/>
    ///     and resets the rest of their addresses to false
    /// </para>
    /// 
    /// <para>
    ///     Resetting the preferred address to false happens directly on the database,<br/>
    ///     marking the incoming address as preferred happens on the EF Core change tracker
    /// </para>
    /// </summary>
    /// <param name="customerId">Id of the customer the address is linked to</param>
    /// <param name="addressToMarkAsPreferred">Address entity</param>
    Task MarkAsPreferredAsync(Guid customerId, Address addressToMarkAsPreferred);

    /// <summary>
    /// Deletes an address from the database that's linked to a customer
    /// </summary>
    /// <param name="customerId">Id of the customer the address is linked to</param>
    /// <param name="addressId">Id of the address in the database</param>
    Task DeleteAsync(Guid customerId, Guid addressId);


    /// <summary>
    /// Persists changes in the EF Core change tracker to the database
    /// </summary>
    /// <returns>A boolean indicating if changes were persisted</returns>
    Task<bool> SaveChangesAsync();
}
