using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CuttingOptimizer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSectorMarketMaterial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MaterialId",
                table: "Sectors",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sectors_MaterialId",
                table: "Sectors",
                column: "MaterialId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sectors_Materials_MaterialId",
                table: "Sectors",
                column: "MaterialId",
                principalTable: "Materials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sectors_Materials_MaterialId",
                table: "Sectors");

            migrationBuilder.DropIndex(
                name: "IX_Sectors_MaterialId",
                table: "Sectors");

            migrationBuilder.DropColumn(
                name: "MaterialId",
                table: "Sectors");
        }
    }
}
