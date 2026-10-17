using CuttingOptimizer.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.Workshops.Commands.UpdateWorkshop;

public sealed class UpdateWorkshopCommandHandler(
    IApplicationDbContext dbContext)
    : IRequestHandler<UpdateWorkshopCommand, bool>
{
    public async Task<bool> Handle(
        UpdateWorkshopCommand request,
        CancellationToken cancellationToken)
    {
        var workshop = await dbContext.Workshops
            .FirstOrDefaultAsync(
                workshop => workshop.Id == request.Id,
                cancellationToken);

        if (workshop is null)
            return false;

        workshop.Name = request.Name.Trim();

        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}