using Microsoft.EntityFrameworkCore;          // DbContext, DbContextOptions, DbSet, Set()
using SeelansTyres.Data.AddressData.Entities; // Address

namespace SeelansTyres.Data.AddressData;

public class AddressDbContext : DbContext
{
	public AddressDbContext(DbContextOptions<AddressDbContext> options) : base(options) { }

	public DbSet<Address> Addresses => Set<Address>();
}
