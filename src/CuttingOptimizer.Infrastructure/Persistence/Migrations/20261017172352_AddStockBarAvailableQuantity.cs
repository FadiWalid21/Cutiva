using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CuttingOptimizer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStockBarAvailableQuantity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AvailableQuantity",
                table: "StockBars",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddCheckConstraint(
                name: "CK_StockBars_AvailableQuantity",
                table: "StockBars",
                sql: "\"AvailableQuantity\" >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_StockBars_AvailableQuantity",
                table: "StockBars");

            migrationBuilder.DropColumn(
                name: "AvailableQuantity",
                table: "StockBars");
        }
    }
}
