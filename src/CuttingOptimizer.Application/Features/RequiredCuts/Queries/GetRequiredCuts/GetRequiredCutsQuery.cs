using MediatR;

namespace CuttingOptimizer.Application.Features.RequiredCuts.Queries.GetRequiredCuts;

public sealed record GetRequiredCutsQuery(
    Guid? ProjectSectionId = null,
    Guid? MaterialId = null
) : IRequest<IReadOnlyList<RequiredCutDto>>;