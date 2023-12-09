using Microsoft.EntityFrameworkCore;                             // DbContext, DbContextOptions, DbSet, Set()
using ShaylenReddy42.UnpublishedUpdatesManagement.Data.Entities; // UnpublishedUpdate

namespace ShaylenReddy42.UnpublishedUpdatesManagement.Data;

/// <summary>
/// The database used to store unpublished updates
/// </summary>
public class UnpublishedUpdateDbContext : DbContext
{
    public UnpublishedUpdateDbContext(DbContextOptions<UnpublishedUpdateDbContext> options) : base(options) { }

    public DbSet<UnpublishedUpdate> UnpublishedUpdates => Set<UnpublishedUpdate>();
}
