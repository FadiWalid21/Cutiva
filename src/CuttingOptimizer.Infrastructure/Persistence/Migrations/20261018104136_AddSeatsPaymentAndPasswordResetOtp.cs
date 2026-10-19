using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CuttingOptimizer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSeatsPaymentAndPasswordResetOtp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PaymentAmount",
                table: "WorkshopSubscriptions",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentConfirmedAtUtc",
                table: "WorkshopSubscriptions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentCurrency",
                table: "WorkshopSubscriptions",
                type: "character varying(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentNote",
                table: "WorkshopSubscriptions",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "WorkshopSubscriptions",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MaxUsers",
                table: "SubscriptionPlans",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPrimaryContact",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "PasswordResetOtps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    CodeHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConsumedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetOtps", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetOtps_UserId_ExpiresAtUtc",
                table: "PasswordResetOtps",
                columns: new[] { "UserId", "ExpiresAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PasswordResetOtps");

            migrationBuilder.DropColumn(
                name: "PaymentAmount",
                table: "WorkshopSubscriptions");

            migrationBuilder.DropColumn(
                name: "PaymentConfirmedAtUtc",
                table: "WorkshopSubscriptions");

            migrationBuilder.DropColumn(
                name: "PaymentCurrency",
                table: "WorkshopSubscriptions");

            migrationBuilder.DropColumn(
                name: "PaymentNote",
                table: "WorkshopSubscriptions");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "WorkshopSubscriptions");

            migrationBuilder.DropColumn(
                name: "MaxUsers",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "IsPrimaryContact",
                table: "AspNetUsers");
        }
    }
}
