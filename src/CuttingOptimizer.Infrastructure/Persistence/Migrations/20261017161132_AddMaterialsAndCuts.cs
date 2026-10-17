using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CuttingOptimizer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMaterialsAndCuts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Materials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SectorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Materials_Sectors_SectorId",
                        column: x => x.SectorId,
                        principalTable: "Sectors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RequiredCuts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectSectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaterialId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    LengthMm = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequiredCuts", x => x.Id);
                    table.CheckConstraint("CK_RequiredCuts_LengthMm", "\"LengthMm\" > 0");
                    table.CheckConstraint("CK_RequiredCuts_Quantity", "\"Quantity\" > 0");
                    table.ForeignKey(
                        name: "FK_RequiredCuts_Materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RequiredCuts_ProjectSections_ProjectSectionId",
                        column: x => x.ProjectSectionId,
                        principalTable: "ProjectSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StockBars",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MaterialId = table.Column<Guid>(type: "uuid", nullable: false),
                    LengthMm = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockBars", x => x.Id);
                    table.CheckConstraint("CK_StockBars_LengthMm", "\"LengthMm\" > 0");
                    table.CheckConstraint("CK_StockBars_UnitPrice", "\"UnitPrice\" >= 0");
                    table.ForeignKey(
                        name: "FK_StockBars_Materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Materials_SectorId_Code",
                table: "Materials",
                columns: new[] { "SectorId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RequiredCuts_MaterialId",
                table: "RequiredCuts",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_RequiredCuts_ProjectSectionId_MaterialId",
                table: "RequiredCuts",
                columns: new[] { "ProjectSectionId", "MaterialId" });

            migrationBuilder.CreateIndex(
                name: "IX_StockBars_MaterialId_IsActive",
                table: "StockBars",
                columns: new[] { "MaterialId", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RequiredCuts");

            migrationBuilder.DropTable(
                name: "StockBars");

            migrationBuilder.DropTable(
                name: "Materials");
        }
    }
}
