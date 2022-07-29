using SeelansTyres.Data.Entities;

namespace SeelansTyres.WebApi.Services;

public interface ISeelansTyresRepository
{
    /***** Addresses *****/
    Task<IEnumerable<Address>?> GetAddressesForCustomerAsync(Guid customerId);

    /***** Brands *****/
    Task<IEnumerable<Brand>> GetAllBrandsAsync();

    /***** Tyres *****/
    Task<IEnumerable<Tyre>> GetAllTyresAsync();
    Task<Tyre?> GetTyreById(int id);

    Task SaveChangesAsync();
}
