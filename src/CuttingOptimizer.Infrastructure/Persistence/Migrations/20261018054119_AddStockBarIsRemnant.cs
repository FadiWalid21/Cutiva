using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CuttingOptimizer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStockBarIsRemnant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRemnant",
                table: "StockBars",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_StockBars_MaterialId_IsRemnant_IsActive",
                table: "StockBars",
                columns: new[] { "MaterialId", "IsRemnant", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StockBars_MaterialId_IsRemnant_IsActive",
                table: "StockBars");

            migrationBuilder.DropColumn(
                name: "IsRemnant",
                table: "StockBars");
        }
    }
}
