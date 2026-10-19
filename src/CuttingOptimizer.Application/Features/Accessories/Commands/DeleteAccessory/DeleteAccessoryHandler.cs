using CuttingOptimizer.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.Accessories.Commands.DeleteAccessory;

public sealed class DeleteAccessoryHandler(
    IApplicationDbContext context)
    : IRequestHandler<DeleteAccessoryCommand, DeleteAccessoryResult>
{
    public async Task<DeleteAccessoryResult> Handle(
        DeleteAccessoryCommand request,
        CancellationToken cancellationToken)
    {
        var accessory = await context.Accessories
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (accessory is null)
            return DeleteAccessoryResult.NotFound;

        var inUse = await context.ProjectSectionAccessories
            .AnyAsync(l => l.AccessoryId == accessory.Id, cancellationToken);

        if (inUse)
        {
            accessory.IsActive = false;
            await context.SaveChangesAsync(cancellationToken);
            return DeleteAccessoryResult.Deactivated;
        }

        context.Accessories.Remove(accessory);
        await context.SaveChangesAsync(cancellationToken);
        return DeleteAccessoryResult.Deleted;
    }
}
