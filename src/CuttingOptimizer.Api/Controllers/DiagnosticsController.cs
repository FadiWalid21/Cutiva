using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CuttingOptimizer.Infrastructure.Persistence;

namespace CuttingOptimizer.Api.Controllers;

/// <summary>Lightweight deploy/schema checks for MonsterASP smoke tests.</summary>
[ApiController]
[Route("api/diagnostics")]
public sealed class DiagnosticsController(ApplicationDbContext db) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("build")]
    public IActionResult Build()
    {
        var assembly = typeof(DiagnosticsController).Assembly;
        var informational = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;
        return Ok(new
        {
            service = "CuttingOptimizer.Api",
            informationalVersion = informational,
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
            utc = DateTime.UtcNow
        });
    }

    [Authorize]
    [HttpGet("materials-schema")]
    public async Task<IActionResult> MaterialsSchema(CancellationToken cancellationToken)
    {
        await using var cmd = db.Database.GetDbConnection().CreateCommand();
        if (cmd.Connection!.State != System.Data.ConnectionState.Open)
            await cmd.Connection.OpenAsync(cancellationToken);

        cmd.CommandText =
            """
            SELECT
              EXISTS (
                SELECT 1 FROM information_schema.columns
                WHERE table_schema = 'public' AND table_name = 'Materials' AND column_name = 'Kind'
              ) AS kind_exists,
              (SELECT COUNT(*)::int FROM "Materials") AS material_count,
              (SELECT COUNT(*)::int FROM "Materials" WHERE "IsActive") AS active_count,
              (SELECT MAX("MigrationId") FROM "__EFMigrationsHistory") AS latest_migration
            """;

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);

        return Ok(new
        {
            kindExists = reader.GetBoolean(0),
            materialCount = reader.GetInt32(1),
            activeCount = reader.GetInt32(2),
            latestMigration = reader.IsDBNull(3) ? null : reader.GetString(3)
        });
    }
}
