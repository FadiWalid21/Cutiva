using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CuttingOptimizer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PersistCuttingPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CuttingPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkshopId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaterialId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: true),
                    KerfMm = table.Column<int>(type: "integer", nullable: false),
                    IsComplete = table.Column<bool>(type: "boolean", nullable: false),
                    TotalPieceLengthMm = table.Column<int>(type: "integer", nullable: false),
                    TotalStockLengthMm = table.Column<int>(type: "integer", nullable: false),
                    TotalBarsUsed = table.Column<int>(type: "integer", nullable: false),
                    TotalMaterialCost = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    TotalKerfMm = table.Column<int>(type: "integer", nullable: false),
                    TotalLeftoverMm = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StrategyNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuttingPlans", x => x.Id);
                    table.CheckConstraint("CK_CuttingPlans_KerfMm", "\"KerfMm\" >= 0");
                    table.CheckConstraint("CK_CuttingPlans_TotalPieceLengthMm", "\"TotalPieceLengthMm\" >= 0");
                    table.CheckConstraint("CK_CuttingPlans_TotalStockLengthMm", "\"TotalStockLengthMm\" >= 0");
                    table.ForeignKey(
                        name: "FK_CuttingPlans_Materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CuttingPlans_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CuttingPlans_Workshops_WorkshopId",
                        column: x => x.WorkshopId,
                        principalTable: "Workshops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CuttingPlanBars",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CuttingPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    StockBarId = table.Column<Guid>(type: "uuid", nullable: false),
                    LengthMm = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    KerfMm = table.Column<int>(type: "integer", nullable: false),
                    RemainingLengthMm = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuttingPlanBars", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CuttingPlanBars_CuttingPlans_CuttingPlanId",
                        column: x => x.CuttingPlanId,
                        principalTable: "CuttingPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CuttingPlanBars_StockBars_StockBarId",
                        column: x => x.StockBarId,
                        principalTable: "StockBars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CuttingPlanUnallocatedPieces",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CuttingPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequiredCutId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    LengthMm = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuttingPlanUnallocatedPieces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CuttingPlanUnallocatedPieces_CuttingPlans_CuttingPlanId",
                        column: x => x.CuttingPlanId,
                        principalTable: "CuttingPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CuttingPlanUnallocatedPieces_RequiredCuts_RequiredCutId",
                        column: x => x.RequiredCutId,
                        principalTable: "RequiredCuts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CuttingPlanPieces",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CuttingPlanBarId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequiredCutId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    LengthMm = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuttingPlanPieces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CuttingPlanPieces_CuttingPlanBars_CuttingPlanBarId",
                        column: x => x.CuttingPlanBarId,
                        principalTable: "CuttingPlanBars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CuttingPlanPieces_RequiredCuts_RequiredCutId",
                        column: x => x.RequiredCutId,
                        principalTable: "RequiredCuts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CuttingPlanBars_CuttingPlanId",
                table: "CuttingPlanBars",
                column: "CuttingPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_CuttingPlanBars_StockBarId",
                table: "CuttingPlanBars",
                column: "StockBarId");

            migrationBuilder.CreateIndex(
                name: "IX_CuttingPlanPieces_CuttingPlanBarId",
                table: "CuttingPlanPieces",
                column: "CuttingPlanBarId");

            migrationBuilder.CreateIndex(
                name: "IX_CuttingPlanPieces_RequiredCutId",
                table: "CuttingPlanPieces",
                column: "RequiredCutId");

            migrationBuilder.CreateIndex(
                name: "IX_CuttingPlans_MaterialId",
                table: "CuttingPlans",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_CuttingPlans_ProjectId",
                table: "CuttingPlans",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_CuttingPlans_WorkshopId_CreatedAtUtc",
                table: "CuttingPlans",
                columns: new[] { "WorkshopId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_CuttingPlanUnallocatedPieces_CuttingPlanId",
                table: "CuttingPlanUnallocatedPieces",
                column: "CuttingPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_CuttingPlanUnallocatedPieces_RequiredCutId",
                table: "CuttingPlanUnallocatedPieces",
                column: "RequiredCutId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CuttingPlanPieces");

            migrationBuilder.DropTable(
                name: "CuttingPlanUnallocatedPieces");

            migrationBuilder.DropTable(
                name: "CuttingPlanBars");

            migrationBuilder.DropTable(
                name: "CuttingPlans");
        }
    }
}
