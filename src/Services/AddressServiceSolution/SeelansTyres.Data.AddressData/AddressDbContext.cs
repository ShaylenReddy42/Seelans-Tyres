using Microsoft.EntityFrameworkCore;          // DbContext, DbContextOptions, DbSet, Set()
using SeelansTyres.Data.AddressData.Entities; // Address

namespace SeelansTyres.Data.AddressData;

public class AddressDbContext(DbContextOptions<AddressDbContext> options) : DbContext(options)
{
    public DbSet<Address> Addresses => Set<Address>();
}
