using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CuttingOptimizer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCuttingPlanComparisonAlternatives : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ComparisonGroupId",
                table: "CuttingPlans",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StrategyCode",
                table: "CuttingPlans",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_CuttingPlans_ComparisonGroupId",
                table: "CuttingPlans",
                column: "ComparisonGroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CuttingPlans_ComparisonGroupId",
                table: "CuttingPlans");

            migrationBuilder.DropColumn(
                name: "ComparisonGroupId",
                table: "CuttingPlans");

            migrationBuilder.DropColumn(
                name: "StrategyCode",
                table: "CuttingPlans");
        }
    }
}
