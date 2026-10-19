using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Application.Features.Accessories;
using CuttingOptimizer.Application.Features.Accessories.Commands.CreateAccessory;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.Accessories.Commands.UpdateAccessory;

public sealed class UpdateAccessoryHandler(
    IApplicationDbContext context)
    : IRequestHandler<UpdateAccessoryCommand, UpdateAccessoryResult>
{
    public async Task<UpdateAccessoryResult> Handle(
        UpdateAccessoryCommand request,
        CancellationToken cancellationToken)
    {
        var accessory = await context.Accessories
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (accessory is null)
            return new UpdateAccessoryNotFound();

        var name = request.Name.Trim();
        var duplicate = await context.Accessories
            .AnyAsync(
                a => a.WorkshopId == accessory.WorkshopId
                     && a.Name == name
                     && a.Id != accessory.Id,
                cancellationToken);

        if (duplicate)
            return new UpdateAccessoryDuplicateName();

        accessory.Name = name;
        accessory.UnitPrice = request.UnitPrice;
        accessory.Brand = string.IsNullOrWhiteSpace(request.Brand)
            ? null
            : request.Brand.Trim();
        accessory.Unit = request.Unit;
        accessory.IsActive = request.IsActive;

        await context.SaveChangesAsync(cancellationToken);

        return new UpdateAccessorySucceeded(
            CreateAccessoryHandler.ToDto(accessory));
    }
}
