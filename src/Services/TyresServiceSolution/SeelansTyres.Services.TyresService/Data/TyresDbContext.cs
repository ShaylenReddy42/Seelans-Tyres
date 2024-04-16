using Microsoft.EntityFrameworkCore;                    // DbContext, DbContextOptions, DbSet, Set(), ModelBuilder
using SeelansTyres.Services.TyresService.Data.Entities; // Brand, Tyre
using System.Diagnostics.CodeAnalysis;                  // SuppressMessage

namespace SeelansTyres.Services.TyresService.Data;

public class TyresDbContext(DbContextOptions<TyresDbContext> options) : DbContext(options)
{
    public DbSet<Tyre> Tyres => Set<Tyre>();
	public DbSet<Brand> Brands => Set<Brand>();

    // Persist sample data to database on creation
    [SuppressMessage("Minor Code Smell", "S1075:URIs should not be hardcoded", Justification = "This is a sample image and isn't meant to be testable")]
    protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

        var sampleTyreImageUrl = "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png";

        modelBuilder
			.Entity<Brand>()
			.HasData(
                new Brand { Id = 1, Name = "BFGoodrich"  },
                new Brand { Id = 2, Name = "Continental" },
                new Brand { Id = 3, Name = "Goodyear"    },
                new Brand { Id = 4, Name = "Hankook"     },
                new Brand { Id = 5, Name = "Michelin"    },
                new Brand { Id = 6, Name = "Pirelli"     }
            );

        modelBuilder
            .Entity<Tyre>()
            .HasData(
                new Tyre { Id = Guid.Parse("24590a0a-d723-43c4-ada7-dd3b58c74a91"), Name = "Sample1",  Width = 115, Ratio = 13, Diameter = 25, VehicleType = "Car",   Price = 200.00M, Available = true, ImageUrl = sampleTyreImageUrl, BrandId = 1 },
                new Tyre { Id = Guid.Parse("4e3a5b63-d76c-44df-84cc-a4271fc117a9"), Name = "Sample2",  Width = 125, Ratio = 13, Diameter = 25, VehicleType = "Car",   Price = 200.00M, Available = true, ImageUrl = sampleTyreImageUrl, BrandId = 2 },
                new Tyre { Id = Guid.Parse("53565375-5bf0-49d8-a330-9c578c9fc80a"), Name = "Sample3",  Width = 135, Ratio = 15, Diameter = 25, VehicleType = "Van",   Price = 220.00M, Available = true, ImageUrl = sampleTyreImageUrl, BrandId = 3 },
                new Tyre { Id = Guid.Parse("ea569f71-25b2-4024-9c97-15f7b7ea81c6"), Name = "Sample4",  Width = 125, Ratio = 14, Diameter = 25, VehicleType = "Car",   Price = 200.00M, Available = true, ImageUrl = sampleTyreImageUrl, BrandId = 4 },
                new Tyre { Id = Guid.Parse("f3cbd832-7239-49a8-8260-d2b5f3bf97a8"), Name = "Sample5",  Width = 145, Ratio = 16, Diameter = 25, VehicleType = "Truck", Price = 250.00M, Available = true, ImageUrl = sampleTyreImageUrl, BrandId = 5 },
                new Tyre { Id = Guid.Parse("6b6b43ed-e158-40ec-adf3-9da013c5318b"), Name = "Sample6",  Width = 155, Ratio = 16, Diameter = 25, VehicleType = "SUV",   Price = 230.00M, Available = true, ImageUrl = sampleTyreImageUrl, BrandId = 6 },
                new Tyre { Id = Guid.Parse("8547a30d-5b54-476d-8725-bdec1921f377"), Name = "Sample7",  Width = 125, Ratio = 14, Diameter = 25, VehicleType = "Car",   Price = 200.00M, Available = true, ImageUrl = sampleTyreImageUrl, BrandId = 5 },
                new Tyre { Id = Guid.Parse("5aaf0da7-e34a-4b43-b754-7287ef83a671"), Name = "Sample8",  Width = 155, Ratio = 16, Diameter = 25, VehicleType = "Truck", Price = 250.00M, Available = true, ImageUrl = sampleTyreImageUrl, BrandId = 4 },
                new Tyre { Id = Guid.Parse("a3f13998-308a-410e-9254-f60a96287be8"), Name = "Sample9",  Width = 135, Ratio = 15, Diameter = 25, VehicleType = "Car",   Price = 200.00M, Available = true, ImageUrl = sampleTyreImageUrl, BrandId = 3 },
                new Tyre { Id = Guid.Parse("8ac7a2a3-43e4-4b9b-93eb-2de6f229a07c"), Name = "Sample10", Width = 115, Ratio = 13, Diameter = 25, VehicleType = "SUV",   Price = 230.00M, Available = true, ImageUrl = sampleTyreImageUrl, BrandId = 2 }
            );
    }
}
