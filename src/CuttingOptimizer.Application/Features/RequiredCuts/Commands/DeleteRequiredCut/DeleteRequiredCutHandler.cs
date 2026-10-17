using CuttingOptimizer.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.RequiredCuts.Commands.DeleteRequiredCut;

public sealed class DeleteRequiredCutHandler(
    IApplicationDbContext context)
    : IRequestHandler<DeleteRequiredCutCommand, bool>
{
    public async Task<bool> Handle(
        DeleteRequiredCutCommand request,
        CancellationToken cancellationToken)
    {
        var requiredCut = await context.RequiredCuts
            .FirstOrDefaultAsync(
                x => x.Id == request.Id,
                cancellationToken);

        if (requiredCut is null)
            return false;

        context.RequiredCuts.Remove(requiredCut);

        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}