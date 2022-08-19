using SeelansTyres.Data.Entities;

namespace SeelansTyres.WebApi.Services;

public interface IAddressRepository
{
    Task<bool> CheckIfCustomerExistsAsync(Guid customerId);
    Task<IEnumerable<Address>> GetAddressesForCustomerAsync(Guid customerId);
    Task<Address?> GetAddressForCustomerAsync(Guid customerId, int addressId);
    Task AddNewAddressForCustomerAsync(Guid customerId, Address newAddress);
    Task MarkAsPrefferedAsync(Guid customerId, Address addressToMarkAsPreferred);
    Task<bool> SaveChangesAsync();
}
