using Microsoft.AspNetCore.Identity;                       // IdentityRole
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;   // IdentityDbContext, Set()
using Microsoft.EntityFrameworkCore;                       // DbContextOptions, DbSet
using SeelansTyres.Services.IdentityService.Data.Entities; // Customer

namespace SeelansTyres.Services.IdentityService.Data;

public class CustomerDbContext : IdentityDbContext<Customer, IdentityRole<Guid>, Guid>
{
    public CustomerDbContext(DbContextOptions<CustomerDbContext> options) : base(options) { }

    public DbSet<Customer> Customers => Set<Customer>();
}
