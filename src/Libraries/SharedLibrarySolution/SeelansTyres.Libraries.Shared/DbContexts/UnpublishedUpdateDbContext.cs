using Microsoft.EntityFrameworkCore;
using SeelansTyres.Libraries.Shared.DbContexts.UnpublishedUpdateDbContext_Entities;

namespace SeelansTyres.Libraries.Shared.DbContexts;

public class UnpublishedUpdateDbContext : DbContext
{
    public UnpublishedUpdateDbContext(DbContextOptions<UnpublishedUpdateDbContext> options) : base(options) { }

    public DbSet<UnpublishedUpdate> UnpublishedUpdates => Set<UnpublishedUpdate>();
}
