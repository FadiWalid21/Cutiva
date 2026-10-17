using CuttingOptimizer.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.Workshops.Queries.GetWorkshopById;

public sealed class GetWorkshopByIdQueryHandler(
    IApplicationDbContext dbContext)
    : IRequestHandler<GetWorkshopByIdQuery, WorkshopDto?>
{
    public async Task<WorkshopDto?> Handle(
        GetWorkshopByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await dbContext.Workshops
            .AsNoTracking()
            .Where(workshop => workshop.Id == request.Id)
            .Select(workshop => new WorkshopDto(
                workshop.Id,
                workshop.Name))
            .FirstOrDefaultAsync(cancellationToken);
    }
}