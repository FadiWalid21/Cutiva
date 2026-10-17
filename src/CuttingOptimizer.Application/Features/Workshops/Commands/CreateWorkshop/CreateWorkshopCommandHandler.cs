using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Domain.Entities;
using MediatR;

namespace CuttingOptimizer.Application.Features.Workshops.Commands.CreateWorkshop;

public sealed class CreateWorkshopCommandHandler(
    IApplicationDbContext dbContext)
    : IRequestHandler<CreateWorkshopCommand, Guid>
{
    public async Task<Guid> Handle(
        CreateWorkshopCommand request,
        CancellationToken cancellationToken)
    {
        var workshop = new Workshop
        {
            Name = request.Name.Trim()
        };

        dbContext.Workshops.Add(workshop);

        await dbContext.SaveChangesAsync(cancellationToken);

        return workshop.Id;
    }
}