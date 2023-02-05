namespace SeelansTyres.Frontends.Mvc.Services;

public interface ITyresService
{
    Task<IEnumerable<BrandModel>> RetrieveAllBrandsAsync();

    Task<bool> CreateTyreAsync(TyreModel tyre);
    Task<IEnumerable<TyreModel>> RetrieveAllTyresAsync(bool availableOnly = true);
    Task<TyreModel?> RetrieveSingleTyreAsync(Guid tyreId);
    Task<bool> UpdateTyreAsync(Guid tyreId, TyreModel tyre);
    Task<bool> DeleteTyreAsync(Guid tyreId);
}
