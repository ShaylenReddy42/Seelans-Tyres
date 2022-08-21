using SeelansTyres.Data.Entities;

namespace SeelansTyres.WebApi.Services;

public interface ITyresRepository
{
    /***** Brands *****/
    Task<IEnumerable<Brand>> GetAllBrandsAsync();

    /***** Tyres *****/
    Task<IEnumerable<Tyre>> GetAllTyresAsync();
    Task<Tyre?> GetTyreByIdAsync(int tyreId);
    Task AddNewTyreAsync(Tyre tyreEntity);
    Task<bool> SaveChangesAsync();
}
