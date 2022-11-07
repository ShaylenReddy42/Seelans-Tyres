using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SeelansTyres.Services.IdentityService.Data.Entities;

namespace SeelansTyres.Services.IdentityService.Data;

public class CustomerDbContext : IdentityDbContext<Customer, IdentityRole<Guid>, Guid>
{
    public CustomerDbContext(DbContextOptions<CustomerDbContext> options) : base(options) { }

    public DbSet<Customer> Customers => Set<Customer>();
}
