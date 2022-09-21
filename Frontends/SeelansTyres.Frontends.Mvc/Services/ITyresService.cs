using SeelansTyres.Frontends.Mvc.Models.External;

namespace SeelansTyres.Frontends.Mvc.Services;

public interface ITyresService
{
    Task<IEnumerable<BrandModel>> RetrieveAllBrandsAsync();
    Task<IEnumerable<TyreModel>> RetrieveAllTyresAsync(bool availableOnly = true);
    Task<TyreModel?> RetrieveSingleTyreAsync(Guid tyreId);
    Task<bool> CreateTyreAsync(TyreModel tyre);
    Task<bool> UpdateTyreAsync(Guid tyreId, TyreModel tyre);
}
