using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Application.Features.ProjectItems;
using CuttingOptimizer.Application.Features.ProjectItems.Commands.CreateProjectItem;
using MediatR;

namespace CuttingOptimizer.Application.Features.ProjectItems.Queries.GetProjectItemById;

public sealed class GetProjectItemByIdHandler(IApplicationDbContext context)
    : IRequestHandler<GetProjectItemByIdQuery, ProjectItemDto?>
{
    public async Task<ProjectItemDto?> Handle(
        GetProjectItemByIdQuery request,
        CancellationToken cancellationToken)
    {
        var item = await CreateProjectItemHandler.LoadForDtoAsync(
            context,
            request.Id,
            cancellationToken);

        return item is null ? null : ProjectItemMapper.ToDto(item);
    }
}
