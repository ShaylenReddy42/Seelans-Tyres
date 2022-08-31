using SeelansTyres.Data.Models;

namespace SeelansTyres.Mvc.Services;

public interface ITyresService
{
    Task<IEnumerable<BrandModel>> GetAllBrandsAsync();
    Task<IEnumerable<TyreModel>> GetAllTyresAsync(bool availableOnly = true);
    Task<TyreModel?> GetTyreByIdAsync(int tyreId);
    Task<bool> AddNewTyreAsync(CreateTyreModel tyre);
    Task<bool> UpdateTyreAsync(int tyreId, CreateTyreModel tyre);
}
