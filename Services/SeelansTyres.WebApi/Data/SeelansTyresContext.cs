using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SeelansTyres.Data.Entities;

namespace SeelansTyres.WebApi.Data;

public class SeelansTyresContext : IdentityDbContext<Customer, IdentityRole<Guid>, Guid>
{
    public SeelansTyresContext(DbContextOptions<SeelansTyresContext> options) : base(options) { }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Address> Addresses => Set<Address>();

}
