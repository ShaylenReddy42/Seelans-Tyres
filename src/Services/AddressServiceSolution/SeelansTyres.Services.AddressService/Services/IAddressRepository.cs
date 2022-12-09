using SeelansTyres.Data.AddressData.Entities;

namespace SeelansTyres.Services.AddressService.Services;

public interface IAddressRepository
{
    Task CreateAsync(Guid customerId, Address newAddress);
    Task<IEnumerable<Address>> RetrieveAllAsync(Guid customerId);
    Task<Address?> RetrieveSingleAsync(Guid customerId, Guid addressId);
    Task MarkAsPrefferedAsync(Guid customerId, Address addressToMarkAsPreferred);
    Task<bool> SaveChangesAsync();
}
