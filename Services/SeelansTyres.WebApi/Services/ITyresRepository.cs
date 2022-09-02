using SeelansTyres.Data.Entities;

namespace SeelansTyres.WebApi.Services;

public interface ITyresRepository
{
    /***** Brands *****/
    Task<IEnumerable<Brand>> RetrieveAllBrandsAsync();

    /***** Tyres *****/
    Task<IEnumerable<Tyre>> RetrieveAllTyresAsync(bool availableOnly);
    Task<Tyre?> RetrieveSingleTyreAsync(int tyreId);
    Task CreateTyreAsync(Tyre tyreEntity);
    Task<bool> SaveChangesAsync();
}
