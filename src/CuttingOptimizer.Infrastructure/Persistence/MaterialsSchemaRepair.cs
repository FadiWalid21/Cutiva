using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CuttingOptimizer.Infrastructure.Persistence;

/// <summary>
/// Idempotent repair for production Materials drift. Runs after MigrateAsync so
/// leftover glass-era columns (Kind NOT NULL) cannot keep breaking INSERT even if
/// a migration was recorded without applying the SQL, or the site was not restarted.
/// Failures are logged only — never take down API startup.
/// </summary>
public static class MaterialsSchemaRepair
{
    public static async Task ApplyAsync(
        ApplicationDbContext db,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await db.Database.ExecuteSqlRawAsync(
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
                """,
                cancellationToken);

            logger.LogInformation("Materials schema repair applied (idempotent).");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Materials schema repair failed — continuing startup. POST /api/materials may fail until the DB is fixed.");
        }
    }
}
