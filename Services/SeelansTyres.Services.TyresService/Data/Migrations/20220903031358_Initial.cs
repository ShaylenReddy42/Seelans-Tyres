using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeelansTyres.Services.TyresService.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Brands",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brands", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tyres",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Width = table.Column<int>(type: "int", nullable: false),
                    Ratio = table.Column<int>(type: "int", nullable: false),
                    Diameter = table.Column<int>(type: "int", nullable: false),
                    VehicleType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Price = table.Column<decimal>(type: "decimal", nullable: false),
                    Available = table.Column<bool>(type: "bit", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BrandId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tyres", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tyres_Brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Brands",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "BFGoodrich" },
                    { 2, "Continental" },
                    { 3, "Goodyear" },
                    { 4, "Hankook" },
                    { 5, "Michelin" },
                    { 6, "Pirelli" }
                });

            migrationBuilder.InsertData(
                table: "Tyres",
                columns: new[] { "Id", "Available", "BrandId", "Diameter", "ImageUrl", "Name", "Price", "Ratio", "VehicleType", "Width" },
                values: new object[,]
                {
                    { new Guid("24590a0a-d723-43c4-ada7-dd3b58c74a91"), true, 1, 25, "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png", "Sample1", 200.00m, 13, "Car", 115 },
                    { new Guid("4e3a5b63-d76c-44df-84cc-a4271fc117a9"), true, 2, 25, "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png", "Sample2", 200.00m, 13, "Car", 125 },
                    { new Guid("53565375-5bf0-49d8-a330-9c578c9fc80a"), true, 3, 25, "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png", "Sample3", 220.00m, 15, "Van", 135 },
                    { new Guid("5aaf0da7-e34a-4b43-b754-7287ef83a671"), true, 4, 25, "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png", "Sample8", 250.00m, 16, "Truck", 155 },
                    { new Guid("6b6b43ed-e158-40ec-adf3-9da013c5318b"), true, 6, 25, "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png", "Sample6", 230.00m, 16, "SUV", 155 },
                    { new Guid("8547a30d-5b54-476d-8725-bdec1921f377"), true, 5, 25, "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png", "Sample7", 200.00m, 14, "Car", 125 },
                    { new Guid("8ac7a2a3-43e4-4b9b-93eb-2de6f229a07c"), true, 2, 25, "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png", "Sample10", 230.00m, 13, "SUV", 115 },
                    { new Guid("a3f13998-308a-410e-9254-f60a96287be8"), true, 3, 25, "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png", "Sample9", 200.00m, 15, "Car", 135 },
                    { new Guid("ea569f71-25b2-4024-9c97-15f7b7ea81c6"), true, 4, 25, "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png", "Sample4", 200.00m, 14, "Car", 125 },
                    { new Guid("f3cbd832-7239-49a8-8260-d2b5f3bf97a8"), true, 5, 25, "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png", "Sample5", 250.00m, 16, "Truck", 145 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tyres_BrandId",
                table: "Tyres",
                column: "BrandId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tyres");

            migrationBuilder.DropTable(
                name: "Brands");
        }
    }
}
