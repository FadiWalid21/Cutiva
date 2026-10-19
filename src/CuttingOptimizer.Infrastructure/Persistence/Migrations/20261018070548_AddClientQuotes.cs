using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CuttingOptimizer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClientQuotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClientQuotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    CuttingPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientQuotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientQuotes_CuttingPlans_CuttingPlanId",
                        column: x => x.CuttingPlanId,
                        principalTable: "CuttingPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClientQuotes_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientQuoteSurcharges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientQuoteId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientQuoteSurcharges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientQuoteSurcharges_ClientQuotes_ClientQuoteId",
                        column: x => x.ClientQuoteId,
                        principalTable: "ClientQuotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientQuotes_CuttingPlanId",
                table: "ClientQuotes",
                column: "CuttingPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientQuotes_ProjectId_CuttingPlanId",
                table: "ClientQuotes",
                columns: new[] { "ProjectId", "CuttingPlanId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientQuoteSurcharges_ClientQuoteId",
                table: "ClientQuoteSurcharges",
                column: "ClientQuoteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientQuoteSurcharges");

            migrationBuilder.DropTable(
                name: "ClientQuotes");
        }
    }
}
