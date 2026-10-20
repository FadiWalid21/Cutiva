using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CuttingOptimizer.Application.Features.Materials.Commands.CreateMaterial;

public sealed class CreateMaterialCommandHandler(
    IApplicationDbContext dbContext,
    ILogger<CreateMaterialCommandHandler> logger)
    : IRequestHandler<CreateMaterialCommand, CreateMaterialResult>
{
    public async Task<CreateMaterialResult> Handle(
        CreateMaterialCommand request,
        CancellationToken cancellationToken)
    {
        var sectorExists = await dbContext.Sectors
            .AnyAsync(
                sector => sector.Id == request.SectorId,
                cancellationToken);

        if (!sectorExists)
            return new CreateMaterialSectorNotFound();

        var code = request.Code.Trim().ToUpperInvariant();
        var name = request.Name.Trim();
        var description = string.IsNullOrWhiteSpace(request.Description)
            ? null
            : request.Description.Trim();

        var existing = await dbContext.Materials
            .FirstOrDefaultAsync(
                material => material.SectorId == request.SectorId
                            && material.Code == code,
                cancellationToken);

        if (existing is not null)
        {
            if (existing.IsActive)
                return new CreateMaterialCodeExists(code);

            existing.Name = name;
            existing.Description = description;
            existing.Category = request.Category;
            existing.ThicknessMm = request.ThicknessMm;
            existing.IsActive = true;

            await dbContext.SaveChangesAsync(cancellationToken);
            return new CreateMaterialSucceeded(existing.Id, code, Reactivated: true);
        }

        var created = new Material
        {
            Id = Guid.NewGuid(),
            SectorId = request.SectorId,
            Code = code,
            Name = name,
            Description = description,
            Category = request.Category,
            ThicknessMm = request.ThicknessMm,
            IsActive = true
        };

        dbContext.Materials.Add(created);

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
            return new CreateMaterialCodeExists(code);
        }

        return new CreateMaterialSucceeded(created.Id, code, Reactivated: false);
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
