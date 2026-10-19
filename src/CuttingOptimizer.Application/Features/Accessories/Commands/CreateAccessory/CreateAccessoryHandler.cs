using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Application.Features.Accessories;
using CuttingOptimizer.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.Accessories.Commands.CreateAccessory;

public sealed class CreateAccessoryHandler(
    IApplicationDbContext context)
    : IRequestHandler<CreateAccessoryCommand, CreateAccessoryResult>
{
    public async Task<CreateAccessoryResult> Handle(
        CreateAccessoryCommand request,
        CancellationToken cancellationToken)
    {
        var workshopExists = await context.Workshops
            .AnyAsync(w => w.Id == request.WorkshopId, cancellationToken);

        if (!workshopExists)
            return new CreateAccessoryWorkshopNotFound();

        var name = request.Name.Trim();
        var duplicate = await context.Accessories
            .AnyAsync(
                a => a.WorkshopId == request.WorkshopId && a.Name == name,
                cancellationToken);

        if (duplicate)
            return new CreateAccessoryDuplicateName();

        var accessory = new Accessory
        {
            WorkshopId = request.WorkshopId,
            Name = name,
            Brand = string.IsNullOrWhiteSpace(request.Brand)
                ? null
                : request.Brand.Trim(),
            Unit = request.Unit,
            UnitPrice = request.UnitPrice,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        context.Accessories.Add(accessory);
        await context.SaveChangesAsync(cancellationToken);

        return new CreateAccessorySucceeded(ToDto(accessory));
    }

    internal static AccessoryDto ToDto(Accessory accessory) =>
        new(
            accessory.Id,
            accessory.WorkshopId,
            accessory.Name,
            accessory.Brand,
            accessory.Unit,
            accessory.UnitPrice,
            accessory.IsActive,
            accessory.CreatedAtUtc);
}
