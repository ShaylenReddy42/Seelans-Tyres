using Microsoft.EntityFrameworkCore;
using SeelansTyres.Data.AddressData.Entities;

namespace SeelansTyres.Data.AddressData;

public class AddressContext : DbContext
{
	public AddressContext(DbContextOptions<AddressContext> options) : base(options) { }

	public DbSet<Address> Addresses => Set<Address>();
}
