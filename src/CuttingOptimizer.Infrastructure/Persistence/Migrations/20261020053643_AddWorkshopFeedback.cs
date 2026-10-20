using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CuttingOptimizer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkshopFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkshopFeedbacks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    WorkshopId = table.Column<Guid>(type: "uuid", nullable: true),
                    OwnerDisplayName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    CompanyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Trigger = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkshopFeedbacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkshopFeedbacks_Workshops_WorkshopId",
                        column: x => x.WorkshopId,
                        principalTable: "Workshops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkshopFeedbacks_CreatedAtUtc",
                table: "WorkshopFeedbacks",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_WorkshopFeedbacks_UserId",
                table: "WorkshopFeedbacks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkshopFeedbacks_WorkshopId",
                table: "WorkshopFeedbacks",
                column: "WorkshopId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkshopFeedbacks");
        }
    }
}
