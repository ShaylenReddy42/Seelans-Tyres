using SeelansTyres.Services.TyresService.Data.Entities;

namespace SeelansTyres.Services.TyresService.Services;

public interface ITyresRepository
{
    /***** Brands *****/
    Task<IEnumerable<Brand>> RetrieveAllBrandsAsync();

    /***** Tyres *****/
    Task CreateTyreAsync(Tyre tyre);
    Task<IEnumerable<Tyre>> RetrieveAllTyresAsync(bool availableOnly);
    Task<Tyre?> RetrieveSingleTyreAsync(Guid tyreId);
    
    Task<bool> SaveChangesAsync();
}
