using Microsoft.EntityFrameworkCore;
using SeelansTyres.Services.AddressService.Data.Entities;

namespace SeelansTyres.Services.AddressService.Data;

public class AddressContext : DbContext
{
	public AddressContext(DbContextOptions<AddressContext> options) : base(options) { }

	public DbSet<Address> Addresses => Set<Address>();
}
