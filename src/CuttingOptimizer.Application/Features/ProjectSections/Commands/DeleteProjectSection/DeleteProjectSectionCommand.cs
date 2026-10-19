using MediatR;

namespace CuttingOptimizer.Application.Features.ProjectSections.Commands.DeleteProjectSection;

public sealed record DeleteProjectSectionCommand(
    Guid ProjectId,
    Guid SectionId) : IRequest<DeleteProjectSectionResult>;

public enum DeleteProjectSectionResult
{
    Deleted,
    NotFound,
    InUse
}
