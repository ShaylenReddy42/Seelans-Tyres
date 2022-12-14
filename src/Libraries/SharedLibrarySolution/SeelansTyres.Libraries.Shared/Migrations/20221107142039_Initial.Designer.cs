// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SeelansTyres.Libraries.Shared.DbContexts;

#nullable disable

namespace SeelansTyres.Libraries.Shared.Migrations
{
    [DbContext(typeof(UnpublishedUpdateDbContext))]
    [Migration("20221107142039_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("SeelansTyres.Libraries.Shared.DbContexts.UnpublishedUpdateDbContext_Entities.UnpublishedUpdate", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"), 1L, 1);

                    b.Property<string>("Destination")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("EncodedUpdate")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Retries")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("UnpublishedUpdates");
                });
#pragma warning restore 612, 618
        }
    }
}
