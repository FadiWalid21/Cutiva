using MediatR;

namespace CuttingOptimizer.Application.Features.ProjectSections.Commands.CreateProjectSection;

public sealed record CreateProjectSectionCommand(
    Guid ProjectId,
    Guid SectorId,
    string Name) : IRequest<Guid?>;