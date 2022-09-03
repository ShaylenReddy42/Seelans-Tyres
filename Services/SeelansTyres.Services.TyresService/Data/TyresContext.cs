using Microsoft.EntityFrameworkCore;
using SeelansTyres.Services.TyresService.Data.Entities;

namespace SeelansTyres.Services.TyresService.Data;

public class TyresContext : DbContext
{
	public TyresContext(DbContextOptions<TyresContext> options) : base(options) { }

	public DbSet<Tyre> Tyres => Set<Tyre>();
	public DbSet<Brand> Brands => Set<Brand>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder
			.Entity<Brand>()
			.HasData(
                new Brand { Id = 1, Name = "BFGoodrich" },
                new Brand { Id = 2, Name = "Continental" },
                new Brand { Id = 3, Name = "Goodyear" },
                new Brand { Id = 4, Name = "Hankook" },
                new Brand { Id = 5, Name = "Michelin" },
                new Brand { Id = 6, Name = "Pirelli" }
            );

        modelBuilder
            .Entity<Tyre>()
            .HasData(
                new Tyre { Id = Guid.NewGuid(), Name = "Sample1", Width = 115, Ratio = 13, Diameter = 25, VehicleType = "Car", Price = 200.00M, Available = true, ImageUrl = "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png", BrandId = 1 },
                new Tyre { Id = Guid.NewGuid(), Name = "Sample2", Width = 125, Ratio = 13, Diameter = 25, VehicleType = "Car", Price = 200.00M, Available = true, ImageUrl = "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png", BrandId = 2 },
                new Tyre { Id = Guid.NewGuid(), Name = "Sample3", Width = 135, Ratio = 15, Diameter = 25, VehicleType = "Van", Price = 220.00M, Available = true, ImageUrl = "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png", BrandId = 3 },
                new Tyre { Id = Guid.NewGuid(), Name = "Sample4", Width = 125, Ratio = 14, Diameter = 25, VehicleType = "Car", Price = 200.00M, Available = true, ImageUrl = "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png", BrandId = 4 },
                new Tyre { Id = Guid.NewGuid(), Name = "Sample5", Width = 145, Ratio = 16, Diameter = 25, VehicleType = "Truck", Price = 250.00M, Available = true, ImageUrl = "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png", BrandId = 5 },
                new Tyre { Id = Guid.NewGuid(), Name = "Sample6", Width = 155, Ratio = 16, Diameter = 25, VehicleType = "SUV", Price = 230.00M, Available = true, ImageUrl = "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png", BrandId = 6 },
                new Tyre { Id = Guid.NewGuid(), Name = "Sample7", Width = 125, Ratio = 14, Diameter = 25, VehicleType = "Car", Price = 200.00M, Available = true, ImageUrl = "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png", BrandId = 5 },
                new Tyre { Id = Guid.NewGuid(), Name = "Sample8", Width = 155, Ratio = 16, Diameter = 25, VehicleType = "Truck", Price = 250.00M, Available = true, ImageUrl = "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png", BrandId = 4 },
                new Tyre { Id = Guid.NewGuid(), Name = "Sample9", Width = 135, Ratio = 15, Diameter = 25, VehicleType = "Car", Price = 200.00M, Available = true, ImageUrl = "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png", BrandId = 3 },
                new Tyre { Id = Guid.NewGuid(), Name = "Sample10", Width = 115, Ratio = 13, Diameter = 25, VehicleType = "SUV", Price = 230.00M, Available = true, ImageUrl = "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png", BrandId = 2 }
            );
    }
}
