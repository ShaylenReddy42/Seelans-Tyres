using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeelansTyres.WebApi.Migrations
{
    public partial class RemoveTyresTableFromDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tyres");

            migrationBuilder.DropTable(
                name: "Brands");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
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
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BrandId = table.Column<int>(type: "int", nullable: false),
                    Available = table.Column<bool>(type: "bit", nullable: false),
                    Diameter = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Price = table.Column<decimal>(type: "decimal", nullable: false),
                    Ratio = table.Column<int>(type: "int", nullable: false),
                    VehicleType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Width = table.Column<int>(type: "int", nullable: false)
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
                    { 1, true, 1, 25, "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png", "Sample1", 200.00m, 13, "Car", 115 },
                    { 2, true, 2, 25, "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png", "Sample2", 200.00m, 13, "Car", 125 },
                    { 3, true, 3, 25, "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png", "Sample3", 220.00m, 15, "Van", 135 },
                    { 4, true, 4, 25, "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png", "Sample4", 200.00m, 14, "Car", 125 },
                    { 5, true, 5, 25, "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png", "Sample5", 250.00m, 16, "Truck", 145 },
                    { 6, true, 6, 25, "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png", "Sample6", 230.00m, 16, "SUV", 155 },
                    { 7, true, 5, 25, "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png", "Sample7", 200.00m, 14, "Car", 125 },
                    { 8, true, 4, 25, "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png", "Sample8", 250.00m, 16, "Truck", 155 },
                    { 9, true, 3, 25, "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png", "Sample9", 200.00m, 15, "Car", 135 },
                    { 10, true, 2, 25, "https://clipartcraft.com/images/tire-clipart-transparent-background-5.png", "Sample10", 230.00m, 13, "SUV", 115 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tyres_BrandId",
                table: "Tyres",
                column: "BrandId");
        }
    }
}
