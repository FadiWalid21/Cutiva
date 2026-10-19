using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CuttingOptimizer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectSectionDimensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HeightMm",
                table: "ProjectSections",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnitQuantity",
                table: "ProjectSections",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WidthMm",
                table: "ProjectSections",
                type: "integer",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_ProjectSections_HeightMm",
                table: "ProjectSections",
                sql: "\"HeightMm\" IS NULL OR \"HeightMm\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ProjectSections_UnitQuantity",
                table: "ProjectSections",
                sql: "\"UnitQuantity\" IS NULL OR \"UnitQuantity\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ProjectSections_WidthMm",
                table: "ProjectSections",
                sql: "\"WidthMm\" IS NULL OR \"WidthMm\" > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_ProjectSections_HeightMm",
                table: "ProjectSections");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ProjectSections_UnitQuantity",
                table: "ProjectSections");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ProjectSections_WidthMm",
                table: "ProjectSections");

            migrationBuilder.DropColumn(
                name: "HeightMm",
                table: "ProjectSections");

            migrationBuilder.DropColumn(
                name: "UnitQuantity",
                table: "ProjectSections");

            migrationBuilder.DropColumn(
                name: "WidthMm",
                table: "ProjectSections");
        }
    }
}
