using SeelansTyres.Libraries.Shared.DbContexts.UnpublishedUpdateDbContext_Entities;

namespace SeelansTyres.Libraries.Shared.Services;

public interface IUnpublishedUpdateRepository
{
    Task CreateAsync(UnpublishedUpdate unpublishedUpdate);
    Task<List<UnpublishedUpdate>> RetrieveAllAsync();
    Task DeleteAsync(UnpublishedUpdate unpublishedUpdate);

    Task<bool> SaveChangesAsync();
}
