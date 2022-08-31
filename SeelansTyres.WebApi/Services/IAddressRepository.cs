using SeelansTyres.Data.Entities;

namespace SeelansTyres.WebApi.Services;

public interface IAddressRepository
{
    Task<bool> CheckIfCustomerExistsAsync(Guid customerId);
    Task<IEnumerable<Address>> RetrieveAllAsync(Guid customerId);
    Task<Address?> RetrieveSingleAsync(Guid customerId, int addressId);
    Task CreateAsync(Guid customerId, Address newAddress);
    Task MarkAsPrefferedAsync(Guid customerId, Address addressToMarkAsPreferred);
    Task<bool> SaveChangesAsync();
}
