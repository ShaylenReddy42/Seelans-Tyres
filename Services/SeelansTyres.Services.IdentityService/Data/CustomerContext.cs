using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SeelansTyres.Services.IdentityService.Data.Entities;

namespace SeelansTyres.Services.IdentityService.Data;

public class CustomerContext : IdentityDbContext<Customer, IdentityRole<Guid>, Guid>
{
    public CustomerContext(DbContextOptions<CustomerContext> options) : base(options) { }

    public DbSet<Customer> Customers => Set<Customer>();
}
