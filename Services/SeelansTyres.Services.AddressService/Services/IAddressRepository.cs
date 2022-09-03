using SeelansTyres.Services.AddressService.Data.Entities;

namespace SeelansTyres.Services.AddressService.Services;

public interface IAddressRepository
{
    Task<IEnumerable<Address>> RetrieveAllAsync(Guid customerId);
    Task<Address?> RetrieveSingleAsync(Guid customerId, Guid addressId);
    Task CreateAsync(Guid customerId, Address newAddress);
    Task MarkAsPrefferedAsync(Guid customerId, Address addressToMarkAsPreferred);
    Task<bool> SaveChangesAsync();
}
