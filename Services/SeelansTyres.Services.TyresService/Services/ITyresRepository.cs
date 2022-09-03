using SeelansTyres.Services.TyresService.Data.Entities;

namespace SeelansTyres.Services.TyresService.Services;

public interface ITyresRepository
{
    /***** Brands *****/
    Task<IEnumerable<Brand>> RetrieveAllBrandsAsync();

    /***** Tyres *****/
    Task<IEnumerable<Tyre>> RetrieveAllTyresAsync(bool availableOnly);
    Task<Tyre?> RetrieveSingleTyreAsync(Guid tyreId);
    Task CreateTyreAsync(Tyre tyreEntity);
    Task<bool> SaveChangesAsync();
}
