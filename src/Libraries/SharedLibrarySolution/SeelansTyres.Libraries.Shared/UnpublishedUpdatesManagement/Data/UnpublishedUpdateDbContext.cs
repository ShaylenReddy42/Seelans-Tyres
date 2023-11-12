using Microsoft.EntityFrameworkCore;                                            // DbContext, DbContextOptions, DbSet, Set()
using SeelansTyres.Libraries.Shared.UnpublishedUpdatesManagement.Data.Entities; // UnpublishedUpdate

namespace SeelansTyres.Libraries.Shared.UnpublishedUpdatesManagement.Data;

public class UnpublishedUpdateDbContext : DbContext
{
    public UnpublishedUpdateDbContext(DbContextOptions<UnpublishedUpdateDbContext> options) : base(options) { }

    public DbSet<UnpublishedUpdate> UnpublishedUpdates => Set<UnpublishedUpdate>();
}
