using CuttingOptimizer.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.ProjectItems.Commands.DeleteProjectItem;

public sealed class DeleteProjectItemHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteProjectItemCommand, bool>
{
    public async Task<bool> Handle(
        DeleteProjectItemCommand request,
        CancellationToken cancellationToken)
    {
        var item = await context.ProjectItems
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (item is null)
            return false;

        var generatedCuts = await context.RequiredCuts
            .Where(c => c.SourceProjectItemId == item.Id)
            .ToListAsync(cancellationToken);

        var sectionIds = generatedCuts
            .Select(c => c.ProjectSectionId)
            .Distinct()
            .ToList();
        var removedCutIds = generatedCuts.Select(c => c.Id).ToHashSet();

        context.RequiredCuts.RemoveRange(generatedCuts);

        foreach (var sectionId in sectionIds)
        {
            var hasOtherCuts = await context.RequiredCuts
                .AnyAsync(
                    c => c.ProjectSectionId == sectionId
                         && !removedCutIds.Contains(c.Id),
                    cancellationToken);

            if (hasOtherCuts)
                continue;

            var section = await context.ProjectSections
                .FirstOrDefaultAsync(s => s.Id == sectionId, cancellationToken);
            if (section is not null)
                context.ProjectSections.Remove(section);
        }

        context.ProjectItems.Remove(item);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
