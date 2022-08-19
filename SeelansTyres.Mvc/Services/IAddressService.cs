using SeelansTyres.Data.Models;

namespace SeelansTyres.Mvc.Services;

public interface IAddressService
{
    Task<IEnumerable<AddressModel>> GetAllAddressesForCustomerAsync(Guid customerId);
    Task<bool> AddNewAddressAsync(CreateAddressModel address, Guid customerId);
    Task<bool> MarkAddressAsPreferredAsync(Guid customerId, int addressId);
}
