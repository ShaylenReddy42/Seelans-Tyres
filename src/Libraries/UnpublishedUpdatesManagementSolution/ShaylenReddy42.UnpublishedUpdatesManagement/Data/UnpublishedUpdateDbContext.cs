using Microsoft.EntityFrameworkCore;                             // DbContext, DbContextOptions, DbSet, Set()
using ShaylenReddy42.UnpublishedUpdatesManagement.Data.Entities; // UnpublishedUpdate

namespace ShaylenReddy42.UnpublishedUpdatesManagement.Data;

public class UnpublishedUpdateDbContext : DbContext
{
    public UnpublishedUpdateDbContext(DbContextOptions<UnpublishedUpdateDbContext> options) : base(options) { }

    public DbSet<UnpublishedUpdate> UnpublishedUpdates => Set<UnpublishedUpdate>();
}
