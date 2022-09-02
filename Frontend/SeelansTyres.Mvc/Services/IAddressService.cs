using SeelansTyres.Data.Models;

namespace SeelansTyres.Mvc.Services;

public interface IAddressService
{
    Task<IEnumerable<AddressModel>> RetrieveAllAsync(Guid customerId);
    Task<bool> CreateAsync(AddressModel address, Guid customerId);
    Task<bool> MarkAddressAsPreferredAsync(Guid customerId, int addressId);
}
