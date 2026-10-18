using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CuttingOptimizer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ApproveCuttingPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAtUtc",
                table: "CuttingPlans",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "CuttingPlans",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Draft");

            migrationBuilder.Sql(
                """UPDATE "CuttingPlans" SET "Status" = 'Draft' WHERE "Status" = '' OR "Status" IS NULL;""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedAtUtc",
                table: "CuttingPlans");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "CuttingPlans");
        }
    }
}
