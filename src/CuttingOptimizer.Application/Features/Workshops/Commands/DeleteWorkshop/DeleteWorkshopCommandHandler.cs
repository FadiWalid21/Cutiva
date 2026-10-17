using CuttingOptimizer.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.Workshops.Commands.DeleteWorkshop;

public sealed class DeleteWorkshopCommandHandler(
    IApplicationDbContext dbContext)
    : IRequestHandler<DeleteWorkshopCommand, bool>
{
    public async Task<bool> Handle(
        DeleteWorkshopCommand request,
        CancellationToken cancellationToken)
    {
        var workshop = await dbContext.Workshops
            .FirstOrDefaultAsync(
                workshop => workshop.Id == request.Id,
                cancellationToken);

        if (workshop is null)
            return false;

        dbContext.Workshops.Remove(workshop);

        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}