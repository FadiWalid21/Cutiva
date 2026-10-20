using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CuttingOptimizer.Application.Features.Materials.Commands.CreateMaterial;

public sealed class CreateMaterialCommandHandler(
    IApplicationDbContext dbContext,
    ILogger<CreateMaterialCommandHandler> logger)
    : IRequestHandler<CreateMaterialCommand, Guid?>
{
    public async Task<Guid?> Handle(
        CreateMaterialCommand request,
        CancellationToken cancellationToken)
    {
        var sectorExists = await dbContext.Sectors
            .AnyAsync(
                sector => sector.Id == request.SectorId,
                cancellationToken);

        if (!sectorExists)
            return null;

        var code = request.Code.Trim().ToUpperInvariant();

        var codeExists = await dbContext.Materials
            .AnyAsync(
                material => material.SectorId == request.SectorId
                            && material.Code == code,
                cancellationToken);

        if (codeExists)
            return null;

        var material = new Material
        {
            Id = Guid.NewGuid(),
            SectorId = request.SectorId,
            Code = code,
            Name = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim(),
            Category = request.Category,
            ThicknessMm = request.ThicknessMm,
            IsActive = true
        };

        dbContext.Materials.Add(material);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            logger.LogWarning(
                ex,
                "Duplicate material code {Code} for sector {SectorId}",
                code,
                request.SectorId);
            return null;
        }

        return material.Id;
    }

    private static bool IsUniqueViolation(DbUpdateException ex)
    {
        for (Exception? current = ex; current is not null; current = current.InnerException)
        {
            var message = current.Message;
            if (message.Contains("23505", StringComparison.OrdinalIgnoreCase)
                || message.Contains("IX_Materials_SectorId_Code", StringComparison.OrdinalIgnoreCase)
                || message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase)
                || message.Contains("UNIQUE constraint", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
