using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CuttingOptimizer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveGlassFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sectors_Materials_GlassMaterialId",
                table: "Sectors");

            migrationBuilder.DropTable(
                name: "GlassRequirements");

            migrationBuilder.DropIndex(
                name: "IX_Sectors_GlassMaterialId",
                table: "Sectors");

            migrationBuilder.DropIndex(
                name: "IX_Materials_Kind",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "GlassMaterialId",
                table: "Sectors");

            migrationBuilder.DropColumn(
                name: "GlassType",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "Kind",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "UnitPricePerM2",
                table: "Materials");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GlassMaterialId",
                table: "Sectors",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GlassType",
                table: "Materials",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Kind",
                table: "Materials",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Profile");

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPricePerM2",
                table: "Materials",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GlassRequirements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GlassMaterialId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectSectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AreaM2 = table.Column<decimal>(type: "numeric(12,4)", precision: 12, scale: 4, nullable: false),
                    GlassType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LineTotal = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    UnitPricePerM2 = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlassRequirements", x => x.Id);
                    table.CheckConstraint("CK_GlassRequirements_AreaM2", "\"AreaM2\" > 0");
                    table.CheckConstraint("CK_GlassRequirements_UnitPricePerM2", "\"UnitPricePerM2\" >= 0");
                    table.ForeignKey(
                        name: "FK_GlassRequirements_Materials_GlassMaterialId",
                        column: x => x.GlassMaterialId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GlassRequirements_ProjectSections_ProjectSectionId",
                        column: x => x.ProjectSectionId,
                        principalTable: "ProjectSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sectors_GlassMaterialId",
                table: "Sectors",
                column: "GlassMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_Materials_Kind",
                table: "Materials",
                column: "Kind");

            migrationBuilder.CreateIndex(
                name: "IX_GlassRequirements_GlassMaterialId",
                table: "GlassRequirements",
                column: "GlassMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_GlassRequirements_ProjectSectionId",
                table: "GlassRequirements",
                column: "ProjectSectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sectors_Materials_GlassMaterialId",
                table: "Sectors",
                column: "GlassMaterialId",
                principalTable: "Materials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
