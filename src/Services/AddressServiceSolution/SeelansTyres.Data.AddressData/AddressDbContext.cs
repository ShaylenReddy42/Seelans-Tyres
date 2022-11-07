using Microsoft.EntityFrameworkCore;
using SeelansTyres.Data.AddressData.Entities;

namespace SeelansTyres.Data.AddressData;

public class AddressDbContext : DbContext
{
	public AddressDbContext(DbContextOptions<AddressDbContext> options) : base(options) { }

	public DbSet<Address> Addresses => Set<Address>();
}
