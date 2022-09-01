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
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Tyre> Tyres => Set<Tyre>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder
            .Entity<Brand>()
            .HasData(
                new Brand { Id = 1, Name = "BFGoodrich" },
                new Brand { Id = 2, Name = "Continental" },
                new Brand { Id = 3, Name = "Goodyear" },
                new Brand { Id = 4, Name = "Hankook" },
                new Brand { Id = 5, Name = "Michelin" },
                new Brand { Id = 6, Name = "Pirelli" }
            );

        builder
            .Entity<Tyre>()
            .HasData(
                new Tyre { Id = 1, Name = "Sample1", Width = 115, Ratio = 13, Diameter = 25, VehicleType = "Car", Price = 200.00M, Available = true, ImageUrl = "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png", BrandId = 1 },
                new Tyre { Id = 2, Name = "Sample2", Width = 125, Ratio = 13, Diameter = 25, VehicleType = "Car", Price = 200.00M, Available = true, ImageUrl = "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png", BrandId = 2 },
                new Tyre { Id = 3, Name = "Sample3", Width = 135, Ratio = 15, Diameter = 25, VehicleType = "Van", Price = 220.00M, Available = true, ImageUrl = "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png", BrandId = 3 },
                new Tyre { Id = 4, Name = "Sample4", Width = 125, Ratio = 14, Diameter = 25, VehicleType = "Car", Price = 200.00M, Available = true, ImageUrl = "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png", BrandId = 4 },
                new Tyre { Id = 5, Name = "Sample5", Width = 145, Ratio = 16, Diameter = 25, VehicleType = "Truck", Price = 250.00M, Available = true, ImageUrl = "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png", BrandId = 5 },
                new Tyre { Id = 6, Name = "Sample6", Width = 155, Ratio = 16, Diameter = 25, VehicleType = "SUV", Price = 230.00M, Available = true, ImageUrl = "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png", BrandId = 6 },
                new Tyre { Id = 7, Name = "Sample7", Width = 125, Ratio = 14, Diameter = 25, VehicleType = "Car", Price = 200.00M, Available = true, ImageUrl = "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png", BrandId = 5 },
                new Tyre { Id = 8, Name = "Sample8", Width = 155, Ratio = 16, Diameter = 25, VehicleType = "Truck", Price = 250.00M, Available = true, ImageUrl = "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png", BrandId = 4 },
                new Tyre { Id = 9, Name = "Sample9", Width = 135, Ratio = 15, Diameter = 25, VehicleType = "Car", Price = 200.00M, Available = true, ImageUrl = "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png", BrandId = 3 },
                new Tyre { Id = 10, Name = "Sample10", Width = 115, Ratio = 13, Diameter = 25, VehicleType = "SUV", Price = 230.00M, Available = true, ImageUrl = "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png", BrandId = 2 }
            );

        builder
            .Entity<Order>()
            .HasOne(order => order.Customer)
            .WithMany(customer => customer.Orders)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Entity<Order>()
            .HasOne(order => order.Address)
            .WithMany()
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .Entity<Order>()
            .HasIndex(order => order.AddressId)
            .IsUnique(false);
    }

}
