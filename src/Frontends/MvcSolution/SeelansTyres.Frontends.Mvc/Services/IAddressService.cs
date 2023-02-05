namespace SeelansTyres.Frontends.Mvc.Services;

public interface IAddressService
{
    Task<IEnumerable<AddressModel>> RetrieveAllAsync(Guid customerId);
    Task<bool> CreateAsync(AddressModel address, Guid customerId);
    Task<bool> MarkAddressAsPreferredAsync(Guid customerId, Guid addressId);
    Task<bool> DeleteAsync(Guid customerId, Guid addressId);
}
