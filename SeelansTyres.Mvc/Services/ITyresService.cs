using SeelansTyres.Data.Models;

namespace SeelansTyres.Mvc.Services;

public interface ITyresService
{
    Task<IEnumerable<BrandModel>> RetrieveAllBrandsAsync();
    Task<IEnumerable<TyreModel>> RetrieveAllTyresAsync(bool availableOnly = true);
    Task<TyreModel?> RetrieveSingleTyreAsync(int tyreId);
    Task<bool> CreateTyreAsync(CreateTyreModel tyre);
    Task<bool> UpdateTyreAsync(int tyreId, CreateTyreModel tyre);
}
