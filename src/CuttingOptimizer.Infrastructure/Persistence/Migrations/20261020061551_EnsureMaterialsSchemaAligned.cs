using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CuttingOptimizer.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Idempotent repair for production Materials drift: drop leftover glass-era
    /// columns (Kind NOT NULL breaks INSERT when the entity no longer maps them)
    /// and ensure V2 Category / IsActive / ThicknessMm exist.
    /// </summary>
    public partial class EnsureMaterialsSchemaAligned : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DROP INDEX IF EXISTS "IX_Materials_Kind";

                ALTER TABLE "Materials" DROP COLUMN IF EXISTS "Kind";
                ALTER TABLE "Materials" DROP COLUMN IF EXISTS "GlassType";
                ALTER TABLE "Materials" DROP COLUMN IF EXISTS "UnitPricePerM2";

                ALTER TABLE "Materials"
                    ADD COLUMN IF NOT EXISTS "Category" character varying(32) NOT NULL DEFAULT 'Aluminum';

                ALTER TABLE "Materials"
                    ADD COLUMN IF NOT EXISTS "IsActive" boolean NOT NULL DEFAULT TRUE;

                ALTER TABLE "Materials"
                    ADD COLUMN IF NOT EXISTS "ThicknessMm" numeric(10, 2) NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op: repair migration; do not reintroduce legacy columns.
        }
    }
}
