using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CuttingOptimizer.Infrastructure.Persistence;

/// <summary>
/// Idempotent repair for production Materials drift. Runs after MigrateAsync so
/// leftover glass-era columns (Kind NOT NULL) cannot keep breaking INSERT even if
/// a migration was recorded without applying the SQL, or the site was not restarted.
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
            var kindBefore = await ColumnExistsAsync(db, "Materials", "Kind", cancellationToken);
            logger.LogWarning(
                "Materials schema repair start. Kind column exists before={KindBefore}",
                kindBefore);

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

            var kindAfter = await ColumnExistsAsync(db, "Materials", "Kind", cancellationToken);
            logger.LogWarning(
                "Materials schema repair applied. Kind column exists after={KindAfter}",
                kindAfter);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Materials schema repair failed — POST /api/materials may still 500 until the DB is fixed.");
            throw;
        }
    }

    private static async Task<bool> ColumnExistsAsync(
        ApplicationDbContext db,
        string table,
        string column,
        CancellationToken cancellationToken)
    {
        await using var cmd = db.Database.GetDbConnection().CreateCommand();
        if (cmd.Connection!.State != System.Data.ConnectionState.Open)
            await cmd.Connection.OpenAsync(cancellationToken);

        cmd.CommandText =
            """
            SELECT EXISTS (
              SELECT 1
              FROM information_schema.columns
              WHERE table_schema = 'public'
                AND table_name = @table
                AND column_name = @column
            );
            """;
        var tableParam = cmd.CreateParameter();
        tableParam.ParameterName = "table";
        tableParam.Value = table;
        cmd.Parameters.Add(tableParam);
        var columnParam = cmd.CreateParameter();
        columnParam.ParameterName = "column";
        columnParam.Value = column;
        cmd.Parameters.Add(columnParam);

        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return Equals(result, true);
    }
}
