﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SeelansTyres.Services.TyresService.Data;

#nullable disable

namespace SeelansTyres.Services.TyresService.Migrations
{
    [DbContext(typeof(TyresContext))]
    partial class TyresContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("SeelansTyres.Services.TyresService.Data.Entities.Brand", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Brands");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Name = "BFGoodrich"
                        },
                        new
                        {
                            Id = 2,
                            Name = "Continental"
                        },
                        new
                        {
                            Id = 3,
                            Name = "Goodyear"
                        },
                        new
                        {
                            Id = 4,
                            Name = "Hankook"
                        },
                        new
                        {
                            Id = 5,
                            Name = "Michelin"
                        },
                        new
                        {
                            Id = 6,
                            Name = "Pirelli"
                        });
                });

            modelBuilder.Entity("SeelansTyres.Services.TyresService.Data.Entities.Tyre", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("Available")
                        .HasColumnType("bit");

                    b.Property<int>("BrandId")
                        .HasColumnType("int");

                    b.Property<int>("Diameter")
                        .HasColumnType("int");

                    b.Property<string>("ImageUrl")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("nvarchar(40)");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal");

                    b.Property<int>("Ratio")
                        .HasColumnType("int");

                    b.Property<string>("VehicleType")
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("nvarchar(40)");

                    b.Property<int>("Width")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("BrandId");

                    b.ToTable("Tyres");

                    b.HasData(
                        new
                        {
                            Id = new Guid("24590a0a-d723-43c4-ada7-dd3b58c74a91"),
                            Available = true,
                            BrandId = 1,
                            Diameter = 25,
                            ImageUrl = "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png",
                            Name = "Sample1",
                            Price = 200.00m,
                            Ratio = 13,
                            VehicleType = "Car",
                            Width = 115
                        },
                        new
                        {
                            Id = new Guid("4e3a5b63-d76c-44df-84cc-a4271fc117a9"),
                            Available = true,
                            BrandId = 2,
                            Diameter = 25,
                            ImageUrl = "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png",
                            Name = "Sample2",
                            Price = 200.00m,
                            Ratio = 13,
                            VehicleType = "Car",
                            Width = 125
                        },
                        new
                        {
                            Id = new Guid("53565375-5bf0-49d8-a330-9c578c9fc80a"),
                            Available = true,
                            BrandId = 3,
                            Diameter = 25,
                            ImageUrl = "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png",
                            Name = "Sample3",
                            Price = 220.00m,
                            Ratio = 15,
                            VehicleType = "Van",
                            Width = 135
                        },
                        new
                        {
                            Id = new Guid("ea569f71-25b2-4024-9c97-15f7b7ea81c6"),
                            Available = true,
                            BrandId = 4,
                            Diameter = 25,
                            ImageUrl = "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png",
                            Name = "Sample4",
                            Price = 200.00m,
                            Ratio = 14,
                            VehicleType = "Car",
                            Width = 125
                        },
                        new
                        {
                            Id = new Guid("f3cbd832-7239-49a8-8260-d2b5f3bf97a8"),
                            Available = true,
                            BrandId = 5,
                            Diameter = 25,
                            ImageUrl = "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png",
                            Name = "Sample5",
                            Price = 250.00m,
                            Ratio = 16,
                            VehicleType = "Truck",
                            Width = 145
                        },
                        new
                        {
                            Id = new Guid("6b6b43ed-e158-40ec-adf3-9da013c5318b"),
                            Available = true,
                            BrandId = 6,
                            Diameter = 25,
                            ImageUrl = "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png",
                            Name = "Sample6",
                            Price = 230.00m,
                            Ratio = 16,
                            VehicleType = "SUV",
                            Width = 155
                        },
                        new
                        {
                            Id = new Guid("8547a30d-5b54-476d-8725-bdec1921f377"),
                            Available = true,
                            BrandId = 5,
                            Diameter = 25,
                            ImageUrl = "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png",
                            Name = "Sample7",
                            Price = 200.00m,
                            Ratio = 14,
                            VehicleType = "Car",
                            Width = 125
                        },
                        new
                        {
                            Id = new Guid("5aaf0da7-e34a-4b43-b754-7287ef83a671"),
                            Available = true,
                            BrandId = 4,
                            Diameter = 25,
                            ImageUrl = "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png",
                            Name = "Sample8",
                            Price = 250.00m,
                            Ratio = 16,
                            VehicleType = "Truck",
                            Width = 155
                        },
                        new
                        {
                            Id = new Guid("a3f13998-308a-410e-9254-f60a96287be8"),
                            Available = true,
                            BrandId = 3,
                            Diameter = 25,
                            ImageUrl = "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png",
                            Name = "Sample9",
                            Price = 200.00m,
                            Ratio = 15,
                            VehicleType = "Car",
                            Width = 135
                        },
                        new
                        {
                            Id = new Guid("8ac7a2a3-43e4-4b9b-93eb-2de6f229a07c"),
                            Available = true,
                            BrandId = 2,
                            Diameter = 25,
                            ImageUrl = "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png",
                            Name = "Sample10",
                            Price = 230.00m,
                            Ratio = 13,
                            VehicleType = "SUV",
                            Width = 115
                        });
                });

            modelBuilder.Entity("SeelansTyres.Services.TyresService.Data.Entities.Tyre", b =>
                {
                    b.HasOne("SeelansTyres.Services.TyresService.Data.Entities.Brand", "Brand")
                        .WithMany("Tyres")
                        .HasForeignKey("BrandId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Brand");
                });

            modelBuilder.Entity("SeelansTyres.Services.TyresService.Data.Entities.Brand", b =>
                {
                    b.Navigation("Tyres");
                });
#pragma warning restore 612, 618
        }
    }
}
